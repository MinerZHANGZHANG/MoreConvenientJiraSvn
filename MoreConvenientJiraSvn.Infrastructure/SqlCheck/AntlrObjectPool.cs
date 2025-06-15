using Antlr4.Runtime;
using Microsoft.Extensions.ObjectPool;

namespace MoreConvenientJiraSvn.Infrastructure;

/// <summary>
/// 对象池管理类，用于管理Antlr相关对象的复用
/// </summary>
public class AntlrObjectPool
{
    public readonly ObjectPool<AntlrInputStream> StreamPool = new DefaultObjectPool<AntlrInputStream>(new AntlrInputStreamPoolPolicy());
    public readonly ObjectPool<PlSqlLexer> LexerPool = new DefaultObjectPool<PlSqlLexer>(new PlSqlLexerPoolPolicy());
    public readonly ObjectPool<CommonTokenStream> TokenStreamPool = new DefaultObjectPool<CommonTokenStream>(new CommonTokenStreamPoolPolicy());
    public readonly ObjectPool<PlSqlParser> ParserPool = new DefaultObjectPool<PlSqlParser>(new PlSqlParserPoolPolicy());
}

// 对象池策略实现
public class AntlrInputStreamPoolPolicy : IPooledObjectPolicy<AntlrInputStream>
{
    public AntlrInputStream Create() => new();

    public bool Return(AntlrInputStream obj)
    {
        obj.Reset();
        return true;
    }
}

public class PlSqlLexerPoolPolicy : IPooledObjectPolicy<PlSqlLexer>
{
    public PlSqlLexer Create() => new(new AntlrInputStream());

    public bool Return(PlSqlLexer obj)
    {
        obj.Interpreter.ClearDFA();
        obj.Reset();
        return true;
    }
}

public class CommonTokenStreamPoolPolicy : IPooledObjectPolicy<CommonTokenStream>
{
    public CommonTokenStream Create() => new(new PlSqlLexer(new AntlrInputStream()));

    public bool Return(CommonTokenStream obj)
    {
        obj.Reset();
        return true;
    }
}

public class PlSqlParserPoolPolicy : IPooledObjectPolicy<PlSqlParser>
{
    public PlSqlParser Create() => new(new CommonTokenStream(new PlSqlLexer(new AntlrInputStream())));

    public bool Return(PlSqlParser obj)
    {
        obj.Interpreter.ClearDFA();
        obj.Reset();
        return true;
    }
}
