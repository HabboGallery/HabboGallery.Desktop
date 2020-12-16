using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Net.Sockets;
using System.Buffers.Text;
using System.Threading.Tasks;

using HabboGallery.Desktop.Habbo;

namespace HabboGallery.Desktop.Network
{
    public sealed class UdpListener : IDisposable
    {
        private const string SCHEME_PREFIX = "habbogallery://";

        public event EventHandler<GalleryRecord> BuyRequestReceived;
        
        private readonly UdpClient _client;
        
        public UdpListener(int port)
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        }

        public async Task ListenAsync(CancellationToken cancellationToken = default)
        {
            static bool TryParseBuy(Span<byte> buffer, out GalleryRecord record)
            {
                record = default;

                // "habbogallery://[OP]/[RECORD_JSON_BASE64]"
                ReadOnlySpan<char> data = Encoding.UTF8.GetString(buffer);

                if (!data.StartsWith(SCHEME_PREFIX))
                    return false;

                data = data.Slice(SCHEME_PREFIX.Length);

                int delimiterIndex = data.IndexOf('/');
                if (delimiterIndex == -1)
                    return false;

                string d = data.Slice(0, delimiterIndex).ToString();

                if (!data.Slice(0, delimiterIndex).SequenceEqual("buy"))
                    return false;

                Span<byte> decoded = new byte[Base64.GetMaxDecodedFromUtf8Length(data.Length - delimiterIndex - 1)];
                var op = Base64.DecodeFromUtf8(buffer.Slice(SCHEME_PREFIX.Length + delimiterIndex + 1), decoded, out _, out int written);

                record = JsonSerializer.Deserialize<GalleryRecord>(decoded.Slice(0, written));
                return true;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult datagram = await _client.ReceiveAsync().ConfigureAwait(false);

                if (!TryParseBuy(datagram.Buffer, out var record))
                    continue; //TODO: some notification for invalid data.

                BuyRequestReceived(this, record);
            }
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
