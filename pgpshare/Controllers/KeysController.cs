using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiTest.Data;
using Microsoft.AspNetCore.Mvc;

namespace PgpShare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeysController : ControllerBase
    {
        private readonly IHelper helper;

        public KeysController(IHelper helper)
        {
            this.helper = helper;
        }

        private string getKeyPath(string fileName, string part = null)
        {
            return (part == null) ? Path.Combine(helper.KeysPath, fileName) : Path.Combine(helper.KeysPath, fileName, part);
        }

        [HttpDelete]
        [Route("{filename}")]
        public async Task Delete(string filename)
        {
            if (filename == "*")
            {
                if (Directory.Exists(helper.KeysPath))
                {
                    Directory.Delete(helper.KeysPath, true);
                }
                Directory.CreateDirectory(helper.KeysPath);
            }
        }

        [HttpPost]
        [Route("{name}/{part}")]
        public async Task CreateKey(string name, string part)
        {
            var path = getKeyPath(name);

            var directory = Directory.CreateDirectory(path);

            foreach (var fi in directory.GetFiles(part)) 
            {
                fi.Delete();
            }

            var keyFile = getKeyPath(name, part);

            using (FileStream outputFileStream = new FileStream(keyFile, FileMode.CreateNew))
            {
                await Request.Body.CopyToAsync(outputFileStream);
            }
        }

        [HttpGet]
        [Route("{filename}/{part}")]
        public async Task GetBlock(string filename, string part)
        {
            var filePath = Path.Combine(helper.KeysPath, filename);

            if (!Directory.Exists(filePath))
            {
                Response.StatusCode = 404;
            }

            var fileBlock = getKeyPath(filename, part);

            Response.ContentType = "application/octet-stream";

            if (System.IO.File.Exists(fileBlock))
            {
                using (FileStream stream = new FileStream(fileBlock, FileMode.Open))
                {
                    await stream.CopyToAsync(Response.Body);
                }
            }
            else
            {
                Response.StatusCode = 404;
            }
        }
    }
}
