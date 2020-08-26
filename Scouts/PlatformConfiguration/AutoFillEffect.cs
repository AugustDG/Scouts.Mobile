using Xamarin.Forms;

namespace Scouts.PlatformConfiguration
{
    public class AutofillEffect : RoutingEffect
    {
        public AutofillHintType HintType { get; set; }
        
        public AutofillEffect() : base("ScoutsEffects.AutofillEffect")
        {
        }
    }
}