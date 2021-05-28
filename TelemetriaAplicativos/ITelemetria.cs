using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetriaAplicativos
{
    /// <summary>
    /// Interfaz de Telemetría de ApplicationInsights
    /// </summary>
    public interface ITelemetria
    {
        /// <summary>
        /// Permite registrar en CustomEvent junto con los Custom Dimensions en ApplicationInsights
        /// </summary>
        /// <param name="mensaje">Mensaje a registrar</param>
        /// <param name="props">Custom Dimensions, información de valor</param>
        bool RegistraEvento(string mensaje, Dictionary<string, string> props);

        /// <summary>
        /// Permite registrar una Excepción junto con los Custom Dimensions en ApplicationInsights
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="props"></param>
        bool RegistraExcepcion(Exception exception, Dictionary<string, string> customDimensions, string tipo);
    }
}
