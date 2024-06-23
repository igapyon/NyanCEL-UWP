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
    public class NyanSqlLog
    {
        private static string logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs/sqllog.txt");
        private static Serilog.ILogger logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day).CreateLogger();

        public static void Success(string sql)
        {
            Console.Write("[SQL] " + sql);
            logger.Information(sql);
        }

        public static void Error(string sql, string errormsg)
        {
            Console.Write("[SQL] " + sql);
            logger.Error(sql + ": " + errormsg);
        }
    }
}