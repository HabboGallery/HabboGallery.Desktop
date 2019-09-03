using HabboGallery.Habbo;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HabboGallery.Communication
{
    public class DatagramListener
    {
        private static UdpClient _client;
        public event EventHandler<OldPhoto> BuyRequestReceived;
        public event EventHandler<ExternalRoomRequest> RoomRequestReceived; //TODO: Implement this one day
        public bool Listening { get; private set; }

        public DatagramListener(int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            _client = new UdpClient(endPoint);
        }

        public async void Listen()
        {
            if (Listening)
                return;

            Listening = true;

            while (Listening)
            {
                UdpReceiveResult datagram = await _client.ReceiveAsync();
                string rawDataString = Encoding.ASCII.GetString(datagram.Buffer);
                string[] dataArray = rawDataString.Split('/');

                if (dataArray.Length != 2)
                    continue;

                string decoded = Encoding.Default.GetString(Convert.FromBase64String(dataArray[1]));

                switch (dataArray[0])
                {
                    case "buy":
                    {
                        try
                        {
                            OldPhoto photo = JsonConvert.DeserializeObject<OldPhoto>(decoded);
                            BuyRequestReceived(null, photo);
                        }
                        catch (Exception)
                        {
                            // TODO: not sure
                        }
                        break;
                    }
                    case "room":
                    {
                        //TODO: implement this in both Laravel and Desktop
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
