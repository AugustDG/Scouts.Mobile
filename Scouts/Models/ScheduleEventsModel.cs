using System;

namespace Scouts.Models
{
    public class ScheduleEventsModel
    {
        public string EventName { get; set; }

        public string EventsDetails { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        public string EventId;
    }
}
