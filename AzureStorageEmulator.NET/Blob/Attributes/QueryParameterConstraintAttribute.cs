using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace AzureStorageEmulator.NET.Blob.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class QueryParameterConstraintAttribute : Attribute, IActionConstraint
    {
        private readonly string _parameterName;

        public QueryParameterConstraintAttribute(string parameterName)
        {
            this._parameterName = parameterName;
        }

        public bool Accept(ActionConstraintContext context)
        {
            return context.RouteContext.HttpContext.Request.Query.Keys.Contains(_parameterName.ToLowerInvariant());
        }

        public int Order { get; }
    }
}
