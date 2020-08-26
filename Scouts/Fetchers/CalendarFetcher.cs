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
            Uri uri = new Uri("https://calendar.google.com/calendar/ical/augustomp55%40gmail.com/private-ad7b213927d47e9a58e6f19087cff40b/basic.ics", UriKind.Absolute);

            using (WebClient client = new WebClient())
            {
                string s = await client.DownloadStringTaskAsync(uri);

                return Calendar.Load(s);
            }
        }
    }
}
