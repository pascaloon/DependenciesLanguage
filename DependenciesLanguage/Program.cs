using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using DependenciesLanguage.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesLanguage
{
    class MyErrorListenner : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception("Invalid Format");
        }

        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception("Invalid Format");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string query = "* --> b";
            AntlrInputStream stream = new AntlrInputStream(new StringReader(query));

            var lexer = new DependenciesGrammarLexer(stream);
            lexer.AddErrorListener(new MyErrorListenner());


            var tokens = new CommonTokenStream(lexer);
            var parser = new DependenciesGrammarParser(tokens);
            parser.AddErrorListener(new MyErrorListenner());

            var tree = parser.compileUnit();
            LanguageVisitor visitor = new LanguageVisitor();
            DependenciesQuery result = visitor.Visit(tree) as DependenciesQuery;
        }
    }
}
