using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetriaAplicativos
{
    public class Telemetria : ITelemetria
    {
        /// <summary>
        /// Habilita enviar a Azure la información de telemetría. Usar para ver inmediatamente la telemetría en el portal. Solo DEV
        /// </summary>
        public bool FlushMode { get; set; } = false;

        /// <summary>
        /// Cliente para telemetría de ApplicationInsights
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Telemetria()
        {

        }

        /// <summary>
        /// Constructor Telemetría
        /// </summary>
        /// <param name="instrumentationKey">Llave de instrumentación</param>
        public Telemetria(string instrumentationKey)
        {
            var conf = new TelemetryConfiguration
            {
                InstrumentationKey = instrumentationKey
            };
            _telemetryClient = new TelemetryClient(conf);
            _telemetryClient.InstrumentationKey = conf.InstrumentationKey;
        }

        /// <summary>
        /// Permite registrar en CustomEvent junto con los Custom Dimensions en ApplicationInsights
        /// </summary>
        /// <param name="mensaje">Mensaje a registrar</param>
        /// <param name="props">Custom Dimensions, información de valor</param>
        public bool RegistraEvento(string mensaje, Dictionary<string, string> props)
        {
            _telemetryClient.TrackEvent(mensaje, props);

            if (FlushMode)
                _telemetryClient.Flush();

            return true;
        }

        /// <summary>
        /// Permite registrar una Excepción junto con los Custom Dimensions en ApplicationInsights
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="props"></param>
        public bool RegistraExcepcion(Exception ex, Dictionary<string, string> props, string tipo)
        {
            if (tipo != "PassThroughPolicy")
            {
                _telemetryClient.TrackException(ex, props);

                if (FlushMode)
                    _telemetryClient.Flush();
            }
            return true;
        }
    }
}
