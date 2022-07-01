using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class AccentColors
    {
        [DllImport("uxtheme.dll", EntryPoint = "#95", CharSet = CharSet.Unicode)]
        internal static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
        [DllImport("uxtheme.dll", EntryPoint = "#96", CharSet = CharSet.Unicode)]
        internal static extern uint GetImmersiveColorTypeFromName(string name);
        [DllImport("uxtheme.dll", EntryPoint = "#98", CharSet = CharSet.Unicode)]
        internal static extern uint GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        public static Color GetColorByTypeName(string name)
        {
            var colorSet = GetImmersiveUserColorSetPreference(false, false);
            var colorType = GetImmersiveColorTypeFromName(name);
            var rawColor = GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            var bytes = BitConverter.GetBytes(rawColor);
            return Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
        }
    }
}
