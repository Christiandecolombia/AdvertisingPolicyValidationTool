using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingPolicyValidationTool
{
    internal class Logger
    {
        internal static string LogFilePath;

        private static readonly object Locker = new object();

        private enum Category
        {
            Info,
            Warning,
            Error,
            LineBreak,
        }

        #region Log

        internal static void LogError(string message, params object[] args)
        {
            Log(Category.Error, message, args);
        }

        internal static void LogInfo(string message, params object[] args)
        {
            Log(Category.Info, message, args);
        }

        internal static void LogWarning(string message, params object[] args)
        {
            Log(Category.Warning, message, args);
        }

        internal static void LogLineBreak()
        {
            Log(Category.LineBreak, "--------------------------------------------------------");
        }

        internal static void StreamBatch(string streamFile, string queryLine)
        {
            lock (Locker)
            {
                File.AppendAllText(streamFile, $@"{queryLine}{Environment.NewLine}");
            }
        }

        #endregion

        private static void Log(Category category, string message, params object[] args)
        {
            var title = category.ToString();

            // If query then don't format the input
            var logMessage = string.Format(message, args);
            var messageToLog = logMessage;

            if (category != Category.LineBreak)
            {
                messageToLog = $"{DateTime.Now:dd-MMM-yyyy HH:mm:ss}|{title}|{logMessage}";
            }

            // logging to file
            lock (Locker)
            {
                File.AppendAllText(LogFilePath, $@"{messageToLog}{Environment.NewLine}");
            }
        }
    }
}
