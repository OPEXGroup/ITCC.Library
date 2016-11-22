using System.Collections;
using System.Linq;
using System.Reflection;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Utils;

namespace ITCC.HTTP.API.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsApiView(this object maybeView)
            => maybeView.GetType().GetTypeInfo().GetCustomAttributes<ApiViewAttribute>().Any();

        public static bool IsApiViewList(this object maybeViewList)
        {
            var type = maybeViewList.GetType();
            if (type.IsConstructedGenericType && maybeViewList is IList)
            {
                var genericTypeParam = type.GenericTypeArguments[0];
                return genericTypeParam.GetTypeInfo().GetCustomAttributes<ApiViewAttribute>().Any();
            }
            return false;
        }

        public static ViewCheckResult CheckAsView(this object view)
        {
            var type = view.GetType();
            var list = view as IList;
            if (list != null && type.IsConstructedGenericType)
            {
                var checkResults = (from object item in list select CheckAsView(item)).ToList();
                if (!checkResults.Any())
                    return ViewCheckResult.Ok();

                var errorViews = checkResults
                    .Where(result => !result.IsCorrect)
                    .Select(result => result.ApiErrorView)
                    .ToList();

                return ViewCheckResult.Error(ApiErrorViewFactory.InnerErrors(view, errorViews));
            }
            return ViewChecker.CheckContract(view);
        }
    }
}
