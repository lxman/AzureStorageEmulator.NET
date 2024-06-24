using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Blob.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class QueryStringConstraintAttribute(string qName, bool include) : ActionMethodSelectorAttribute
    {
        public string QueryStringName { get; set; } = qName;
        public bool Include { get; set; } = include;

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            routeContext.HttpContext.Request.Query.TryGetValue(QueryStringName, out StringValues value);

            if (QueryStringName == string.Empty && Include)
            {
                return true;
            }
            if (Include)
            {
                return !StringValues.IsNullOrEmpty(value);
            }

            // Exclude
            return StringValues.IsNullOrEmpty(value);
        }
    }
}