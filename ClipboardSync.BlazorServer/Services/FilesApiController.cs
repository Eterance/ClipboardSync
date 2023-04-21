using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml.Serialization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClipboardSync.BlazorServer.Services
{
    [Authorize]
    [Route("api/files")]
    [ApiController]
    public class FilesApiController : ControllerBase
    {
        private readonly string folderName = "ClipboardSync_Server";

        // GET: api/<FilesApiController>
        [HttpGet("stringlist")]
        public IActionResult StringListGet(string filename)
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string fullFileName = Path.Combine(directoryPath, filename);
                if (System.IO.File.Exists(fullFileName) == false)
                {
                    Put(new List<string>(), filename);
                }
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                using (StreamReader reader = new StreamReader(fullFileName, Encoding.UTF8))
                {
                    List<string>? list = serializer.Deserialize(reader) as List<string>;
                    return Ok(list ?? new());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Get file {filename} fail.");
            }
        }
    



        // PUT api/<FilesApiController>/5
        [HttpPut("stringList")]
        public IActionResult Put([FromBody] List<string> list, string filename)
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string fullFileName = Path.Combine(directoryPath, filename);
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                using (StreamWriter writer = new StreamWriter(fullFileName, false, Encoding.UTF8))
                {
                    serializer.Serialize(writer, list);
                }
                return Ok($"Save file {filename} compeleted");
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Save file {filename} fail.");
            }
        }
    }
}
