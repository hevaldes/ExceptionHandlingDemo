# Exception Handling / Telemetry / Application Insights / NuGet Packages 

## Introducción

El adecuado manejo de excepciones en una aplicación es muy importante. Se trata entonces de registrar la excepción relacionada al problema ocurrido en el lugar exacto. 

Algunas veces al intentar manejar las excepciones terminamos registrando la misma excepción mas de una vez, resultando en logs de excepciones difíciles de leer. 

El objetivo de este artículo es mostrar una estrategia de manejo de excepciones basada en políticas además del registro de esas excepciones en un componente de _Application Insights_

---

## Implementación de la solución

### Implementación del componente de _Application Insights_

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

4. Presionar el botón _Connect to Feed_
5. Seleccionar _dotnet_
6. Mantener esta página abierta ya que se requerirá mas adelante. 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/FeedConnect.PNG "Azure DevOps")

#### Creación del componente de telemetría de Application Insights

En este caso, esta implementación refiere a crear un paquete NuGet. Este paquete permite manejar de una manera mas eficiente las dependencias de los proyectos. Estos pasos pueden ser replicados para otros proyectos.

1. Crearemos una solución en blanco en Visual Studio llamada _slnDemoExceptionHandling_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/Solution.PNG "Visual Studio Solution")

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
Nota: El método RegistraExcepcion registra el error en _Application Insights_ solamente cuando la política es diferente a _PassThroughPolicy_. El hecho de solicitar el manejo de una excepción bajo la política _PassThroughPolicy_ solo significa que se dará por "procesada" la excepción sin registrarla, esto debido a que pudo ser registrada antes.  

