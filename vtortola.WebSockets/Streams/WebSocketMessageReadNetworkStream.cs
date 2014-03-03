﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.WebSockets
{
    public sealed class WebSocketMessageReadNetworkStream : WebSocketMessageReadStream
    {
        readonly WebSocketClient _client;
        readonly WebSocketFrameHeader _header;
        public override WebSocketMessageType MessageType 
        { 
            get { return (WebSocketMessageType)_header.Flags.Option; } 
        }
        public override WebSocketFrameHeaderFlags Flags
        {
            get { return _header.Flags; }
        }

        public WebSocketMessageReadNetworkStream(WebSocketClient client, WebSocketFrameHeader header)
        {
            _client = client;
            _header = header;
        }

        private Int32 CheckBoundaries(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (count < buffer.Length - offset)
                throw new ArgumentException("There is not space in the array for that length considering that offset.");

            if (_client.Header == null)
                return 0;

            if (_client.Header.ContentLength < (UInt64)count)
                count = (Int32)_client.Header.ContentLength;

            if (_client.Header.RemainingBytes < (UInt64)count)
                count = (Int32)_client.Header.RemainingBytes;

            return count;
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            count = CheckBoundaries(buffer, offset, count);

            if (count == 0 || !_client.IsConnected)
                return 0;

            Int32 readed = _client.ReadInternal(buffer, offset, count);

            if (_client.Header.RemainingBytes == 0)
                _client.CleanHeader();

            return readed;
        }

        public override async Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
        {
            count = CheckBoundaries(buffer, offset, count);

            if (count == 0 || !_client.IsConnected)
                return 0;

            Int32 readed = await _client.ReadInternalAsync(buffer, offset, count, cancellationToken);

            if (_client.Header.RemainingBytes == 0)
                _client.CleanHeader();

            return readed;
        }
    }

}
