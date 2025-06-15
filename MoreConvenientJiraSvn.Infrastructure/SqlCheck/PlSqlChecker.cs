using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using System.Collections.Concurrent;
using System.Text;

namespace MoreConvenientJiraSvn.Infrastructure;

/// <summary>
/// PLSql检测
/// </summary>
public class PlSqlChecker : IPlSqlIssueChecker
{
    /// <summary>
    /// Sql问题
    /// </summary>
    public List<SqlIssue> SqlIssues { get; init; } = [];

    /// <summary>
    /// 视图出现次数统计字典
    /// </summary>
    private ConcurrentDictionary<string, int> _viewScriptCountDict { get; set; } = [];

    /// <summary>
    /// 记录上下文后续统一删除
    /// </summary>
    public List<PlSqlPipelineContext> PipelineContexts = [];

    /// <summary>
    /// Antlr对象池
    /// </summary>
    public readonly AntlrObjectPool _antlrObjectPool = new();

    /// <summary>
    /// 检测单个SQL文件
    /// </summary>
    public List<SqlIssue> CheckSingleFile(string filePath, SqlCheckSetting sqlCheckSetting)
    {
        try
        {
            PlSqlPipelineContext context;
            if (sqlCheckSetting.IsUseAntlrObjectPool)
            {
                context = new PlSqlPipelinePoolContext
                {
                    FilePath = filePath,
                    ViewScriptCountDict = _viewScriptCountDict,
                };
            }
            else
            {
                context = new PlSqlPipelineContext
                {
                    FilePath = filePath,
                    ViewScriptCountDict = _viewScriptCountDict,
                };
            }
            List<IPipelineStep<PlSqlPipelineContext>> pipelineSteps = [];
            pipelineSteps.Add(new ReadSqlFileStep(sqlCheckSetting.Encoding));
            pipelineSteps.Add(new ClearPromptsStep());
            pipelineSteps.Add(new ParseSqlStep(this));
            pipelineSteps.Add(new CheckWhereConditionStep());
            pipelineSteps.Add(new CheckInsertSafetyStep());

            if (sqlCheckSetting.IsCheckCommitAndSlash)
            {
                pipelineSteps.Add(new CheckCommitStep());
                pipelineSteps.Add(new CheckEndSlash());
            }

            if (sqlCheckSetting.IsCheckRepeatViews)
            {
                pipelineSteps.Add(new CheckSameViewStep());
            }

            if (!sqlCheckSetting.IsReleaseObjectNow)
            {
                PipelineContexts.Add(context);
            }

            ExecutePipeline(context, pipelineSteps);

            // 立即清理上下文对象，或稍后清理
            if (sqlCheckSetting.IsReleaseObjectNow)
            {
                context.Dispose();
            }

            return SqlIssues;
        }
        catch (Exception ex)
        {
            SqlIssues.Add(new()
            {
                IssueType = "管道执行错误",
                FilePath = filePath,
                Level = InfoLevel.Error,
                Message = $"管道执行发生异常: {ex.Message}"
            });
            return SqlIssues;
        }
    }

    /// <summary>
    /// 检测多个SQL文件
    /// </summary>
    public async Task<List<SqlIssue>> CheckMultipleFilesAsync(IEnumerable<string> filePaths, SqlCheckSetting sqlCheckSetting, Action<int>? progressAction)
    {
        // 复制一个避免修改原始设置
        sqlCheckSetting = sqlCheckSetting with { };

        SqlIssues.Clear();
        int progress = 0;
        await Parallel.ForEachAsync(filePaths, (path, token) =>
        {
            CheckSingleFile(path, sqlCheckSetting);
            progressAction?.Invoke(++progress);

            return ValueTask.CompletedTask;
        });

        // 释放管道上下文资源
        if (!sqlCheckSetting.IsReleaseObjectNow)
        {
            foreach (var item in PipelineContexts)
            {
                item?.Dispose();
            }
            PipelineContexts.Clear();
        }

        return SqlIssues;
    }

