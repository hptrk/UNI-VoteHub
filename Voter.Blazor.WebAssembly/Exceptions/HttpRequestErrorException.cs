namespace Voter.Blazor.WebAssembly.Exceptions
{
    public class HttpRequestErrorException : Exception
    {
        public int StatusCode { get; }

        public HttpRequestErrorException(string message) : base(message)
        {
        }

        public HttpRequestErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HttpRequestErrorException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpRequestErrorException(int statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
