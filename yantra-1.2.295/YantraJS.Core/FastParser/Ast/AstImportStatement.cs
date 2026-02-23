#nullable enable
namespace YantraJS.Core.FastParser;

public class AstImportStatement(
    FastToken token,
    AstIdentifier? defaultIdentifier,
    AstIdentifier? all,
    IFastEnumerable<(StringSpan, StringSpan)>? members,
    AstLiteral source) : AstStatement(token, FastNodeType.ImportStatement, source.End)
{
    public readonly AstIdentifier? Default = defaultIdentifier;
    public readonly AstIdentifier? All = all;
    public readonly IFastEnumerable<(StringSpan name, StringSpan asName)>? Members = members;
    public readonly AstLiteral Source = source;
}