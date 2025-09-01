namespace Lemon.Services.StaticFiles;

public class StaticFilePaths
{
    public string AbsolutePath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
}

/// <summary>
/// 静态文件服务接口
/// </summary>
public interface IStaticFilesService
{
    /// <summary>
    /// 获取文件的完整URL
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>完整的文件URL</returns>
    string GetFileUrl(string filePath);

    /// <summary>
    /// 将URL转换为别名
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    string Url2Alias(string text);

    /// <summary>
    /// 将别名转换为URL
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>替换后的文本</returns>
    string Alias2Url(string text);

    /// <summary>
    /// 获取文件存储路径
    /// </summary>
    /// <param name="subDirectory">子目录</param>
    /// <returns>文件的存储路径</returns>
    StaticFilePaths GetStoragePath(string subDirectory = "");

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    bool DeleteFile(string filePath);
}
