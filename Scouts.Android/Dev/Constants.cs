namespace Scouts.Droid.Dev
{
    public static class Constants
    {
        public static string[] NotificationChannelName { get; set; } = new []{"Infos","Messages"};
        public static string NotificationHubName { get; set; } = "scouts-news";
        public static string ListenConnectionString { get; set; } = "Endpoint=sb://scouts.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=2iJWfQT50JXlSj9n4Znbg2WswFLMGQ0/o99ySn56dBU=";
        public static string DebugTag { get; set; } = "ScoutsDebug";
        public static string[] SubscriptionTags { get; set; } = { "default" };
        public static string FcmTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";
        public static string ApnTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
    }
}