# Exception Handling / Telemetry / Application Insights / NuGet Packages 

## Introducción

El adecuado manejo de excepciones en una aplicación es muy importante. Se trata entonces de registrar la excepción relacionada al problema ocurrido en el lugar exacto. 

Algunas veces al intentar manejar las excepciones terminamos registrando la misma excepción mas de una vez, resultando en logs de excepciones difíciles de leer.

El objetivo de este artículo es mostrar una estrategia de manejo de excepciones basada en políticas además del registro de esas excepciones en un componente de _Application Insights_
---

## Implementación de la solución

### Creación del componente de _Application Insights_

#### ¿Qué es Application Insights?

Application Insights es una característica de Azure Monitor que es un servicio de Application Performance Management (APM) extensible para desarrolladores y profesionales de DevOps. Úselo para supervisar las aplicaciones en directo. Detectará automáticamente anomalías en el rendimiento e incluye eficaces herramientas de análisis que le ayudan a diagnosticar problemas y a saber lo que hacen realmente los usuarios con la aplicación. Está diseñado para ayudarle a mejorar continuamente el rendimiento y la facilidad de uso. Funciona con diversas aplicaciones y en una amplia variedad de plataformas, como .NET, Node.js, Java y Python, hospedadas en el entorno local, de forma híbrida o en cualquier nube pública. Se integra con el proceso de DevOps y tiene puntos de conexión a numerosas herramientas de desarrollo. Puede supervisar y analizar la telemetría de aplicaciones móviles mediante la integración con Visual Studio App Center.

¿Cómo funciona Application Insights?
Instale un paquete de instrumentación pequeño (SDK) en la aplicación o habilite Application Insights mediante el agente de Application Insights cuando se admita. La instrumentación supervisa la aplicación y dirige los datos de telemetría a un recurso de Azure Application Insights mediante un GUID único al que se hace referencia como una clave de instrumentación.

No solo puede instrumentar la aplicación de servicio web, sino también todos los componentes en segundo plano y JavaScript en las propias páginas web. La aplicación y sus componentes se pueden ejecutar en cualquier lugar; no tienen que estar hospedados en Azure.

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/ApplicationInsightsDiagram.png "Microsoft Applicaition Insights")

[Referencia Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview#how-does-application-insights-work "Referencia Application Insights")

#### Creación de recurso de Application Insghts

Lo primero que haremos será crear un recurso de _Microsoft Application Insights_ para registrar la telemetría de nuestra aplicación, en otras palabas las excepciones. 

Para esto, se recomienda seguir el siguiente artículo en Microsoft Docs

1. [Sign in to Microsoft Azure](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource#sign-in-to-microsoft-azure "Sign in to Microsoft Azure")
2. [Create an Application Insights resource](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource#sign-in-to-microsoft-azure "Create an Application Insights resource")
3. [Copy the instrumentation key](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource#copy-the-instrumentation-key "Copy the instrumentation key")

El resultado debería ser algo similar a la siguiente imágen:

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/AppIns.PNG "Application Insights")

#### Creación del _Feed_ en _Artifacts_ de _Azure DevOps_

1. En el portal de _Azure DevOps_ dar clic en la sección de _Artifacts_ dentro de algún proyecto. 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/AzDOProject.PNG "Azure DevOps")

2. Dar clic en _Create Feed_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/ArtifactsCreateFeed.PNG "Azure DevOps")

3. Dar un nombre a este nuevo _Feed_. El nombre puede ser _FeedDemo_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/CreateFeedDiagBox.PNG "Azure DevOps")

4. El resultado será: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/FeedCreado.PNG "Azure DevOps")

5. Presionar el botón _Connect to Feed_
6. Seleccionar _dotnet_
7. Mantener esta página abierta ya que se requerirá mas adelante. 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/FeedConnect.PNG "Azure DevOps")

#### Creación del componente de telemetría de Application Insights

En este caso, esta implementación refiere a crear un paquete NuGet. Este paquete permite manejar de una manera mas eficiente las dependencias de los proyectos. Estos pasos pueden ser replicados para otros proyectos.

1. Crearemos una solución en blanco en Visual Studio llamada _slnDemoExceptionHandling_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/Solution.gif "Visual Studio Solution")

2. A esta solución agregaremos un proyecto de tipo _Class Library / .NET Standard_ que le llamaremos _TelemetriaAplicativos_

* Tipo de Proyecto: .NET Standard 2.0

3. Agregar al proyecto un archivo llamado _nuget.config_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetConfig.PNG "Visual Studio Solution")

4. Del portal de _Azure DevOps_ copiar el _XML_ en el archivo creado. 

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="FeedDemo" value="https://pkgs.dev.azure.com/StdrPocAzDO/ExceptionHandling/_packaging/FeedDemo/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Quedando de la siguiente forma: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetConfig2.PNG "Visual Studio Solution")

5. Es tiempo de complementar el componente de telemetría. 

* Agregue una interfaz pública llamada _ITelemetria.cs_ y agregar el siguiente código. Tendrá 2 eventos. Uno para eventos personalizados y otro para Excepciones. En ambos casos se podrán agregar porpiedades extra denominadas _Custom Dimensions_.

```
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
```

6. Elimine el archivo _Class1.cs_
7. Agregue una clase pública llamada _Telemetria.cs_ 
8. Implemente la clase _Telemetria.cs_ como sigue: 

```
/// <summary>
    /// Clase de Telemetría que implementa la interfaz ITelemetría
    /// </summary>
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
```

9. Resuelva la dependencia de la línea donde está el objeto _TelemetryClient_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/TelemetryClient.PNG "Visual Studio Solution")

10. Agregue la siguiente referencia al inicio de su archivo _Telemetria.cs_

```
using Microsoft.ApplicationInsights.Extensibility;
```

11. Clic derecho al proyecto, seleccionar _Properties_
12. En la sección de _Package_ seleccionar _Generate NuGet package in build_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetProperties.PNG "Visual Studio Solution")

13. Compilar el programa, debe hacerse sin errores. Debe generar un archivo llamado _TelemetriaAplicativos.1.0.0.nupkg_ 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/PaqueteNuGet.PNG "Visual Studio Solution")

---

#### Publicación del componente de telemetría de Application Insights en Azure DevOps Artifacts

1. Desde una ventana de comando, ejecutar el siguiente comando referenciando al paquete llamado _TelemetriaAplicativos.1.0.0.nupkg_ 

Los parámetros solicitados son: 

* -Source: Se obtiene del portal de Azure DevOps o bien desde el archivo nuget.config, en este caso es: _StdrPocAzDO_

```
nuget push -Source FeedDemo -ApiKey az [_PackagePath_]
```
![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPush.PNG "Azure DevOps Artifacts")

2. El resultado de esta ejecución será: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPushResult.PNG "Azure DevOps Artifacts")

3. Al regresar al portal de Azure DevOps a la sección de _Artifacts_, ya se podrá ver el paquete agregado.  

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPublished.PNG "Azure DevOps Artifacts")