using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;

using HabboGallery.Desktop.Habbo;

namespace HabboGallery.Desktop.Network
{
    public class UdpListener : IDisposable
    {
        private readonly UdpClient _client;
        
        public event EventHandler<OldPhoto> BuyRequestReceived;
        //public event EventHandler<ExternalRoomRequest> RoomRequestReceived; //TODO: Implement this one day
        
        public UdpListener(int port)
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        }

        public async Task ListenAsync(CancellationToken cancellationToken = default)
        {
            while (cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult datagram = await _client.ReceiveAsync().ConfigureAwait(false);

                string rawDataString = Encoding.UTF8.GetString(datagram.Buffer);
                string[] dataArray = rawDataString.Split('/');

                if (dataArray.Length != 2)
                    continue;

                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(dataArray[1]));

                switch (dataArray[0])
                {
                    case "buy":
                    {
                        try
                        {
                            OldPhoto photo = JsonSerializer.Deserialize<OldPhoto>(decoded);
                            BuyRequestReceived(null, photo);
                        }
                        catch (Exception) { } //TODO:
                        break;
                    }
                    case "room":
                    {
                        //TODO: implement this in both Web and Desktop
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
