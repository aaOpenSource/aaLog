using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace aaLogReader
{
    [Serializable]
    public class aaLogReaderException : Exception
    {

        // Sourced from http://www.codeproject.com/Tips/90646/Custom-exceptions-in-C-NET
        public aaLogReaderException()
            : base() { }

        public aaLogReaderException(string message)
            : base(message) { }

        public aaLogReaderException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public aaLogReaderException(string message, Exception innerException)
            : base(message, innerException) { }

        public aaLogReaderException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected aaLogReaderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
