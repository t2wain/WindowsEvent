using System;
using WinEventLib.Entities;
using System.Linq;
using EFX = WinEventLib.EventFilterExtensions;
using System.IO;
using System.Collections.Generic;

namespace WinEventLib
{
    public class LogReaderExample
    {
        public List<WinLogEvent> ReadLog()
        {
            // testing with Outlook event
            var source = OutlookLogParam;

            // specify specific filters
            var preds = new string[]
            {
                source.FilterByProvider(),
                source.FilterByLevel(),
                source.FilterByRecentDate(),
            };

            source.QueryFilter = EFX.FilterByPredicates(preds);
            var lstEvt1 = source.EnumerateReader().ToList();
            return lstEvt1;

        }

        public void ListLogs()
        {
            var source = OutlookLogParam;

            // use filters based on params
            var lstEvt2 = source.EnumerateLog().ToList();

        }

        public List<WinLogEvent> ExportToLogFile()
        {
            // testing with Outlook event
            var source = OutlookLogParam;
            var fileName = Path.Combine("c:\\temp", "winlogtest.evtx");
            source.ExportWindowsEventLog(fileName);

            source.LogFile = fileName;
            var logs = source.EnumerateReader().ToList();

            if (File.Exists(fileName))
                File.Delete(fileName);

            return logs;
        }

        public void ReadLogFile()
        {
            // testing with exported Outlook event file
            var source = OutlookLogFileParam;

            // use filters based on params
            var logs = source.EnumerateReader().ToList();
        }

        public WinLogParam OutlookLogParam =>
            new WinLogParam
            {
                FromRecentTime = TimeSpan.FromDays(30),
                Provider = "Outlook"
            };

        public WinLogParam OutlookLogFileParam =>
            new WinLogParam
            {
                LogFile = Path.Combine("c:\\temp", "winlogtest.evtx")
            };
    }
}
