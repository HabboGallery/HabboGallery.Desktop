using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HIntArrayStuffData : HStuffData
    {
        public int[] Data { get; set; }

        public HIntArrayStuffData()
            : base(HStuffDataFormat.IntArray)
        { }
        public HIntArrayStuffData(HPacket packet)
            : this()
        {
            Data = new int[packet.ReadUInt16()];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = packet.ReadInt32();
            }
        }
    }
}
