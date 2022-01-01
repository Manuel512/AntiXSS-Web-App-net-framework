using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using XSSWebApp.helpers;
using XSSWebApp.Models;

namespace XSSWebApp.filters
{
    public class XSSProtectionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionParameters != null)
            {
                var newActionParameters = new Dictionary<string, object>();
                foreach (var parameter in actionContext.ActionParameters)
                {
                    try
                    {
                        if (XSSHelper.CheckXSSInProperties(
                            objectValue: parameter.Value,
                            updateValueAction: (object newValue) => newActionParameters[parameter.Key] = newValue,
                            actionIfExistsXSS: () => ReturnBadRequest(actionContext)))
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                foreach (var item in newActionParameters)
                {
                    actionContext.ActionParameters[item.Key] = item.Value;
                }
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(ActionExecutedContext actionContext)
        {
            var viewResult = actionContext.Result as ViewResultBase;
            var jsonResult = actionContext.Result as JsonResult;
            var redirectResult = actionContext.Result as RedirectResult;

            if (redirectResult != null)
            {
                string sanitizedUrl = XSSHelper.Sanitize(redirectResult.Url);
                actionContext.Result = new RedirectResult(sanitizedUrl, redirectResult.Permanent);
            }
            else
            {
                var modelData = viewResult?.ViewData?.Model ?? jsonResult?.Data;
                var viewData = viewResult?.ViewData;

                XSSHelper.CheckXSSInProperties(
                    objectValue: modelData,
                    updateValueAction: (object newValue) => UpdateResultOnResponse(viewResult, jsonResult, newValue),
                    onlySanitize: true);

                if (viewData != null && viewData.Count > 0)
                {
                    var newViewData = new Dictionary<string, object>();
                    foreach (var item in viewData)
                    {
                        XSSHelper.CheckXSSInProperties(
                            objectValue: item.Value,
                            updateValueAction: (object newValue) => newViewData[item.Key] = newValue,
                            onlySanitize: true);
                    }

                    foreach (var item in newViewData)
                    {
                        viewResult.ViewData[item.Key] = item.Value;
                    }
                }
            }

            base.OnActionExecuted(actionContext);
        }

        private void UpdateResultOnResponse(ViewResultBase viewResult, JsonResult jsonResult, object newValue)
        {
            if (viewResult != null)
                viewResult.ViewData.Model = newValue;
            else if (jsonResult != null)
                jsonResult.Data = newValue;
        }

        private void ReturnBadRequest(ActionExecutingContext actionContext)
        {
            actionContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There are characters not allowed on the payload");
        }
    }
}