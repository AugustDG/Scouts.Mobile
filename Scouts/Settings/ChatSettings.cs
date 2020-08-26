using Scouts.Models;

namespace Scouts.Settings
{
    public static class ChatSettings
    {
        public static bool UseHttps;
        public static string ServerIp;
        
        public static string CurrentGroup;

        public static ChatUserModel ConnectedUser;
        public static ChatUserModel LocalUser;
    }
}