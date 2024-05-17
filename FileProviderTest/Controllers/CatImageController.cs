using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.IO.Compression;
using FileProviderTest.Provider;

namespace FileProviderTest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CatImageController : ControllerBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly IFileProvider _catProvider;
        private readonly static string[] _allowImageContentTypes =
            ["image/png",
            "image/jpeg",
            "image/jpeg",
            "image/gif"];
        private readonly static Dictionary<string, string> _contentType = new Dictionary<string, string>();


        public CatImageController(IFileProvider fileProvider, CatImageProvider catImageProvider)
        {
            _fileProvider = fileProvider;
            _catProvider = catImageProvider;
        }

        // 自訂一個 Provider，搜尋 cat
        // 不小心做了太多有的沒的
        [HttpGet]
        public IActionResult DownloadAllCatZip(string fileName)
        {
            var directoryContents = _catProvider.GetDirectoryContents("Cat");
            var file = directoryContents.Where(x => x.Name.StartsWith(fileName));
            if (file.Count() == 0)
            {
                return NotFound();
            }

            // 壓成 zip，才可回傳多檔案
            var zipName = $"TestFiles-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //required: using System.IO.Compression;  
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    //QUery the Products table and get all image content  
                    foreach (var item in file)
                    {
                        var entry = zip.CreateEntry(item.Name);
                        using (var fileStream = item.CreateReadStream())
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    };
                }
                return File(memoryStream.ToArray(), "application/zip", zipName);
            }
        }

        /// <summary>
        /// 取得貓咪圖檔 by id
        /// 限定png
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 取得任何圖檔 by filename
        /// 限定png
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IResult> UploadImage(IFormFile file)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName)) { return Results.BadRequest(); }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "File/", file.FileName);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    stream.Close();
                }
                return Results.Ok(new { message = "上傳成功" });
            }
            catch (Exception)
            {
                return Results.BadRequest(new { message = "上傳失敗" });
            }
        }

        /// <summary>
        /// 上傳多個檔案
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IResult> UploadMultiImage(List<IFormFile> fileList)
        {
            if (fileList == null || fileList.Count == 0) { return Results.BadRequest(); }
            try
            {
                foreach (var item in fileList)
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


        /// <summary>
        /// 上傳檔案 限制是圖檔
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IResult> UploadImageLimitContentType(IFormFile image)
        {
            if (!_allowImageContentTypes.Contains(image.ContentType))
            {
                return Results.BadRequest(new { message = "格式不符" });
            }
            return await UploadImage(image);
        }

        /// <summary>
        /// 上傳檔案 限制大小
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [RequestFormLimits(MultipartBodyLengthLimit = 51200)] // 50 kb
        [HttpPost]
        public async Task<IResult> UploadImageLimitSize(IFormFile file)
        {
            return await UploadImage(file);
        }

    }
}
