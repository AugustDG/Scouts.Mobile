using System;
using Scouts.Models;

namespace Scouts.Events
{
    public static class AppEvents
    {
        //App Events
        public static EventHandler WipeAllUserData;
        public static EventHandler PageIndexChanged;
        public static EventHandler<int> SwitchHomePage;
        public static EventHandler UserTypeChanged;

        //Info Page Events
        public static EventHandler ClearFilter;
        public static EventHandler<FilterEventArgs> FilterInfos;
        public static EventHandler<InfoModel> OpenInfoDetails;
        public static EventHandler RefreshInfoFeed;
    }
}