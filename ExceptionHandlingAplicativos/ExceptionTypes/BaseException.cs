using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ExceptionHandlingAplicativos.ExceptionTypes
{
    public class BaseException : System.Exception, ISerializable
    {
        public BaseException()
           : base()
        {
            // Agregar implementación si fuera requerida
        }

        public BaseException(string message)
           : base(message)
        {
            // Agregar implementación si fuera requerida
        }

        public BaseException(string message, System.Exception inner)
           : base(message, inner)
        {
            // Agregar implementación si fuera requerida
        }

        protected BaseException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            // Agregar implementación si fuera requerida
        }
    }
}
