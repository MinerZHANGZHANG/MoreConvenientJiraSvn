using Antlr4.Runtime;

public class PlSqlLexerBase : Lexer
{
    ICharStream myinput;

    public override string[] RuleNames => throw new NotImplementedException();

    public override IVocabulary Vocabulary => throw new NotImplementedException();

    public override string GrammarFileName => throw new NotImplementedException();

    protected PlSqlLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
        myinput = input;
    }

    public PlSqlLexerBase(ICharStream input)
        : base(input)
    {
        myinput = input;
    }

    public bool IsNewlineAtPos(int pos)
    {
        int la = myinput.LA(pos);
        return la == -1 || la == '\n';
    }
}
