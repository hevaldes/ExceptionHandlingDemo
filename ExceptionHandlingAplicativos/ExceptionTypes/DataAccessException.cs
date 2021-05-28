using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ExceptionHandlingAplicativos.ExceptionTypes
{
    public class DataAccessException : BaseException, ISerializable
    {
        public DataAccessException()
           : base()
        {
            // Agregar implementación si fuera requerida
        }

        public DataAccessException(string message)
           : base(message)
        {
            // Add implemenation (if required)
        }

        public DataAccessException(string message, System.Exception inner)
           : base(message, inner)
        {
            // Agregar implementación si fuera requerida
        }

        protected DataAccessException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            // Agregar implementación si fuera requerida
        }
    }
}
