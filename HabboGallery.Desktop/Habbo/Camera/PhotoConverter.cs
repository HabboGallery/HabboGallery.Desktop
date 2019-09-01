using System.Drawing;
using System.Collections.Generic;
using Sulakore.Habbo;
using Sulakore.Habbo.Layers;
using System.Linq;
using System.Text;
using System;

namespace HabboGallery.Habbo.Camera
{
    public static class PhotoConverter
    {
        public static HPhoto OldToNew(OldPhoto old)
        {
            HPhoto finalPhoto = new HPhoto();
            Plane firstPlane = new Plane
            {
                Z = -2.142686649821824,
                Color = 0
            };
            firstPlane.CornerPoints.AddRange(new List<Point> { new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0) });

            Sprite outOfBoundsFurniSprite = new Sprite
            {
                X = -3000,
                Y = -3000,
                Z = -7.470998649821824,
                Name = Constants.SANTORINI_FURNI_NAME,
                Color = 16777215
            };
            Sprite adsBackgroundSprite = new Sprite
            {
                X = -3000,
                Y = -3000,
                Z = -10.963789112594961,
                Name = Constants.ADS_BACKGROUND_FURNI_NAME,
                Color = 16777215
            };
            Sprite strFillSprite = new Sprite
            {
                X = 79,
                Y = 109,
                Z = -10.96449621933915,
                Width = 160,
                Height = 110,
                Name = Encoding.UTF8.GetString(Convert.FromBase64String(UnfoldStrFill(old.StrFill))),
                Color = 16777215
            };

            finalPhoto.Planes.Add(firstPlane);
            finalPhoto.Sprites.Add(outOfBoundsFurniSprite);
            finalPhoto.Sprites.Add(adsBackgroundSprite);
            finalPhoto.Sprites.Add(strFillSprite);

            return finalPhoto;
        }

        private static string UnfoldStrFill(string filler)
        {
            return !string.IsNullOrEmpty(filler) ? new string(filler.ToCharArray().Select(_ => { return (char)((_ >= 0x61 && _ <= 0x7A) ? ((_ + 0xD > 0x7A) ? _ - 0xD : _ + 0xD) : (_ >= 0x41 && _ <= 0x5A ? (_ + 0xD > 0x5A ? _ - 0xD : _ + 0xD) : _)); }).ToArray()) : filler;
        }
    }
}