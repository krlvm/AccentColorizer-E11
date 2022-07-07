using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class Program
    {
        private const string ARGUMENT_TAKEOWN = "TakeOwnership";

        private static string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\";
        private const string PACKAGE_21H2 = @"MicrosoftWindows.Client.CBS_cw5n1h2txyewy\";
        private const string PACKAGE_22H2 = @"MicrosoftWindows.Client.Core_cw5n1h2txyewy\";

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

            if (Directory.Exists(BASE_PATH + PACKAGE_22H2))
            {
                BASE_PATH += PACKAGE_22H2;
            }
            else
            {
                BASE_PATH += PACKAGE_21H2;
            }
            BASE_PATH += @"FileExplorerExtensions\Assets\images\contrast-standard\theme-";

            Console.WriteLine(BASE_PATH);

            if (args.Length == 1 && ARGUMENT_TAKEOWN.Equals(args[0]))
            {
                Utility.TakeRegistryOwnership(key);

                Utility.TakeOwnership(BASE_PATH + "light");
                Utility.TakeOwnership(BASE_PATH + "dark");
            }

            ApplyColorization(key);

            key.Close();
        }

        private static void ApplyColorization(RegistryKey key)
        {
            ColorizeGlyphs("light", AccentColors.GetColorByTypeName("ImmersiveSystemAccent"), "#0078D4", key);
            ColorizeGlyphs("dark", AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"), "#4CC2FF", key);
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
