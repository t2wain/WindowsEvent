using System;
using System.Collections.Generic;

namespace WinEventLib.Entities
{
    /// <summary>
    /// Essential data from Windows event log
    /// </summary>
    public record WinLogEvent
    {
        public long? Index { get; set; }
        public string Provider { get; set; } = null!;
        public int LogLevel { get; set; }
        public DateTimeOffset TimeCreated { get; set; }
        public string Server { get; set; } = null!;
        public string Message { get; set; } = null!;
        public IEnumerable<string> EventData { get; set; } = new List<string>();
    }
}
