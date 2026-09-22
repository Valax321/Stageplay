using System.Collections.Immutable;
using MemoryPack;
using Radish.Serialization;
using SDL3;

namespace Radish.IO;

internal partial class LocalSettingsStore(GameInfo gi, string? storagePath = null)
{
    private const string LocalSettingsConfigFile = "local_settings_store.bin";

    [MemoryPackable]
    public partial class SerializedData : IBinarySerializable
    {
        public static FourCC HeaderMagic { get; } = new("SSTO");

        public required ImmutableDictionary<string, IVariant> Values { get; init; }
    }
    
    private readonly Dictionary<string, IVariant> _values = [];
    private readonly string _storagePath = storagePath ?? SDL.SDL_GetPrefPath(
        gi.Organization,
        gi.ApplicationName
    );
    
    public void Load()
    {
        LoadSettingsFromConfigFile(_storagePath);
    }

    public void Save()
    {
        SaveSettingsToConfigFile(_storagePath);
    }
    
    private void LoadSettingsFromConfigFile(string rootPath)
    {
        var path = Path.Combine(rootPath, LocalSettingsConfigFile);
        
        if (!File.Exists(path))
            return;
        
        using var fs = File.OpenRead(path);
        var table = BinaryObject.FromStream<SerializedData>(fs);

        foreach (var (k, v) in table.Values)
        {
            _values.Add(k, v);
        }
    }

    private void SaveSettingsToConfigFile(string rootPath)
    {
        var path = Path.Combine(rootPath, LocalSettingsConfigFile);
        
        var dir = Path.GetDirectoryName(path);
        if (dir != null)
            Directory.CreateDirectory(dir);

        var table = new SerializedData { Values = _values.ToImmutableDictionary() };

        using var fs = File.OpenWrite(path);
        fs.SetLength(0);
        
        BinaryObject.ToStream(table, fs);
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_values.TryGetValue(key, out var n))
        {
            return n.AsBoolean;
        }

        SetBool(key, defaultValue);
        return defaultValue;
    }

    public void SetBool(string key, bool value)
    {
        _values[key] = (BoolVariant)value;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_values.TryGetValue(key, out var n))
        {
            return (int)n.AsInteger;
        }

        SetInt(key, defaultValue);
        return defaultValue;
    }

    public void SetInt(string key, int value)
    {
        _values[key] = (IntVariant)value;
    }
}