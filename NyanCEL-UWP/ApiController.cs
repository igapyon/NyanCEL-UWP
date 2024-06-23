// Copyright (c) 2024 Toshiki Iga
//
// Released under the MIT license
// https://opensource.org/license/mit

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Data.Sqlite;
using Nyan;
using NyanCEL;

namespace NyanCELUWP
{
    /////////////////////////////////////////////////////////////////////////////////////
    // EmbedIO

    // TODO POST対応。

    public class ApiController : WebApiController
    {
        /// <summary>
        /// http://IPADDR:28096/api?sql=SELECT... でアクセスできます。
        /// NOTE: loopback not works by Windows default.
        /// </summary>
        /// <returns></returns>
        [Route(HttpVerbs.Get, "/")]
        public async Task GetApi()
        {
            SetCors();

            var sql = HttpContext.Request.QueryString["sql"];
            if (string.IsNullOrEmpty(sql))
            {
                HttpContext.Response.StatusCode = 400;
                HttpContext.Response.ContentType = "application/json";
                using (var writer = new StreamWriter(HttpContext.Response.OutputStream, Encoding.UTF8))
                {
                    await writer.WriteAsync("{\"error\": \"The 'sql' query parameter missing\"}");
                }
                return;
            }

            var fmt = HttpContext.Request.QueryString["fmt"];
            var target = HttpContext.Request.QueryString["target"];
            if (string.IsNullOrEmpty(fmt))
            {
                fmt = "json";
            }
            else if (fmt == "xml")
            {
                fmt = "xml";
            }
            else if (fmt == "xlsx")
            {
                fmt = "xlsx";
            }
            else
            {
                fmt = "json";
            }

            string resultString = null; // json, xml
            byte[] resultBinary = null; // xlsx
            SqliteConnection connection = await NyanCELUtil.GetXlsxDatabaseInstance();
            try
            {
                if (fmt =="xml")
                {
                    var xpath = HttpContext.Request.QueryString["xpath"];
                    if (string.IsNullOrEmpty(xpath))
                    {
                        resultString = await NyanSql2Xml.Sql2Xml(connection, sql);
                    }
                    else
                    {
                        resultString = await NyanSql2Xml.Sql2XmlWithXPath(connection, sql, xpath);
                    }
                }
                else if (fmt =="xlsx")
                {
                    resultBinary = await NyanSql2Xlsx.Sql2Xlsx(connection, sql);
                }
                else
                {
                    var jsonpath = HttpContext.Request.QueryString["jsonpath"];
                    if (string.IsNullOrEmpty(jsonpath))
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            resultString = await NyanSql2Json.Sql2Json(connection, sql);
                        }
                        else
                        {
                            resultString = await NyanSql2Json.Sql2JsonWithTarget(connection, sql, target);
                        }
                    }
                    else
                    {
                        resultString = await NyanSql2Json.Sql2JsonWithJSONPath(connection, sql, jsonpath);
                    }
                }
                NyanSqlLog.Success(sql);
            }
            catch (Exception ex)
            {
                NyanSqlLog.Error(sql, ex.Message);
                NyanLog.Error("Unexpected: " + ex.ToString());
                HttpContext.Response.StatusCode = 400;
                HttpContext.Response.ContentType = "application/json";
                using (var writer = new StreamWriter(HttpContext.Response.OutputStream, Encoding.UTF8))
                {
                    await writer.WriteAsync("{\"error\": \"The 'sql' got error: "+ex.Message+"\"}");
                }
                return;
            }

            if (fmt == "json")
            {
                if (string.IsNullOrEmpty(target))
                {
                    HttpContext.Response.ContentType = "application/json";
                }
                else
                {
                    HttpContext.Response.ContentType = "text/plain";
                }
            }
            else if (fmt == "xml")
            {
                var xpath = HttpContext.Request.QueryString["xpath"];
                if (string.IsNullOrEmpty(xpath))
                {
                    HttpContext.Response.ContentType = "application/xml";
                }
                else
                {
                    HttpContext.Response.ContentType = "text/plain";
                }
            }
            else if (fmt == "xlsx")
            {
                HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                HttpContext.Response.Headers.Add("content-disposition", "attachment;filename=Book1.xlsx");
            }

            if (fmt == "xlsx")
            {
                HttpContext.Response.OutputStream.Write(resultBinary, 0, resultBinary.Length);
                HttpContext.Response.OutputStream.Flush();
            }
            else
            {
                using (var writer = new StreamWriter(HttpContext.Response.OutputStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(resultString);
                }
            }
        }

        private void SetCors()
        {
            // CORS 設定
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "*");
            Response.Headers.Add("Access-Control-Allow-Headers", "*");
        }
    }
}
