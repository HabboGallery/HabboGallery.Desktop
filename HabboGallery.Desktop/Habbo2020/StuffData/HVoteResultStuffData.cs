using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HVoteResultStuffData : HStuffData
    {
        public string State { get; set; }
        public int Result { get; set; }

        public HVoteResultStuffData()
            : base(HStuffDataFormat.VoteResult)
        { }
        public HVoteResultStuffData(HPacket packet)
            : this()
        {
            State = packet.ReadUTF8();
            Result = packet.ReadInt32();
        }
    }
}
