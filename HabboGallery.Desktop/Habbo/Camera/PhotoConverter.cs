﻿using System.Drawing;
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
        public static HPhoto OldToNew(OldPhoto old, int zoom)
        {
            HPhoto photo = new HPhoto();
            Plane mainPlane = new Plane
            {
                Z = 3546.8730429908073,
                Color = 0,
                IsBottomAligned = false
            };
            for (int i = 0; i < 4; i++)
                mainPlane.CornerPoints.Add(new Point(0, 0));

            Sprite mainSprite = new Sprite
            {
                Name = Encoding.UTF8.GetString(Convert.FromBase64String(UnfoldStrFill(old.StrFill))),
                X = 79,
                Y = 102,
                Z = 3545.0969389908073,
                Color = 16777215,
                Width = 162,
                Height = 117
            };

            photo.Planes.Add(mainPlane);
            photo.Sprites.Add(mainSprite);

            photo.Zoom = zoom;

            return photo;
        }

        private static string UnfoldStrFill(string filler)
        {
            return !string.IsNullOrEmpty(filler) ? new string(filler.ToCharArray().Select(_ => { return (char)((_ >= 0x61 && _ <= 0x7A) ? ((_ + 0xD > 0x7A) ? _ - 0xD : _ + 0xD) : (_ >= 0x41 && _ <= 0x5A ? (_ + 0xD > 0x5A ? _ - 0xD : _ + 0xD) : _)); }).ToArray()) : filler;
        }
    }
}