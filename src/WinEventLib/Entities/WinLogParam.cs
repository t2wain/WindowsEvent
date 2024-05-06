using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WinEventLib.Entities
{
    #pragma warning disable CA1416

    /// <summary>
    /// Various parameters for filtering and reading Windows event log
    /// </summary>
    public class WinLogParam
    {
        public string LogName { get; set; } = "Application";

        /// <summary>
        /// Retrieve data from the 
        /// exported Windows event log file
        /// </summary>
        public string? LogFile { get; set; }

        public EventLogEntryType LogType { get; set; } = EventLogEntryType.Information;
        public string LogTypeDisplayName => LogType.ToString();
        public string ComputerName { get; set; } = "localhost";
        public List<string> ComputerNames { get;set; } = new List<string>();

        /// <summary>
        /// Event source or application
        /// that publish the event.
        /// </summary>
        public string? Provider { get; set; } = null!;

        /// <summary>
        /// To retrieve only events within
        /// the last number of days.
        /// </summary>
        public TimeSpan? FromRecentTime { get; set; }

        /// <summary>
        /// Date range of the event
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Date range of the event
        /// </summary>
        public DateTime? ToDate { get; set; }

        public long? FromEventIndex { get; set; }

        public string? Domain { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }

        /// <summary>
        /// XPath query for filtering Windows event log.
        /// The query limit which events to retrieve.
        /// </summary>
        public string? QueryFilter { get; set; }

        /// <summary>
        /// Further event log filtering based on message data.
        /// This limit which events to return.
        /// </summary>
        public Regex? MessageFilter { get; set;}
        public int? MaxCount { get; set; }
    }

    #pragma warning restore CA1416

}
