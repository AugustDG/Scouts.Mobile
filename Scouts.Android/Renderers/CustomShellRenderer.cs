using Android.Content;
using Android.Content.Res;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(Scouts.AppShell), typeof(Scouts.Droid.Renderers.CustomShellRenderer))]
namespace Scouts.Droid.Renderers
{

    public class CustomShellRenderer: ShellRenderer
    {
        public CustomShellRenderer(Context context) : base(context)
        {
        }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new CustomShellBottomNavViewAppearanceTracker(this);
        }
    }

    internal class CustomShellBottomNavViewAppearanceTracker : IShellBottomNavViewAppearanceTracker
    {
        private CustomShellRenderer m_shellRenderer = null;

        public CustomShellBottomNavViewAppearanceTracker(CustomShellRenderer p_shellRenderer)
        {
            m_shellRenderer = p_shellRenderer;
        }

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public void SetAppearance(Google.Android.Material.BottomNavigation.BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            bottomView.ItemIconTintList = null;
            bottomView.SetBackgroundColor(new Color(37, 37, 37));
            bottomView.ItemTextColor = new ColorStateList(new[]
                {
                    new[]{Android.Resource.Attribute.StatePressed, Android.Resource.Attribute.StateFocused}, //1
                    new[] {Android.Resource.Attribute.StateChecked}, //2
                    new[]{-Android.Resource.Attribute.StateChecked} //3
                },
                new int[] {
                    new Color(56, 158, 50), //1
                    new Color(56, 158, 50), //2
                    Color.White //3
                });
        }

        public void ResetAppearance(Google.Android.Material.BottomNavigation.BottomNavigationView bottomView)
        {
            /*bottomView.ItemIconTintList = null;
            bottomView.SetBackgroundColor(new Color(37, 37, 37));
            bottomView.ItemTextColor = new ColorStateList(new[]
                {
                    new[]{Android.Resource.Attribute.StatePressed, Android.Resource.Attribute.StateFocused, Android.Resource.Attribute.StateChecked}, //1
                    new[]{-Android.Resource.Attribute.StateChecked} //2
                },
                new int[] {
                    new Color(56, 158, 50), //1
                    Color.White //2
                });*/
        }
    }
}