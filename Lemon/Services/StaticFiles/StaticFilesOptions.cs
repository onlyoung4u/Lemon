namespace Lemon.Services.StaticFiles;

/// <summary>
/// 静态文件配置选项
/// </summary>
public class StaticFilesOptions
{
    /// <summary>
    /// 是否启用静态文件服务
    /// </summary>
    public bool Enable { get; set; } = false;

    /// <summary>
    /// 静态文件存储目录
    /// </summary>
    public string UploadDirectory { get; set; } = "uploads";

    /// <summary>
    /// 静态文件请求路径
    /// </summary>
    public string RequestPath { get; set; } = "/uploads";

    /// <summary>
    /// 请求URL（用于生成完整URL）
    /// </summary>
    public string RequestUrl { get; set; } = string.Empty;

    /// <summary>
    /// 获取文件存储的完整路径
    /// </summary>
    /// <param name="contentRootPath">应用程序内容根路径</param>
    /// <returns>文件存储的完整路径</returns>
    public string GetStoragePath(string contentRootPath)
    {
        return Path.Combine(contentRootPath, UploadDirectory);
    }

    /// <summary>
    /// 获取完整的文件URL
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>完整的文件URL</returns>
    public string GetFileUrl(string filePath)
    {
        if (!string.IsNullOrEmpty(RequestUrl))
        {
            return $"{RequestUrl.TrimEnd('/')}/{filePath.TrimStart('/')}";
        }

        return filePath;
    }

    /// <summary>
    /// 将URL转换为别名
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    public string Url2Alias(string text)
    {
        if (string.IsNullOrEmpty(RequestUrl))
        {
            return text;
        }

        return text.Replace(RequestUrl, "__STATIC_FILES_URL__");
    }

    /// <summary>
    /// 将别名转换为URL
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    public string Alias2Url(string text)
    {
        if (string.IsNullOrEmpty(RequestUrl))
        {
            return text;
        }

        return text.Replace("__STATIC_FILES_URL__", RequestUrl);
    }
}
