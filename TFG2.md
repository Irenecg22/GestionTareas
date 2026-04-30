DOCUMENTO TÉCNICO DE ESTUDIO - PROYECTO GESTIÓN DE TAREAS
Versión actualizada: Junio 2025 (Backend Colaborativo tipo Trello)

VISIÓN GENERAL DEL PROYECTO

Este proyecto constituye una aplicación multiplataforma para la gestión de proyectos y tareas desarrollada como parte del Trabajo de Fin de Grado del Ciclo Formativo de Grado Superior en Desarrollo de Aplicaciones Multiplataforma. La aplicación permite a los usuarios gestionar proyectos, asignar tareas, visualizar estadísticas en tiempo real y administrar usuarios del equipo.

El sistema está construido sobre .NET MAUI (Multi-platform App UI), el framework multiplataforma moderno de Microsoft que permite ejecutar la misma base de código en Android, iOS, macOS y Windows. La aplicación actúa como cliente frontend que consume servicios REST de un backend desarrollado en FastAPI (Python).

El propósito principal es proporcionar una interfaz intuitiva y moderna para la gestión colaborativa del trabajo, donde equipos pueden coordinar proyectos compartidos con múltiples miembros, asignar roles diferenciados (owner, editor, viewer) dentro de cada proyecto, seguir el progreso de tareas mediante un tablero tipo Kanban, y obtener métricas visuales sobre el estado general del sistema. El backend aplica control de acceso basado en pertenencia a proyectos: un usuario solo ve los proyectos donde es miembro, y sus permisos dentro de cada proyecto dependen del rol asignado.

Tecnológicamente, el proyecto utiliza:
- .NET 10 como versión objetivo del framework (net10.0)
- .NET MAUI con XAML Source Generation (MauiXamlInflator) para mejor rendimiento
- CommunityToolkit.Mvvm versión 8.4.2 para la implementación del patrón MVVM
- Microsoft.Extensions.DependencyInjection versión 10.0.0 para inyección de dependencias
- HttpClient nativo para comunicación REST
- System.Text.Json para serialización de datos
- XAML para definición declarativa de interfaces con soporte AppThemeBinding para temas claro/oscuro

El proyecto está actualmente en desarrollo activo en la rama develop, hospedado en GitHub bajo los autores Jhojahn Sebastian Ramirez Marin e Irene Gomez Cañada (usuario Irenecg22).

ESTRUCTURA DEL PROYECTO

La estructura sigue una arquitectura limpia y organizada basada en el patrón MVVM, con una clara separación de responsabilidades entre las distintas capas de la aplicación.

Model: Contiene las clases de datos que representan las entidades del dominio. Incluye Usuario, Proyecto, ProyectoUsuario, Tarea, Rol y Comentario. Estas clases son Plain Old CLR Objects (POCOs) decoradas con atributos de serialización JSON para mapear correctamente con las respuestas de la API. Utilizan JsonPropertyName para especificar los nombres de los campos tal como vienen del backend en formato snake_case (por ejemplo, id_usuario, id_proyecto, creado_por_id, rol_proyecto), mientras que en el código C# se usan nombres en PascalCase. Las clases tienen propiedades de navegación como Usuario.TareasAsignadas y Proyecto.Tareas para representar relaciones. El modelo ProyectoUsuario representa la relación muchos-a-muchos entre proyectos y usuarios con roles diferenciados (owner, editor, viewer).

View: Alberga las páginas XAML y sus code-behind correspondientes. Cada vista es una ContentPage que define la interfaz gráfica de manera declarativa. Las vistas principales incluyen:
- LoginView: Pantalla de autenticación inicial con navegación a registro
- PanelPrincipal: Dashboard con estadísticas, métricas y vista general
- ProyectoView: Lista horizontal de proyectos con tarjetas visuales
- TareaView: Tablero Kanban con tres columnas (To Do, Doing, Done)
- SettingsView: Configuración de usuario y cambio de tema
- SignUpView: Formulario de registro con validación mejorada y mensajes de error específicos
- CrearProyectoView y CrearTareaView: Formularios de creación
- ProyectoDetalleView y TareaDetalleView: Vistas de detalle con información completa

Los archivos code-behind son mínimos, conteniendo principalmente la inicialización y conexión con el ViewModel mediante inyección de dependencias en el constructor. Algunos incluyen manejadores de eventos para navegación directa (OnNuevoProyectoClicked, OnNuevaTareaClicked).

ViewModel: Contiene la lógica de presentación de cada vista. Los ViewModels heredan de ObservableObject del CommunityToolkit.Mvvm y utilizan atributos como ObservableProperty para generar automáticamente el código de notificación de cambios mediante Source Generators. También hacen uso de RelayCommand y AsyncRelayCommand para exponer acciones ejecutables desde las vistas. Cada ViewModel consume uno o más servicios inyectados mediante el contenedor de dependencias.

ViewModels implementados:
- LoginViewModel: Gestiona autenticación y navegación a AppShell tras login exitoso
- SignUpViewModel: Registro con validación exhaustiva (formato email, longitud contraseña, coincidencia), mensajes de error específicos visibles en UI, y propiedades HasError/ErrorMessage para feedback visual
- PanelPrincipalViewModel: Carga datos de tres servicios simultáneamente, calcula estadísticas en tiempo real, muestra información del usuario autenticado
- ProyectoViewModel: Lista proyectos (el backend ya filtra por membresía), elimina con confirmación y manejo de permisos 403, navega a detalles
- TareaViewModel: Implementa tablero Kanban con propiedades computadas para cada columna, filtra por proyecto opcionalmente, maneja errores 403 al eliminar
- SettingsViewModel: Gestiona perfil de usuario y cambio de tema con aplicación inmediata
- CrearProyectoViewModel y CrearTareaViewModel: Implementan INotifyPropertyChanged manualmente (estilo legacy)
- ProyectoDetalleViewModel: Carga tareas relacionadas y miembros del proyecto al detectar cambio de proyecto
- TareaDetalleViewModel: Simple presentación de datos

Services: Esta capa implementa la comunicación con la API REST. Contiene:
- IRestService<T>: Interfaz genérica con métodos GetAllAsync() y DeleteAsync(int id)
- UserService: Implementa autenticación JWT completa con LoginAsync (envía credenciales a /auth/login, recibe token, lo guarda en SecureStorage), CreateAsync para registro, GetCurrentUserAsync (obtiene usuario desde endpoint /auth/me o decodificando JWT), GetTokenAsync, Logout, y métodos auxiliares como GetEmailFromToken que decodifica el payload JWT Base64. Incluye clases privadas TokenResponse y RegisterRequest para serialización.
- ProyectoService: CRUD de proyectos colaborativos con método AddAuthHeaderAsync que añade Bearer token a cada request. Incluye métodos para gestión de miembros: GetMiembrosAsync, AddMiembroAsync, UpdateMiembroRolAsync, RemoveMiembroAsync. También expone DeleteWithStatusAsync para manejo granular de errores de permisos (403/401).
- TareaService: CRUD de tareas con autenticación Bearer. Ofrece CreateWithStatusAsync y DeleteWithStatusAsync para distinguir errores de permisos.

Cada servicio encapsula la lógica de construcción de peticiones HTTP, serialización, deserialización y manejo de errores con Debug.WriteLine extensivo para troubleshooting. La clase estática ApiConfig centraliza la configuración de la URL base de la API, adaptándose según la plataforma usando directivas de compilación (10.0.2.2 para Android, 127.0.0.1 para otras).

