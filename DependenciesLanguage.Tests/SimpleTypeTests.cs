using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using DependenciesLanguage.Language;
using NUnit.Framework;

namespace DependenciesLanguage.Tests
{
    class TestsErrorListenner : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
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

    static class Utils
    {
        public static DependenciesQuery ParseQuery(string query)
        {
            AntlrInputStream stream = new AntlrInputStream(new StringReader(query));

            var lexer = new DependenciesGrammarLexer(stream);
            lexer.AddErrorListener(new TestsErrorListenner());

            var tokens = new CommonTokenStream(lexer);
            var parser = new DependenciesGrammarParser(tokens);
            parser.AddErrorListener(new TestsErrorListenner());

            var tree = parser.compileUnit();
            Assert.IsFalse(parser.ErrorHandler.InErrorRecoveryMode(parser));
            LanguageVisitor visitor = new LanguageVisitor();
            return visitor.Visit(tree) as DependenciesQuery;
        }
    }


    [TestFixture]
    public class SimpleTypeTests
    {
        [Test]
        [TestCase("a --> b", typeof(OnlyProjectDependency), typeof(OnlyProjectDependency))]
        [TestCase("* --> b", typeof(OnlyProjectDependency), typeof(OnlyProjectDependency))]
        [TestCase("a --> *", typeof(OnlyProjectDependency), typeof(OnlyProjectDependency))]
        public void TypesNameTests(string query, Type expectedUserType, Type expectedDependencyType)
        {
            DependenciesQuery parsedQuery = Utils.ParseQuery(query);
            Assert.AreEqual(expectedUserType, parsedQuery.User.GetType());
            Assert.AreEqual(expectedDependencyType, parsedQuery.Dependency.GetType());
        }
        
        [Test]
        [TestCase("allo --> bye", new string[] { "allo", "bye", "allobye", "cia"}, new bool[] { true, false, false, false }, new bool[] { false, true, false, false })]
        [TestCase("allo* --> bye", new string[] { "allo", "bye", "allobye", "cia" }, new bool[] { true, false, true, false }, new bool[] { false, true, false, false })]
        [TestCase("allo --> *bye", new string[] { "allo", "bye", "allobye", "cia" }, new bool[] { true, false, false, false }, new bool[] { false, true, true, false })]
        [TestCase("* --> *", new string[] { "allo", "bye", "allobye", "cia" }, new bool[] { true, true, true, true }, new bool[] { true, true, true, true })]
        public void ProjectNameTests(string query, string[] projects, bool[] expectedResultsLeft, bool[] expectedResultsRight)
        {
            DependenciesQuery parsedQuery = Utils.ParseQuery(query);

            for (int i = 0; i < projects.Length; i++)
            {
                Assert.AreEqual(expectedResultsLeft[i], parsedQuery.User.IsProjectValid(projects[i]));
            }

            for (int i = 0; i < projects.Length; i++)
            {
                Assert.AreEqual(expectedResultsRight[i], parsedQuery.Dependency.IsProjectValid(projects[i]));
            }
        }


        [Test]
        [TestCase("hello/bye --> salut/bonjour", "hello", "bye", "salut", "bonjour")]
        [TestCase("hello/bye --> salut/bonjour/aurevoir", "hello", "bye", "salut", "bonjour/aurevoir")]
        [TestCase("hello/* --> salut/*", "hello", "anything", "salut", "anything")]
        public void ProjectPathTests(string query, string userProject, string userPath, string dependencyProject, string dependencyPath)
        {
            DependenciesQuery parsedQuery = Utils.ParseQuery(query);
            Assert.IsTrue(parsedQuery.User.IsProjectValid(userProject));
            Assert.IsTrue(parsedQuery.User.IsPathValid(userPath));
            Assert.IsTrue(parsedQuery.Dependency.IsProjectValid(dependencyProject));
            Assert.IsTrue(parsedQuery.Dependency.IsPathValid(dependencyPath));
        }
    }
}
