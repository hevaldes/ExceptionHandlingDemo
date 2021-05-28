using System;
using System.Collections.Generic;
using System.Text;
using ExceptionHandlingAplicativos.ExceptionTypes;
using System.Configuration;
using TelemetriaAplicativos;

namespace ExceptionHandlingAplicativos.ExceptionHandlers
{
    public static class BusinessLogicExceptionHandler
    {
        public static bool HandleException(ref Exception ex, Dictionary<string, string> customDimensions)
        {
            //Inicializa el cliente de Telemetría de Application Insights. Nos referermos al NuGet de TelemetriaAplicativos
            Telemetria t = new Telemetria(ConfigurationManager.AppSettings["InstrumentationKey"]);

            //Esta variable nos servirá para indicar si propagamos la excepción hacia las capas superiores.
            bool rethrow;

            //Si es una excepción ocurrida en la base de datos, es decir que venga desde esa capa en teoría ya está registrada. 
            //Por ello, mandamos una política de PassThroughPolicy
            if (ex is DataAccessException)
            {
                rethrow = t.RegistraExcepcion(ex, customDimensions, "PassThroughPolicy");
                ex = new PassThroughException(ex.Message);
            }
            //Si la excepción no ocurró en base de datos, entonces significa que ocurrió en la capa de negocio. 
            //Por ello se usa una política BusinessLogicPolicy y se enmascara el error de negocio. 
            else
            {
                rethrow = t.RegistraExcepcion(ex, customDimensions, "BusinessLogicPolicy");
                ex = new BusinessLogicException("BUS: Error de sistema, intente mas tarde.");
            }

            //Entonces, si se manejó la excepción, independientemente de la política, la excepción se propaga.
            //En este caso es una excepción nueva sin información sensible de la base de datos. 
            if (rethrow)
                throw ex;

            return rethrow;
        }
    }
}
