using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class Program
    {
        private const string ARGUMENT_TAKEOWN = "TakeOwnership";
        private static readonly string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\MicrosoftWindows.Client.Core_cw5n1h2txyewy\FileExplorerExtensions\Assets\images\contrast-standard\theme-";

        static void Main(string[] args)
        {
            RegistryKey key;

            try
            {
                key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\AccentColorizer", true);
            }
            catch (UnauthorizedAccessException)
            {
                Utility.RunElevated(ARGUMENT_TAKEOWN);
                return;
            }

            if (args.Length == 1 && ARGUMENT_TAKEOWN.Equals(args[0]))
            {
                Utility.TakeRegistryOwnership(key);

                Utility.TakeOwnership(BASE_PATH + "light");
                Utility.TakeOwnership(BASE_PATH + "dark");
            }

            ColorizeGlyphs("light", AccentColors.GetColorByTypeName("ImmersiveSystemAccent"), "#0078D4", key);
            ColorizeGlyphs("dark", AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"), "#4CC2FF", key);

            key.Close();
        }

        private static void ColorizeGlyphs(string theme, Color replacementColor, string defaultColor, RegistryKey key)
        {
            var color = Utility.ColorToHex(replacementColor);

            var currentColor = (string)key.GetValue(theme, defaultColor);
            key.SetValue(theme, color);

            var dir = new DirectoryInfo(BASE_PATH + theme);

            foreach (var file in dir.GetFiles("*.svg"))
            {
                var path = file.FullName;

                var raw = Utility.ReadFile(path);
                raw = raw.Replace(currentColor, color).Replace(defaultColor, color);

                try
                {
                    Utility.WriteFile(path, raw);
                }
                catch (UnauthorizedAccessException)
                {
                    Utility.RunElevated(ARGUMENT_TAKEOWN);
                    break;
                }
            }
        }
    }
}
