using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HHighScoreData
    {
        public int Score { get; set; }
        public string[] Users { get; set; }

        public HHighScoreData(HPacket packet)
        {
            Score = packet.ReadInt32();
            Users = new string[packet.ReadUInt16()];
            for (int i = 0; i < Users.Length; i++)
            {
                Users[i] = packet.ReadUTF8();
            }
        }
    }
}
