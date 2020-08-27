using System;
using System.Collections.Generic;

namespace Scouts.Models
{
    public class DeviceInstallationModel
    {
        public string InstallationId { get; set; }
        public string Platform { get; set; }
        public string PushChannel { get; set; }
        public List<string> Tags { get; set; }
        
        public long ExpirationTime { get; set; }
    }
}