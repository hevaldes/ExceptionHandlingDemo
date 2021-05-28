using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ExceptionHandlingAplicativos.ExceptionTypes
{
    public class BusinessLogicException : BaseException, ISerializable
    {
        public BusinessLogicException()
           : base()
        {
            // Agregar implementación si fuera requerida
        }

        public BusinessLogicException(string message)
           : base(message)
        {
            // Agregar implementación si fuera requerida
        }

        public BusinessLogicException(string message, System.Exception inner)
           : base(message, inner)
        {
            // Agregar implementación si fuera requerida
        }

        protected BusinessLogicException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            // Agregar implementación si fuera requerida
        }
    }
}
