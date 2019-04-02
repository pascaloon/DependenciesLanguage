using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using DependenciesLanguage.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesLanguage
{
    class LanguageVisitor : DependenciesGrammarBaseVisitor<DependencyExpression>
    {
        // path --> path
        public override DependencyExpression VisitDependencyExpression([NotNull] DependenciesGrammarParser.DependencyExpressionContext context)
        {
            var walker = Walker<DependenciesGrammarParser.ProjectExprContext>(context);
            var leftPath = walker(0) as ProjectDependency;
            var rightPath = walker(1) as ProjectDependency;

            if (leftPath == null || rightPath == null)
                throw new Exception("Invalid Dependency Expression.");

            return new DependenciesQuery(leftPath, rightPath);
        }



        // NAME
        //public override DependencyExpression VisitSimpleName([NotNull] DependenciesGrammarParser.SimpleNameContext context)
        //{
        //    string name = context.GetText();
        //    if (name == "*")
        //        return new AnythingProjectDependency();
        //    else
        //        return new OnlyProjectDependency(name);
        //}

        //// NAME/NAME
        //public override DependencyExpression VisitComposedName([NotNull] DependenciesGrammarParser.ComposedNameContext context)
        //{
        //    string text = context.GetText();
        //    int firstSplit = text.IndexOf('/');
        //    string name = text.Substring(0, firstSplit);
        //    string path = text.Substring(firstSplit, text.Length - firstSplit - 1);
        //    if (path == "*") // NAME/*
        //        return new OnlyProjectDependency(name);
        //    else // NAME/PATH
        //        return new FullProjectDependency(name, path);
        //}

        //public override DependencyExpression VisitPath([NotNull] DependenciesGrammarParser.PathContext context)
        //{
        //    return base.VisitPath(context);
        //}

        



        public override DependencyExpression VisitOnlyProject([NotNull] DependenciesGrammarParser.OnlyProjectContext context)
        {
            INameProjectValidator nameValidator = Goto<DependenciesGrammarParser.NameExprContext>(context, 0) as INameProjectValidator;
            return new OnlyProjectDependency(nameValidator);
        }

        public override DependencyExpression VisitComposedName([NotNull] DependenciesGrammarParser.ComposedNameContext context)
        {
            var nameValidator = Goto<DependenciesGrammarParser.NameExprContext>(context, 0) as INameProjectValidator;
            var pathValidator = Goto<DependenciesGrammarParser.PathExprContext>(context, 0) as IPathProjectValidator;

            return new ComposedProjectDependency(nameValidator, pathValidator);
        }

        public override DependencyExpression VisitPathCatchAll([NotNull] DependenciesGrammarParser.PathCatchAllContext context)
        {
            return new AnyPathProjectValidator();
        }

        public override DependencyExpression VisitFullPath([NotNull] DependenciesGrammarParser.FullPathContext context)
        {
            return new PathProjectValidator(context.GetText());
        }

        public override DependencyExpression VisitNameCatchAll([NotNull] DependenciesGrammarParser.NameCatchAllContext context)
        {
            return new AnyNameProjectValidator();
        }

        public override DependencyExpression VisitFullName([NotNull] DependenciesGrammarParser.FullNameContext context)
        {
            return new NameProjectValidator(context.GetText());
        }

        private Func<int, DependencyExpression> Walker<T>(ParserRuleContext context) where T : ParserRuleContext =>
            i => Goto<T>(context, i);

        private DependencyExpression Goto<T>(ParserRuleContext context, int i) where T : ParserRuleContext =>
            Visit(context.GetRuleContext<T>(i));
    }
}
