using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client
{
    public static class Log
    {
        private const string appLocalDataFolderName = "private_chat";
        private static readonly string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appLocalDataFolderName);
        private static string logFileName;

        public static void SetLogFileName(string fileName) {
            logFileName = fileName;
        }

        public static void LogText(string text) {
            string fullPath = Path.Combine(logFilePath, logFileName);
            File.AppendAllText(fullPath, text + Environment.NewLine);
        }

    }
}
