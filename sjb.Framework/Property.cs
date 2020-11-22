
using System;
using System.Linq.Expressions;
using System.Reflection;
//using Prism.Properties;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Globalization;

namespace sjb.Framework
{
    ///<summary>
    /// Provides support for extracting property information based on a property expression.
    ///</summary>
    public static class Property
    {
        /// <summary>
        /// Returns a string representing the path to the expression from the source.
        /// Example: source.PropertyPath( (s) => s.obj1.obj2.obj3) returns "obj1.obj2.obj3".
        /// The expression must have this form, where the body is just a nonempty series of property
        /// accesses rooted at the parameter ("s" in this case). 
        /// I got an idea of how to do this from the following article by n.podbielski:
        /// http://www.codeproject.com/Articles/733296/Expression-Parsing-and-Nested-Properties 
        /// and with a bit of help from Jackson Dunstan: http://jacksondunstan.com/articles/3199
        /// What you see below is quite different from that, however. 
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string Path<TObj, TResult>(Expression<Func<TObj, TResult>> expression)
        {
            LambdaExpression lambda = expression as LambdaExpression; 
            if (lambda == null) { throw new ArgumentException("expression is not a lambda"); }
            var parameters = lambda.Parameters;
            if (parameters.Count != 1) { throw new ArgumentException("expression parameter count not equal to 1"); }
            string pathString = null;
            var pathParameter = lambda.Body as ParameterExpression; // Parameter, a
            var pathExpression = lambda.Body as MemberExpression;   // Member access, a.b
            while (pathExpression != null) {
                pathString = (pathString == null) ? pathExpression.Member.Name: pathExpression.Member.Name + "." + pathString;
                pathParameter = pathExpression.Expression as ParameterExpression;
                pathExpression = pathExpression.Expression as MemberExpression;
            }
            if (pathParameter == null || pathParameter.Name != parameters[0].Name) { throw new ArgumentException("expression is not rooted at the lambda parameter."); }
            if (pathString == null) { throw new ArgumentException("expression does not end with a member access."); }
            return pathString; 
        }

    }
}

