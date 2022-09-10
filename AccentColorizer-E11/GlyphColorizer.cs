using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class GlyphColorizer
    {
        private readonly string basePath;
        private readonly RegistryKey key;

        public GlyphColorizer(string basePath, RegistryKey key)
        {
            this.basePath = basePath;
            this.key = key;
        }

        public void ApplyColorization()
        {
            ColorizeGlyphs("light", AccentColors.GetColorByTypeName("ImmersiveSystemAccent"), "#0078D4");
            ColorizeGlyphs("dark", AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"), "#4CC2FF");
        }

        public void ColorizeGlyphs(string theme, Color replacementColor, string defaultColor)
        {
            var color = Utility.ColorToHex(replacementColor);

            var currentColor = (string)key.GetValue(theme, defaultColor);
            key.SetValue(theme, color);

            var dir = new DirectoryInfo(basePath + theme);

            foreach (var file in dir.GetFiles("*.svg"))
            {
                var path = file.FullName;

                var raw = Utility.ReadFile(path);
                raw = raw.Replace(currentColor, color).Replace(defaultColor, color);
                if ("light".Equals(theme))
                {
                    // Windows Spotlight has a different color used (WHY?)
                    raw = raw.Replace("#0C59A4", color);
                }

                try
                {
                    Utility.WriteFile(path, raw);
                }
                catch (UnauthorizedAccessException)
                {
                    Utility.RunElevated(Program.ARGUMENT_TAKEOWN);
                    break;
                }
            }
        }
    }
}
