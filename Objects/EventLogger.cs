using System;
using System.IO;
using System.Linq;

namespace SPTC_APPLICATION.Objects
{
    public class EventLogger
    {
        private static readonly string LogFilePath = System.IO.Path.Combine(
            System.Environment.GetEnvironmentVariable("LOG_DIR") ?? "Logs", "log.txt");
        private static readonly int MaxLines = 10000;

        public static void Post(string message)
        {
            string logEntry = $"{DateTime.Now:ddd MMM-dd HH:mm} :: {message}{Environment.NewLine}";

            try
            {
                EnsureLogFileExists();

                string currentLogContents = File.ReadAllText(LogFilePath);
                string updatedLogContents = logEntry + currentLogContents;

                if (updatedLogContents.CountLines() > MaxLines)
                {
                    string[] lines = updatedLogContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    updatedLogContents = string.Join(Environment.NewLine, lines.Take(MaxLines));
                }

                File.WriteAllText(LogFilePath, updatedLogContents);
            }
            catch (Exception ex)
            {
                // Log to console as fallback when file logging fails (e.g., in container environments)
                Console.Error.WriteLine($"EventLogger :: Failed to write log: {ex.Message}");
                Console.Error.WriteLine(logEntry);
            }
        }

        private static void EnsureLogFileExists()
        {
            if (!File.Exists(LogFilePath))
            {
                try
                {
                    string? dir = Path.GetDirectoryName(LogFilePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                    File.Create(LogFilePath).Close();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"EventLogger :: Failed to create log file: {ex.Message}");
                }
            }
        }
    }

    public static class StringExtensions
    {
        public static int CountLines(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 1;
            int position = 0;
            while ((position = text.IndexOf(Environment.NewLine, position)) != -1)
            {
                count++;
                position += Environment.NewLine.Length;
            }

            return count;
        }
    }
}
