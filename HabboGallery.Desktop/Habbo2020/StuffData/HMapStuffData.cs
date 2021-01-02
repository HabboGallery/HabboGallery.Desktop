using Sulakore.Habbo;
using Sulakore.Network.Protocol;

using System.Collections.Generic;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HMapStuffData : HStuffData
    {
        public Dictionary<string, string> Data { get; set; }

        public HMapStuffData()
            : base(HStuffDataFormat.Map)
        { }
        public HMapStuffData(HPacket packet)
            : this()
        {
            ushort length = packet.ReadUInt16();
            Data = new(length);

            for (int i = 0; i < length; i++)
            {
                Data[packet.ReadUTF8()] = packet.ReadUTF8();
            }
        }
    }
}
