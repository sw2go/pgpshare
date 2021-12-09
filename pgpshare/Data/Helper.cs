using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest.Data
{
    public class Helper : IHelper
    {
        public string FilesPath { get; private set; }
        public string KeysPath { get; private set; }
        public string CurrentNLogFilePath { get; private set; }
        public string AppLogFilePath { get; private set; }
        public string PutLogFilePath { get; private set; }
        public string UploadFilePath { get; private set; }

        public int PKCount { get; set; }

        public Helper(IWebHostEnvironment webHostEnvironment)
        {
            FilesPath = Path.Combine(webHostEnvironment.WebRootPath, "data/files");
            KeysPath = Path.Combine(webHostEnvironment.WebRootPath, "data/keys");

            CurrentNLogFilePath = Path.Combine(webHostEnvironment.WebRootPath, string.Format("nlog-own-{0}.log", DateTime.Now.ToString("yyyy-MM-dd")));
            AppLogFilePath = Path.Combine(webHostEnvironment.WebRootPath, "app.log");
            PutLogFilePath = Path.Combine(webHostEnvironment.WebRootPath, "put.log");
            UploadFilePath = Path.Combine(webHostEnvironment.WebRootPath, "LastUpload.dat");

            PKCount = 0;
        }

        public static void EnsureDirectory(string filePath)
        {
            string path = System.IO.Path.GetDirectoryName(filePath);

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public static async Task<string> LogEchoRequest(string format, HttpRequest Request, string logFilePath)
        {
            StringBuilder body = new StringBuilder();

            if (format == "base64")
            {
                using (var ms = new MemoryStream(2048))
                {
                    await Request.Body.CopyToAsync(ms);
                    body.Append(Convert.ToBase64String(ms.ToArray()));
                }
            }
            else
            {
                try
                {
                    var encoding = Encoding.GetEncoding(format);
                    using (StreamReader reader = new StreamReader(Request.Body, encoding))
                    {
                        body.Append(await reader.ReadToEndAsync());
                    }
                }
                catch
                {
                    body.AppendLine($"{format} CONVERTER NOT FOUND!");
                    body.AppendLine();
                    body.AppendLine($"Use one of these:");
                    foreach (var e in Encoding.GetEncodings())
                    {
                        body.AppendLine($"{e.Name}");
                    }
                }
            }
            var buffer = new RawBuffer(Request, body.ToString());

            await Data.Helper.ToFile(logFilePath, Data.LogData.Time);
            await Data.Helper.ToFile(logFilePath, Data.LogData.Request, buffer.ToString());

            return body.ToString();
        }

        public async static Task ToFile(string logFilePath, LogData logData, string content = null)
        {
            EnsureDirectory(logFilePath);

            try
            {
                using (var stream = System.IO.File.AppendText(logFilePath))
                {
                    if (logData == LogData.Request)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteAsync("----- Request -----");
                    }
                    else if (logData == LogData.ParsedRequest)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteAsync("----- ParsedRequest -----");
                    }
                    else if (logData == LogData.Response)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteAsync("----- Response ----");
                    }
                    else if (logData == LogData.Error)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteAsync("----- Error -------");
                    }
                    else if (logData == LogData.Time)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteLineAsync();
                        await stream.WriteAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));                         
                    }

                    if (content != null)
                    {
                        await stream.WriteLineAsync();
                        await stream.WriteAsync(content);
                    }
                    
                }
            }
            catch (Exception ex)
            {
            }
        }

 
        public static async Task<int> AddFileAsync(IFormFile file, string filePath)
        {
            EnsureDirectory(filePath);

            long megaByte = 1024 * 1024;

            if (file == null)
            {
                return StatusCodes.Status400BadRequest;
            }

            if (file.Length > 50 * megaByte)
            {
                return StatusCodes.Status413PayloadTooLarge;
            }

            var fi = new FileInfo($"{Path.GetTempPath()}{Path.GetRandomFileName()}");

            using (var stream = fi.Create())
            {
                await file.CopyToAsync(stream);
            }
            
            System.IO.File.Move(fi.FullName, filePath, true);

            return StatusCodes.Status201Created;
        }

    }

    public enum LogData
    {
        Time,
        Request,
        Response,
        ParsedRequest,
        Error
    }

    public interface IHelper
    {
        public string FilesPath { get; }
        public string KeysPath { get; }

        public string CurrentNLogFilePath { get; }
        public string AppLogFilePath { get; }
        public string PutLogFilePath { get; }
        public string UploadFilePath { get; }



        public int PKCount { get; set; }
    }


}
