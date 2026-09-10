using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Utils
{
    public class BvsLogger
    {
        readonly ConcurrentQueue<LogItem> logQueue;
        readonly WaitHandle[] operationHandles;

        Thread processWorkerThread;
        AutoResetEvent logEvent;
        AutoResetEvent exitEvent;
        private static readonly ILog _logCsv = LogManager.GetLogger("logcsv");

        public BvsLogger()
        {
            logQueue = new ConcurrentQueue<LogItem>();
            logEvent = new AutoResetEvent(false);
            exitEvent = new AutoResetEvent(false);
            operationHandles = new WaitHandle[] { logEvent, exitEvent };
        }

        public void Restart()
        {
            if ((null == processWorkerThread) || (!processWorkerThread.IsAlive)) processWorkerThread = new Thread(ProcessWhileHasInput) { IsBackground = true };

            if (!processWorkerThread.IsAlive) processWorkerThread.Start();
        }

        private void ProcessWhileHasInput()
        {
            while (true)
            {
                var evIndex = WaitHandle.WaitAny(operationHandles);

                if (operationHandles[evIndex] == logEvent)
                {
                    LogItem item;
                    while (logQueue.TryDequeue(out item))
                    {
                        switch (item.Level)
                        {
                            case LogLevel.INFO:
                                Info(item.LogName, item.Entry);
                                break;

                            case LogLevel.BUG:
                                Bug(item.LogName, item.Entry);
                                break;
                            case LogLevel.CSV:
                                Csv(item.LogName, item.Entry);
                                break;
                        }
                    }
                }
                else if (operationHandles[evIndex] == exitEvent) break;
            }
        }

        public void StopAllLogger()
        {
            exitEvent.Set();

            foreach (var hierarchy in LogManager.GetAllRepositories())
            {
                hierarchy.ResetConfiguration();
            }

            LogManager.GetRepository().Shutdown();
            LogManager.GetRepository().ResetConfiguration();
        }

        public void Start(string strName, bool dateName = false)
        {
            if (Settings.EnableLogging)
            {
                var hierarchy = (Hierarchy)LogManager.GetAllRepositories().FirstOrDefault(l => l.Name.Equals(strName));
                var pattern = new PatternLayout(Settings.LoggingFormat);
                var roller = new RollingFileAppender();
                var strDatePattern = dateName ? $"yyyy-MM-dd\\\\yyyy-MM-dd'.log'" : $"yyyy-MM-dd\\\\'{strName}.log'";
                if (null == hierarchy) hierarchy = (Hierarchy)LogManager.CreateRepository(strName);

                hierarchy.Clear();
                hierarchy.ResetConfiguration();

                pattern.ActivateOptions();
                roller.AppendToFile = true;
                roller.File = $"{Settings.LoggingDirectory}\\";
                //roller.File = string.Format(@"{0}\{1}.log", Settings.LoggingDirectory, strName);
                roller.Layout = pattern;
                roller.MaxSizeRollBackups = 5;
                roller.MaximumFileSize = "10MB";
                roller.RollingStyle = RollingFileAppender.RollingMode.Composite;
                roller.StaticLogFileName = false;
                roller.DatePattern = strDatePattern;
                roller.ActivateOptions();

                hierarchy.Root.AddAppender(roller);
                hierarchy.Root.Level = Level.All;
                hierarchy.Configured = true;

                Restart();
            }

            Log(strName, "-------------------[ Begin new Logging section ]-------------------");
        }

        public void StartCsv(string strName, bool dateName = false)
        {
            if (Settings.EnableLogging)
            {
                var hierarchy = (Hierarchy)LogManager.GetAllRepositories().FirstOrDefault(l => l.Name.Equals(strName));
                var pattern = new PatternLayout(Settings.CsvFormat);
                var roller = new RollingFileAppender();
                var strDatePattern = dateName ? $"yyyy-MM-dd\\\\yyyy-MM-dd'.csv'" : $"yyyy-MM-dd\\\\'{strName}.csv'";
                if (null == hierarchy) hierarchy = (Hierarchy)LogManager.CreateRepository(strName);

                hierarchy.Clear();
                hierarchy.ResetConfiguration();

                pattern.ActivateOptions();
                roller.AppendToFile = true;
                roller.File = $"{Settings.LoggingDirectory}\\";
                roller.Layout = pattern;
                roller.MaxSizeRollBackups = 5;
                roller.MaximumFileSize = "10MB";
                roller.RollingStyle = RollingFileAppender.RollingMode.Composite;
                roller.StaticLogFileName = false;
                roller.DatePattern = strDatePattern;
                roller.ActivateOptions();

                hierarchy.Root.AddAppender(roller);
                hierarchy.Root.Level = Level.All;
                hierarchy.Configured = true;

                Restart();
            }
        }

        private void Log(string strName, string strLog)
        {
            if (Settings.EnableLogging)
            {
                GetLogger(strName)?.Info(strLog);
            }
        }

        private void Info(string strName, string strFormat, params object[] objs)
        {
            if (Settings.EnableLogging)
            {
                Log(strName, string.Format(strFormat, objs));
            }
        }

        private void Bug(string strName, string strLog)
        {
            if (Settings.EnableLogging)
            {
                GetLogger(strName)?.Debug(strLog);
            }
        }

        private void Csv(string strName, string strLog)
        {
            if (Settings.EnableLogging)
            {
                GetLogger(strName)?.Info(strLog);
            }
        }

        private ILog GetLogger(string strLogName)
        {
            var repo = LogManager.GetAllRepositories().FirstOrDefault(l => l.Name.Equals(strLogName));
            if (null != repo) return LogManager.GetLogger(strLogName, strLogName);
            return null;
        }

        public void AddLog(LogItem log)
        {
            logQueue.Enqueue(log);
            logEvent.Set();
        }
    }

    public enum LogLevel
    {
        INFO,
        BUG,
        CSV
    }

    public class LogItem
    {
        public string LogName { get; set; } = string.Empty;
        public string Entry { get; set; } = string.Empty;
        public LogLevel Level { get; set; } = LogLevel.INFO;
    }
}