    /// <summary>
    /// 执行管道步骤
    /// </summary>
    protected void ExecutePipeline(PlSqlPipelineContext context, IEnumerable<IPipelineStep<PlSqlPipelineContext>> pipelineSteps)
    {
        foreach (var step in pipelineSteps)
        {
            var result = step.Execute(context);
            if (!result.Success)
            {
                SqlIssues.Add(new SqlIssue
                {
                    IssueType = step.Name,
                    FilePath = context.FilePath,
                    Level = InfoLevel.Error,
                    Message = result.ErrorMessage!
                });

                // 如果关键步骤失败，终止管道执行
                if (step is ICriticalPipelineStep)
                {
                    break;
                }
            }
        }
    }
}

#region Pipeline Steps

/// <summary>
/// 读取Sql文件
/// </summary>
public class ReadSqlFileStep(string encoding) : PlSqlPipelineStepBase(), ICriticalPipelineStep
{
    public override string Name { get; init; } = "读取文件";

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (!File.Exists(context.FilePath))
        {
            return PipelineStepResult.Fail("找不到路径下的文件");
        }

        try
        {
            Encoding _fileEncoding = Encoding.GetEncoding(encoding);
            context.ProcessedContent = File.ReadAllText(context.FilePath, _fileEncoding);

            return PipelineStepResult.Ok();
        }
        catch (Exception ex)
        {
            return PipelineStepResult.Fail($"读取文件失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 清理提示词
/// </summary>
public class ClearPromptsStep() : PlSqlPipelineStepBase(), ICriticalPipelineStep
{
    public override string Name { get; init; } = "关键字清理";

    private const string _prompt = "prompt";
    private static readonly string[] _stopwords = ["set feedback off", "set define off"];
    private static readonly string _separator = Environment.NewLine;

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (string.IsNullOrEmpty(context.ProcessedContent))
            return PipelineStepResult.Fail("没有内容可以处理");

        var lines = context.ProcessedContent.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var trimLine = lines[i].TrimStart().ToLower();
            if (trimLine.StartsWith(_prompt) || _stopwords.Contains(trimLine))
            {
                lines[i] = string.Empty;
            }
        }

        context.ProcessedContent = string.Join(_separator, lines);
        return PipelineStepResult.Ok();
    }
}

/// <summary>
/// 解析SQL语句
/// </summary>
public class ParseSqlStep(PlSqlChecker plSqlChecker) : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "SQL语法解析";

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (string.IsNullOrEmpty(context.ProcessedContent))
            return PipelineStepResult.Fail("没有内容可以解析");

        try
        {
            AntlrInputStream inputStream;
            PlSqlLexer lexer;
            CommonTokenStream tokenStream;
            PlSqlParser parser;

            // 从对象池获取对象
            if (context is PlSqlPipelinePoolContext poolContext)
            {
                poolContext.AntlrObjectPool = plSqlChecker._antlrObjectPool;

                inputStream = plSqlChecker._antlrObjectPool.StreamPool.Get();
                inputStream.Load(new StringReader(context.ProcessedContent), AntlrInputStream.InitialBufferSize, AntlrInputStream.ReadBufferSize);

                lexer = plSqlChecker._antlrObjectPool.LexerPool.Get();
                lexer.SetInputStream(inputStream);

                tokenStream = plSqlChecker._antlrObjectPool.TokenStreamPool.Get();
                tokenStream.SetTokenSource(lexer);

                parser = plSqlChecker._antlrObjectPool.ParserPool.Get();
                parser.TokenStream = tokenStream;
            }
            else
            {
                inputStream = new AntlrInputStream(context.ProcessedContent);
                lexer = new PlSqlLexer(inputStream);
                tokenStream = new CommonTokenStream(lexer);
                parser = new PlSqlParser(tokenStream);
            }
            context.Parser = parser;
            context.InputStream = inputStream;
            context.Lexer = lexer;
            context.TokenStream = tokenStream;


            // 添加自定义错误监听器
            var errorListener = new CustomErrorListener();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var statement = parser.sql_script();
            context.ParsedStatement = statement;
            if (parser.NumberOfSyntaxErrors > 0)
            {
                return PipelineStepResult.Fail(
                    $"存在语法错误：\n{string.Join("\n", errorListener.Errors)}");
            }

            return PipelineStepResult.Ok();
        }
        catch (Exception ex)
        {
            return PipelineStepResult.Fail($"解析发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 语法错误监听器
    /// </summary>
    class CustomErrorListener : BaseErrorListener
    {
        public List<string> Errors { get; } = [];

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Errors.Add($"语法错误在第 {line} 行，第 {charPositionInLine} 列，内容：{msg}");
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }
}

/// <summary>
/// 判断Update或Delete语句是否有Where条件
/// </summary>
public class CheckWhereConditionStep() : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "可疑的删改操作";

    public List<string> WarnningMessages { get; } = [];

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (context.ParsedStatement == null)
        {
            return PipelineStepResult.Fail("没有解析好的语句可以检查");
        }

        CheckUpdateStatements(context.ParsedStatement, context.ProcessedContent);
        CheckDeleteStatements(context.ParsedStatement, context.ProcessedContent);

        if (WarnningMessages.Count == 0)
        {
            return PipelineStepResult.Ok();
        }

        return PipelineStepResult.Fail(string.Join(Environment.NewLine, WarnningMessages));
    }

