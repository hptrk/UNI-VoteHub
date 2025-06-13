using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Voter.DataAccess.Exceptions;

namespace Voter.WebAPI.Infrastructure
{
    public class ExceptionToProblemDetailsHandler(
        ILogger<ExceptionToProblemDetailsHandler> logger,
        IHostEnvironment environment) : IExceptionHandler
    {
        private readonly ILogger<ExceptionToProblemDetailsHandler> _logger = logger;
        private readonly IHostEnvironment _environment = environment;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            ProblemDetails problemDetails = new()
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case EntityNotFoundException ex:
                    problemDetails.Title = "Entity not found";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Detail = ex.Message;
                    break;

                case SaveFailedException ex:
                    problemDetails.Title = "Failed to save changes";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = ex.Message;
                    break;

                case VoteAlreadyCastException ex:
                    problemDetails.Title = "Vote already cast";
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    problemDetails.Detail = ex.Message;
                    break;

                case PollNotActiveException ex:
                    problemDetails.Title = "Poll not active";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;

                case ArgumentException ex:
                    problemDetails.Title = "Invalid argument";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;

                default:
                    problemDetails.Title = "An unexpected error occurred";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = _environment.IsDevelopment()
                        ? exception.ToString()
                        : "An unexpected error occurred";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
