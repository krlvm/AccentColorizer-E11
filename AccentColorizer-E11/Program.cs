using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class Program
    {
        private static readonly string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\MicrosoftWindows.Client.Core_cw5n1h2txyewy\FileExplorerExtensions\Assets\images\contrast-standard\theme-";

        static void Main(string[] args)
        {
            ColorizeGlyphs("light", AccentColors.GetColorByTypeName("ImmersiveSystemAccent"), "#0078D4");
            ColorizeGlyphs("dark", AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"), "#4CC2FF");
        }

        private static void ColorizeGlyphs(string theme, Color replacementColor, string defaultColor)
        {
            var color = Utility.ColorToHex(replacementColor);

            var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\AccentColorizer");
            var currentColor = (string)key.GetValue(theme, defaultColor);
            key.SetValue(theme, color);
            key.Close();

            var dir = new DirectoryInfo(BASE_PATH + theme);

            foreach (var file in dir.GetFiles("*.svg"))
            {
                var path = file.FullName;
                Console.WriteLine(path);

                //Utility.TakeOwnership(path);

                var raw = Utility.ReadFile(path);
                raw = raw.Replace(currentColor, color).Replace(defaultColor, color);
                Utility.WriteFile(path, raw);

                //Utility.NormalizeOwnership(path);
            }
        }
    }
}
