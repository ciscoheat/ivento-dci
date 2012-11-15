using System;
using System.Collections.Generic;
using System.Linq;

namespace Ivento.Dci.Abstract
{
    /// <summary>
    /// Resolves singly registered services that support arbitrary object creation.
    /// </summary>
    /// <remarks>
    /// Identical to System.Web.Mvc.IDependencyResolver
    /// http://msdn.microsoft.com/en-us/library/system.web.mvc.idependencyresolver(v=vs.98).aspx
    /// </remarks>
    public interface IDependencyResolver
    {
        object GetService(Type serviceType);
        IEnumerable<object> GetServices(Type serviceType);
    }

    public static class DependencyResolverExtensions
    {
        public static TService GetService<TService>(this IDependencyResolver resolver)
        {
            return (TService)resolver.GetService(typeof(TService));
        }

        public static IEnumerable<TService> GetServices<TService>(this IDependencyResolver resolver)
        {
            return resolver.GetServices(typeof(TService)).Cast<TService>();
        }
    }
}