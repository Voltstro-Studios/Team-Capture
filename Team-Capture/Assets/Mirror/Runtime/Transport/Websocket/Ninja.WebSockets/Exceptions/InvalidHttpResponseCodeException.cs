using System;

namespace Ninja.WebSockets.Exceptions
{
    [Serializable]
    public class InvalidHttpResponseCodeException : Exception
    {
        public InvalidHttpResponseCodeException()
        {
        }

        public InvalidHttpResponseCodeException(string message) : base(message)
        {
        }

        public InvalidHttpResponseCodeException(string responseCode, string responseDetails, string responseHeader) :
            base(responseCode)
        {
            ResponseCode = responseCode;
            ResponseDetails = responseDetails;
            ResponseHeader = responseHeader;
        }

        public InvalidHttpResponseCodeException(string message, Exception inner) : base(message, inner)
        {
        }

        public string ResponseCode { get; }

        public string ResponseHeader { get; }

        public string ResponseDetails { get; }
    }
}