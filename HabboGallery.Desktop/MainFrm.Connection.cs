using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;

using HabboGallery.Desktop.Habbo.Network;

using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;
using Sulakore.Network.Protocol;

using Flazzy;

using Eavesdrop;

namespace HabboGallery.Desktop
{
    [ToolboxItem(false)]
    public partial class MainFrm
    {
        private Guid _randomQuery;

        public bool IsIncomingEncrypted { get; private set; }
        public HotelEndPoint HotelServer { get; set; }

        private Task InjectGameClientAsync(object sender, RequestInterceptedEventArgs e)
        {
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return Task.CompletedTask;

            Eavesdropper.RequestInterceptedAsync -= InjectGameClientAsync;

            Uri remoteUrl = e.Request.RequestUri;

            string clientPath = Path.Combine(Master.DataDirectory.FullName,
                $@"Modified Clients\{remoteUrl.Host}\{remoteUrl.LocalPath}");

            if (!File.Exists(clientPath))
            {
                _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT);
                Eavesdropper.ResponseInterceptedAsync += InterceptGameClientAsync;
            }
            else
            {
                _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
                using var game = new HGame(clientPath);
                game.Disassemble();

                _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
                game.GenerateMessageHashes("Hashes.ini");

                //We don't need this stuff in HabboGallery
                foreach (HMessage message in game.Out.Concat(game.In))
                {
                    message.Class = null;
                    message.Parser = null;
                    message.Structure = null;
                    message.References.Clear();
                }

                Master.In = game.In;
                Master.Out = game.Out;

                Task interceptConnectionTask = InterceptConnectionAsync();

                e.Request = WebRequest.Create(new Uri(clientPath));
                TerminateProxy();
            }

            return Task.CompletedTask;
        }
        private async Task InterceptGameClientAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (e.ContentType != "application/x-shockwave-flash") return;
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return;
            Eavesdropper.ResponseInterceptedAsync -= InterceptGameClientAsync;

            string clientPath = Path.Combine(Master.DataDirectory.FullName, $@"Modified Clients\{e.Uri.Host}\{e.Uri.LocalPath}"); ;
            string clientDirectory = Path.GetDirectoryName(clientPath);
            Directory.CreateDirectory(clientDirectory);

            _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
            using var game = new HGame(await e.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                Location = clientPath
            };

            game.Disassemble();

            _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
            game.GenerateMessageHashes("Hashes.ini");

            //We don't need this stuff in HabboGallery
            foreach (HMessage message in game.Out.Concat(game.In))
            {
                message.Class = null;
                message.Parser = null;
                message.Structure = null;
                message.References.Clear();
            }

            Master.In = game.In;
            Master.Out = game.Out;

            _ui.SetStatusMessage(Constants.MODIFYING_CLIENT);
            game.DisableHostChecks();
            game.InjectKeyShouter(4001);
            game.InjectEndPointShouter(4000);
            game.InjectEndPoint("127.0.0.1", Connection.ListenPort);

            CompressionKind compression = CompressionKind.ZLIB;
#if DEBUG
            compression = CompressionKind.None;
#endif

            _ui.SetStatusMessage(Constants.ASSEMBLING_CLIENT);
            byte[] payload = game.ToArray(compression);
            e.Headers[HttpResponseHeader.ContentLength] = payload.Length.ToString();

            e.Content = new ByteArrayContent(payload);
            using (FileStream clientStream = File.Open(clientPath, FileMode.Create, FileAccess.Write))
            {
                clientStream.Write(payload);
            }

            TerminateProxy();
            Task interceptConnectionTask = InterceptConnectionAsync();
        }
        private async Task InterceptClientPageAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (e.Content == null) return;

            string contentType = e.ContentType.ToLower();
            if (!contentType.Contains("text") && !contentType.Contains("javascript")) return;

            string body = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!body.Contains("info.host") && !body.Contains("info.port")) return;

            Eavesdropper.ResponseInterceptedAsync -= InterceptClientPageAsync;
            Master.GameData.Source = body;

            body = body.Replace(".swf", $".swf?{_randomQuery = Guid.NewGuid()}");
            e.Content = new StringContent(body);

            _ui.SetStatusMessage(Constants.INJECTING_CLIENT);
            Eavesdropper.RequestInterceptedAsync += InjectGameClientAsync;
        }
        private async Task InterceptConnectionAsync()
        {
            _ui.SetStatusMessage(Constants.INTERCEPTING_CONNECTION);
            await Connection.InterceptAsync(HotelServer).ConfigureAwait(false);
        }

        private void ConnectionClosed(object sender, EventArgs e)
        {
            _ui.SetStatusMessage(Constants.DISCONNECTED);

            SearchBtn.Enabled = false;
            Resources.RenderButtonState(SearchBtn, SearchBtn.Enabled);

            Environment.Exit(0);
        }
        private void ConnectionOpened(object sender, ConnectedEventArgs e)
        {
            HPacket endPointPkt = Connection.Local.ReceivePacketAsync().Result;
            e.HotelServer = HotelServer = HotelEndPoint.Parse(endPointPkt.ReadUTF8().Split('\0')[0], endPointPkt.ReadInt32());

            e.HotelServerSource.SetResult(HotelServer);

            _ui.SetStatusMessage(Constants.CONNECTED);

            Invoke((MethodInvoker)delegate
            {
                SearchBtn.Enabled = true;
                Resources.RenderButtonState(SearchBtn, SearchBtn.Enabled);
            });
        }
    }
}
