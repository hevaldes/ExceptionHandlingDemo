using ExceptionHandlingAplicativos.ExceptionTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using TelemetriaAplicativos;

namespace ExceptionHandlingAplicativos.ExceptionHandlers
{
    public static class DataAccessExceptionHandler
    {
        public static bool HandleException(ref Exception ex, Dictionary<string, string> customDimensions)
        {
            //Inicializa el cliente de Telemetría de Application Insights. Nos referermos al NuGet de TelemetriaAplicativos
            Telemetria t = new Telemetria(ConfigurationManager.AppSettings["InstrumentationKey"]);

            //Esta variable nos servirá para indicar si propagamos la excepción hacia las capas superiores.
            bool rethrow;

            //Este es el método principal de registro de telemetría. 
            rethrow = t.RegistraExcepcion(ex, customDimensions, "DataAccessPolicy");

            //En este punto la excepción ya fué registrada. 
            //Podemos esconder el error real y enmascarar con una nueva excepción cuidando así, la información mostrada en capas superiores
            //La nueva excepción es de tipo DataAccessException
            ex = new DataAccessException("DAL: Error de sistema, intente mas tarde.");

            //Entonces, si se manejó la excepción, independientemente de la política, la excepción se propaga.
            //En este caso es una excepción nueva sin información sensible de la base de datos. 
            if (rethrow)
                throw ex;

            return rethrow;
        }
    }
}
