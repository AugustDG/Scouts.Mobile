using System;
using System.Net;
using System.Threading.Tasks;
using Ical.Net;

namespace Scouts.Fetchers
{
    class CalendarFetcher
    {
        public async Task<Calendar> GetCalendar()
        {
            var uri = new Uri("CALENDAR_URL", UriKind.Absolute);

            using var client = new WebClient();
            var s = await client.DownloadStringTaskAsync(uri);

            return Calendar.Load(s);
        }
    }
}
