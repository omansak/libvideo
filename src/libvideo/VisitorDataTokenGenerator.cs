using System;
using System.IO;
using System.Linq;
using System.Text;

namespace VideoLibrary
{
    internal class VisitorDataTokenGenerator : IDisposable
    {
        private readonly MemoryStream _byteBuffer = new MemoryStream();
        private bool _disposed;

        public static string Generate()
        {
            Random r = new Random();
            var tkn = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_", 11).Select(s => s[r.Next(s.Length)]).ToArray());

            return new VisitorDataTokenGenerator()
                .WriteStringField(1, tkn)
                .WriteVarintField(5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - r.Next(600000))
                .WriteBytesField(6, new VisitorDataTokenGenerator()
                    .WriteStringField(1, "US")
                    .WriteBytesField(2, new VisitorDataTokenGenerator()
                        .WriteStringField(2, "")
                        .WriteVarintField(4, r.Next(255) + 1)
                        .ToBytes())
                    .ToBytes())
                .ToUrlencodedBase64();
        }

        private VisitorDataTokenGenerator WriteStringField(int fieldNumber, string value)
        {
            WriteBytesField(fieldNumber, Encoding.UTF8.GetBytes(value));
            return this;
        }

        private VisitorDataTokenGenerator WriteVarintField(int fieldNumber, long value)
        {
            WriteFieldHeader(fieldNumber, 0);
            WriteVarint(value);

            return this;
        }

        private VisitorDataTokenGenerator WriteBytesField(int fieldNumber, byte[] value)
        {
            WriteFieldHeader(fieldNumber, 2);
            WriteVarint(value.Length);
            _byteBuffer.Write(value, 0, value.Length);

            return this;
        }

        private byte[] ToBytes() => _byteBuffer.ToArray();

        private string ToUrlencodedBase64() => Uri.EscapeDataString(Convert.ToBase64String(ToBytes()).Replace('+', '-').Replace('/', '_').TrimEnd('='));

        private void WriteFieldHeader(int fieldNumber, byte wireType) => WriteVarint(((long)fieldNumber << 3) | ((long)wireType & 0x07));

        private void WriteVarint(long value)
        {
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0)
                {
                    b |= 0x80;
                }
                _byteBuffer.WriteByte(b);
            } while (value != 0);
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _byteBuffer?.Dispose();
                _disposed = true;
            }
        }

        ~VisitorDataTokenGenerator()
        {
            Dispose();
        }
    }
}
