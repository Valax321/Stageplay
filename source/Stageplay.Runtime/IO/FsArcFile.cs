using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Radish.Serialization;
using Radish.Utility;

namespace Radish.IO;

/// <summary>
/// A container for a read-only filesystem.
/// </summary>
[PublicAPI]
public class FsArcFile
{
    private static readonly FourCC FileIdentifier = new("RPAK");
    private const uint CurrentVersion = 2;
    
    /// <summary>
    /// Canonical extension for FSARC files.
    /// </summary>
    public const string FileExtension = "fsarc";
    
    /// <summary>
    /// A type of entry in a pak.
    /// </summary>
    public enum EntryType : byte
    {
        /// <summary>
        /// A file entry.
        /// </summary>
        File,
        
        /// <summary>
        /// A directory entry.
        /// </summary>
        Directory,
        
        /// <summary>
        /// Special root directory node.
        /// </summary>
        Root
    }

    private ISubStreamFactory _streamFactory;
    private readonly List<Entry> _entries = [];

    private FsArcFile(ISubStreamFactory streamFactory)
    {
        _streamFactory = streamFactory;
    }

    [DebuggerDisplay("{Name} ({Type})")]
    private class Entry(uint index, EntryType type, string name, long offset, long size, IList<Entry> childNodes, Entry? parent)
    {
        private readonly List<uint> _childIndices = [];
        public uint Index { get; } = index;
        public EntryType Type { get; } = type;
        public string Name { get; } = name;
        public long Offset { get; } = offset;
        public long Size { get; } = size;
        public IList<Entry> ChildNodes { get; } = childNodes;
        public Entry? Parent { get; set; } = parent;

        public string FullName
        {
            get
            {
                var sb = StringBuilderPool.Rent();
                var n = this;
                
                while (n != null && n.Type != EntryType.Root)
                {
                    sb.Insert(0, n.Name);
                    sb.Insert(0, '/');
                    n = n.Parent;
                }

                sb.Remove(0, 1);
                var result = sb.ToString();
                StringBuilderPool.Return(sb);
                return result;
            }
        }

        public Entry? Next
        {
            get
            {
                if (Parent is null)
                    return null;

                var myIndex = Parent.ChildNodes.IndexOf(this);
                if (myIndex < 0)
                    return null;

                if (myIndex >= Parent.ChildNodes.Count)
                    return Parent.Next;
                
                return Parent.ChildNodes[myIndex + 1];
            }
        }

