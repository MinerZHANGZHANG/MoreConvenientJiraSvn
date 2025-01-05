using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MoreConvenientJiraSvn.Core.Model;
using System.IO;

namespace MoreConvenientJiraSvn.Plugin.SqlCheck;

public record SqlIssue
{
    public required string FilePath { get; set; }
    public required string Message { get; set; }
    public InfoLevel Level { get; set; }
}

public class SqlCheckPipeline
{
    public List<SqlIssue> SqlIssues { get; set; } = [];

    private const string prompt = "prompt";
    private string fileFullName;
    private string? fileContent;
    private PlSqlParser.Sql_scriptContext? statement;
    private AntlrInputStream inputStream;
    private PlSqlLexer lexer;
    private CommonTokenStream tokenStream;
    private PlSqlParser parser;
    private static readonly string separator = Environment.NewLine;

    public SqlCheckPipeline(string fileFullName)
    {
        ArgumentNullException.ThrowIfNull(fileFullName);

        this.fileFullName = fileFullName.Trim();
    }

    public SqlCheckPipeline? ReadSqlFile()
    {
        if (!File.Exists(fileFullName))
        {
            SqlIssues.Add(new() { FilePath = fileFullName, Level = InfoLevel.Error, Message = "找不到路径下的文件" });
            return null;
        }
        try
        {
            fileContent = File.ReadAllText(fileFullName);
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

    public SqlCheckPipeline? ClearPrompts()
    {
        if (fileContent == null)
        {
            return null;
        }

        var lines = fileContent.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i <= lines.Length - 1; i++)
        {
            if (lines[i].TrimStart().StartsWith(prompt))
            {
                lines[i] = string.Empty;
            }
            else
            {
                break;
            }
        }
        fileContent = string.Join(separator, lines);

        return this;
    }

    public SqlCheckPipeline? ParserSql()
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

    public SqlCheckPipeline? CheckWhereCondition()
    {
        if (fileContent == null || statement == null)
        {
            return null;
        }

        CheckUpdateStatements(statement);
        CheckDeleteStatements(statement);
        return this;
    }

    public SqlCheckPipeline? CheckInsideIfBlock()
    {
        if (fileContent == null || statement == null)
        {
            return null;
        }

        CheckInsertStatement(statement);
        return this;
    }


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

    private bool IsInsideIfBlock(PlSqlParser.Insert_statementContext insertStatement)
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

    private void CheckInsertOnView(IParseTree context)
    {
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);

            if (child is PlSqlParser.Insert_statementContext insertStatement)
            {
                var targetTable = insertStatement.single_table_insert()?.GetText();
            }
            else
            {
                CheckInsertOnView(child);
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
}

class CustomErrorListener : BaseErrorListener
{
    public List<string> Errors { get; } = new List<string>();

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add($"语法错误在第 {line} 行，第 {charPositionInLine} 列，内容：{msg}");
        base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
    }
}



