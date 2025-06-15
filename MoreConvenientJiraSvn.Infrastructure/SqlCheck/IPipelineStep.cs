using Antlr4.Runtime;
using System.Collections.Concurrent;

namespace MoreConvenientJiraSvn.Infrastructure;

/// <summary>
/// 管道操作结果
/// </summary>
public class PipelineStepResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public object? Data { get; }

    private PipelineStepResult(bool success, string? errorMessage, object? data)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Data = data;
    }

    public static PipelineStepResult Ok(object? data = null) => new(true, null, data);

    public static PipelineStepResult Fail(string errorMessage) => new(false, errorMessage, null);
}

/// <summary>
/// 管道接口
/// </summary>
/// <typeparam name="TInput">输入类型</typeparam>
public interface IPipelineStep<in TInput>
{
    public string Name { get; init; }

    PipelineStepResult Execute(TInput input);
}

/// <summary>
/// 标记关键步骤接口    
/// </summary>
public interface ICriticalPipelineStep
{ }

/// <summary>
/// PLSQL管道上下文
/// </summary>
public class PlSqlPipelineContext : IDisposable
{
    /// <summary>
    /// Sql文件地址
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Sql文件内容
    /// </summary>
    public string ProcessedContent { get; set; } = string.Empty;

    /// <summary>
    /// 解析PL/SQL语句的解析器
    /// </summary>
    public PlSqlParser? Parser { get; set; }
    public AntlrInputStream? InputStream { get; internal set; }
    public PlSqlLexer? Lexer { get; internal set; }
    public CommonTokenStream? TokenStream { get; internal set; }

    /// <summary>
    /// PL/SQL上下文
    /// </summary>
    public PlSqlParser.Sql_scriptContext? ParsedStatement { get; set; }

    /// <summary>
    /// 视图出现次数统计字典
    /// </summary>
    public ConcurrentDictionary<string, int> ViewScriptCountDict { get; set; } = [];

    public virtual void Dispose()
    {
        Parser?.Interpreter.ClearDFA();
        Parser?.Reset();
        Parser = null;

        InputStream?.Reset();
        InputStream = null;

        Lexer?.Interpreter.ClearDFA();
        Lexer?.Reset();
        Lexer = null;

        TokenStream?.Reset();
        TokenStream = null;

        ParsedStatement = null;
        ProcessedContent = string.Empty;

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 使用对象池的管道上下文
/// </summary>
public class PlSqlPipelinePoolContext : PlSqlPipelineContext
{
    public AntlrObjectPool? AntlrObjectPool { get; set; }

    public override void Dispose()
    {
        if (Parser != null)
        {
            AntlrObjectPool?.ParserPool.Return(Parser);
        }
        if (InputStream != null)
        {
            AntlrObjectPool?.StreamPool.Return(InputStream);
        }
        if (Lexer != null)
        {
            AntlrObjectPool?.LexerPool.Return(Lexer);
        }
        if (TokenStream != null)
        {
            AntlrObjectPool?.TokenStreamPool.Return(TokenStream);
        }

        ParsedStatement = null;
        ProcessedContent = string.Empty;

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 具体管道步骤
/// </summary>
public abstract class PlSqlPipelineStepBase() : IPipelineStep<PlSqlPipelineContext>
{
    public abstract string Name { get; init; }

    public abstract PipelineStepResult Execute(PlSqlPipelineContext context);

    /// <summary>
    /// 从解析后的上下文Token转换回原始文本
    /// </summary>
    protected string GetOriginalText(object parserContext, string originContext)
    {
        if (parserContext is not ParserRuleContext context || string.IsNullOrEmpty(originContext))
        {
            return string.Empty;
        }

        var start = context.Start.StartIndex;
        var stop = context.Stop.StopIndex;

        // 使用文件内容来提取原始文本  
        return originContext.Substring(start, stop - start + 1);
    }
}
