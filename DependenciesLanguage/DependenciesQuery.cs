using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DependenciesLanguage
{
    abstract class DependencyExpression
    {

    }

    abstract class ProjectDependency : DependencyExpression
    {
        public abstract bool IsPathValid(string path);
        public abstract bool IsProjectValid(string project);
    }

    class AnythingProjectDependency : ProjectDependency
    {
        public override bool IsPathValid(string path) => true;
        public override bool IsProjectValid(string project) => true;
    }

    interface INameProjectValidator
    {
        bool IsValid(string value);
    }

    interface IPathProjectValidator
    {
        bool IsValid(string value);
    }

    class AnyNameProjectValidator : DependencyExpression, INameProjectValidator
    {
        public bool IsValid(string value) => true;
    }

    class AnyPathProjectValidator : DependencyExpression, IPathProjectValidator
    {
        public bool IsValid(string value) => true;
    }

    class QueryValidator : DependencyExpression
    {
        private readonly Regex _regex;

        public QueryValidator(string query)
        {
            _regex = ParserUtils.ConvertQueryToRegex(query);
        }

        public virtual bool IsValid(string value) => _regex.IsMatch(value);
    }

    class NameProjectValidator : QueryValidator, INameProjectValidator
    {
        public NameProjectValidator(string regex) : base(regex) { }
    }

    class PathProjectValidator : QueryValidator, IPathProjectValidator
    {
        public PathProjectValidator(string regex) : base(regex) { }
    }

    class OnlyProjectDependency : ProjectDependency
    {
        private readonly INameProjectValidator _projectValidator;

        public OnlyProjectDependency(INameProjectValidator projectValidator)
        {
            _projectValidator = projectValidator ?? throw new ArgumentNullException(nameof(projectValidator));
        }

        public override bool IsPathValid(string project) => true;
        public override bool IsProjectValid(string project) => _projectValidator.IsValid(project);
    }

    class ComposedProjectDependency : ProjectDependency
    {
        private readonly INameProjectValidator _projectValidator;
        private readonly IPathProjectValidator _pathValidator;

        public ComposedProjectDependency(INameProjectValidator projectValidator, IPathProjectValidator pathValidator)
        {
            _projectValidator = projectValidator ?? throw new ArgumentNullException(nameof(projectValidator));
            _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
        }

        public override bool IsPathValid(string path) => _pathValidator.IsValid(path);
        public override bool IsProjectValid(string project) => _projectValidator.IsValid(project);
    }

    class DependenciesQuery : DependencyExpression
    {
        public ProjectDependency User { get; }
        public ProjectDependency Dependency { get; }

        public DependenciesQuery(ProjectDependency user, ProjectDependency dependency)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        }
    }


    static class ParserUtils
    {
        static public Regex ConvertQueryToRegex(string query)
        {
            string safeQuery = Regex.Escape(query);
            string regex = safeQuery.Replace(@"\*", @"\w*");
            return new Regex($@"^{regex}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
