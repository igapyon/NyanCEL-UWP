// Copyright (c) 2024 Toshiki Iga
//
// Released under the MIT license
// https://opensource.org/license/mit

using System;
using System.IO;
using Windows.Storage;
using Serilog;

namespace Nyan
{
    public class NyanLog
    {
        public const bool IS_DEBUG = true;
        private static string logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs/log.txt");
        private static Serilog.ILogger logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day).CreateLogger();

        public static void Debug(String msg)
        {
            if (!IS_DEBUG)
#pragma warning disable CS0162 // 到達できないコードが検出されました
                return;

            Console.Write("[Debug] " + msg);
            logger.Debug(msg);
#pragma warning restore CS0162 // 到達できないコードが検出されました
        }

        public static void Info(String msg)
        {
            Console.Write("[Info ] " + msg);
            logger.Information(msg);
        }

        public static void Warn(String msg)
        {
            Console.Write("[Warn ] " + msg);
            logger.Warning(msg);
        }

        public static void Error(String msg)
        {
            Console.Write("[Error] " + msg);
            logger.Error(msg);
        }
    }
}