using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Lemon.Services.StaticFiles;

/// <summary>
/// 静态文件服务实现
/// </summary>
public class StaticFilesService : IStaticFilesService
{
    private readonly StaticFilesOptions _options;
    private readonly string _storagePath;

    public StaticFilesService(IOptions<StaticFilesOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        _storagePath = _options.GetStoragePath(environment.ContentRootPath);
    }

    /// <summary>
    /// 获取文件的完整URL
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>完整的文件URL</returns>
    public string GetFileUrl(string filePath)
    {
        return _options.GetFileUrl(filePath);
    }

    /// <summary>
    /// 将URL转换为别名
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    public string Url2Alias(string text)
    {
        return _options.Url2Alias(text);
    }

    /// <summary>
    /// 将别名转换为URL
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    public string Alias2Url(string text)
    {
        return _options.Alias2Url(text);
    }

    /// <summary>
    /// 获取文件存储路径
    /// </summary>
    /// <returns>文件存储的完整路径</returns>
    public StaticFilePaths GetStoragePath(string subDirectory = "")
    {
        var datePath = DateTime.Now.ToString("yyyyMMdd");

        if (!string.IsNullOrEmpty(subDirectory))
        {
            subDirectory = subDirectory.Trim('/').Replace("/", "-");

            datePath = Path.Combine(subDirectory, datePath);
        }

        var absolutePath = Path.Combine(_storagePath, datePath);

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }

        return new StaticFilePaths { AbsolutePath = absolutePath, RelativePath = datePath };
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_storagePath, filePath.TrimStart('/'));
        return File.Exists(fullPath);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteFile(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
