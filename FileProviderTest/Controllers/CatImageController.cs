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
        public IActionResult GetImage([FromQuery] string name)
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

        [RequestFormLimits(MultipartBodyLengthLimit = 51200)] // 50 kb
        [HttpPost]
        public async Task<IResult> UploadImageLimitSize(IFormFile image)
        {
            return await UploadImage(image);
        }

        [HttpPost]
        public async Task<IResult> UploadImage(IFormFile image)
        {
            if (image == null || string.IsNullOrEmpty(image.FileName)) { return Results.BadRequest(); }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "File/", image.FileName);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                    stream.Close();
                }
                return Results.Ok(new { message = "上傳成功" });
            }
            catch (Exception)
            {
                return Results.BadRequest(new { message = "上傳失敗" });
            }
        }

        [HttpPost]
        public async Task<IResult> UploadMulitImage(List<IFormFile> imageList)
        {
            if (imageList == null || imageList.Count == 0) { return Results.BadRequest(); }
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
                return Results.Ok(new { message = "上傳多個檔案成功" });
            }
            catch (Exception)
            {
                return Results.BadRequest(new { message = "上傳時發生錯誤" });
            }
        }
    }
}
