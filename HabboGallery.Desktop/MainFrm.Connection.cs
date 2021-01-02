using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using HabboGallery.Desktop.Habbo.Network;

using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Network.Protocol;

using Eavesdrop;

using Wazzy;
using Wazzy.Types;
using Wazzy.Bytecode;
using Wazzy.Sections.Subsections;
using Wazzy.Bytecode.Instructions.Control;
using Wazzy.Bytecode.Instructions.Numeric;
using Wazzy.Bytecode.Instructions.Variable;
using Wazzy.Bytecode.Instructions.Parametric;
using Sulakore.Cryptography.Ciphers;
using System.Buffers;

namespace HabboGallery.Desktop
{
    [ToolboxItem(false)]
    public partial class MainFrm
    {
        const string USER_JSON_END = ";window.geoLocation";
        const string USER_JSON_START = "<script>window.session=";

        private byte[] _nonce;
        private Guid _randomQuery;
        private bool _wasBlacklisted;
        private int _unhandledUnityAssets;

        private Task _initializeStreamCiphersTask;

        public bool IsIncomingEncrypted { get; private set; }
        public HotelEndPoint HotelServer { get; set; }

        private Task InjectResourceAsync(object sender, RequestInterceptedEventArgs e)
        {
            if (!_wasBlacklisted && !e.Uri.Query.Contains(_randomQuery.ToString())) return null;

            string resourcePath = Path.GetFullPath($"Cache/{e.Uri.Host}/{e.Uri.LocalPath}");
            if ((_wasBlacklisted) && !File.Exists(resourcePath)) return null;

            if (File.Exists(resourcePath))
            {
                if (--_unhandledUnityAssets == 0)
                {
                    TerminateProxy();
                    _ = InterceptConnectionAsync();
                }
                e.Request = WebRequest.Create(new Uri(resourcePath));
            }
            return null;
        }
        private async Task InterceptResourceAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (!_wasBlacklisted && !e.Uri.Query.Contains(_randomQuery.ToString())) return;

            string resourcePath = Path.GetFullPath($"Cache/{e.Uri.Host}/{e.Uri.LocalPath}");
            string resourceDirectory = Path.Combine(Master.DataDirectory.FullName, resourcePath);
            Directory.CreateDirectory(resourceDirectory);

            byte[] replacement = null;
            string resourceName = e.Uri.Segments[^1];
            switch (resourceName)
            {
                case "habbo2020-global-prod.data.unityweb":
                {
                    //TODO: Pull modified resource from servers.
                    if (!File.Exists(resourceName))
                    {

                    }
                    else
                    {

                    }
                    break;
                }
                case "habbo2020-global-prod.wasm.code.unityweb":
                {
                    replacement = await e.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    replacement = InjectKeyShouter(replacement);
                    break;
                }
                case "habbo2020-global-prod.wasm.framework.unityweb":
                {
                    string body = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
                    body = body.Replace("new WebSocket(instance.url);", $"new WebSocket(\"ws://localhost:{Constants.PROXY_PORT}/websocket\");");
                    replacement = Encoding.UTF8.GetBytes(body);
                    break;
                }
            }

            if (replacement != null)
            {
                e.Content = new ByteArrayContent(replacement);
                e.Headers[HttpResponseHeader.ContentLength] = replacement.Length.ToString();

                using FileStream cacheStream = File.Open(resourcePath, FileMode.Create, FileAccess.Write);
                cacheStream.Write(replacement, 0, replacement.Length);
            }
            --_unhandledUnityAssets;

            if (_unhandledUnityAssets == 0)
            {
                TerminateProxy();
                Task interceptConnectionTask = InterceptConnectionAsync();
            }
        }
        private async Task InterceptClientPageAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (e.Content == null) return;
            string contentType = e.ContentType.ToLower();
            bool hasText = contentType.Contains("text");
            bool hasJson = contentType.Contains("json");
            bool hasJavascript = contentType.Contains("javascript");
            if (!hasText && !hasJson && !hasJavascript) return;

            string body = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (hasText || hasJavascript)
            {
                int userTriggersFound = GetTriggersCount(Master.Configuration.UserInterceptionTriggers, body);
                bool hasUserJson = userTriggersFound >= Master.Configuration.UserInterceptionTriggerCountThreshold;

                if (hasUserJson)
                {
                    int jsonStart = body.IndexOf(USER_JSON_START) + USER_JSON_START.Length;
                    int jsonLength = body.IndexOf(USER_JSON_END) - jsonStart;

                    Master.User = JsonSerializer.Deserialize<HUser>(body.Substring(jsonStart, jsonLength), HAPI.SerializerOptions);
                    return;
                }
            }
            else if (hasJson && GetTriggersCount(Master.Configuration.WASMInterceptionTriggers, body) >= 3)
            {
                body = body.Replace(".unityweb", $".unityweb?{_randomQuery = Guid.NewGuid()}.unityweb");

                e.Content = new StringContent(body);
                e.Headers[HttpResponseHeader.ContentLength] = body.Length.ToString();

                _unhandledUnityAssets = 3;
            }
            else return;

            Eavesdropper.ResponseInterceptedAsync -= InterceptClientPageAsync;
            
