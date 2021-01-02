using System.Globalization;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
#nullable enable
    public class HNewWallItem
    {
        public long Id { get; set; }
        public int TypeId { get; set; }

        public HPoint? Local { get; set; }
        public HPoint? Global { get; set; }

        public float? Y { get; set; }
        public float? Z { get; set; }

        public int State { get; set; }
        public string Data { get; set; }
        public string Location { get; set; }
        public HUsagePolicy UsagePolicy { get; set; }
        public string? Placement { get; set; }

        public long OwnerId { get; set; }
        public string? OwnerName { get; set; }

        public HNewWallItem(HPacket packet)
        {
            Id = packet.ReadInt64();
            TypeId = packet.ReadInt32();
            Location = packet.ReadUTF8();
            Data = packet.ReadUTF8();
            UsagePolicy = (HUsagePolicy)packet.ReadInt32();
            OwnerId = packet.ReadInt64();

            if (float.TryParse(Data, out _))
            {
                State = int.Parse(Data);
            }

            string[] locations = Location.Split(' ');
            if (Location.IndexOf(":") == 0 && locations.Length >= 3)
            {
                Placement = locations[2];

                if (locations[0].Length <= 3 || locations[1].Length <= 2) return;
                string firstLoc = locations[0].Substring(3);
                string secondLoc = locations[1].Substring(2);

                locations = firstLoc.Split(',');
                if (locations.Length >= 2)
                {
                    Global = new HPoint(int.Parse(locations[0]), int.Parse(locations[1]));
                    locations = secondLoc.Split(',');

                    if (locations.Length >= 2)
                    {
                        Local = new HPoint(int.Parse(locations[0]), int.Parse(locations[1]));
                    }
                }
            }
            else if (locations.Length >= 2)
            {
                Placement = locations[0];
                if (Placement == "rightwall" || Placement == "frontwall")
                {
                    Placement = "r";
                }
                else Placement = "l";

                string[] coords = locations[1].Split(',');
                if (coords.Length >= 3)
                {
                    Y = float.Parse(coords[0], CultureInfo.InvariantCulture);
                    Z = float.Parse(coords[1], CultureInfo.InvariantCulture);
                }
            }
        }

        public static HNewWallItem[] Parse(HPacket packet)
        {
            ushort ownersCount = packet.ReadUInt16();
            var owners = new Dictionary<long, string>(ownersCount);
            for (int i = 0; i < ownersCount; i++)
            {
                owners.Add(packet.ReadInt64(), packet.ReadUTF8());
            }

            var wallItems = new HNewWallItem[packet.ReadUInt16()];
            for (int i = 0; i < wallItems.Length; i++)
            {
                var wallItem = new HNewWallItem(packet);
                wallItem.OwnerName = owners[wallItem.OwnerId];

                wallItems[i] = wallItem;
            }
            return wallItems;
        }
    }
}
