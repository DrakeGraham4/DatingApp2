using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net;
using API.Errors;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        public RequestDelegate _next;
        public ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }
        //We have access to HttpContext because this is happening in the context of a http request
        public async Task InvokeAsync(HttpContext context)
        {
            //NOTE Passing in HttpContext
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                //NOTE Write out exception to response
                context.Response.ContentType = "application/json";
                //500
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                //Creating Response
                //Checking environment, are we running in development mode?
                var response = _env.IsDevelopment()
                //NOTE Ternary operator to say what we are going to do if this is our development mode
                //Creating new exception using our own Api Exception Class from Api.Errors
                //In case StackTrace is null, add ? as to not cause an exception within your exception
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    //When not in development mode, no details, just show string
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                //Creating options to send back in JSON, by default in camelcase
                //Ensures that our response goes back in normal Json formatted response
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };


                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}