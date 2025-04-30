using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Devolutions.NowClient;

namespace Devolutions.NowDvcApp
{
    internal class NowProtoPipeTransport(NamedPipeClientStream pipe) : INowTransport
    {
        public async Task Write(byte[] data)
        {
            Trace.WriteLine($"sending {data.Length} bytes to pipe");

            await pipe.WriteAsync(data, 0, data.Length);
            await pipe.FlushAsync();
        }

        public async Task<byte[]> Read()
        {
            Trace.WriteLine($"Reading from pipe...");

            var bytesRead = await pipe.ReadAsync(_buffer, 0, _buffer.Length);

            Trace.WriteLine($"Read {bytesRead} from pipe");

            if (bytesRead == 0)
            {
                throw new EndOfStreamException("End of stream reached (DVC)");
            }

            return _buffer[..bytesRead];
        }

        // 64K message buffer
        private readonly byte[] _buffer = new byte[64 * 1024];
    }
}