using System;

namespace Ninja.WebSockets.Exceptions
{
    [Serializable]
    public class SecWebSocketKeyMissingException : Exception
    {
        public SecWebSocketKeyMissingException()
        {
        }

        public SecWebSocketKeyMissingException(string message) : base(message)
        {
        }

        public SecWebSocketKeyMissingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}