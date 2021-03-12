namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System.Reflection;
    using System.Reflection.Emit;

    public static class DependenciesContainerExtensions
    {
        public static T SelfOrResolve<T>(this T obj)
        {
            if (obj != null)
            {
                return (T)obj;
            }

            return DependenciesContainer.Instance.Resolve<T>();
        }
    }
}