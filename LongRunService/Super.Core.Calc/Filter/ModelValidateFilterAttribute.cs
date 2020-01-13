using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Super.Core.Calc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Super.Core.Calc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ModelValidateFilterAttribute : ActionFilterAttribute
    {
        readonly ILoggerFactory _loggerfactory;
        public ModelValidateFilterAttribute(ILoggerFactory loggerfactory)
        {
            this._loggerfactory = loggerfactory;
        }


        public IDictionary<string, object> RequestArguments { get; private set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var res = new ModelValidateFailResponse(context.ModelState);
                WriteErrorToLog(context, res.message);
                context.Result = new BadRequestObjectResult(res);
                return;
            }

            this.RequestArguments = context.ActionArguments;

            // do something before the action executes
            var resultContext = await next();
            // do something after the action executes; resultContext.Result will be set
        }


        private void WriteErrorToLog(ActionExecutingContext context, string errMsg)
        {
            var msg = new ApiErrorLogModel();
            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            msg.ControllerName = action.ControllerName;
            msg.ActionName = action.ActionName;
            msg.HttpMethod = context.HttpContext.Request.Method;
            msg.Path = context.HttpContext.Request.Path;
            msg.User = context.HttpContext.User.Identity.Name;
            msg.RequestJson = JsonConvert.SerializeObject(context.ActionArguments);
            msg.Msg = errMsg;
            var logger = _loggerfactory.CreateLogger(context.Controller.ToString());
            logger.LogError(msg.ToString());
        }

    }

    public class ModelValidateFailResponse : ErrorResponseModel
    {
        readonly ModelStateDictionary _modelState;
        public ModelValidateFailResponse(ModelStateDictionary modelState)
        {
            this._modelState = modelState;
            this.code = (int)HttpStatusCode.BadRequest;
            GenErrorMsg();
        }

        private void GenErrorMsg()
        {
            if (!string.IsNullOrEmpty(this.message))
                return;

            if (this._modelState.ErrorCount < 1)
                return;

            List<string> lstMsg = new List<string>();
            List<object> lstdetail = new List<object>();
            foreach (var item in this._modelState)
            {

                if (item.Value.ValidationState != ModelValidationState.Invalid)
                    continue;

                if (item.Value.Errors.Count < 1)
                    continue;

                string fieldName = string.IsNullOrEmpty(item.Key) ? "model" : item.Key;

                string msg = $"{fieldName}:{item.Value.Errors[0].ErrorMessage}";

                lstMsg.Add(msg);

                lstdetail.Add(new { field = fieldName, errorMessage = item.Value.Errors[0].ErrorMessage, value = item.Value.RawValue });
            }

            this.message = string.Join(';', lstMsg);
            this.detail = lstdetail;
        }
    }
}
