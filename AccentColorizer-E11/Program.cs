using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace AccentColorizer_E11
{
    class Program
    {
        public const string ARGUMENT_APPLY = "-" + "Apply";
        public const string ARGUMENT_REVERT = "-" + "Revert";
        public const string ARGUMENT_TAKEOWN = "-" + "TakeOwnership";

        public const string LISTENER_MUTEX = "-" + "ACCENTCLRE11";

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

            var paths = FindPaths();

            if (args.Length > 0 && ARGUMENT_TAKEOWN.Equals(args[0]))
            {
                Utility.TakeRegistryOwnership(key);

                foreach (var path in paths)
                {
                    Utility.TakeOwnership(path);
                }
            }

            var colorizer = new GlyphColorizer(paths.Select(path => path + @"\theme-").ToArray(), key);

            if ((args.Length == 1 && ARGUMENT_APPLY.Equals(args[0])) || (args.Length == 2 && ARGUMENT_APPLY.Equals(args[1])))
            {
                colorizer.ApplyColorization();
                key.Close();
            }
            else if ((args.Length == 1 && ARGUMENT_REVERT.Equals(args[0])) || (args.Length == 2 && ARGUMENT_REVERT.Equals(args[1])))
            {
                colorizer.RevertColorization();
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

        private static List<string> FindPaths()
        {
            var paths = new List<string>();

            string sysAppsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\";
            string[] knownPackages =
            {
                @"MicrosoftWindows.Client.CBS_cw5n1h2txyewy\",    // 21H2
                @"MicrosoftWindows.Client.Core_cw5n1h2txyewy\",   // 22H2
                @"MicrosoftWindows.Client.FileExp_cw5n1h2txyewy\", // WASDK
                @"SxS\MicrosoftWindows.54792954.Filons_cw5n1h2txyewy\" // WASDK (newer 24H2 builds)
            };

            foreach (var pkg in knownPackages)
            {
                var path = sysAppsPath + pkg + @"FileExplorerExtensions\Assets\images\contrast-standard";
                if (!Directory.Exists(path))
                {
                    continue;
                }
                paths.Add(path);
            }

            return paths;
        }
    }
}
