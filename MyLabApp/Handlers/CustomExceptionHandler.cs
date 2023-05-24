
﻿using Common.Exceptions;
using Common.Resources;
using Lab.Domain.Dto;

using Common.Utils.Exceptions;
using Microsoft.AspNetCore.Http.Features;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace MyLabApp.Handlers
{
    public class CustomExceptionHandler : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            HttpResponseException responseException = new HttpResponseException();

            ResponseDto response = new ResponseDto();

            if (context.Exception is DuplicatedSkillException)
            {
                responseException.Status = StatusCodes.Status406NotAcceptable;
                response.Message = context.Exception.Message;
                context.ExceptionHandled = true;
            } else if (context.Exception is SkillNotFoundException)
            {
                responseException.Status = StatusCodes.Status400BadRequest;
                response.Message = context.Exception.Message;
             }
            if (context.Exception is BusinessException)
            {
                responseException.Status = StatusCodes.Status400BadRequest;
                response.Message = context.Exception.Message;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is UnauthorizedAccessException) 
            {
                responseException.Status = StatusCodes.Status401Unauthorized;
                response.Message = "Usuario no autenticado correctamente";
                context.ExceptionHandled = true;
            }
            else
            {
                responseException.Status = StatusCodes.Status500InternalServerError;
                response.Result = JsonConvert.SerializeObject(context.Exception);
                response.Message = GeneralMessages.Error500;
                context.ExceptionHandled = true;
            }


            context.Result = new ObjectResult(responseException.Value)
            {
                StatusCode = responseException.Status,
                Value = responseException.Value
            };

            if (responseException.Status == StatusCodes.Status500InternalServerError)
            {
                context.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = GeneralMessages.Error500;
            }
        }
    }
}
