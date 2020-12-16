using System;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Habbo.Camera;

namespace HabboGallery.Desktop.Habbo.Camera
{
    public static class PhotoConverter
    {
        public static HPhoto OldToNew(string foldedData, int zoom)
        {
            return new HPhoto
            {
                Planes = new List<Plane>
                {
                    new Plane
                    {
                        Z = 3546.8730429908073,
                        Color = 0,
                        IsBottomAligned = false,

                        CornerPoints = new List<Point>
                        {
                            new(0, 0), new(0, 0),
                            new(0, 0), new(0, 0)
                        }
                    }
                },
                Sprites = new List<Sprite>
                {
                    new Sprite
                    {
                        Name = Encoding.UTF8.GetString(Convert.FromBase64String(UnfoldStrFill(foldedData))),
                        X = 79,
                        Y = 102,
                        Z = 3545.0969389908073,
                        Color = 16777215,
                        Width = 162,
                        Height = 117
                    }
                },

                Zoom = zoom
            };
        }

        private static string UnfoldStrFill(string filler)
            => !string.IsNullOrEmpty(filler) ? new string(filler.ToCharArray().Select(_ => (char)((_ >= 0x61 && _ <= 0x7A) ? ((_ + 0xD > 0x7A) ? _ - 0xD : _ + 0xD) : (_ >= 0x41 && _ <= 0x5A ? (_ + 0xD > 0x5A ? _ - 0xD : _ + 0xD) : _))).ToArray()) : filler;
    }
}