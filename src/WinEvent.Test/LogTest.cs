using WinEventLib;

namespace WinEvent.Test
{
    public class LogTest
    {
        [Fact]
        public void Should_read_local_windows_event()
        {
            var ex = new LogReaderExample();
            var logs = ex.ReadLog();
            Assert.True(logs.Count > 0);
        }


        [Fact]
        public void Should_export_read_windows_log_file()
        {
            var ex = new LogReaderExample();
            var logs = ex.ExportToLogFile();

            Assert.True(logs.Count > 0);
        }

    }
}