9. Resuelva la dependencia de la línea donde está el objeto _TelemetryClient_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/TelemetryClient.png "Visual Studio Solution")

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
nuget push -Source FeedDemo -ApiKey az .\TelemetriaAplicativos.1.0.0.nupkg
```
![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPush.PNG "Azure DevOps Artifacts")

2. El resultado de esta ejecución será: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPushResult.PNG "Azure DevOps Artifacts")

3. Al regresar al portal de Azure DevOps a la sección de _Artifacts_, ya se podrá ver el paquete agregado.  

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPublished.PNG "Azure DevOps Artifacts")

#### Resumen 

* Se ha creado un componente para almacenar la telemetría de la aplicación. Esta telemetría pueden ser eventos personalizados y bien errores. 
* Este componente al ser un paquete puede ser referenciado por cualquier otro proyecto dentro de la organización. 
* El paquete está listo, mas adelante cuando desarrollemos el cliente de prueba haremos la configuración final.

---

### Implementación del componente de _Exception Handling_

Crearemos ahora el componente para manejo de excepciones basado en políticas. 

Este tipo de estrategia estará utilizando el concepto de una política para registrar o no la excepción. Veremos en la implementación que si queremos registrar lo ocurrido en la capa de datos por ejemplo, registraremos la excepción normalmente, pero el flujo de ejecución al regresar a la capa superior se evaluará la política aplicada previamente y de ameritarlo, registrará la excepción o de lo contrario solo se propagará. 

#### Configuración de Visual Studio para leer NuGet publicados. 

1. Ir al proyecto de _Azure DevOps_
2. Seleccionar _Artifacts_
3. Seleccionar el _Feed_ que hemos creado llamado _FeedDemo_
4. Seleccionar en la parte superior la opción _Connect to feed_
5. Seleccionar Visual Studio

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/FeedConnect2.PNG "Visual Studio Solution")

6. Con estos datos configurar Visual Studio: 

* _Name: FeedDemo_
* _Source: https://pkgs.dev.azure.com/StdrPocAzDO/ExceptionHandling/_packaging/FeedDemo/nuget/v3/index.json_

7. En Visual Studio, ir al menu _Tools_
8. Seleccionar: _NuGet Package Manager_
9. Seleccionar: _Package Manager Settings_
10. En la ventana de opciones que aparece, seleccionar _Package  Sources_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/PackageSources1.PNG "Visual Studio Solution")

11. En la parte superior derecha, presionar el botón ![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/Mas.PNG "Visual Studio Solution")
12. Asignar los valores como son solicitados. Ver punto #6 de esta sección.

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/PackageSources2.PNG "Visual Studio Solution")

13. Presionar _Update_
14. Presionar _OK_

#### Configuración del componente de _Exception Handling_

1. A la solución agregaremos un nuevo proyecto de tipo _Class Library / .NET Standard_ que le llamaremos _ExceptionHandlingAplicativos_

* Tipo de Proyecto: .NET Standard 2.0

2. Eliminamos el archivo _Class1.cs_
3. Al proyecto agregamos un nuevo foder llamado: ExceptionHandlers
4. Agregamos un folder mas llamado: ExceptionTypes
5. Al proyecto agregamos un archivo llamado _nuget.config_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/EstructuraProyectoEx.PNG "Visual Studio Solution")

6. Copiar el contenido del archivo nuget.config del proyecto de _TelemetriaAplicativos_ en el archivo nuget.config del proyecto de _ExceptionHandlingAplicativos_. Utilizaremos el mismo _Feed_.

7. Agregar la referencia al NuGet de TelemetriaAplicativos. Esto puede ser realizando lo siguiente: 
* Clic derecho el proyecto _ExceptionHandlingAplicativos_
* Seleccionar _Manage NuGet Packages..._

8. Seleccionar en la parte superior izquierda: _Browse_
9. Seleccionar en la parte superior derecha en _Package Source_: _FeedDemo_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/PackageSources3.PNG "Visual Studio Solution")

10. Seleccionar el paquete de TelemetriaAplicativos
11. Clic en _Install_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/InstallTelemetry.PNG "Visual Studio Solution")

12. Una vez instalado, aparecerá en los paquetes referenciados. 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/Referencias.PNG "Visual Studio Solution")

#### Implementación del componente de _Exception Handling_

Ya que tenemos configurado el proyecto, procederemos a la implementación. Esta implementación estará dividida en 2 partes: 
* _ExceptionTypes_
* _ExceptionHandlers_

 Esto puede variar de implementación a implementación, solo úsese como una guía. 

##### ExceptionTypes

En esta sección crearemos nuestras excepciones personalizadas las cuales nos darán un mejor manejo de cualquier error que pudiese ocurrir. 

1. Iniciamos creando una clase llamada _BaseException.cs_. Esta será la excepción base del resto. 

```
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
```

 2. Ahora crearemos una excepción para cuando ocurran errores en la capa de negocio. Esta excepción heredará de _BaseException_. Este archivo se llamará _BusinessLogicException.cs_

```
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
```

3. Ahora crearemos una excepción para cuando ocurran errores en la capa de datos. Esta excepción heredará de _BaseException_. Este archivo se llamará _DataAccessException.cs_

```
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
```

3. Finalmente crearemos una excepción para cuando no se necesite registrar la excepción, es decir que solo se propague. Esta excepción heredará de _BaseException_. Este archivo se llamará _PassThroughException.cs_

```
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
```

4. Agregue a las 4 clases creadas la siguiente referencia
```
using System.Runtime.Serialization;
```

5. Compilar el proyecto, no debería marcar ningún error quedando de la siguiente forma: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/ExTypes.PNG "Visual Studio Solution")

##### ExceptionHandlers

Ahora, en esta sección crearemos los manejadores de excepciones basados en los _Exceptiontype_s que acabamos de generar. 

1. Dentro del folder _ExceptionHandlers_ agregaremos los siguientes archivos: 
* Clase llamada: _BusinessLogicExceptionHandler.cs_
* Clase llamada: _DataAccessExceptionHandler.cs_
* Clase llamada: _UserInterfaceExceptionHandler.cs_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/ExHdlr.PNG "Visual Studio Solution")

2. Implementemos el manejador de errores referente a la capa de datos _DataAccessExceptionHandler.cs_.

* En este escenario SIEMPRE registraremos la excepción ya que es la capa mas baja que tenemos. 
* Nótese como se solicita un manejo de excepción con una poítica _DataAccessPolicy_. Esto significa que que se manejará la excepción y se enviará al Application Insights. 
* Si la política hubiese sido _PassThroughPolicy_, el manejador marca como procesada la excepción, pero no la hubiera enviado a _Application Insights_

Agregar las siguientes referencias

```
using ExceptionHandlingAplicativos.ExceptionTypes;
using System.Configuration;
using TelemetriaAplicativos;
```

Implemente la clase como sigue: 

```
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
```
3. Implementemos el manejador de errores referente a la capa de negocio llamado _BusinessLogicExceptionHandler.cs_.

* En este escenario SIEMPRE registraremos la excepción que suceda en la capa de negocio, mas no la que ya fué registrada en la capa de datos. 
* Dentro del método se evalúa si la excepción viene de la base de datos, si fuera así la excepción sería: _DataAccessException_. Si fuera este caso, el código entiende que ya se ha registrado esta excepción y se manda una política de  _PassThroughPolicy_
* En caso contrario, se evalúa que la excepcíon ocurrida no viene de la base de datos, por lo que se manda una política llamada _BusinessLogicPolicy_. Esto indica que el componente de telemetría debe registrar la excepción.

Agregar las siguientes referencias

```
using ExceptionHandlingAplicativos.ExceptionTypes;
using System.Configuration;
using TelemetriaAplicativos;
```
Implemente la clase como sigue: 

```
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
```

4. Ya tenemos manejador para Datos y Negocio. Falta implementar cuando ocurra un error en la capa de presentación. 

Agregar las siguientes referencias

```
using ExceptionHandlingAplicativos.ExceptionTypes;
using System.Configuration;
using TelemetriaAplicativos;
```

Implemente la clase como sigue: 

```
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
                    //Registramos excepción bajo la política de UserInterfacePolicy
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
```

---

#### Configuración para empaquetar el componente ExceptionHandlingAplicativos 

1. Clic derecho al proyecto, seleccionar _Properties_
2. En la sección de _Package_ seleccionar _Generate NuGet package in build_
3. Compilar el programa, debe hacerse sin errores. Debe generar un archivo llamado _ExceptionHandlingAplicativos.1.0.0.nupkg_ 

#### Publicación del componente de manejo de excepcionesen Azure DevOps Artifacts

1. Desde una ventana de comando, ejecutar el siguiente comando referenciando al paquete llamado _TelemetriaAplicativos.1.0.0.nupkg_ 

Los parámetros solicitados son: 

* -Source: Se obtiene del portal de Azure DevOps o bien desde el archivo nuget.config, en este caso es: _StdrPocAzDO_

```
nuget push -Source FeedDemo -ApiKey az .\ExceptionHandlingAplicativos.1.0.0.nupkg
```

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPushEx.PNG "Azure DevOps Artifacts")

Resultando: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPushResultEx.PNG "Azure DevOps Artifacts")

2. En Azure DevOps Artifact se verá como sigue: 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/NugetPublished2.PNG "Azure DevOps Artifacts")

---

#### Resumen

* Se ha creado un componente para manejar las excepciones de la aplicación. Este componente utiliza un paquete NuGet de Telemetría.
* Este componente al ser un paquete puede ser referenciado por cualquier otro proyecto dentro de la organización. Así iniciar un manejo centralizado de excepciones. 
* El paquete está listo, mas adelante cuando desarrollemos el cliente de prueba haremos la configuración final.

Ya casi terminamos, solo falta crear un cliente para probar todo. 

---

### Implementación del Cliente de Pruebas

Esta implementación será crear un cliente que simule tener ejecución en capa de presentación, negocio y datos. Provocaremos errores en las 3 capas y veremos funcionando la política de excepciones + componente de telemetría. 


1. Agregar un proyecto nuevo a la solución _slnDemoExceptionHandling_ que se llame _TestClient_. Puede ser una aplicación de consola en .NET Core. 

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/TestClient.PNG "Azure DevOps Artifacts")

2. Dar clic derecho al proyecto _TestClient_ y agregar la referencia al paquete _NuGet_ de _ExceptionHandlingAplicativos_

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/RefException.PNG "Azure DevOps Artifacts")

3. Instalar la referencia a _ExceptionHandlingAplicativos_

4. Agregar la siguiente referencia al inicia del archivo _Program.cs_

```
using System.Collections.Generic;
using ExceptionHandlingAplicativos.ExceptionHandlers;
using System.Data.SqlClient;
using System.IO;
```

Implementar el resto de la clase _Program_ como sigue: 

```
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                //Ejecución en la capa de UI
              
                //int x = 100;
                //int z = 0;
                //cDimensions.Add("Numero A", x.ToString());
                //cDimensions.Add("Numero B", z.ToString());
                //int w = x / z;

                //Navega a la capa de negocio
                CapaBusiness();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto UI
                UserInterfaceExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Programa Terminado");
            Console.ReadLine();
        }

        static void CapaBusiness()
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                //Ejecución capa de negocio
                //string fileName = @"x:\demo.txt";
                //cDimensions.Add("Archivo buscado", fileName);
                //File.Open($"{fileName}", FileMode.Open);

                //Navega a capa de datos
                CapaData();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto de negocio
                BusinessLogicExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
        }

        static void CapaData()
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                string strCN = "myConnectionString";
                cDimensions.Add("Cadena de conexión", strCN);
                SqlConnection cn = new SqlConnection("myCadenaConexion");
                cn.Open();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto de datos
                DataAccessExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
        }
    }
```

5. Finalmente, agregue a su proyecto de pruebas un archivo llamado App.config con el contenido de la llave de Application Insights.

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<appSettings>
		<add key="InstrumentationKey" value="[INSTRUMENTATIONKEY]"/>
	</appSettings>
</configuration>
```

6. Compilar
7. Establecer el proyecto de pruebas como proyecto de inicio y provocar los diferentes errores. 
8. Al depurar se podrá ver aplicada la política de manejo de excepciones junto con la telemetría. 
9. Al finalizar la depuración simulando las 3 capas, se podrá ver en el _Application Insights_ la siguiente información.

![Image](https://github.com/hevaldes/PolicyExceptionHandling/blob/master/assets/EvidenciaAppIns.PNG "Azure DevOps Artifacts")