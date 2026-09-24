using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ReelManager
{
    public class VideoInfo
    {
        public long Size { get; set; }
        public double DurationSeconds { get; set; }
        public string Camera { get; set; }
        public string Resolution { get; set; }
        public int Fps { get; set; }
        public DateTime CaptureDate { get; set; }
    }

    public static class VideoHelper
    {
        public static readonly string[] SupportedExtensions = new string[]
        {
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".m4v",
            ".mpg", ".mpeg", ".mts", ".m2ts", ".3gp", ".webm"
        };

        public static bool IsVideoFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            foreach (var e in SupportedExtensions)
                if (e == ext) return true;
            return false;
        }

        public static VideoInfo GetInfo(string filePath)
        {
            var info = new VideoInfo();
            try
            {
                var fi = new FileInfo(filePath);
                info.Size = fi.Length;
                info.CaptureDate = fi.LastWriteTime;

                try
                {
                    Type shellType = Type.GetTypeFromProgID("Shell.Application");
                    if (shellType != null)
                    {
                        dynamic shell = Activator.CreateInstance(shellType);
                        string folder = Path.GetDirectoryName(filePath);
                        string fileName = Path.GetFileName(filePath);
                        dynamic shellFolder = shell.NameSpace(folder);
                        if (shellFolder != null)
                        {
                            dynamic item = shellFolder.ParseName(fileName);
                            if (item != null)
                            {
                                // Duration (index 27 في معظم إصدارات ويندوز)
                                string durStr = shellFolder.GetDetailsOf(item, 27);
                                if (!string.IsNullOrWhiteSpace(durStr))
                                {
                                    double sec = ParseDuration(durStr);
                                    if (sec > 0) info.DurationSeconds = sec;
                                }

                                // Resolution (index 31)
                                string resStr = shellFolder.GetDetailsOf(item, 31);
                                if (!string.IsNullOrWhiteSpace(resStr))
                                    info.Resolution = resStr.Trim();

                                // Frame rate (index 315 في ويندوز 10/11)
                                string fpsStr = shellFolder.GetDetailsOf(item, 315);
                                if (!string.IsNullOrWhiteSpace(fpsStr))
                                {
                                    int fps = ParseFps(fpsStr);
                                    if (fps > 0) info.Fps = fps;
                                }

                                // Date taken (index 12)
                                string dateStr = shellFolder.GetDetailsOf(item, 12);
                                if (!string.IsNullOrWhiteSpace(dateStr))
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(dateStr, out dt))
                                        info.CaptureDate = dt;
                                }

                                // Camera / Model (index 279)
                                string camStr = shellFolder.GetDetailsOf(item, 279);
                                if (!string.IsNullOrWhiteSpace(camStr))
                                    info.Camera = camStr.Trim();
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }
            return info;
        }

        private static double ParseDuration(string s)
        {
            try
            {
                s = s.Trim();
                // الصيغة: "00:03:25" أو "01:20:15"
                string[] parts = s.Split(':');
                if (parts.Length == 3)
                {
                    int h = int.Parse(parts[0]);
                    int m = int.Parse(parts[1]);
                    int sec = int.Parse(parts[2]);
                    return h * 3600 + m * 60 + sec;
                }
                if (parts.Length == 2)
                {
                    int m = int.Parse(parts[0]);
                    int sec = int.Parse(parts[1]);
                    return m * 60 + sec;
                }
            }
            catch { }
            return 0;
        }

        private static int ParseFps(string s)
        {
            try
            {
                s = s.Trim();
                // "29.97" أو "30" أو "25 fps"
                s = s.Replace("fps", "").Trim();
                double d;
                if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out d))
                    return (int)Math.Round(d);
            }
            catch { }
            return 0;
        }

        public static string FormatDuration(double seconds)
        {
            if (seconds <= 0) return "—";
            int s = (int)seconds;
            int h = s / 3600;
            int m = (s % 3600) / 60;
            int sec = s % 60;
            if (h > 0) return h.ToString("D2") + ":" + m.ToString("D2") + ":" + sec.ToString("D2");
            return m.ToString("D2") + ":" + sec.ToString("D2");
        }

        public static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }

        public static string GetStatusColor(string status)
        {
            return status ?? "";
        }

        public static string[] DefaultTags = new string[]
        {
            "B-Roll", "مقابلة", "Drone", "Slow Motion", "صوت فقط",
            "مرفوض", "Timelapse", "Wide Shot", "Close Up", "Main"
        };

        public static string[] ProjectStatuses = new string[]
        {
            "قيد التصوير", "قيد المونتاج", "بانتظار العميل", "تم التسليم", "مدفوع", "ملغي"
        };
    }
}
