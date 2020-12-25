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

        public static byte[] GetResourceBytes(string embeddedResourceName)
        {
            using var stream = _cachedResources[embeddedResourceName];
            byte[] data = new byte[stream.Length];
            stream.Read(data);
            return data;
        }

        public static Image GetImageResource(string embeddedImageName)
            => Image.FromStream(_cachedResources[embeddedImageName]);

        public void RenderButtonState(Control buttonControl, bool enabled)
        {
            if (buttonControl.InvokeRequired)
                buttonControl.Invoke((MethodInvoker)delegate { RenderButtonState(buttonControl, enabled); });
            else
            {
                string buttonResourceName = buttonControl.Name;

                if (!enabled)
                    buttonResourceName += "_Disabled";

                buttonControl.BackgroundImage = GetImageResource(buttonResourceName + ".png");
            }
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
