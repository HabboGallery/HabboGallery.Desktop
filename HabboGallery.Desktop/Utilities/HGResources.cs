using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

namespace HabboGallery.Desktop.Utilities
{
    //TODO: not happy yet, might just load those resources to static vars
    public class HGResources
    {
        private const string RESOURCES_PREFIX = "HabboGallery.Desktop.Resources."; //ew

        private static readonly Dictionary<string, Stream> _cachedResources = new Dictionary<string, Stream>();

        static HGResources()
        {
            foreach (string resourceFullName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                string resourceName = resourceFullName[RESOURCES_PREFIX.Length..];
                _cachedResources.Add(resourceName, GetResourceStream(resourceFullName));
            }
        }

        public byte[] GetResourceBytes(string embeddedResourceName)
        {
            using var ms = _cachedResources[embeddedResourceName];
            byte[] data = new byte[ms.Length];
            ms.Read(data);
            return data;
        }

        public Image GetImageResource(string embeddedImageName)
            => Image.FromStream(_cachedResources[embeddedImageName]);

        public void RenderButtonState(Control buttonControl, bool enabled)
        {
            buttonControl.Invoke((MethodInvoker)delegate {
                string buttonResourceName = buttonControl.Name;

                if (!enabled)
                    buttonResourceName += "_Disabled";

                buttonControl.BackgroundImage = GetImageResource(buttonResourceName + ".png");
            });
        }

        public static Stream GetResourceStream(string embeddedResourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(embeddedResourceName, StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrEmpty(fullResourceName))
                throw new ArgumentException("Could not find specified resource name from the manifest.", nameof(embeddedResourceName));

            return Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName)
                ?? throw new InvalidOperationException("Could not load manifest resource stream.");
        }
    }
}
