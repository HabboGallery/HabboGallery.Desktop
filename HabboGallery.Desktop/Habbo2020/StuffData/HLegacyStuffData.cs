using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HLegacyStuffData : HStuffData
    {
        public string Data { get; set; }

        public HLegacyStuffData()
            : base(HStuffDataFormat.Legacy)
        { }
        public HLegacyStuffData(HPacket packet)
            : this()
        {
            Data = packet.ReadUTF8();
        }
    }
}
