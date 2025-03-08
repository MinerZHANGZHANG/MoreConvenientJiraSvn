using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using System.Text;

namespace MoreConvenientJiraSvn.Infrastructure;

public class PlSqlCheckPipeline : IPlSqlCheckPipeline
{
    public List<SqlIssue> SqlIssues { get; set; } = [];

    private static readonly string prompt = "prompt";
    private static readonly string[] stopwords = ["set feedback off", "set define off"];
    private static readonly string separator = Environment.NewLine;

    private readonly string? fileFullName;
    private string? fileContent;
    private PlSqlParser.Sql_scriptContext? statement;
    private AntlrInputStream? inputStream;
    private PlSqlLexer? lexer;
    private CommonTokenStream? tokenStream;
    private PlSqlParser? parser;

    public PlSqlCheckPipeline()
    {

    }

    public PlSqlCheckPipeline(string fileFullName)
    {
        ArgumentNullException.ThrowIfNull(fileFullName);

        this.fileFullName = fileFullName.Trim();
    }

    public List<SqlIssue> CheckSingleFile(string filePath, Dictionary<string, int> viewAlertCountDict)
    {
        PlSqlCheckPipeline sqlCheckPipeline = new(filePath);
        sqlCheckPipeline.ReadSqlFile()
                     ?.ClearPrompts()
                     ?.ParserSql()
                     ?.CheckWhereCondition()
                     ?.CheckInsideIfBlock()
                     ?.CheckIsSame(viewAlertCountDict);

        return sqlCheckPipeline.SqlIssues ?? [];
    }

    #region pipeline part

    public PlSqlCheckPipeline? ReadSqlFile()
    {
        if (!File.Exists(fileFullName))
        {
            SqlIssues.Add(new() { FilePath = fileFullName, Level = InfoLevel.Error, Message = "找不到路径下的文件" });
            return null;
        }
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            fileContent = File.ReadAllText(fileFullName, Encoding.GetEncoding("gb2312"));
        }
        catch (Exception ex)
        {
            SqlIssues.Add(new()
            {
                FilePath = fileFullName,
                Level = InfoLevel.Error,
                Message = $"读取文件失败,ex:{ex.Message}"
            });
            return null;
        }

        return this;
    }

    public PlSqlCheckPipeline? ClearPrompts()
    {
        if (fileContent == null)
        {
            return null;
        }

        var lines = fileContent.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i <= lines.Length - 1; i++)
        {
            var trimLine = lines[i].TrimStart().ToLower();
            if (trimLine.StartsWith(prompt) || stopwords.Contains(trimLine))
            {
                lines[i] = string.Empty;
            }
        }
        fileContent = string.Join(separator, lines);

        return this;
    }

    public PlSqlCheckPipeline? ParserSql()
    {
        if (fileContent == null)
        {
            return null;
        }

        inputStream = new AntlrInputStream(fileContent);
        lexer = new PlSqlLexer(inputStream);
        tokenStream = new CommonTokenStream(lexer);
        parser = new PlSqlParser(tokenStream);

        var errorListener = new CustomErrorListener();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        var statement = parser.sql_script();

        if (parser.NumberOfSyntaxErrors > 0)
        {
            SqlIssues.Add(new()
            {
                FilePath = fileFullName,
                Level = InfoLevel.Error,
                Message = $"存在语法错误：\n{string.Join("\n", errorListener.Errors)}"
            });
            return null;
        }

        this.statement = statement;
        return this;
    }

    public PlSqlCheckPipeline? CheckWhereCondition()
    {
        if (fileContent == null || statement == null)
        {
            return null;
        }

        CheckUpdateStatements(statement);
        CheckDeleteStatements(statement);
        return this;
    }

    public PlSqlCheckPipeline? CheckInsideIfBlock()
    {
        if (fileContent == null || statement == null)
        {
            return null;
        }

        CheckInsertStatement(statement);
        return this;
    }

    public PlSqlCheckPipeline? CheckIsSame(Dictionary<string, int> viewUpdateCountDict)
    {
        if (fileContent == null || statement == null)
        {
            return null;
        }

        CheckSameViewStatements(statement, viewUpdateCountDict);
        return this;
    }

    #endregion

    #region helper function

    private void CheckUpdateStatements(IParseTree context)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Update_statementContext updateStatement)
            {
                if (updateStatement.where_clause() == null)
                {
                    SqlIssues.Add(new()
                    {
                        FilePath = fileFullName,
                        Level = InfoLevel.Warning,
                        Message = $"UPDATE 语句缺少 WHERE 子句，位于第[{updateStatement.Start.Line}]行到第[{updateStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(updateStatement)}"
                    });
                }
            }
            else
            {
                CheckUpdateStatements(child);
            }
        }
    }

    private void CheckDeleteStatements(IParseTree context)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Delete_statementContext deleteStatement)
            {
                if (deleteStatement.where_clause() == null)
                {
                    SqlIssues.Add(new()
                    {
                        FilePath = fileFullName,
                        Level = InfoLevel.Warning,
                        Message = $"DELETE 语句缺少 WHERE 子句，位于第[{deleteStatement.Start.Line}]行到第[{deleteStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(deleteStatement)}"
                    });
                }
            }
            else
            {
                CheckDeleteStatements(child);
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

    private void CheckInsertStatement(IParseTree context)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);

            if (child is PlSqlParser.Insert_statementContext insertStatement)
            {
                if (!IsInsideIfBlock(insertStatement))
                {
                    SqlIssues.Add(new()
                    {
                        FilePath = fileFullName,
                        Level = InfoLevel.Warning,
                        Message = $"Insert语句没有包含在if-endif块中，位于第[{insertStatement.Start.Line}]行到第[{insertStatement.Stop.Line}]行,请确定是否需要增加过滤条件:{GetOriginalText(insertStatement)}"
                    });
                }
            }
            else
            {
                CheckInsertStatement(child);
            }
        }
    }


    private string GetOriginalText(ParserRuleContext context)
    {
        if (context == null || fileContent == null)
        {
            return string.Empty;
        }

        // 提取上下文源文本  
        var start = context.Start.StartIndex;
        var stop = context.Stop.StopIndex;

        // 使用文件内容来提取原始文本  
        return fileContent.Substring(start, stop - start + 1);
    }

    private void CheckSameViewStatements(IParseTree context, Dictionary<string, int> viewUpdateCountDict)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is PlSqlParser.Create_viewContext createStatement && createStatement.v != null)
            {
                var viewName = GetOriginalText(createStatement.v);
                if (viewUpdateCountDict.TryGetValue(viewName, out int value))
                {
                    viewUpdateCountDict[viewName] = ++value;
                    SqlIssues.Add(new()
                    {
                        FilePath = fileFullName,
                        Level = InfoLevel.Normal,
                        Message = $"视图{viewName}在文件夹内有多次提交记录,请确保后续提交的视图包含了之前的修改"
                    });
                }
                else
                {
                    viewUpdateCountDict.Add(viewName, 0);
                }
            }
            else
            {
                CheckSameViewStatements(child, viewUpdateCountDict);
            }
        }
    }

    #endregion
}

class CustomErrorListener : BaseErrorListener
{
    public List<string> Errors { get; } = [];

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add($"语法错误在第 {line} 行，第 {charPositionInLine} 列，内容：{msg}");
        base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
    }
}





