using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSocket.Config
{
    public class ChannelOptions
    {
        // 4k by default
        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        // 4k by default
        public int SendBufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// in milliseconds
        /// </summary>
        /// <value></value>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// in milliseconds
        /// </summary>
        /// <value></value>
        public int SendTimeout { get; set; }
        public int SendingQueueSize { get; set; }
    }
}
