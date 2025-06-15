using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record SqlCheckSetting
{
    /// <summary>
    /// Global setting
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.Empty;

    /// <summary>
    /// 默认检测的目录
    /// </summary>
    public string DefaultDir { get; set; } = string.Empty;

    /// <summary>
    /// 是否检查重复的视图
    /// </summary>
    public bool IsCheckRepeatViews { get; set; } = true;

    /// <summary>
    /// 是否检查事务和脚本结尾
    /// </summary>
    public bool IsCheckCommitAndSlash { get; set; } = true;

    /// <summary>
    /// 需要跳过的目录名称
    /// </summary>
    public string SkipDirectoryName{ get; set; } = string.Empty;

    /// <summary>
    /// 是否启用对象池
    /// </summary>
    public bool IsUseAntlrObjectPool { get; set; } = true;

    /// <summary>
    /// 是否在处理完后立即释放对象
    /// </summary>
    public bool IsReleaseObjectNow { get; set; } = false;

    /// <summary>
    /// 处理时的编码格式
    /// </summary>
    public string Encoding { get; set; } = "utf-8";
}
