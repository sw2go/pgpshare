using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiTest.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PgpShare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IHelper helper;

        public FilesController(IHelper helper)
        {
            this.helper = helper;
        }

        private string getFilePath(string fileName, int? part = null)
        {
            return (part == null) ? Path.Combine(helper.FilesPath, fileName) : Path.Combine(helper.FilesPath, fileName, part.Value.ToString());            
        }


        [HttpGet]
        [Route("{key}")]
        public async Task<string> RawBinarydData(string key)
        {
            return key;
        }


        [HttpDelete]
        [Route("{filename}")]
        public async Task Delete(string filename)
        {
            if (filename == "*")
            {
                if (Directory.Exists(helper.FilesPath))
                {
                    Directory.Delete(helper.FilesPath, true);
                }
                Directory.CreateDirectory(helper.FilesPath);
            }            
        }



        [HttpPost]
        [Route("{filename}/{part}")]
        public async Task CreateBlock(string filename, int part)
        {
            var path = getFilePath(filename);

            var directory = Directory.CreateDirectory(path);
            if (part == 0)
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            var fileBlock = getFilePath(filename, part);

            using (FileStream outputFileStream = new FileStream(fileBlock, FileMode.CreateNew))
            {
                await Request.Body.CopyToAsync(outputFileStream);
            }
        }

        [HttpGet]
        [Route("{filename}/{part}")]
        public async Task GetBlock(string filename, int part)
        {
            var filePath = Path.Combine(helper.FilesPath, filename);

            if (!Directory.Exists(filePath))
            {
                Response.StatusCode = 404;
            }

            var fileBlock = getFilePath(filename, part);
           
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
                using (MemoryStream stream = new MemoryStream())
                {
                    await stream.CopyToAsync(Response.Body);
                }
            }


        }

    }
}
