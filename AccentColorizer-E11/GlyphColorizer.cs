using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class GlyphColorizer
    {
        private const string DEFAULT_COLOR_LIGHT = "#0078D4";
        private const string DEFAULT_COLOR_DARK  = "#4CC2FF";

        private readonly string[] paths;
        private readonly RegistryKey key;

        public GlyphColorizer(string[] paths, RegistryKey key)
        {
            this.paths = paths;
            this.key = key;
        }

        public void ApplyColorization()
        {
            ColorizeGlyphs("light", AccentColors.GetColorByTypeName("ImmersiveSystemAccent"), DEFAULT_COLOR_LIGHT);
            ColorizeGlyphs("dark", AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"), DEFAULT_COLOR_DARK);
        }

        public void ColorizeGlyphs(string theme, Color replacementColor, string defaultColor)
        {
            var color = Utility.ColorToHex(replacementColor);

            var currentColor = (string)key.GetValue(theme, defaultColor);
            key.SetValue(theme, color);

            foreach (var basePath in paths)
            {
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
}