        public Entry? FindChild(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var c in ChildNodes)
            {
                if (c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return c;
            }

            return null;
        }
        
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte)0xff);
            writer.Write(Name);
            writer.Write((byte)Type);
            writer.Write(Offset);
            writer.Write(Size);
            writer.Write((ushort)ChildNodes.Count);
            foreach (var n in ChildNodes)
            {
                writer.Write(n.Index);
            }
            writer.Write((byte)0xff);
        }

        public static Entry ReadFrom(BinaryReader reader, uint index)
        {
            reader.ReadByte();
            
            var name = reader.ReadString();
            var type = (EntryType)reader.ReadByte();
            var offset = reader.ReadInt64();
            var size = reader.ReadInt64();
            var nodeCount = reader.ReadUInt16();

            var e = new Entry(index, type, name, offset, size, [], null);
            for (var i = 0; i < nodeCount; ++i)
            {
                e._childIndices.Add(reader.ReadUInt32());
            }

            reader.ReadByte();
            return e;
        }

        public void ResolveNodeReferences(IReadOnlyList<Entry> nodeList)
        {
            foreach (var n in _childIndices)
            {
                var nn = nodeList[(int)n];
                nn.Parent = this;
                ChildNodes.Add(nn);
            }
        }
    }

    /// <summary>
    /// Gets whether the given file exists in the pak.
    /// </summary>
    /// <param name="path">The path to the file to check for.</param>
    /// <returns>True if present, otherwise false.</returns>
    public bool FileExists(string path)
    {
        var node = FindEntry(path);
        if (node is null)
            return false;

        return node.Type == EntryType.File;
    }

    /// <summary>
    /// Gets whether the given directory exists in the pak.
    /// </summary>
    /// <param name="path">The path to the directory to check for.</param>
    /// <returns>True if present, otherwise false.</returns>
    public bool DirectoryExists(string path)
    {
        var node = FindEntry(path);
        if (node is null)
            return false;

        return node.Type == EntryType.Directory;
    }
    
    public Stream OpenRead(string path)
    {
        var node = FindEntry(path);
        if (node is null)
            throw new FileNotFoundException("File not found in pak", path);

        if (node.Type != EntryType.File)
            throw new FileNotFoundException("Entry in pak was not a file", path);

        return _streamFactory.OpenWithOffset(node.Offset, node.Size);
    }

    public IEnumerable<(string, EntryType)> EnumerateDirectory(string? path = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var node = path is not null ? FindEntry(path) : _entries[0];
        if (node is null)
            throw new DirectoryNotFoundException("Directory not found in pak");

        if (node.ChildNodes.Count == 0)
            return [];

        var collection = new LinkedList<(string, EntryType)>();
        if (searchOption == SearchOption.TopDirectoryOnly)
        {
            for (var i = 0; i < node.ChildNodes.Count; ++i)
            {
                var n = node.ChildNodes[i];
                collection.AddLast((n.FullName, n.Type));
            }
        }
        else
        {
            var n = node.ChildNodes[0];
            while (n != node && n is not null)
            {
                collection.AddLast((n.FullName, n.Type));
                n = n.Next;
            }
        }
        
        return collection;
    }
    
    #region RPAK reading

    /// <summary>
    /// Opens an rpak file for reading.
    /// </summary>
    /// <remarks>The storage container must remain valid for the lifetime of the rpak object.</remarks>
    /// <param name="source">The location to load the file header from.</param>
    /// <param name="subStreamFactory">Interface that can generate streams at a given offset within a file.</param>
    /// <param name="path">The path to the rpak file.</param>
    /// <param name="keepOpen">If true then <see cref="source"/> will not be disposed when the rpak is disposed.</param>
    /// <returns>The opened rpak file.</returns>
    public static FsArcFile OpenRead(Stream source, ISubStreamFactory subStreamFactory, string path, bool keepOpen = false)
    {
        using var reader = new BinaryReader(source, Encoding.UTF8, keepOpen);
        
        var identifier = new FourCC(reader.ReadUInt32());
        if (identifier != FileIdentifier)
            throw new FsArcReadException(path, "Invalid file identifier. File does not appear to be an rpak.");

        var version = reader.ReadUInt32();
        if (version != CurrentVersion)
            throw new FsArcReadException(path, $"Unknown rpak version {version}");

        var pak = new FsArcFile(subStreamFactory);

        var tocPos = reader.ReadInt64();
        FsArcReadException.AssertIsPositive(path, tocPos);
        reader.BaseStream.Seek(tocPos, SeekOrigin.Begin);

        var entryCount = reader.ReadInt32();
        FsArcReadException.AssertIsPositive(path, entryCount);
        if (entryCount == 0)
            throw new FsArcReadException(path, "Pak must have at least one entry node");

        for (var i = 0; i < entryCount; ++i)
        {
            pak._entries.Add(Entry.ReadFrom(reader, (uint)i));
        }
        
        foreach (var e in pak._entries)
            e.ResolveNodeReferences(pak._entries);

        if (pak._entries[0].Type != EntryType.Root)
            throw new FsArcReadException(path, "First entry node in pak must be a root node");

        return pak;
    }
    
    #endregion
    
    #region RPAK building

    private class BuildData
    {
        public required Stream DestFile;
        public long Offset => DestFile.Position;
        public required IList<Entry> Entries;

        public Entry NewFileEntry(FileInfo file, Entry parentNode)
        {
            var idx = Entries.Count;
            var e = new Entry((uint)idx, EntryType.File, file.Name, Offset, file.Length, [], parentNode);
            Entries.Add(e);
            parentNode.ChildNodes.Add(e);
            return e;
        }

        public Entry NewDirectoryEntry(DirectoryInfo dir, Entry parentNode)
        {
            var idx = Entries.Count;
            var e = new Entry((uint)idx, EntryType.Directory, dir.Name, 0, 0, [], parentNode);
            Entries.Add(e);
            parentNode.ChildNodes.Add(e);
            return e;
        }

        public Entry MakeRootEntry()
        {
            var idx = Entries.Count;
            var e = new Entry((uint)idx, EntryType.Root, "__ROOT_FS_NODE__", 0, 0, [], null);
            Entries.Add(e);
            return e;
        }
    }

    public static void CreateFromDirectory(FileInfo outFile, DirectoryInfo dir)
    {
        var data = new BuildData
        {
            DestFile = outFile.OpenWrite(),
            Entries = new List<Entry>()
        };
        
        data.DestFile.SetLength(0);

        using var bw = new BinaryWriter(data.DestFile);
        bw.Write(FileIdentifier.EncodedValue);
        bw.Write(CurrentVersion);
        
        var tocOffsetPos = data.DestFile.Position;
        bw.Write((long)0); // This is for the toc offset, written later
        data.DestFile.PadBytes(32);

        // Entry 0 is always the root node
        var rootEntry = data.MakeRootEntry();
        
        foreach (var root in dir.EnumerateDirectories())
        {
            AddDirectoryRecursive(data, root, rootEntry);
        }

        data.DestFile.PadBytes(4096);
        var tocPos = data.DestFile.Position;
        
        bw.Write(data.Entries.Count);
        foreach (var e in data.Entries)
        {
            e.WriteTo(bw);
        }

        data.DestFile.Seek(tocOffsetPos, SeekOrigin.Begin);
        bw.Write(tocPos);
    }

    private static void AddDirectoryRecursive(BuildData data, DirectoryInfo here, Entry parentEntry)
    {
        var myEntry = data.NewDirectoryEntry(here, parentEntry);
        foreach (var e in here.EnumerateFileSystemInfos().OrderBy(x => x.Name))
        {
            // Ignore all dotfiles
            if (e.Name.StartsWith('.'))
                continue;
            
            if (e is FileInfo file)
            {
                // Pad the file if bigger than a patch delta block size.
                // We don't pad smaller files so that we can pack multiple small files within one block
                if (file.Length > 4096)
                    data.DestFile.PadBytes(4096);
                
                data.NewFileEntry(file, myEntry);
                using var fs = file.OpenRead();
                fs.CopyTo(data.DestFile);
            }
            else if (e is DirectoryInfo dir)
            {
                AddDirectoryRecursive(data, dir, myEntry);
            }
        }
    }
    
    #endregion

    #region StorageContainer interface

    private Entry? FindEntry(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        
        var pathSegments = path.Split('/');
        
        Debug.Assert(_entries.Count > 0);
        var n = _entries[0]; // Entry 0 is always the root node
        Debug.Assert(n.Type == EntryType.Root);

        foreach (var nodePath in pathSegments)
        {
            n = n.FindChild(nodePath);
            if (n == null)
                return null;
        }

        return n;
    }
    
    #endregion
}