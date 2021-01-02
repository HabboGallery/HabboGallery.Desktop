using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HStringArrayStuffData : HStuffData
    {
        public string[] Data { get; set; }

        public HStringArrayStuffData()
            : base(HStuffDataFormat.StringArray)
        { }
        public HStringArrayStuffData(HPacket packet)
            : this()
        {
            Data = new string[packet.ReadUInt16()];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = packet.ReadUTF8();
            }
        }
    }
}
