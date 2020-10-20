using Android.Views;
using Com.Microsoft.Appcenter.Ingestion.Models;
using Scouts.Droid.Services;
using Scouts.Interfaces;
using Xamarin.Forms;
using Application = Android.App.Application;
using Device = Xamarin.Forms.Device;

[assembly: Dependency(typeof(PlatformAdjust))]
namespace Scouts.Droid.Services
{
    public class PlatformAdjust : IPlatformAdjust
    {
        public void ChangeWindowInputMode(int inputMode = 32)
        {
            MainActivity.Instance.Window?.SetSoftInputMode((SoftInput)inputMode);
        }
    }
}