            _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT_REQUEST_RESPONSE);
            Eavesdropper.RequestInterceptedAsync += InjectResourceAsync;
            Eavesdropper.ResponseInterceptedAsync += InterceptResourceAsync;

        }
        private static byte[] InjectKeyShouter(byte[] contentBytes)
        {
            var module = new WASMModule(contentBytes);
            module.Disassemble();
            for (int i = 0; i < module.CodeSec.Count; i++)
            {
                // Begin searching for the ChaChaEngine.SetKey method.
                var funcTypeIndex = (int)module.FunctionSec[i];
                FuncType functionType = module.TypeSec[funcTypeIndex];
                CodeSubsection codeSubSec = module.CodeSec[i];

                if (codeSubSec.Locals.Count != 1) continue;
                if (functionType.ParameterTypes.Count != 4) continue;

                bool hasValidParamTypes = true;
                for (int j = 0; j < functionType.ParameterTypes.Count; j++)
                {
                    if (functionType.ParameterTypes[j] == typeof(int)) continue;
                    hasValidParamTypes = false;
                    break;
                }
                if (!hasValidParamTypes) continue; // If all of the parameters are not of type int.

                if (codeSubSec.Expression[0].OP != OPCode.ConstantI32) continue;
                if (codeSubSec.Expression[1].OP != OPCode.LoadI32_8S) continue;
                if (codeSubSec.Expression[2].OP != OPCode.EqualZeroI32) continue;
                if (codeSubSec.Expression[3].OP != OPCode.If) continue;

                // Dig through the block/branching expressions
                var expandedInstructions = WASMInstruction.ConcatNestedExpressions(codeSubSec.Expression).ToArray();
                for (int j = 0, k = expandedInstructions.Length - 2; j < expandedInstructions.Length; j++)
                {
                    WASMInstruction instruction = expandedInstructions[j];
                    if (instruction.OP != OPCode.ConstantI32) continue;

                    var constanti32Ins = (ConstantI32Ins)instruction;
                    if (constanti32Ins.Constant != 12) continue;

                    if (expandedInstructions[++j].OP != OPCode.AddI32) continue;
                    if (expandedInstructions[++j].OP != OPCode.TeeLocal) continue;
                    if (expandedInstructions[++j].OP != OPCode.LoadI32) continue;
                    if (expandedInstructions[++j].OP != OPCode.ConstantI32) continue;
                    if (expandedInstructions[++j].OP != OPCode.SubtractI32) continue;

                    if (expandedInstructions[k--].OP != OPCode.Call) continue;
                    if (expandedInstructions[k--].OP != OPCode.ConstantI32) continue;
                    if (expandedInstructions[k--].OP != OPCode.ConstantI32) continue;
                    if (expandedInstructions[k--].OP != OPCode.ConstantI32) continue;

                    codeSubSec.Expression.InsertRange(0, new WASMInstruction[]
                    {
                        new ConstantI32Ins(0),      // WebSocket Instance Id
                        new GetLocalIns(1),         // Key Pointer
                        new ConstantI32Ins(48),     // Key Length
                        new CallIns(126),           // _WebSocketSend
                        new DropIns(),
                    });
                    return module.ToArray();
                }
            }
            return null;
        }
        private static int GetTriggersCount(IList<string> triggers, string body)
        {
            int triggersFound = 0;
            foreach (string trigger in triggers)
            {
                if (!body.Contains(trigger, StringComparison.CurrentCulture)) continue;
                triggersFound++;
            }
            return triggersFound;
        }

        private async Task InterceptConnectionAsync()
        {
            _ui.SetStatusMessage(Constants.INTERCEPTING_CONNECTION);
            HotelServer = HotelEndPoint.Parse($"game-{Master.User.UniqueId.Substring(2, 2)}.habbo.com", 30001);

            await Master.Connection.InterceptAsync(HotelServer).ConfigureAwait(false);
            _ui.SetStatusMessage(Constants.CONNECTED);
        }

        private async Task InitializeStreamCiphersAsync()
        {
            Master.Connection.Local.BypassReceiveSecureTunnel = 2;
            using IMemoryOwner<byte> key = MemoryPool<byte>.Shared.Rent(48);

            int received = await Master.Connection.Local.ReceiveAsync(key.Memory).ConfigureAwait(false);
            received = await Master.Connection.Local.ReceiveAsync(key.Memory).ConfigureAwait(false);

            Memory<byte> keyRegion = key.Memory.Slice(16, 32);
            InitializeChaChaInstances(keyRegion.Span);
        }
        private void InitializeChaChaInstances(ReadOnlySpan<byte> key)
        {
            Master.Connection.Local.Encrypter = new ChaCha20(key, _nonce);
            Master.Connection.Local.Decrypter = new ChaCha20(key, _nonce);

            Master.Connection.Remote.Encrypter = new ChaCha20(key, _nonce);
            Master.Connection.Remote.Decrypter = new ChaCha20(key, _nonce);
        }

        private void ConnectionClosed(object sender, EventArgs e)
        {
            _ui.SetStatusMessage(Constants.DISCONNECTED);
            Environment.Exit(0);
        }
        private void ConnectionOpened(object sender, ConnectedEventArgs e)
        {
            HPacket endPointPkt = Connection.Local.ReceiveAsync().Result;
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
