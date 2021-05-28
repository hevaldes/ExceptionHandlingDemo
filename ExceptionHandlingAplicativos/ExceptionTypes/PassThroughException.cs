using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ExceptionHandlingAplicativos.ExceptionTypes
{
    public class PassThroughException : BaseException, ISerializable
    {
        public PassThroughException()
           : base()
        {
            // Agregar implementación si fuera requerida
        }

        public PassThroughException(string message)
           : base(message)
        {
            // Agregar implementación si fuera requerida
        }

        public PassThroughException(string message, System.Exception inner)
           : base(message, inner)
        {
            // Agregar implementación si fuera requerida
        }

        protected PassThroughException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            // Agregar implementación si fuera requerida
        }
    }
}
