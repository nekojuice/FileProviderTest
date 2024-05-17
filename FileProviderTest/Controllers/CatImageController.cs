using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace FileProviderTest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CatImageController : ControllerBase
    {
        private readonly IFileProvider _fileProvider;
        public CatImageController(IFileProvider fileProvider) => _fileProvider = fileProvider;


        [HttpGet]
        [Route("{id}")]
        public IActionResult GetCat([FromRoute] string id)
        {
            try
            {
                var fileInfo = _fileProvider.GetFileInfo($"cat{id}.png");
                return File(fileInfo.CreateReadStream(), "image/png");
            }
            catch (Exception)
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("{name}")]
        public IActionResult GetImage([FromRoute] string name)
        {
            try
            {
                var fileInfo = _fileProvider.GetFileInfo($"{name}.png");
                return File(fileInfo.CreateReadStream(), "image/png");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || string.IsNullOrEmpty(image.FileName)) { return BadRequest(); }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "File/", image.FileName);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                    stream.Close();
                }
                return Ok(new { message = "上傳成功" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "上傳失敗" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadMulitImage(List<IFormFile> imageList)
        {
            if (imageList == null || imageList.Count == 0) { return BadRequest(); }
            try
            {
                foreach (var item in imageList)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "File/", item.FileName);

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        await item.CopyToAsync(stream);
                        stream.Close();
                    }

                }
                return Ok(new { message = "上傳多個檔案成功" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "上傳時發生錯誤" });
            }
        }
    }
}
