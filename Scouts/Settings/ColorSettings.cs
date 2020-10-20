using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace Scouts.Settings
{
    public static class ColorSettings
    {
        /// <summary>
        /// InfoModel colors according to target public
        /// </summary>
        public static Color[] InfoModelColors = new[]
        {
            Color.FromRgb(50, 222, 138),
            Color.FromRgb(0, 105, 146),
            Color.FromRgb(70, 34, 85),
            Color.FromRgb(240, 58, 71),
            Color.FromRgb(255, 74, 28),
            Color.FromRgb(229, 99, 153),
            Color.FromRgb(244, 224, 77),
            Color.FromRgb(37, 37, 37),
        };
        
        /// <summary>
        /// Alert Dialogs color according to chosen theme
        /// </summary>
        public static MaterialAlertDialogConfiguration DefaultMaterialAlertDialogConfiguration { get; } = new MaterialAlertDialogConfiguration
        {
            BackgroundColor = (Color) Application.Current.Resources["MainBackgroundColor"],
            TitleTextColor = (Color) Application.Current.Resources["SyncPrimaryForegroundColor"],
            MessageTextColor =
                ((Color) Application.Current.Resources["SyncPrimaryForegroundColor"]).MultiplyAlpha(0.8),
            TintColor = (Color) Application.Current.Resources["SyncPrimaryColor"],
            CornerRadius = 5,
            ScrimColor = Color.FromHex("#232F34").MultiplyAlpha(0.32),
            ButtonAllCaps = true
        };
    }
}