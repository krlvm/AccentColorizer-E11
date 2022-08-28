using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;

namespace AccentColorizer_E11
{
    class Program
    {
        public const string ARGUMENT_APPLY = "-" + "Apply";
        public const string ARGUMENT_TAKEOWN = "-" + "TakeOwnership";

        public const string LISTENER_MUTEX = "-" + "ACCENTCLRE11";

        private static string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\";
        private static string ASSETS_PATH = @"FileExplorerExtensions\Assets\images\contrast-standard\theme-";
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
                Utility.RunElevated(ARGUMENT_TAKEOWN + (args.Length == 1 ? " " + args[0] : ""));
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
            BASE_PATH += ASSETS_PATH;

            if (args.Length > 0 && ARGUMENT_TAKEOWN.Equals(args[0]))
            {
                Utility.TakeRegistryOwnership(key);

                Utility.TakeOwnership(BASE_PATH + "light");
                Utility.TakeOwnership(BASE_PATH + "dark");
            }

            var colorizer = new GlyphColorizer(BASE_PATH, key);

            if ((args.Length == 1 && ARGUMENT_APPLY.Equals(args[0])) || (args.Length == 2 && ARGUMENT_APPLY.Equals(args[1])))
            {
                colorizer.ApplyColorization();
                key.Close();
            }
            else
            {
                Mutex mutex;
                if (Mutex.TryOpenExisting(LISTENER_MUTEX, out mutex))
                {
                    key.Close();
                }
                else
                {
                    mutex = new Mutex(false, LISTENER_MUTEX);
                    var handler = new AccentColorListener(colorizer);
                    handler.ApplyColorization();
                    System.Windows.Forms.Application.Run(handler);
                }
            }
        }
    }
}
