using System.Text;

namespace Radish;

internal static class BinaryExtensions
{
    extension(Stream s)
    {
        public void PadBytes(long multiple, byte value = 0xff)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(multiple);
            while (s.Position % multiple != 0)
            {
                s.Write([value]);
            }
        }
    }
    
    extension(BinaryReader reader)
    {
        public string ReadFixedString(int length)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < length; ++i)
                sb.Append((char)reader.ReadByte());
            return sb.ToString();
        }
    }

    extension(BinaryWriter writer)
    {
        public void WriteFixedString(ReadOnlySpan<char> s, int length)
        {
            for (var i = 0; i < length; ++i)
            {
                if (i < s.Length)
                {
                    var c = (byte)s[i];
                    writer.Write(c);
                }
                else
                {
                    writer.Write(0);
                }
            }
        }
    }
}