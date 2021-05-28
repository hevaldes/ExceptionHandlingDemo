using System;
using System.Collections.Generic;
using System.Text;
using ExceptionHandlingAplicativos.ExceptionTypes;
using System.Configuration;
using TelemetriaAplicativos;

namespace ExceptionHandlingAplicativos.ExceptionHandlers
{
    public static class UserInterfaceExceptionHandler
    {
        public static bool HandleException(ref Exception ex, Dictionary<string, string> customDimensions)
        {
            //Inicializa el cliente de Telemetría de Application Insights. Nos referermos al NuGet de TelemetriaAplicativos
            Telemetria t = new Telemetria(ConfigurationManager.AppSettings["InstrumentationKey"]);

            //Esta variable nos servirá para indicar si propagamos la excepción hacia las capas superiores.
            bool rethrow = false;
            try
            {
                if (ex is BaseException)
                {
                    rethrow = t.RegistraExcepcion(ex, customDimensions, "PassThroughPolicy");
                    //O bien no hacer nada debido a que la excepción fué registrada ya.
                }
                else
                {
                    //Registramos excepsión bajo la política de UserInterfacePolicy
                    rethrow = t.RegistraExcepcion(ex, customDimensions, "UserInterfacePolicy");
                }
            }
            catch
            {
                //Si falla el manejo de excepciones, estamos perdidos. :(
            }
            return rethrow;
        }
    }
}
