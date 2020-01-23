using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Trail365
{
    public static class ActionResultExtension
    {
        public static Task<T> ToModel<T>(this Task<Microsoft.AspNetCore.Mvc.IActionResult> result) where T : class
        {
            return Task.FromResult(ToModel<T>(result.GetAwaiter().GetResult()));
        }

        public static Task<T> ToModel<T>(this Task<Microsoft.AspNetCore.Mvc.ActionResult<T>> result) where T : class
        {
            var resolved = result.GetAwaiter().GetResult();
            return Task.FromResult(ToModel<T>(resolved));
        }

        public static T ToModel<T>(this Microsoft.AspNetCore.Mvc.ActionResult<T> result) where T : class
        {
            IConvertToActionResult converter = result;
            return ToModel<T>(converter.Convert());
        }

        public static T ToModel<T>(this Microsoft.AspNetCore.Mvc.IActionResult result) where T : class
        {
            return ToModel<T>(result, out var _);
        }

        private static T ToModel<T>(this Microsoft.AspNetCore.Mvc.IActionResult result, out IDictionary<string, object> dict) where T : class
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            dict = null;
            if (result is CreatedAtActionResult car)
            {
                if (car.Value is T x)
                {
                    return x;
                }
                throw new InvalidOperationException("Model not found");
            }

            if (result is PartialViewResult pvr)
            {
                dict = pvr.ViewData;
                if (pvr.Model == null) throw new InvalidOperationException("Model is null on current PartialViewResult");
                if (pvr.Model is T x)
                {
                    return x;
                }
                throw new InvalidOperationException("Model not found");
            }

            if (result is ViewResult vr)
            {
                dict = vr.ViewData;
                if (vr.Model == null) throw new InvalidOperationException("Model is null on current ViewResult");
                if (vr.Model is T x)
                {
                    return x;
                }
                throw new InvalidOperationException("Model not found");
            }

            if (result is ObjectResult or1)
            {
                if (or1.Value is T x)
                {
                    return x;
                }
                throw new InvalidOperationException("Model not found");
            }
            throw new NotSupportedException("To-Model not supported for " + result.GetType().ToString());
        }
    }
}
