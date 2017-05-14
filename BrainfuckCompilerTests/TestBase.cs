using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests
{
    public abstract class TestBase
    {
        public const int DefaultTestTimeout = 1000;
        public const bool TimeoutEnabled = false;

        public TestContext TestContext { get; set; }

        public CsvConfiguration CsvConfig { get; } = new CsvConfiguration();

        public string LogName => $@"{this.GetType().Name}-{this.TestContext.TestName}";

        public string LogPath => $@"log\{this.LogName}.log";

        [TestInitialize]
        public void BeginTest()
        {
            Trace.AutoFlush = true;
            Directory.CreateDirectory(Path.GetDirectoryName(this.LogPath));
            File.Delete(this.LogPath);
            Trace.Listeners.Add(new TextWriterTraceListener(this.LogPath, this.LogName));
        }

        [TestCleanup]
        public void EndTest()
        {
            Trace.Listeners.Remove(this.LogName);
        }

        public static void TestWithTimeout(Action timedAction, int millisecondsTimeout = DefaultTestTimeout)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (TimeoutEnabled)
            {
                try
                {
                    var task = Task.Run(timedAction);
                    if (!task.Wait(millisecondsTimeout))
                    {
                        throw new TimeoutException();
                    }
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
            else
            {
                timedAction();
            }
        }
    }
}
