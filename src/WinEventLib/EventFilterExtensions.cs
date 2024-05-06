using System.Collections.Generic;
using WinEventLib.Entities;
using System.Linq;

namespace WinEventLib
{
    /// <summary>
    /// Utility method to create a 
    /// proper formatted XPath query
    /// </summary>
    public static class EventFilterExtensions
    {
        /*
            *[System
                [(Level=4 or Level=0)]
                [Provider/@Name='Outlook']
                [EventRecordID > {0}]
                [TimeCreated[timediff(@SystemTime) <= {0}]]
                [TimeCreated/@SystemTime >= '{1}']
            ]
         */

        public static string FilterByRecentDate(this WinLogParam source) =>
            source.FromRecentTime switch
            {
                null => "",
                _ => string.Format("[TimeCreated[timediff(@SystemTime) <= {0}]]",
                        source.FromRecentTime.Value.TotalMilliseconds)
            };

        public static string FilterByProvider(this WinLogParam source) =>
            source.Provider switch
            {
                null => "",
                _ => string.Format("[Provider/@Name='{0}']", source.Provider)
            };

        public static string FilterByDateRange(this WinLogParam source) =>
            (source.FromDate, source.ToDate) switch
            {
                (null, _) => "",
                (var fd, null) => string.Format("[TimeCreated/@SystemTime >= '{0}']",
                    fd.Value.ToUniversalTime().ToString("O")),
                _ => string.Format("[TimeCreated[(@SystemTime >= '{0}' and @SystemTime <= '{1}')]",
                        source.FromDate.Value.ToUniversalTime().ToString("O"),
                        source.ToDate.Value.ToUniversalTime().ToString("O")),
            };

        public static string FilterByLastEventIndex(this WinLogParam source) =>
            source.FromEventIndex switch {
                null => "",
                _ => string.Format("[EventRecordID > {0}]", source.FromEventIndex)
            };

        public static string FilterByLevel(this WinLogParam source) =>
            string.Format("[Level >= {0}]", (int)source.LogType);

        public static string FilterAll(this WinLogParam source) =>
            FilterByPredicates(new string[]
            {
                source.FilterByProvider(),
                source.FilterByLastEventIndex(),
                source.FilterByDateRange(),
                source.FilterByRecentDate(),
                source.FilterByLevel()
            });

        public static string FilterByPredicates(IEnumerable<string> predicates) =>
            predicates.Where(p => !string.IsNullOrWhiteSpace(p)).ToList() switch
            {
                var lp when lp.Count() == 0 => "*",
                var lp => string.Format("*[System {0}]", string.Join(" ", lp))
            };
    }
}
