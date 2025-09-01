using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Lemon.Services.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/upload")]
[RequireJwtAuth]
public class UploadController(
    IResponseBuilder responseBuilder,
    IStaticFilesService staticFilesService
) : LemonController(responseBuilder)
{
    private readonly IStaticFilesService _staticFilesService = staticFilesService;

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string path = "")
    {
        if (file is null || file.Length == 0)
        {
            return Error("未检测到上传文件");
        }

        var uploadPaths = _staticFilesService.GetStoragePath(path);
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var fullPath = Path.Combine(uploadPaths.AbsolutePath, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var filePath = Path.Combine(uploadPaths.RelativePath, fileName);
        var fileUrl = _staticFilesService.GetFileUrl(filePath);

        return Success(new { url = fileUrl, fileName });
    }
}
