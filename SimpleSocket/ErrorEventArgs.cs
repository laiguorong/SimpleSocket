using System;

namespace SimpleSocket
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}
