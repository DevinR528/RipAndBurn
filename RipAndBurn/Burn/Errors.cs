using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RipAndBurn.Burn {
    [System.Serializable]
    public class BurnException : Exception {
        public BurnException() { }
        public BurnException(string message) : base(message) { }
        public BurnException(string message, Exception inner) : base(message, inner) { }
        protected BurnException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class FormatException : Exception {
        public FormatException() { }
        public FormatException(string message) : base(message) { }
        public FormatException(string message, Exception inner) : base(message, inner) { }
        protected FormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
