namespace Scouts.Dev
{
    public partial class ConnectionStringUtility
    {
        public string Endpoint { get; private set; }
        public string SasKeyName { get; private set; }
        public string SasKeyValue { get; private set; }

        public ConnectionStringUtility(string connectionString)
        {
            //Parse Connectionstring
            char[] separator = { ';' };
            var parts = connectionString.Split(separator);
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("Endpoint"))
                    Endpoint = "https" + parts[i].Substring(11);
                if (parts[i].StartsWith("SharedAccessKeyName"))
                    SasKeyName = parts[i].Substring(20);
                if (parts[i].StartsWith("SharedAccessKey"))
                    SasKeyValue = parts[i].Substring(16);
            }
        }
    }
}