    private void CheckUpdateStatements(IParseTree context, string originContext)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Update_statementContext updateStatement)
            {
                if (updateStatement.where_clause() == null)
                {
                    WarnningMessages.Add($"UPDATE 语句缺少 WHERE 子句，位于第[{updateStatement.Start.Line}]行到第[{updateStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(updateStatement, originContext)}");
                }
            }
            else
            {
                CheckUpdateStatements(child, originContext);
            }
        }
    }

    private void CheckDeleteStatements(IParseTree context, string originContext)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Delete_statementContext deleteStatement)
            {
                if (deleteStatement.where_clause() == null)
                {
                    WarnningMessages.Add($"DELETE 语句缺少 WHERE 子句，位于第[{deleteStatement.Start.Line}]行到第[{deleteStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(deleteStatement, originContext)}");
                }
            }
            else
            {
                CheckDeleteStatements(child, originContext);
            }
        }
    }
}

/// <summary>
/// 校验Insert语句是否在If块内或使用Insert-Select语句
/// </summary>
public class CheckInsertSafetyStep() : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "可疑的插入语句";

    public List<string> WarnningMessages { get; } = [];

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (context.ParsedStatement == null)
        {
            return PipelineStepResult.Fail("没有解析好的语句可以检查");

        }

        CheckInsertStatement(context.ParsedStatement, context.ProcessedContent);

        if (WarnningMessages.Count == 0)
        {
            return PipelineStepResult.Ok();
        }

        return PipelineStepResult.Fail(string.Join(Environment.NewLine, WarnningMessages));
    }

    private void CheckInsertStatement(IParseTree context, string originContext)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);

            if (child is PlSqlParser.Insert_statementContext insertStatement)
            {
                if (!IsInsideIfBlock(insertStatement))
                {
                    // 检查是否是Insert-Select语句
                    if (insertStatement.children.Count >= 2 && insertStatement.children[1] is PlSqlParser.Single_table_insertContext insertContext)
                    {
                        if (insertContext.children.Count >= 2 && insertContext.children[1] is PlSqlParser.Select_statementContext)
                        {
                            continue;
                        }
                    }

                    WarnningMessages.Add($"Insert语句没有 包含在if-endif块中/使用Insert-Select语句，位于第[{insertStatement.Start.Line}]行到第[{insertStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(insertStatement, originContext)}");
                }
            }
            else
            {
                CheckInsertStatement(child, originContext);
            }
        }
    }

    private static bool IsInsideIfBlock(PlSqlParser.Insert_statementContext insertStatement)
    {
        var parent = insertStatement.Parent;
        while (parent != null)
        {
            if (parent is PlSqlParser.If_statementContext)
            {
                return true;
            }
            parent = parent.Parent;
        }

        return false;
    }

}

