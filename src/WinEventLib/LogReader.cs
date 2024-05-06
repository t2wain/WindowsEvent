using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Security;
using WinEventLib.Entities;
using System.Linq;

namespace WinEventLib
{

    public static class LogReader
    {
        public static int MAX_COUNT = 10000; 

        /// <summary>
        /// Reading Windows event log using EventLogReader.
        /// This is a preferred method.
        /// </summary>
        public static IEnumerable<WinLogEvent> EnumerateReader(this WinLogParam source)
        {
            var src = source;

            // get session for server and user logon
            using var session = src.GetSession();

            // create query
            var xpath = src.QueryFilter ?? src.FilterAll();
            var max = src.MaxCount ?? MAX_COUNT;

            var query = src.LogFile switch
            {
                null => new EventLogQuery(source.LogName, PathType.LogName, xpath),
                _ => new EventLogQuery(src.LogFile, PathType.FilePath, xpath)
            };
            query.ReverseDirection = true; // Read logs in reverse order (most recent first)
            query.Session = session;
            query.TolerateQueryErrors = false;

            // create reader
            using var reader = new EventLogReader(query);
            reader.BatchSize = 1000;

            var cnt = 0;
            var rx = src.MessageFilter;

            // reading log
            for (var evt = reader.ReadEvent(); evt != null && cnt++ <= max; evt = reader.ReadEvent())
            {
                // extract event data and message
                var msg = evt.FormatDescription();
                var data = evt.Properties
                    .Where(p => p.Value != null)
                    .Select(p => p.Value.ToString()!)
                    .ToList();

                // filter data and message
                if (rx != null 
                    && !((data.Any(v => rx.IsMatch(v)) 
                            || (!string.IsNullOrWhiteSpace(msg) && rx.IsMatch(msg))
                        ))) 
                    continue;

                // construct data
                if (string.IsNullOrWhiteSpace(msg) && data.Count() > 0)
                    msg = string.Join(", ", data);

                // event time is stored as UTC time
                // but when read, it is converted to local time
                // without timezone info
                var dt = evt.TimeCreated!.Value;


                // convert back to UTC
                var dtz = new DateTimeOffset(dt.ToUniversalTime(), TimeSpan.Zero);

                var d = new WinLogEvent
                {
                    Index = evt.RecordId,
                    Provider = evt.ProviderName,
                    LogLevel = (int)evt.Level,
                    TimeCreated = dtz,
                    Server = evt.MachineName,
                    Message = msg,
                    EventData = data,
                };

                //InspectEventRecord(evt);

                yield return d;
            }
        }


        /// <summary>
        /// Reading Windows event log using EventLog
        /// </summary>
        public static IEnumerable<WinLogEvent> EnumerateLog(this WinLogParam source)
        {
            var src = source;

            var el = src.Provider switch
            {
                null => new EventLog(src.LogName, src.ComputerName),
                _ => new EventLog(src.LogName, src.ComputerName, src.Provider)
            };

            // read Windows Event Log
            var q = el.Entries.Cast<EventLogEntry>()
                .Where(e => e.EntryType == src.LogType
                    && src.IsInDateRange(e.TimeGenerated));

            /* it seems EventLog doos not filter by provider */
            if (!string.IsNullOrWhiteSpace(src.Provider)) 
                q = q.Where(e => e.Source == src.Provider); 

            var events = q.Select(e => new WinLogEvent
                {
                    Index = e.Index,
                    Provider = e.Source,
                    TimeCreated = e.TimeGenerated,
                    Server = e.MachineName,
                    Message = e.Message
                }).Take(MAX_COUNT);

            return events;

        }


        /// <summary>
        /// Export data from Windows Event log 
        /// to a *.evtx file 
        /// </summary>
        /// <param name="fileName">*.evtx</param>
        /// <param name="filter">XPath query</param>
        public static void ExportWindowsEventLog(this WinLogParam source, string fileName, string? filter = null)
        {
            using var session = source.GetSession();
            var xpath = filter ?? source.FilterAll();
            try
            {
                session.ExportLogAndMessages(
                    source.LogName, 
                    PathType.LogName, 
                    xpath, 
                    fileName, 
                    false, 
                    CultureInfo.CurrentCulture
                );
            }
            catch (Exception ex) 
            {
                /*
                 * The export method will attempt to create
                 * sub-folder "LocaleMetaData" but it might lacks
                 * security privilege. When run as "Admin",
                 * this error will not occurred.
                 */
                var msg = ex.Message;
                if (msg != "The directory name is invalid.")
                    throw;
            }
        }

        #region Utilities

        static EventLogSession GetSession(this WinLogParam source)
        {
            var spwd = !string.IsNullOrWhiteSpace(source.Password) ?
                source.Password!.ToCharArray().Aggregate(new SecureString(), (pwd, c) =>
                {
                    pwd.AppendChar(c);
                    return pwd;
                }) : null;

            return !string.IsNullOrWhiteSpace(source.Password) switch
            {
                true => new EventLogSession(source.ComputerName, source.Domain, source.UserName, spwd, SessionAuthentication.Negotiate),
                false when !string.IsNullOrWhiteSpace(source.ComputerName) => new EventLogSession(source.ComputerName),
                _ => new EventLogSession()
            };

        }

        static bool IsInDateRange(this WinLogParam src, DateTime timeCreated) =>
            timeCreated >= src.FromDate && (!src.ToDate.HasValue || timeCreated <= src.ToDate);

        static void InspectEventRecord(EventRecord evt)
        {
            var rid = evt.RecordId;
            var prov = evt.ProviderName;
            var time = evt.TimeCreated;
            var mach = evt.MachineName;
            var msg = evt.FormatDescription();

            var props = evt.Properties;
            foreach ( var prop in props )
            {
                var val = prop.Value;
            }
        }

        #endregion

    }

}