Resources: Contiene recursos compartidos como fuentes personalizadas OpenSans (Regular y Semibold) en la carpeta Fonts, imágenes en Images (dotnet_bot.png), archivos raw en Raw, y estilos globales en Styles (Colors.xaml y Styles.xaml con soporte AppThemeBinding). También incluye el icono de la aplicación (appicon.svg con foreground appiconfg.svg) y la pantalla de splash (splash.svg con color #512BD4).

Platforms: Almacena código específico de cada plataforma. Contiene subcarpetas para Android (AndroidManifest.xml, MainActivity.cs, MainApplication.cs), iOS (AppDelegate.cs, Info.plist, Program.cs, PrivacyInfo.xcprivacy), MacCatalyst (AppDelegate.cs, Entitlements.plist, Info.plist, Program.cs) y Windows (app.manifest, App.xaml, App.xaml.cs, Package.appxmanifest). Cada plataforma tiene configuración de versión mínima: iOS/MacCatalyst 15.0, Android 21 (Lollipop), Windows 10.0.17763.0.

En la raíz del proyecto se encuentran archivos fundamentales:
- MauiProgram.cs: Punto de entrada donde se configura el contenedor de dependencias con registro detallado de servicios (UserService como Singleton con doble registro, ProyectoService y TareaService como Transient con registro de interfaces), todos los ViewModels y Views como Transient, configuración de fuentes y logging de debug
- App.xaml y App.xaml.cs: Define el objeto Application, recibe IServiceProvider por inyección y crea la ventana inicial con LoginView en un NavigationPage (patrón actualizado, no usa AppShell inicialmente)
- AppShell.xaml y AppShell.xaml.cs: Implementa la estructura de navegación Shell con cuatro pestañas principales (Dashboard, Proyectos, Tareas, Ajustes) y registro de rutas modales (tareaDetalle, proyectoDetalle, CrearProyectoView, CrearTareaView)
- GestionTareas.csproj: Archivo de proyecto que define targets multiplataforma, dependencias, configuraciones de compilación y habilita características modernas como MauiXamlInflator SourceGen y Nullable enable
- TFG2.md: Este documento técnico completo
- README.md: Documentación de usuario con instrucciones de instalación y ejecución

FLUJO GENERAL DE LA APLICACIÓN

Al iniciar la aplicación, el runtime de .NET MAUI ejecuta el método Main generado automáticamente, que a su vez invoca CreateMauiApp en la clase estática MauiProgram. Este método construye el MauiApp configurando el framework mediante el builder pattern. Primero se llama a UseMauiApp<App>() especificando la clase App como aplicación principal, luego se configuran las fuentes personalizadas con ConfigureFonts y se habilita el logging de debug con builder.Logging.AddDebug() solo en compilaciones DEBUG mediante directiva de preprocesador.

A continuación, se registran todas las dependencias en el contenedor de servicios siguiendo un orden específico:

Servicios (capa de datos):
- UserService se registra como Singleton para mantener el estado de autenticación durante toda la sesión
- Se registra doblemente: como sí mismo (UserService) y como IRestService<Usuario> delegando al mismo singleton mediante GetRequiredService
- TareaService y ProyectoService se registran como Transient (instancia nueva por resolución)
- También se registran sus interfaces IRestService<Tarea> e IRestService<Proyecto> delegando a las implementaciones concretas

ViewModels (todos como Transient para instancias frescas en cada navegación):
- LoginViewModel, SignUpViewModel
- PanelPrincipalViewModel, ProyectoViewModel, TareaViewModel
- SettingsViewModel, ProyectoDetalleViewModel, TareaDetalleViewModel
- CrearProyectoViewModel, CrearTareaViewModel

Views (todas como Transient):
- LoginView, SignUpView
- PanelPrincipal, ProyectoView, TareaView
- SettingsView, ProyectoDetalleView, TareaDetalleView
- CrearProyectoView, CrearTareaView

Esta configuración permite que el contenedor inyecte automáticamente las dependencias en los constructores sin necesidad de resolución manual.

Una vez construido el MauiApp mediante builder.Build(), App.xaml.cs recibe el IServiceProvider por inyección y ejecuta el método CreateWindow. El método sobrescrito crea la ventana inicial resolviendo LoginView desde el contenedor y envolviéndola en un NavigationPage. Esta es la primera pantalla que ve el usuario.

LoginView se enlaza automáticamente con LoginViewModel gracias al binding establecido en el constructor del code-behind. El usuario introduce email y contraseña, y al pulsar "Iniciar sesión", se ejecuta el comando LoginCommand que:
1. Valida campos no vacíos
2. Llama a UserService.LoginAsync(email, password)
3. El servicio envía credenciales al endpoint /auth/login usando form-encoded content
4. Recibe un token JWT en formato JSON
5. Deserializa la respuesta en TokenResponse
6. Guarda el access_token en SecureStorage usando MAUI's secure storage
7. Retorna true si todo fue exitoso

Si el login es exitoso, LoginViewModel ejecuta: Application.Current.Windows[0].Page = new AppShell(), reemplazando completamente la página raíz de la ventana con el AppShell. Este cambio transiciona la aplicación desde la pantalla de login hacia la interfaz principal con pestañas.

AppShell define cuatro ShellContent mediante DataTemplate para lazy loading:
- Dashboard (PanelPrincipal) en ruta "dashboardPage"
- Proyectos (ProyectoView) en ruta "proyectosPage"
- Tareas (TareaView) en ruta "tareasPage"
- Ajustes (SettingsView) en ruta "settingsPage"

Al inicializar, AppShell ejecuta RegisterRoutes() que registra rutas adicionales para navegación modal:
- "tareaDetalle" → TareaDetalleView
- "proyectoDetalle" → ProyectoDetalleView
- "CrearProyectoView" → CrearProyectoView
- "CrearTareaView" → CrearTareaView

La navegación principal se realiza mediante pestañas visibles en TabBar. Al seleccionar una pestaña, Shell instancia automáticamente la vista correspondiente, resolviendo su constructor desde el contenedor de dependencias e inyectando el ViewModel asociado.

Cuando PanelPrincipal se crea, recibe PanelPrincipalViewModel inyectado en su constructor y lo asigna a BindingContext. En ese momento, el constructor del ViewModel ejecuta inmediatamente _ = LoadData() (fire-and-forget pattern) que:
1. Llama simultáneamente a _proyectoService.GetAllAsync(), _userService.GetCurrentUserAsync() y _tareaService.GetAllAsync()
2. UserService.GetCurrentUserAsync intenta obtener el usuario desde endpoint /auth/me incluyendo el Bearer token en headers
3. Si falla, decodifica el token JWT localmente (método GetEmailFromToken que hace Base64 decode del payload), extrae el campo "sub" (subject/email), llama GetAllAsync y busca el usuario por email
4. Procesa los datos calculando estadísticas: cuenta tareas por estado (Pendiente/To Do, En Progreso/Doing/Progreso, Completada/Done)
5. Calcula PorcentajeGlobal = tareasCompletadas / totalTareas
6. Actualiza todas las propiedades observables usando MainThread.BeginInvokeOnMainThread para garantizar thread-safety en actualizaciones de UI

Las vistas están enlazadas mediante data binding a las propiedades del ViewModel. Cuando una propiedad observable cambia, el framework automáticamente actualiza los controles visuales correspondientes mediante INotifyPropertyChanged. Los comandos expuestos por el ViewModel están enlazados a eventos de botones mediante sintaxis {Binding CommandName}.

La navegación entre pantallas se realiza mediante el sistema de enrutamiento de Shell:
- Navegación entre pestañas: automática al seleccionar tab
- Navegación a detalles: await Shell.Current.GoToAsync("tareaDetalle", new Dictionary<string,object> { {"Tarea", tarea} })
- Navegación a formularios: await Shell.Current.GoToAsync(nameof(CrearProyectoView))
- Navegación hacia atrás: await Shell.Current.GoToAsync("..") o await Navigation.PopAsync()

Las vistas receptoras declaran propiedades decoradas con [QueryProperty(nameof(Tarea), "Tarea")] para recibir parámetros. El método parcial OnTareaChanged generado por el Source Generator se ejecuta automáticamente cuando el parámetro se asigna.

Las operaciones asíncronas como creación, actualización o eliminación de entidades se realizan desde los ViewModels invocando métodos de los servicios. Los servicios:
1. Llaman a AddAuthHeaderAsync() para incluir el Bearer token
2. Serializan el objeto usando JsonSerializer con PropertyNameCaseInsensitive
3. Crean StringContent con encoding UTF-8 y content-type application/json
4. Ejecutan la petición HTTP (POST, DELETE, GET, PUT)
5. Verifican response.IsSuccessStatusCode
6. Deserializan la respuesta si es GET
7. Retornan bool indicando éxito o lista de entidades
8. Capturan excepciones (HttpRequestException específicamente, Exception genérico como fallback) y escriben logs detallados con Debug.WriteLine

Los ViewModels manejan estos resultados mostrando alertas al usuario mediante Shell.Current.DisplayAlert o Application.Current.MainPage.DisplayAlert y refrescando las listas de datos invocando LoadDataAsync nuevamente.

Para el registro de usuarios, SignUpView ahora incluye un Frame con mensaje de error visible condicionalmente basado en HasError. SignUpViewModel implementa ValidateInputs() que valida:
- Campos no vacíos
- Nombre mínimo 3 caracteres
- Email con formato válido usando Regex
- Contraseña mínima 6 caracteres
- Contraseñas coincidentes
Cada validación fallida llama a ShowError(mensaje) que establece ErrorMessage y HasError = true, mostrando feedback inmediato en la UI sin necesidad de DisplayAlert.

ANÁLISIS POR MÓDULOS

Módulo de Modelos de Datos

Este módulo define las estructuras de datos fundamentales del sistema. La clase Usuario representa a un usuario del sistema con propiedades como Id, Nombre, Email, Password y RolId (rol global del sistema: admin o usuario). También contiene propiedades de navegación como Rol (referencia al objeto Rol asociado) y TareasAsignadas (lista de tareas que tiene el usuario). Todos los identificadores usan el sufijo Id para claridad.

La clase Proyecto representa un proyecto colaborativo con propiedades Id, Nombre, Descripcion (nullable), FechaCreacion (nullable, almacenada como string en formato ISO) y CreadoPorId (identificador del usuario que creó el proyecto). Anteriormente existía un campo UsuarioId que indicaba un único propietario; este ha sido reemplazado por CreadoPorId para reflejar que los proyectos ahora son compartidos entre múltiples miembros. La lista de Tareas asociadas se mantiene como propiedad de navegación marcada con [JsonIgnore] para evitar conflictos de serialización.

La clase ProyectoUsuario es nueva y representa la relación muchos-a-muchos entre proyectos y usuarios. Modela la tabla intermedia del backend con propiedades:
- IdProyectoUsuario: identificador único de la membresía
- IdProyecto: proyecto al que pertenece
- IdUsuario: usuario miembro
- RolProyecto: rol dentro del proyecto ("owner", "editor" o "viewer")

Este modelo permite que un proyecto tenga múltiples miembros con diferentes niveles de acceso, y que un usuario participe en múltiples proyectos con roles distintos en cada uno. Además se definen dos DTOs auxiliares: ProyectoUsuarioCreateRequest (para añadir miembros con id_usuario y rol_proyecto) y ProyectoUsuarioUpdateRequest (para cambiar el rol de un miembro existente).

Los roles dentro de un proyecto determinan permisos:
- owner: creador del proyecto, control total (editar, borrar, gestionar miembros)
- editor: puede editar proyecto, crear y modificar tareas
- viewer: solo puede ver el proyecto y sus tareas, sin modificar nada

La clase Tarea es el núcleo operativo del sistema, con propiedades Id, Titulo, Descripcion, Estado (string con valores como Pendiente, En Progreso, Completada), ProyectoId (identificador del proyecto padre) y UsuarioId nullable (usuario asignado a la tarea). El estado permite implementar flujos tipo Kanban. Las tareas están protegidas por los permisos del proyecto al que pertenecen.

La clase Rol define perfiles globales de usuario con Id y Nombre (admin, usuario), junto con una lista de Usuarios asociados. Los roles globales determinan si un usuario es administrador del sistema (puede ver todos los proyectos) o usuario normal (solo ve proyectos donde es miembro).

La clase Comentario está definida pero no se utiliza en la implementación actual. Representa comentarios en tareas con propiedades Id, Texto, Fecha, TareaId y UsuarioId, preparada para funcionalidad de colaboración futura.

Todas las clases utilizan atributos JsonPropertyName para mapear entre la nomenclatura snake_case del backend Python y PascalCase de C#. Esto permite una integración transparente sin necesidad de transformaciones manuales.

Módulo de Servicios de API

IRestService es la interfaz genérica que define el contrato básico para todos los servicios REST. Declara dos métodos: GetAllAsync que devuelve una lista de entidades de tipo T, y DeleteAsync que recibe un identificador entero y devuelve un booleano indicando éxito o fallo.

UserService implementa IRestService para Usuario y gestiona toda la autenticación JWT del sistema. El método LoginAsync envía las credenciales al endpoint /auth/login del backend usando FormUrlEncodedContent con campos username (email) y password. El backend valida las credenciales, hashea y compara contraseñas, y retorna un token JWT con estructura {access_token, token_type}. UserService deserializa esta respuesta y almacena el token en SecureStorage para uso posterior. También ofrece CreateAsync para registro contra /auth/register, GetCurrentUserAsync que intenta obtener el usuario autenticado desde /auth/me (con fallback de decodificación local del JWT), GetTokenAsync para recuperar el token almacenado, y Logout para eliminar el token de SecureStorage. Incluye clases privadas TokenResponse y RegisterRequest para serialización de las respuestas y peticiones de autenticación.

ProyectoService implementa el servicio para proyectos colaborativos. Todas las peticiones incluyen autenticación mediante el método privado AddAuthHeaderAsync() que obtiene el token JWT de SecureStorage y lo añade como header Authorization: Bearer {token}. El backend usa este token para identificar al usuario y aplicar control de acceso.

Métodos CRUD principales:
- GetAllAsync(): GET /proyectos/ — El backend devuelve automáticamente solo los proyectos donde el usuario es miembro (o todos si es admin global). No se filtra en frontend.
- GetByIdAsync(int id): GET /proyectos/{id} — Solo funciona si el usuario es miembro o admin.
- CreateAsync(Proyecto): POST /proyectos/ — Envía solo nombre, descripcion y fecha_creacion. NO envía creado_por_id; el backend lo obtiene del token JWT y automáticamente añade al usuario creador como owner del proyecto.
- UpdateAsync(int id, Proyecto): PATCH /proyectos/{id} — Solo permitido para admin, owner o editor. Envía campos editables.
- DeleteAsync(int id): DELETE /proyectos/{id} — Solo permitido para admin u owner.
- DeleteWithStatusAsync(int id): Variante que retorna tupla (bool Success, int StatusCode) para que los ViewModels puedan distinguir entre errores 403 (sin permisos) y 401 (sesión expirada).

Métodos de gestión de miembros:
- GetMiembrosAsync(int proyectoId): GET /proyectos/{proyectoId}/miembros — Retorna List<ProyectoUsuario> con todos los miembros del proyecto y sus roles.
- AddMiembroAsync(int proyectoId, int usuarioId, string rolProyecto): POST /proyectos/{proyectoId}/miembros — Añade un usuario como miembro con el rol especificado.
- UpdateMiembroRolAsync(int proyectoId, int usuarioId, string rolProyecto): PATCH /proyectos/{proyectoId}/miembros/{usuarioId} — Cambia el rol de un miembro existente.
- RemoveMiembroAsync(int proyectoId, int usuarioId): DELETE /proyectos/{proyectoId}/miembros/{usuarioId} — Elimina un miembro del proyecto.

Todos los métodos de miembros retornan tupla (bool Success, int StatusCode) para manejo granular de errores de permisos.

TareaService maneja las operaciones de tareas contra /tareas con autenticación Bearer en todas las peticiones. Implementa GetAllAsync (el backend ya filtra según proyectos accesibles del usuario), CreateAsync y DeleteAsync. Adicionalmente ofrece CreateWithStatusAsync y DeleteWithStatusAsync que retornan el código de estado HTTP para distinguir errores 403 ("No tienes permisos para modificar tareas en este proyecto") de otros errores. Las tareas se filtran frecuentemente por proyecto en los ViewModels después de obtenerlas.

ApiConfig es una clase estática con una propiedad BaseUrl calculada. En tiempo de compilación, utiliza directivas de preprocesador para determinar la plataforma. Si es Android, devuelve http://10.0.2.2:8000 (dirección especial del emulador para acceder al localhost del host). Para cualquier otra plataforma devuelve http://127.0.0.1:8000. Esta configuración evita tener que cambiar URLs manualmente según la plataforma de desarrollo.

Todos los servicios crean instancias nuevas de HttpClient en cada servicio. En producción esto no es ideal (debería usarse IHttpClientFactory), pero para propósitos de prototipo funciona correctamente. Todos los servicios autentican cada petición mediante Bearer token JWT, y el backend es quien aplica los permisos reales de acceso. El frontend solo mejora la experiencia de usuario mostrando mensajes apropiados según los códigos de respuesta HTTP.

Módulo ViewModels - Panel Principal

PanelPrincipalViewModel es el cerebro del dashboard. Se inyectan tres servicios concretos: ProyectoService, UserService y TareaService. Define propiedades observables para todas las métricas que se muestran en la pantalla: Proyectos (colección), TodasLasTareas, UsuarioLogueado, TotalProyectos, TotalTareas, contadores por estado de tarea y PorcentajeGlobal.

En el constructor, inicializa las colecciones y lanza inmediatamente la tarea LoadData sin esperar, usando el operador de descarte _ =. LoadData realiza tres llamadas asíncronas a los servicios, procesa las listas resultantes calculando estadísticas (cuenta tareas por estado, calcula porcentaje de completitud), y actualiza todas las propiedades observables desde el hilo principal. El uso de MainThread.BeginInvokeOnMainThread garantiza que las actualizaciones de UI sean seguras.

Esta arquitectura reactiva hace que la vista refleje automáticamente los cambios cuando las propiedades se actualizan. El ViewModel no conoce nada sobre la vista, manteniendo la separación de responsabilidades.

Módulo ViewModels - Gestión de Proyectos

ProyectoViewModel maneja la lista de proyectos. Inyecta ProyectoService directamente (no la interfaz genérica) para acceder a métodos específicos como DeleteWithStatusAsync. Define una colección observable Proyectos y una propiedad ProyectoSeleccionado. Expone tres comandos: VerDetallesCommand (síncrono), LoadDataCommand (asíncrono) y EliminarProyectoCommand (asíncrono).

LoadDataAsync consulta ProyectoService.GetAllAsync() y actualiza la colección. El backend ya devuelve únicamente los proyectos donde el usuario autenticado es miembro (o todos si es admin global), por lo que no se realiza ningún filtrado manual en frontend. Este método es público y se invoca desde la vista cuando aparece en pantalla (método OnAppearing) para asegurar datos frescos tras navegaciones.

VerDetalles navega a la vista de detalle usando Shell navigation y pasa el proyecto completo como parámetro. El sistema Shell serializa el objeto y lo pasa a la vista destino.

EliminarProyectoAsync primero muestra un diálogo de confirmación mediante DisplayAlert. Si el usuario confirma, llama a ProyectoService.DeleteWithStatusAsync que retorna tanto el resultado como el código de estado HTTP. Si la eliminación es exitosa, muestra confirmación y refresca la lista. Si recibe un código 403, muestra un mensaje específico: "No tienes permisos para eliminar este proyecto. Solo el owner o un administrador puede hacerlo." Si recibe un 401, indica que la sesión ha expirado. Este manejo granular de errores de permisos es esencial dado que el backend aplica control de acceso basado en roles de proyecto (solo owner y admin global pueden eliminar).

CrearProyectoViewModel implementa INotifyPropertyChanged manualmente (a diferencia de otros ViewModels que usan CommunityToolkit). Define propiedades Nombre, Descripcion y FechaCreacion con notificación manual en los setters. El comando CrearProyectoCommand valida que el nombre no esté vacío, construye un nuevo objeto Proyecto con solo nombre, descripción y fecha, y lo envía al servicio. Importante: NO se envía creado_por_id ni usuario_id; el backend obtiene el usuario creador automáticamente del token JWT y lo añade como owner del proyecto. Tras éxito, muestra feedback y navega de vuelta usando Shell.Current.GoToAsync con ".." (equivalente a back).

ProyectoDetalleViewModel recibe un proyecto completo mediante QueryProperty y carga tanto sus tareas relacionadas como los miembros del proyecto. Define TareasFiltradas y Miembros como colecciones observables. Inyecta tanto TareaService como ProyectoService. Cuando la propiedad Proyecto cambia (gracias al método parcial OnProyectoChanged generado por ObservableProperty), se dispara la carga combinada: CargarTareasDelProyecto consulta el servicio de tareas y filtra por ProyectoId, mientras que CargarMiembrosDelProyecto llama a ProyectoService.GetMiembrosAsync para obtener la lista de miembros con sus roles dentro del proyecto. Este enfoque permite mostrar las tareas relevantes del proyecto actual junto con la información de quién colabora en él y con qué nivel de permisos.

Módulo ViewModels - Gestión de Tareas

TareaViewModel es el más complejo del módulo de tareas. Inyecta TareaService directamente (no la interfaz genérica) para acceder a DeleteWithStatusAsync. Define una colección Tareas y tres propiedades computadas de solo lectura: TareasPendientes, TareasEnProgreso y TareasCompletadas. Estas propiedades filtran la colección principal según el estado de cada tarea, permitiendo implementar la vista tipo Kanban.

LoadDataCommand consulta todas las tareas del servicio. Si ProyectoId es mayor que cero (significa que se está viendo desde el contexto de un proyecto específico), filtra las tareas por ese proyecto. Luego actualiza la colección y llama a RefreshColumns.

RefreshColumns es un método privado que manualmente dispara OnPropertyChanged para las tres propiedades computadas. Esto es necesario porque aunque Tareas sea observable, las propiedades computadas no se actualizan automáticamente cuando cambia la colección base. Al notificar manualmente, se fuerza la reevaluación de los filtros y actualización de las vistas vinculadas.

EliminarTareaCommand muestra confirmación, ejecuta el borrado mediante DeleteWithStatusAsync del servicio, y si es exitoso remueve la tarea de la colección local y refresca las columnas. Si recibe un código 403, muestra un mensaje indicando que el usuario no tiene permisos para eliminar tareas en ese proyecto (solo editor, owner o admin pueden hacerlo). Si recibe 401, indica sesión expirada.

CrearTareaViewModel gestiona la creación de nuevas tareas. Define propiedades para Titulo, Descripcion, Estado (con valor por defecto "Pendiente") y ProyectoId. El comando CrearTareaCommand valida campos obligatorios, construye el objeto Tarea y lo envía al servicio. Tras éxito, navega hacia atrás.

TareaDetalleViewModel es minimalista, simplemente recibe una Tarea como parámetro de navegación y la expone como propiedad observable para que la vista pueda enlazarse. No contiene lógica adicional, es puramente para presentación de detalles.

Módulo ViewModels - Configuración y Usuarios

SettingsViewModel maneja la configuración de la aplicación. Inyecta el servicio de usuarios y define propiedades para UsuarioActual, una lista de temas disponibles (Unspecified, Light, Dark) y el tema seleccionado actualmente.

En el constructor carga el tema actual desde Application.Current.UserAppTheme y ejecuta LoadUserData que consulta el servicio de usuarios y obtiene el primero (asumiendo usuario único en desarrollo). Cuando SelectedTheme cambia, el método parcial OnSelectedThemeChanged actualiza Application.Current.UserAppTheme, lo que dispara el cambio de tema en toda la aplicación instantáneamente.

SaveSettingsCommand actualmente solo actualiza la propiedad SelectedTheme, pero está preparado para extenderse con guardado persistente de preferencias usando Preferences o SecureStorage de MAUI.

SignUpViewModel gestiona el registro de nuevos usuarios. Define propiedades para los campos del formulario: Nombre, Email, Password, ConfirmPassword y un flag IsLoading para UI. El comando RegisterCommand valida que todos los campos estén llenos, que las contraseñas coincidan, construye un nuevo objeto Usuario con RolId predeterminado 2 (probablemente usuario estándar), envía la petición de creación y navega de vuelta tras éxito. Los errores se capturan y muestran al usuario mediante alerts.

Módulo de Vistas - Arquitectura XAML

Las vistas están construidas en XAML declarativo con code-behind mínimo. Todas heredan de ContentPage y utilizan data binding extensivamente para conectarse con sus ViewModels.

PanelPrincipal.xaml implementa un dashboard visual con cuatro secciones principales en un Grid de filas: cabecera decorativa con gradiente púrpura mostrando saludo personalizado al usuario y fecha actual, fila de indicadores con cinco badges compactos mostrando métricas clave (proyectos totales, tareas totales, pendientes, en progreso, y porcentaje de éxito), sección principal dividida en dos columnas con CollectionViews mostrando proyectos activos con barras de progreso a la izquierda y lista de tareas a la derecha, y pie con un banner oscuro mostrando estadísticas acumuladas.

El diseño utiliza Border extensivamente con BorderRadius para esquinas redondeadas y Shadow para efectos de profundidad. Los colores utilizan AppThemeBinding para adaptarse automáticamente entre tema claro y oscuro. Los elementos están enlazados a propiedades del ViewModel mediante sintaxis {Binding Property}.

ProyectoView.xaml muestra una lista de proyectos en formato horizontal. Tiene una cabecera decorativa con gradiente, botón para crear nuevo proyecto, indicadores de totales, y un CollectionView con orientación horizontal mostrando tarjetas de proyectos. Cada tarjeta es un Border ancho con sombra conteniendo nombre, descripción, lista de miembros del equipo (aunque esta funcionalidad no está completamente implementada) y dos botones: Eliminar y Detalles.

Los comandos de los botones dentro del DataTemplate utilizan una técnica importante: {Binding BindingContext.EliminarProyectoCommand, Source={x:Reference ProyectoPage}}. Esto es necesario porque dentro de un DataTemplate el BindingContext es el item individual (Proyecto), pero el comando está en el ViewModel de la página. La referencia x:Reference resuelve esto accediendo al BindingContext de la página.

TareaView.xaml implementa un tablero Kanban con tres columnas: To Do, Doing y Done. Cada columna es una CollectionView enlazada a una propiedad filtrada del ViewModel (TareasPendientes, TareasEnProgreso, TareasCompletadas). Las tarjetas de tarea muestran título, descripción, proyecto asociado, y botones para editar y eliminar representados con emojis.

El layout utiliza Grid con tres columnas iguales. Cada columna tiene un encabezado con estilo diferenciado por color y una CollectionView vertical. El diseño es completamente responsive gracias a los Grids anidados y el uso de VerticalOptions/HorizontalOptions.

CrearProyectoView.xaml y CrearTareaView.xaml son formularios simples con Entry fields para entrada de texto, DatePicker para fecha, Picker para selección de estado, y un botón de envío. Los campos están enlazados bidirecionalmente mediante Binding con Mode=TwoWay a las propiedades del ViewModel, permitiendo que los cambios en UI actualicen el ViewModel y viceversa.

SignUpView.xaml es un formulario de registro con campos para nombre, email, contraseña y confirmación. Incluye un ActivityIndicator enlazado a IsLoading para mostrar feedback durante el proceso de registro asíncrono.

SettingsView.xaml muestra información del usuario actual y un Picker para seleccionar tema de la aplicación. El tema se aplica inmediatamente al cambiar la selección gracias al binding en el ViewModel.

Las vistas de detalle (ProyectoDetalleView.xaml, TareaDetalleView.xaml) muestran información completa de una entidad individual con diseño más espaciado y tipografía más grande para enfatizar los detalles.

Módulo AppShell - Navegación

AppShell.xaml define la estructura de navegación principal mediante Shell, una de las características más poderosas de MAUI. Define cuatro ShellContent principales: Dashboard (PanelPrincipal), Proyectos (ProyectoView), Tareas (TareaView) y Ajustes (SettingsView).

Cada ShellContent tiene un Title que aparece en la barra de navegación y un ContentTemplate que especifica qué tipo de vista instanciar. El uso de DataTemplate en vez de instancia directa permite lazy loading: las vistas solo se crean cuando se navega a ellas.

En el code-behind, el método RegisterRoutes registra rutas adicionales para navegación modal usando Routing.RegisterRoute. Las rutas tareaDetalle, proyectoDetalle, CrearProyectoView y CrearTareaView se asocian a sus tipos de vista correspondientes. Estas rutas se usan luego en los ViewModels mediante Shell.Current.GoToAsync.

El sistema Shell proporciona características adicionales como FlyoutMenu (menú lateral), SearchHandler, TabBar y más, aunque en esta implementación se usa principalmente como navegación por pestañas.

Módulo de Configuración - MauiProgram

MauiProgram.cs es el corazón de la configuración de la aplicación. El método CreateMauiApp usa el patrón builder para configurar todos los aspectos de la app.

La línea UseMauiApp<App>() especifica la clase principal de aplicación. ConfigureFonts registra las fuentes personalizadas OpenSans.

El bloque condicional #if DEBUG agrega logging de debug solo en builds de desarrollo. Esto es importante para rendimiento en producción.

El registro de servicios sigue principios de inversión de dependencias. Los servicios concretos se registran con sus interfaces, permitiendo que el contenedor resuelva dependencias automáticamente. El lifetime de cada servicio es importante: Transient crea nueva instancia cada vez, mientras Singleton mantiene la misma instancia durante toda la vida de la app.

UserService tiene un registro especial dual: se registra como Singleton de sí mismo y luego se registra la interfaz IRestService<Usuario> delegando al mismo singleton mediante GetRequiredService. Esto permite que cualquier código pueda pedir IRestService<Usuario> y recibir la misma instancia singleton de UserService.

Todos los ViewModels y Views se registran como Transient. Esto es correcto porque queremos instancias frescas cada vez que navegamos a una vista, evitando state contamination entre navegaciones.

El método Build construye finalmente el MauiApp configurado que será usado por el runtime.

AUTENTICACIÓN Y SEGURIDAD

El proyecto actualmente implementa un sistema de autenticación basado en JWT (JSON Web Tokens) con implementación funcional pero con limitaciones de seguridad para un entorno de producción.

Flujo de autenticación implementado:
1. Usuario introduce email y contraseña en LoginView
2. LoginViewModel.LoginCommand valida campos no vacíos y llama UserService.LoginAsync
3. UserService.LoginAsync envía POST a /auth/login con FormUrlEncodedContent conteniendo username (email) y password
4. El backend FastAPI valida credenciales y retorna JSON con estructura {access_token: "jwt_string", token_type: "bearer"}
5. UserService deserializa la respuesta en clase privada TokenResponse
6. El access_token se almacena en SecureStorage.SetAsync("access_token", token) - almacenamiento seguro específico de plataforma (Android Keystore, iOS Keychain, Windows Credential Locker)
7. LoginViewModel cambia la página raíz a AppShell indicando login exitoso

Flujo de autenticación en requests:
1. Cada servicio (ProyectoService, TareaService) tiene un método privado AddAuthHeaderAsync()
2. Este método obtiene el token de SecureStorage via UserService.GetTokenAsync()
3. Limpia headers existentes (_client.DefaultRequestHeaders.Authorization = null)
4. Si hay token, añade header: Authorization: Bearer {token}
5. Todas las operaciones CRUD llaman a AddAuthHeaderAsync() antes de hacer la petición HTTP
6. El backend valida el token JWT y extrae información del usuario (email, roles) del payload

Obtención de usuario actual:
UserService.GetCurrentUserAsync implementa estrategia de doble intento:
1. Primero intenta GET /auth/me con Bearer token incluido
2. Si el endpoint existe y responde 200 OK, deserializa directamente el Usuario
3. Si falla (endpoint no implementado o error), usa método de fallback
4. GetEmailFromToken decodifica el JWT localmente: split por '.', extrae payload (segunda parte), ajusta padding Base64, decodifica, parsea JSON, extrae campo "sub" que contiene el email
5. Llama GetAllAsync() para obtener lista de usuarios y busca por email con StringComparison.OrdinalIgnoreCase
6. Retorna el Usuario encontrado o null

Registro de usuarios:
SignUpViewModel.RegisterCommand ahora incluye validaciones detalladas:
- Formato de email validado con Regex: ^[^@\s]+@[^@\s]+\.[^@\s]+$
- Longitud mínima de nombre (3 caracteres)
- Longitud mínima de contraseña (6 caracteres)
- Verificación de coincidencia de contraseñas
- Mensajes de error específicos mostrados en UI mediante Frame con HasError/ErrorMessage binding

UserService.CreateAsync envía POST a /auth/register con JSON body:
```json
{
  "nombre": "string",
  "email": "string",
  "password": "string",
  "id_rol": 1
}
```
Usa clase privada RegisterRequest con JsonPropertyName para mapeo snake_case.

Logout:
UserService.Logout() simplemente ejecuta SecureStorage.Remove("access_token"), eliminando el token del almacenamiento seguro. No hay invalidación de token en servidor ni blacklist.

Limitaciones de seguridad actuales:

1. COMUNICACIÓN NO CIFRADA:
- Todas las URLs usan HTTP sin SSL/TLS
- Tokens JWT viajan en texto plano sobre la red
- Vulnerable a ataques man-in-the-middle
- Las contraseñas se envían sin cifrado adicional (aunque FastAPI debería hashearlas en backend)

2. VALIDACIÓN DE TOKEN:
- No hay verificación de expiración del token en el cliente
- No se implementa refresh token para renovar sesiones
- No hay manejo de tokens expirados (error 401) con renovación automática
- El cliente asume que cualquier token almacenado es válido

3. GESTIÓN DE SESIONES:
- No hay timeout de inactividad
- SecureStorage persiste indefinidamente hasta que el usuario haga logout manualmente o desinstale la app
- No hay detección de múltiples sesiones

4. REGISTRO DE USUARIOS:
- No hay verificación de email (confirmación por correo)
- No hay captcha o protección anti-bot
- La validación de contraseña es básica (solo longitud mínima)
- No requiere caracteres especiales, mayúsculas o números
- No hay check de contraseñas comúnmente hackeadas

5. MANEJO DE ERRORES:
- Los mensajes de error podrían exponer información del sistema
- Debug.WriteLine escribe información sensible en logs de desarrollo
- No hay ofuscación de información sensible en excepciones

6. ROLES Y PERMISOS:
- El backend implementa un sistema completo de permisos con roles globales (admin, usuario) y roles por proyecto (owner, editor, viewer)
- El frontend maneja correctamente respuestas 403 (Forbidden) mostrando mensajes informativos al usuario
- El frontend NO implementa restricciones de seguridad propias; toda la seguridad real la aplica el backend
- El frontend mejora la UX mostrando mensajes claros cuando una operación es rechazada por falta de permisos
- En futuras iteraciones, el frontend podría ocultar botones según el rol conocido del usuario para evitar intentos innecesarios

7. ALMACENAMIENTO:
- Aunque SecureStorage es seguro, no hay cifrado adicional de capa aplicación
- No hay detección de dispositivos rooteados/jailbroken donde SecureStorage podría ser comprometido

Mejoras necesarias para producción:

1. IMPLEMENTAR HTTPS:
   - Migrar ApiConfig a URLs https://
   - Configurar certificados SSL/TLS válidos en backend
   - Implementar certificate pinning para prevenir MITM

2. GESTIÓN DE TOKENS ROBUSTA:
   - Implementar refresh tokens con endpoint /auth/refresh
   - Parsear fecha de expiración del JWT (campo "exp")
   - Renovar automáticamente tokens antes de expirar
   - Implementar interceptor HTTP que detecta 401 Unauthorized, renueva token y reintenta request
   - Manejar caso donde refresh token también expiró (forzar nuevo login)

3. VALIDACIÓN MEJORADA:
   - Validación de fortaleza de contraseña con biblioteca especializada
   - Verificación de email mediante código enviado por correo
   - Implementar rate limiting en intentos de login (3 intentos fallidos = espera exponencial)
   - Captcha en registro

4. AUTORIZACIÓN:
   - El backend ya implementa control de acceso completo basado en roles globales y de proyecto
   - El frontend podría mejorarse ocultando botones según el rol conocido (ej: no mostrar botón Eliminar a viewers)
   - La seguridad real siempre la aplica el backend; el frontend solo mejora UX evitando acciones que serán rechazadas

5. SEGURIDAD ADICIONAL:
   - Implementar biometría (fingerprint, Face ID) como opción de autenticación rápida
   - Detectar dispositivos comprometidos
   - Implementar certificate pinning
   - Ofuscar código sensible (especialmente ApiConfig URLs si contuvieran secrets)
   - Limpiar logs de información sensible en builds Release

6. AUDITORÍA:
   - Logging estructurado de eventos de autenticación (login, logout, intentos fallidos)
   - Telemetría de seguridad
   - Alertas de actividades sospechosas

7. COMPLIANCE:
   - Cumplir con GDPR para datos de usuarios europeos
   - Implementar políticas de privacidad y términos de servicio
   - Permitir eliminación de cuenta y exportación de datos

El sistema actual es adecuado para desarrollo, pruebas de concepto y demostración educativa (TFG), pero requiere refactorización significativa de seguridad antes de cualquier despliegue público o producción comercial.

CONFIGURACIÓN Y VARIABLES DE ENTORNO

El proyecto utiliza una configuración extremadamente simple sin variables de entorno formales. La única configuración externa es la URL del backend API.

La clase ApiConfig define estáticamente la BaseUrl. No lee de archivos de configuración ni variables de sistema. La URL se determina en tiempo de compilación usando directivas de preprocesador específicas de plataforma. Para Android devuelve http://10.0.2.2:8000 y para otras plataformas http://127.0.0.1:8000. El puerto 8000 está hardcodeado, asumiendo que el backend FastAPI se ejecuta en ese puerto.

La dirección 10.0.2.2 es una característica especial de los emuladores Android que redirige al localhost de la máquina host. Esto permite que la app en el emulador se comunique con servidores de desarrollo en la máquina real.

No hay configuración de entornos múltiples (desarrollo, staging, producción). Si se quisiera apuntar a diferentes servidores según el entorno, habría que modificar manualmente el código fuente.

No se utilizan archivos de configuración como appsettings.json comunes en aplicaciones ASP.NET. No hay secrets management para almacenar claves API u otras credenciales sensibles.

Las fuentes personalizadas se configuran en MauiProgram mediante el método ConfigureFonts. Los archivos de fuente deben estar en Resources/Fonts y se referencian por nombre de archivo.

Los recursos visuales como colores y estilos se centralizan en Resources/Styles/Colors.xaml y Styles.xaml. Estos archivos XAML definen ResourceDictionary con valores reutilizables. Por ejemplo, pueden definir colores primarios, secundarios, fuentes por defecto, estilos de botones, etc. Se cargan en App.xaml mediante MergedDictionaries.

Los estilos utilizan AppThemeBinding extensivamente para definir valores diferentes según el tema claro u oscuro. Por ejemplo: BackgroundColor="{AppThemeBinding Light=White, Dark=#1E1E24}" aplica blanco en tema claro y gris oscuro en tema oscuro.

El archivo .csproj contiene configuración importante: TargetFrameworks especifica las plataformas objetivo (Android, iOS, macOS, Windows), SupportedOSPlatformVersion define versiones mínimas de cada OS, ApplicationId es el identificador único de la app en stores, y PackageReference define las dependencias NuGet.

Las dependencias actuales son: CommunityToolkit.Mvvm versión 8.4.2 (para patrón MVVM moderno), Microsoft.Maui.Controls con versión flotante basada en MauiVersion (el framework principal), y Microsoft.Extensions.Logging.Debug versión 10.0.0 (para logging en desarrollo).

Para añadir configuración más robusta en el futuro, se podría usar Microsoft.Extensions.Configuration para cargar archivos JSON, usar el sistema de preferencias de MAUI mediante Preferences API para configuración persistente del usuario, implementar detección de entorno mediante build configurations en el csproj, y usar SecureStorage para credenciales sensibles.

ESTILO DE CÓDIGO Y CALIDAD

El código sigue en gran medida las convenciones estándar de C# y .NET, aunque con algunas inconsistencias. El proyecto usa espacios para indentación (configuración por defecto de Visual Studio) y naming conventions estándar: PascalCase para clases, métodos y propiedades públicas, camelCase para parámetros y variables locales (con prefijo _ para campos privados en algunos lugares), y ALL_CAPS no se usa.

Las propiedades automáticas son predominantes, especialmente cuando se usan atributos de ObservableProperty. Los métodos suelen ser concisos con responsabilidad única. Sin embargo, hay algunos métodos largos como LoadData en PanelPrincipalViewModel que podrían descomponerse.

El código utiliza características modernas de C# como async/await consistentemente para operaciones asíncronas, pattern matching en algunos lugares con FirstOrDefault y condiciones, null-coalescing operators (?? y ??=), y expresiones lambda ampliamente en LINQ.

El manejo de errores es básico pero presente. Todos los métodos asíncronos en servicios están envueltos en bloques try-catch que capturan Exception genérico, escriben a Debug.WriteLine y retornan valores por defecto seguros (listas vacías, false, null). Este enfoque previene crashes pero pierde información detallada de errores. No hay logging estructurado ni telemetría.

Una debilidad notable es la falta de validación de entrada robusta. Los ViewModels validan campos vacíos con IsNullOrWhiteSpace pero no validan formatos (por ejemplo, validar que el email sea email válido) ni rangos de valores.

No hay comentarios de documentación XML en el código. Los métodos y clases no tienen summaries ni param tags. Esto dificulta la generación de documentación automática y la experiencia de IntelliSense. Sin embargo, los nombres son generalmente autodescriptivos, lo que mitiga parcialmente esta falta.

El proyecto no incluye un archivo .editorconfig para forzar estilos de código consistentes. Tampoco hay evidencia de linters o analizadores estáticos como StyleCop o FxCop configurados en el csproj. Las propiedades Nullable está habilitada en el proyecto (Nullable>enable en csproj), lo que ayuda a prevenir null reference exceptions en tiempo de compilación.

La separación de responsabilidades es buena a nivel de carpetas (Model, View, ViewModel, Services) pero algunos ViewModels tienen lógica de negocio mezclada con lógica de presentación. Por ejemplo, el cálculo de estadísticas en PanelPrincipalViewModel podría estar en una capa de lógica de negocio separada.

Hay duplicación de código entre servicios. UserService, ProyectoService y TareaService tienen implementaciones casi idénticas de GetAllAsync, CreateAsync y DeleteAsync. Esto podría refactorizarse en una clase base genérica RestServiceBase<T> que implemente la lógica común.

La consistencia en el uso de CommunityToolkit.Mvvm varía. Algunos ViewModels usan ObservableProperty y RelayCommand de los atributos, mientras CrearProyectoViewModel y CrearTareaViewModel implementan INotifyPropertyChanged manualmente. Esto sugiere desarrollo incremental o múltiples autores. Sería mejor estandarizar en el approach moderno del toolkit.

Las vistas XAML son consistentemente bien estructuradas con indentación clara. El uso de Grid y StackLayout es apropiado. Los binding paths son claros y correctos. Los estilos inline prevalecen sobre estilos reutilizables en algunos casos, lo que podría mejorarse extrayendo estilos comunes a ResourceDictionaries.

No hay evidencia de tests unitarios ni de integración. No existe una carpeta Tests y el csproj no referencia frameworks de testing como xUnit, NUnit o MSTest. Para un proyecto académico esto puede ser aceptable pero en producción sería crítico.

El control de versiones muestra desarrollo activo en la rama develop, lo cual es una buena práctica de Git Flow. El repositorio está en GitHub permitiendo colaboración.

En general, el código es legible y funcional pero se beneficiaría de refactoring para reducir duplicación, mejorar manejo de errores, añadir validaciones robustas y documentación, e implementar tests.

DEPENDENCIAS EXTERNAS

El proyecto depende de tres paquetes NuGet principales además de las librerías del framework .NET MAUI, manteniendo un enfoque minimalista de dependencias.

**CommunityToolkit.Mvvm versión 8.4.2**

Esta es la dependencia más importante después del framework mismo. El MVVM Toolkit (anteriormente conocido como Microsoft.Toolkit.Mvvm) es desarrollado y mantenido por Microsoft Community y proporciona infraestructura moderna para implementar el patrón MVVM de forma eficiente y con menos código boilerplate.

Componentes clave utilizados en el proyecto:

1. **ObservableObject**: Clase base abstracta para ViewModels que implementa INotifyPropertyChanged. Todos los ViewModels modernos del proyecto heredan de esta clase (excepto CrearProyectoViewModel y CrearTareaViewModel que usan implementación manual legacy).

2. **[ObservableProperty]**: Atributo Source Generator que transforma un campo privado en una propiedad pública con notificación completa de cambios. Por ejemplo:
   ```csharp
   [ObservableProperty]
   private string nombre;
   ```
   Genera automáticamente:
   ```csharp
   public string Nombre 
   { 
       get => nombre;
       set => SetProperty(ref nombre, value);
   }
   ```
   Además genera métodos parciales OnNombreChanging(value) y OnNombreChanged(value) para hooks personalizados.

3. **[RelayCommand]**: Atributo que transforma un método en una propiedad ICommand. Soporta métodos síncronos (genera RelayCommand) y asíncronos Task (genera AsyncRelayCommand). Incluye:
   - Generación automática de comandos con nombres terminados en "Command"
   - Soporte para parámetros genéricos
   - Manejo automático de CanExecute mediante métodos Can[MethodName]
   - Gestión de IsRunning para comandos asíncronos

4. **[QueryProperty]**: Atributo para recibir parámetros de navegación Shell. Ejemplo en TareaDetalleViewModel:
   ```csharp
   [QueryProperty(nameof(Tarea), "Tarea")]
   public partial class TareaDetalleViewModel : ObservableObject
   {
       [ObservableProperty]
       private Tarea tarea;
   }
   ```
   Genera código que recibe el parámetro "Tarea" de la navegación y lo asigna a la propiedad, disparando OnTareaChanged automáticamente.

5. **Source Generators**: La magia del toolkit se basa en C# Source Generators (característica de C# 9.0+) que generan código en tiempo de compilación. Esto proporciona:
   - Rendimiento equivalente a código manual
   - Sin reflexión en runtime
   - IntelliSense completo
   - Debugging del código generado posible
   - Detección de errores en compile-time

Ventajas sobre implementación manual:
- Reduce código boilerplate en ~70%
- Elimina errores comunes de implementación INotifyPropertyChanged
- Mejora mantenibilidad
- Código más legible y expresivo
- Sin overhead de rendimiento vs código manual

El proyecto usa este toolkit consistentemente excepto en CrearProyectoViewModel y CrearTareaViewModel que implementan INotifyPropertyChanged manualmente, probablemente código legacy o escrito antes de adoptar el toolkit completamente.

**Microsoft.Extensions.DependencyInjection versión 10.0.0**

Aunque .NET MAUI incluye DI built-in basado en Microsoft.Extensions.DependencyInjection, este paquete se referencia explícitamente para garantizar la versión exacta 10.0.0 correspondiente a .NET 10.

Proporciona el contenedor IoC (Inversion of Control) usado extensivamente en MauiProgram.cs. Métodos clave utilizados:

1. **AddSingleton<T>()**: Registra servicio con ciclo de vida Singleton (una instancia para toda la aplicación). Usado para UserService porque necesita mantener el token de autenticación consistente.

2. **AddTransient<T>()**: Registra servicio con ciclo de vida Transient (instancia nueva cada vez). Usado para todos los ViewModels, Views y servicios de datos (ProyectoService, TareaService) para garantizar estado fresco.

3. **AddTransient<TInterface>(Func<IServiceProvider, TImplementation>)**: Registra interfaz con factory delegate. Usado para registrar IRestService<T> delegando a las implementaciones concretas:
   ```csharp
   builder.Services.AddTransient<IRestService<Proyecto>>(sp => 
       sp.GetRequiredService<ProyectoService>());
   ```

4. **GetRequiredService<T>()**: Resuelve servicio del contenedor, lanzando excepción si no está registrado. Usado en varios lugares para resolver dependencias manualmente cuando la inyección automática no está disponible (ej: LoginViewModel.GoToRegisterCommand).

Patrones de registro implementados:
- Double registration de UserService (como clase y como interfaz)
- Factory delegates para mapear interfaces a implementaciones
- Registro consistente de pares ViewModel-View
- Lifecycles diferenciados según necesidades de estado

El contenedor se construye implícitamente por MauiApp.CreateBuilder() y es inyectado automáticamente como IServiceProvider en constructores que lo requieran (como App.xaml.cs).

**Microsoft.Maui.Controls con $(MauiVersion)**

Este es el paquete principal del framework. La versión está configurada con la propiedad MSBuild $(MauiVersion) que resuelve automáticamente la versión del SDK MAUI instalado. Esto permite actualizaciones automáticas del framework cuando se actualiza el SDK sin modificar el csproj.

Incluye todos los componentes fundamentales:
- Controles UI: ContentPage, Grid, StackLayout, ScrollView, CollectionView, Entry, Button, Label, Border, Frame, etc.
- Sistema de navegación: Shell con routing, NavigationPage, TabbedPage
- Data binding engine: BindingContext, BindableProperty, Binding markup extensions
- Abstracciones multiplataforma: Platform-specific implementations, handlers
- Gestures: TapGestureRecognizer, SwipeGestureRecognizer, PanGestureRecognizer
- Animations: ViewExtensions.TranslateTo, FadeTo, ScaleTo, etc.
- Theming: ResourceDictionary, AppThemeBinding, Visual State Manager
- Messaging: MessagingCenter (aunque no usado en este proyecto)
- APIs: SecureStorage, Preferences, Connectivity, DeviceInfo, MainThread

El proyecto usa $(MauiVersion) variable lo que significa que la versión exacta depende del SDK instalado. Para .NET 10 Preview, esto sería aproximadamente 10.0.0-preview.X.

**Microsoft.Extensions.Logging.Debug versión 10.0.0**

Paquete de logging que añade provider de Debug para escribir logs en la ventana Output de Visual Studio durante desarrollo. Solo se registra en builds DEBUG mediante:
```csharp
#if DEBUG
    builder.Logging.AddDebug();
#endif
```

Esto hace que todo el Debug.WriteLine extensivo usado en los servicios y ViewModels aparezca en la ventana Output > Debug, facilitando troubleshooting durante desarrollo.

En builds Release este paquete y su registro no están activos, reduciendo el overhead y el tamaño de la aplicación.

**Capacidades Built-in utilizadas (sin paquetes adicionales):**

1. **System.Net.Http**: HttpClient para peticiones REST. Cada servicio crea su propia instancia (anti-pattern en producción real pero aceptable para prototipo).

2. **System.Text.Json**: Serialización/deserialización JSON. Mucho más rápido que Newtonsoft.Json en .NET moderno. Usado con JsonSerializerOptions configurado con PropertyNameCaseInsensitive = true para mapeo flexible.

3. **System.Text.RegularExpressions**: Regex para validación de formato de email en SignUpViewModel.

4. **SecureStorage** (parte de Microsoft.Maui.Essentials integrado en MAUI): Almacenamiento seguro multiplataforma para tokens JWT. Usa Android Keystore, iOS Keychain, Windows Credential Locker según plataforma.

5. **MainThread** (parte de Microsoft.Maui.Essentials): BeginInvokeOnMainThread para garantizar que actualizaciones de UI se ejecuten en el thread correcto, previene InvalidOperationException cross-thread.

**Dependencias notablemente ausentes:**

El proyecto no usa varias librerías comunes en apps MAUI, manteniendo footprint mínimo:

- **Newtonsoft.Json**: Reemplazado completamente por System.Text.Json nativo
- **Refit**: No usa typed REST client, todas las llamadas son manuales con HttpClient
- **FluentValidation**: Validación manual en ViewModels en vez de framework declarativo
- **SQLite-net-pcl**: No hay base de datos local, todo es contra API remota
- **Polly**: No hay retry policies ni resilience patterns implementados
- **CommunityToolkit.Maui**: No usa controles adicionales como Popup, Toast, MediaElement
- **ReactiveUI**: Alternativa a MVVM Toolkit, no usada
- **Prism**: Framework MVVM completo, no usado (prefiere CommunityToolkit más ligero)

Esta estrategia minimalista tiene ventajas (menor superficie de ataque, builds más rápidos, app más ligera) pero limita capacidades avanzadas y requiere más código manual para funcionalidades que librerías especializadas proporcionarían out-of-the-box.

**Gestión de versiones:**

El csproj usa versiones exactas (8.4.2, 10.0.0) en vez de rangos o floating versions. Esto garantiza builds reproducibles pero requiere actualización manual de paquetes. No hay uso de central package management ni Directory.Build.props para centralizar versiones.

Para actualizar paquetes se debe:
1. Usar NuGet Package Manager en Visual Studio
2. Modificar manualmente el csproj
3. Ejecutar `dotnet add package [nombre] --version [version]`

No hay evidencia de uso de Dependabot o renovate bot para mantener dependencias actualizadas automáticamente.

PUNTOS FUERTES DEL DISEÑO

El proyecto demuestra varios aspectos de diseño sólido que muestran comprensión de patrones modernos y mejores prácticas de desarrollo.

Arquitectura MVVM clara y consistente. La separación entre View, ViewModel y Model está bien definida. Las vistas son puramente declarativas en XAML con code-behind mínimo, los ViewModels contienen toda la lógica de presentación y estado, y los Models son POCOs limpios. Esta separación facilita testing (aunque no hay tests actualmente), mantenimiento y escalabilidad.

Uso efectivo de inyección de dependencias. El proyecto aprovecha el contenedor DI built-in de .NET configurado en MauiProgram. Todos los servicios, ViewModels y Views se resuelven automáticamente, eliminando acoplamiento fuerte y facilitando testing de unidades. El patrón de registrar servicios con lifetimes apropiados (Singleton para UserService, Transient para otros) muestra comprensión de gestión de estado.

Implementación moderna con CommunityToolkit.Mvvm. El uso de source generators mediante ObservableProperty reduce significativamente el código boilerplate. Los comandos se definen de forma declarativa con RelayCommand, haciendo el código más legible y mantenible. Esta es una práctica recomendada oficial de Microsoft para desarrollo MAUI.

Data binding extensivo y apropiado. Las vistas están completamente data-bound a los ViewModels, eliminando la necesidad de manipular controles desde code-behind. El uso de binding modes apropiados (OneWay para presentación, TwoWay para entrada) es correcto. Los binding paths son claros y siguen naming conventions.

Shell navigation. El uso de AppShell para navegación es el approach recomendado en MAUI. Proporciona navegación basada en URI, tipado fuerte de parámetros mediante QueryProperty, y experiencia de usuario consistente. El registro de rutas separadas de la definición de Shell permite navegación modal y jerárquica flexible.

Diseño visual cuidado. Las vistas demuestran atención al diseño con uso de colores coherentes, spacing consistente, shadows para profundidad, border radius para suavizar UI, y gradientes para atractivo visual. El uso de AppThemeBinding para soporte de tema claro/oscuro es excelente y muestra consideración por preferencias del usuario.

Interfaz genérica IRestService. Aunque simple, define un contrato común para operaciones REST. Esto permite escribir código que funciona con cualquier tipo de entidad implementando el patrón Repository. Facilita mock para testing y permite cambiar implementaciones sin afectar consumers.

Manejo de threading apropiado. El código usa async/await consistentemente para operaciones de IO, previniendo bloqueo de UI thread. Los updates de UI desde tasks en background usan MainThread.BeginInvokeOnMainThread correctamente, evitando crashes por acceso cross-thread.

Estructura de proyecto organizada. Las carpetas separan claramente responsabilidades. La nomenclatura de archivos es consistente (ViewModels terminan en ViewModel, Views en View, Services en Service). Esto facilita navegación del código y onboarding de nuevos desarrolladores.

Configuración multiplataforma. ApiConfig demuestra awareness de diferencias entre plataformas (Android emulator networking) y abstrae estas diferencias. Aunque simple, este approach de detección de plataforma mediante directivas de compilación es correcto.

Uso de colecciones observables. ObservableCollection se usa apropiadamente para listas que se muestran en CollectionViews. Esto permite que los cambios en las colecciones se reflejen automáticamente en UI sin necesidad de rebind manual.

El proyecto serviría bien como base para expansión futura o como ejemplo educativo de aplicación MAUI bien estructurada. Con las mejoras mencionadas en seguridad, validación y testing, podría evolucionar a un sistema production-ready.

CÓMO EJECUTAR Y DESARROLLAR EL PROYECTO

Para ejecutar este proyecto localmente se requiere un entorno de desarrollo .NET configurado apropiadamente.

Requisitos previos: Visual Studio 2022 versión 17.8 o superior (Community, Professional o Enterprise), con la carga de trabajo "Desarrollo de .NET Multi-plataforma" instalada, .NET 10 SDK (aunque es una versión futura, el proyecto apunta a ella), y para ejecutar en Android se necesita un emulador configurado o dispositivo físico conectado. Para ejecutar en Windows se necesita Windows 10 versión 1809 o superior con Windows App SDK. Para ejecutar en macOS o iOS se necesita un Mac con Xcode instalado.

Clonar el repositorio usando Git:
git clone https://github.com/Irenecg22/GestionTareas.git

Navegar al directorio del proyecto y abrir la solución en Visual Studio haciendo doble click en GestionTareas.csproj o abriendo desde Visual Studio mediante File > Open > Project/Solution.

Al abrir el proyecto por primera vez, Visual Studio debería restaurar automáticamente los paquetes NuGet. Si no ocurre, hacer click derecho en la solución y seleccionar "Restore NuGet Packages".

Antes de ejecutar, asegurarse de que el backend API esté corriendo. El proyecto asume un servidor FastAPI en localhost:8000. Sin el backend, la app se abrirá pero las pantallas estarán vacías ya que no podrá cargar datos. Verificar la URL en Services/ApiConfig.cs y modificarla si el backend está en diferente puerto.

Seleccionar plataforma objetivo en la barra de herramientas de Visual Studio. Las opciones disponibles son: Framework de Android (para emuladores o dispositivos Android), Framework de Windows (para ejecutar como app Windows nativa), Framework de iOS (requiere Mac para build), Framework de Mac Catalyst (para ejecutar en macOS).

Para Android, se recomienda usar el emulador Pixel 5 API 33 o superior. Asegurarse de que el emulador esté iniciado antes de ejecutar. Para Windows, simplemente seleccionar "Windows Machine".

Presionar F5 o hacer click en el botón "Start Debugging" con la configuración Debug seleccionada. Visual Studio compilará el proyecto, desplegará la app en el target seleccionado y la iniciará con el debugger adjunto.

La primera compilación será lenta (varios minutos) porque MAUI genera código para múltiples plataformas y compila recursos. Las compilaciones subsiguientes son incrementales y mucho más rápidas.

Al iniciar la app, debería aparecer el AppShell con las pestañas de navegación. Si el backend está corriendo correctamente, el Dashboard cargará proyectos y tareas automáticamente. Si hay errores de red, aparecerán en la ventana Output de Visual Studio bajo la categoría Debug.

Para desarrollar nuevas funcionalidades: Crear el Model primero en la carpeta Model si introduces nueva entidad, implementar el Service correspondiente en Services para operaciones de API, crear el ViewModel en ViewModel con las propiedades y comandos necesarios, diseñar la View en View con XAML usando data binding, registrar ViewModel y View en MauiProgram.cs en el contenedor DI, si es una ruta navegable, registrarla en AppShell.cs usando Routing.RegisterRoute.

Para debugging efectivo, usar breakpoints en ViewModels y Services. Los breakpoints en archivos XAML no funcionan porque es markup declarativo. Para inspeccionar valores en runtime usar Watch window o Debug Console. Los errores de binding aparecen en Output window con prefijo [BINDING].

El hot reload de XAML está habilitado por defecto. Cambios en archivos XAML se pueden ver sin reiniciar la app mientras está corriendo en modo debug. Simplemente guardar el archivo XAML y la UI se actualizará. Sin embargo, cambios en código C# requieren rebuild.

Para modificar la configuración del backend API, editar Services/ApiConfig.cs. Para ambientes de producción, esta configuración debería moverse a un sistema de configuración más robusto.

Para ejecutar en dispositivo físico Android: Habilitar opciones de desarrollador en el dispositivo Android, activar depuración USB, conectar el dispositivo via USB, autorizar la conexión en el dispositivo cuando se solicite, seleccionar el dispositivo en la lista de targets de Visual Studio y ejecutar. El dispositivo debe estar en la misma red que el servidor backend, y ApiConfig debe apuntar a la IP de red de la máquina del backend (no localhost).

Para ejecutar en iOS requiere: un Mac en la red con Xcode y herramientas de build instaladas, Visual Studio configurado para conectarse al Mac mediante "Pair to Mac", un dispositivo iOS con provisioning profile correcto o simulador iOS, y las mismas consideraciones de networking que Android para acceso al backend.

El proyecto no incluye tests, pero para añadirlos se recomienda: crear un proyecto de pruebas separado (GestionTareas.Tests), agregar referencias a xUnit o NUnit y Microsoft.NET.Test.Sdk, usar Moq para mockear IRestService y probar ViewModels en aislamiento, y configurar CI/CD en GitHub Actions para ejecutar tests automáticamente.

Para deployment en tiendas (Google Play, App Store, Microsoft Store) se requieren pasos adicionales de firma, empaquetado y configuración específicos de cada plataforma que están más allá del scope de desarrollo local.

El proyecto está configurado con ApplicationId "com.companyname.gestiontareas" que debe cambiarse a un identificador único antes de publicación real. Este identificador se configura en GestionTareas.csproj.

Para performance profiling en MAUI puede usarse dotnet-trace y PerfView en Windows o Instruments en macOS. El código de release está optimizado con más agresividad que debug builds.