/// <summary>
/// 校验是否存在重复的视图提交
/// </summary>
/// <param name="pipeline"></param>
public class CheckSameViewStep() : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "重复的视图";

    public List<string> WarnningMessages { get; } = [];

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (context.ParsedStatement == null)
        {
            return PipelineStepResult.Fail("缺少必要的上下文数据");
        }

        CheckSameViewStatements(context.ParsedStatement, context.ViewScriptCountDict, context.ProcessedContent);

        if (WarnningMessages.Count == 0)
        {
            return PipelineStepResult.Ok();
        }

        return PipelineStepResult.Fail(string.Join(Environment.NewLine, WarnningMessages));
    }

    private void CheckSameViewStatements(IParseTree context, ConcurrentDictionary<string, int> viewUpdateCountDict, string originContext)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Create_viewContext createStatement && createStatement.v != null)
            {
                var viewName = GetOriginalText(createStatement.v, originContext);
                if (viewUpdateCountDict.TryGetValue(viewName, out int value))
                {
                    viewUpdateCountDict[viewName] = ++value;
                    WarnningMessages.Add($"视图{viewName}在文件夹内有多次提交记录,请确保后续提交的视图包含了之前的修改");
                }
                else
                {
                    viewUpdateCountDict.TryAdd(viewName, 0);
                }
            }
            else
            {
                CheckSameViewStatements(child, viewUpdateCountDict, originContext);
            }
        }
    }
}

/// <summary>
/// 检测DML语句是否包含COMMIT
/// </summary>
public class CheckCommitStep() : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "可疑的事务";

    public List<string> WarnningMessages { get; } = [];

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (context.ParsedStatement == null)
        {
            return PipelineStepResult.Fail("没有解析好的语句可以检查");
        }

        bool hasDmlStatement = false;
        bool hasCommit = false;

        CheckStatements(context.ParsedStatement, ref hasDmlStatement, ref hasCommit, context.ProcessedContent);

        if (hasDmlStatement && !hasCommit)
        {
            return PipelineStepResult.Fail("脚本中包含INSERT/UPDATE/DELETE语句, 但缺少COMMIT提交语句");
        }

        return PipelineStepResult.Ok();
    }

    private void CheckStatements(IParseTree context, ref bool hasDmlStatement, ref bool hasCommit, string originContext)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);

            // 检查DML语句
            if (child is PlSqlParser.Insert_statementContext
                || child is PlSqlParser.Update_statementContext
                || child is PlSqlParser.Delete_statementContext)
            {
                hasDmlStatement = true;
            }
            // 检查COMMIT语句
            else if (child is PlSqlParser.Commit_statementContext || child.GetText().Equals("COMMIT", StringComparison.OrdinalIgnoreCase))
            {
                hasCommit = true;
            }
            else
            {
                CheckStatements(child, ref hasDmlStatement, ref hasCommit, originContext);
            }

            // 如果已经找到了DML和COMMIT，就可以提前退出
            if (hasDmlStatement && hasCommit)
            {
                return;
            }
        }
    }
}

/// <summary>
/// 检测脚本是否以斜杠结尾
/// </summary>
public class CheckEndSlash() : PlSqlPipelineStepBase()
{
    public override string Name { get; init; } = "可疑的结尾";

    public List<string> WarnningMessages { get; } = [];

    public override PipelineStepResult Execute(PlSqlPipelineContext context)
    {
        if (context.ParsedStatement == null)
        {
            return PipelineStepResult.Fail("没有解析好的语句可以检查");
        }

        bool hasForwardSlash = context.ProcessedContent.TrimEnd().EndsWith('/');
        if (!hasForwardSlash)
        {
            return PipelineStepResult.Fail("脚本未使用正斜杠结尾");
        }

        return PipelineStepResult.Ok();
    }
}
#endregion