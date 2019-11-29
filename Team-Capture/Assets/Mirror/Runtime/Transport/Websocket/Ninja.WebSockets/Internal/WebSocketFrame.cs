using System.Net.WebSockets;

namespace Ninja.WebSockets.Internal
{
    internal class WebSocketFrame
    {
        public WebSocketFrame(bool isFinBitSet, WebSocketOpCode webSocketOpCode, int count)
        {
            IsFinBitSet = isFinBitSet;
            OpCode = webSocketOpCode;
            Count = count;
        }

        public WebSocketFrame(bool isFinBitSet, WebSocketOpCode webSocketOpCode, int count,
            WebSocketCloseStatus closeStatus, string closeStatusDescription) : this(isFinBitSet, webSocketOpCode, count)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }

        public bool IsFinBitSet { get; }

        public WebSocketOpCode OpCode { get; }

        public int Count { get; }

        public WebSocketCloseStatus? CloseStatus { get; }

        public string CloseStatusDescription { get; }
    }
}