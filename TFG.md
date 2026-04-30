# TFG — Documentación Técnica: Gestión de Proyectos y Tareas (Backend)

---

## 1. Visión general del proyecto

Este proyecto es el **backend** de una aplicación web de gestión de proyectos y tareas colaborativa tipo Trello, desarrollado como Trabajo de Fin de Grado (TFG). Su propósito es ofrecer una API RESTful que permita a los usuarios crear proyectos compartidos, gestionar miembros con diferentes roles, asignar tareas colaborativamente, y añadir comentarios en un entorno de trabajo en equipo.

El tipo de aplicación es un **microservicio API REST**, completamente stateless, que delega la persistencia en una base de datos relacional PostgreSQL y se comunica con los clientes (frontend u otros servicios) mediante JSON sobre HTTP.

### Características Principales

- **Proyectos Compartidos**: Múltiples usuarios pueden colaborar en el mismo proyecto
- **Sistema de Roles Granular**: Tres niveles de permisos dentro de cada proyecto (owner, editor, viewer)
- **Gestión de Miembros**: Añadir, eliminar y cambiar roles de colaboradores
- **Control de Acceso**: Solo los miembros del proyecto pueden ver y trabajar en él
- **Roles Globales**: Administradores con acceso completo y usuarios normales
- **Seguridad**: Autenticación JWT con validación de permisos a nivel de proyecto
- **Arquitectura Escalable**: Diseño por capas con separación de responsabilidades

### Tecnologías principales

| Tecnología | Versión | Rol |
|---|---|---|
| Python | 3.12 | Lenguaje de programación |
| FastAPI | Latest (con soporte standard) | Framework web / API |
| SQLModel | Latest | ORM y validación de modelos |
| SQLAlchemy | Latest | Motor de base de datos subyacente |
| PostgreSQL | 15 | Base de datos relacional |
| python-jose[cryptography] | Latest | Generación y validación de JWT |
| passlib[bcrypt] | 1.7.4 (fijado) | Context de hasheo de contraseñas |
| bcrypt | 4.0.1 (fijado) | Backend de hasheo seguro |
| pydantic-settings | Latest | Gestión de configuración y variables de entorno |
| python-multipart | Latest | Soporte para formularios OAuth2 |
| Uvicorn | Latest | Servidor ASGI |
| Docker / Docker Compose | — | Contenerización del servicio |
| pytest + httpx | Latest | Testing |

---

## 2. Estructura del proyecto

```
gestion-proyectos-tareas-Backend/
│
├── main.py                    # Punto de entrada de la aplicación
├── requirements.txt           # Dependencias del proyecto
├── Dockerfile                 # Imagen Docker de la API
├── docker-compose.yml         # Orquestación de servicios (API + DB)
├── TFG.md                     # Documentación técnica completa
├── REFACTORIZACION_TRELLO.md  # Guía de refactorización colaborativa
│
├── app/
│   ├── core/
│   │   ├── config.py          # Configuración y variables de entorno
│   │   ├── security.py        # Hashing de contraseñas, JWT, validaciones
│   │   └── auth.py            # Autenticación (get_current_user, is_global_admin)
│   │
│   ├── db/
│   │   └── session.py         # Motor SQLAlchemy y generador de sesiones
│   │
│   ├── models/                # Modelos ORM (tablas de la base de datos)
│   │   ├── usuario.py         # Usuarios del sistema
│   │   ├── rol.py             # Roles globales (admin, usuario)
│   │   ├── proyecto.py        # Proyectos colaborativos
│   │   ├── proyecto_usuario.py # Relación usuarios-proyectos con roles (NUEVO)
│   │   ├── tarea.py           # Tareas dentro de proyectos
│   │   └── comentario.py      # Comentarios en tareas
│   │
│   ├── schemas/               # Esquemas de entrada/salida (DTOs)
│   │   ├── usuario.py
│   │   ├── rol.py
│   │   ├── proyecto.py
│   │   ├── proyecto_usuario.py # Schemas para gestión de miembros (NUEVO)
│   │   ├── tarea.py
│   │   └── comentario.py
│   │
│   ├── services/              # Lógica de negocio y acceso a base de datos
│   │   ├── usuario_service.py
│   │   ├── rol_service.py
│   │   ├── proyecto_service.py # Refactorizado: 13 métodos para colaboración
│   │   ├── tarea_service.py    # Actualizado: control de permisos por proyecto
│   │   └── comentario_service.py # Actualizado: control de permisos por proyecto
│   │
│   └── routers/               # Endpoints HTTP por recurso
│       ├── auth.py            # Registro, login, /me
│       ├── usuarios.py
│       ├── roles.py
│       ├── proyectos.py       # Refactorizado: +4 endpoints para gestión de miembros
│       ├── tareas.py          # Actualizado: autenticación en todos los endpoints
│       └── comentarios.py     # Actualizado: autenticación en todos los endpoints
│
└── tests/
    ├── test_main.py           # Tests de los endpoints raíz y health
    └── test_security.py       # Tests del sistema de seguridad y hashing


### Responsabilidad de cada carpeta

- **`app/core/`**: Contiene la configuración transversal de la aplicación: lectura de variables de entorno, lógica criptográfica y el sistema de autenticación basado en tokens JWT.
- **`app/db/`**: Centraliza la creación del motor de base de datos y el proveedor de sesiones que se inyecta en los servicios.
- **`app/models/`**: Define las entidades del dominio como clases Python que además son tablas en la base de datos, usando `SQLModel` con `table=True`.
- **`app/schemas/`**: Define los contratos de la API: qué campos se reciben en las peticiones (Create, Update) y qué se devuelve al cliente (Response). Son clases `SQLModel` sin `table=True`.
- **`app/services/`**: Contiene toda la lógica de negocio y las operaciones CRUD sobre la base de datos. Son clases que reciben la sesión de base de datos por inyección de dependencias.
- **`app/routers/`**: Agrupa los endpoints HTTP. Son finos y delegan todo el trabajo a la capa de servicios.

---

## 3. Flujo general de la aplicación

### Arranque

Cuando se inicia la aplicación con Uvicorn (`uvicorn main:app`), FastAPI ejecuta el evento `startup` definido en `main.py`, que llama a `SQLModel.metadata.create_all(engine)`. Esto inspecciona todos los modelos registrados y crea las tablas en la base de datos si aún no existen, actuando como una migración básica automática.

A continuación, FastAPI registra todos los routers importados en `main.py` con sus respectivos prefijos de URL:

```
/roles        → roles.router
/usuarios     → usuarios.router
/proyectos    → proyectos.router
/tareas       → tareas.router
/comentarios  → comentarios.router
/auth         → auth.router
```

### Ciclo de vida de una petición HTTP

1. El cliente envía una petición HTTP a uno de los endpoints definidos en los routers.
2. FastAPI valida el cuerpo de la petición contra el schema correspondiente (p. ej. `UsuarioCreate`). Si la validación falla, devuelve un error `422 Unprocessable Entity` automáticamente.
3. FastAPI resuelve las dependencias declaradas en el endpoint. La más importante es `Depends()` sobre la clase de servicio, que a su vez inyecta una `Session` de base de datos mediante `Depends(get_session)`.
4. El router llama al método correspondiente del servicio (p. ej. `service.create(usuario)`).
5. El servicio ejecuta la lógica de negocio y las operaciones de base de datos, lanzando `HTTPException` si se producen errores de negocio (recurso no encontrado, credenciales inválidas, etc.).
6. El resultado se serializa según el `response_model` declarado en el router y se devuelve al cliente como JSON.

### Diagrama de dependencias entre capas

```
Router → Service → Model (ORM)
           ↑
         Session (db/session.py)
           ↑
         Engine (config.py → DATABASE_URL)
```

---

## 4. Análisis por módulos

### 4.1 `app/core/config.py` — Configuración

Utiliza `pydantic-settings` para leer variables de entorno y combinarlas con valores por defecto. La clase `Settings` declara dos variables:

- `database_url`: cadena de conexión a PostgreSQL. Tiene un valor por defecto que apunta a `localhost:5432`, pero en Docker se sobreescribe con la variable de entorno `DATABASE_URL`.
- `debug`: boolean que activa el logging SQL de SQLAlchemy cuando es `True`.

Se instancia un objeto global `settings` que es importado por `app/db/session.py`.

### 4.2 `app/db/session.py` — Sesión de base de datos

Crea el `engine` de SQLAlchemy usando la URL de `settings`. Define la función generadora `get_session()`, que abre una sesión de base de datos, la cede (`yield`) al consumidor (el servicio) y la cierra automáticamente al finalizar la petición. Esta función se usa exclusivamente como dependencia de FastAPI con `Depends(get_session)`.

### 4.3 `app/models/` — Modelos ORM

Cada modelo hereda de `SQLModel` con `table=True`, lo que genera tanto la tabla en la base de datos como el modelo Pydantic para validación. Las relaciones entre modelos se definen con `Relationship` de SQLModel, y los imports circulares entre modelos se resuelven usando `TYPE_CHECKING`, de modo que las referencias a otros modelos solo se evalúan en tiempo de análisis estático, no en tiempo de ejecución.

El diagrama de relaciones entre entidades es el siguiente:

```
                    ┌──────────┐
                    │   Rol    │ (Roles globales: admin, usuario)
                    └─────┬────┘
                          │
                          │ 1:N
                          ▼
                    ┌──────────┐
         ┌──────────│ Usuario  │──────────┐
         │          └────┬─────┘          │
         │               │                │
         │ N:M           │ 1:N            │ 1:N
         │  (via         │                │
         │   ProyectoUsu)│                │
         │               │                │
         ▼               ▼                ▼
┌─────────────────┐   ┌────────┐   ┌────────────┐
│ProyectoUsuario  │   │ Tarea  │   │ Comentario │
│ (Tabla          │   │ (asig) │   │            │
│  intermedia)    │   └────────┘   └────────────┘
├─────────────────┤        ▲              ▲
│ - id_proyecto   │        │              │
│ - id_usuario    │        │              │
│ - rol_proyecto  │        │ 1:N          │ N:1
│   (owner/editor │        │              │
│    /viewer)     │        │              │
└────────┬────────┘        │              │
         │                 │              │
         │ N:1             │              │
         ▼                 │              │
    ┌──────────┐           │              │
    │ Proyecto │───────────┘              │
    │          │ 1:N                      │
    │          │──────────────────────────┘
    └──────────┘
```

#### Descripción detallada de relaciones:

**Rol ↔ Usuario** (1:N):
- Un `Rol`global tiene muchos `Usuario`s.
- Cada `Usuario` pertenece a un `Rol` global (admin o usuario).
- Determina permisos a nivel de sistema.

**Usuario ↔ ProyectoUsuario ↔ Proyecto** (N:M con atributos):
- Un `Usuario` puede ser miembro de muchos `Proyecto`s.
- Un `Proyecto` puede tener muchos `Usuario`s como miembros.
- La tabla intermedia `ProyectoUsuario` almacena el rol específico del usuario en cada proyecto (`owner`, `editor`, `viewer`).
- Esta es la relación clave que permite la colaboración.

**Proyecto → Tarea** (1:N con cascada):
- Un `Proyecto` tiene muchas `Tarea`s.
- Una `Tarea` pertenece a un único `Proyecto`.
- Al eliminar un proyecto, todas sus tareas se eliminan automáticamente (`cascade_delete=True`).

**Usuario → Tarea** (1:N opcional):
- Un `Usuario` puede tener varias `Tarea`s asignadas.
- Una `Tarea` puede tener un `Usuario` asignado (o ninguno).
- Esta es la asignación individual de trabajo.

**Tarea → Comentario** (1:N con cascada):
- Una `Tarea` puede tener muchos `Comentario`s.
- Un `Comentario` pertenece a una única `Tarea`.
- Al eliminar una tarea, todos sus comentarios se eliminan automáticamente (`cascade_delete=True`).

**Usuario → Comentario** (1:N):
- Un `Usuario` puede escribir muchos `Comentario`s.
- Un `Comentario` tiene un único autor (`Usuario`).

**Usuarios creadores de proyectos**:
- Cada `Proyecto` tiene un campo `creado_por_id` que referencia al `Usuario` que lo creó.
- El creador se añade automáticamente como miembro con rol `owner` en `ProyectoUsuario`.

#### Modelo ProyectoUsuario (Tabla Intermedia)

Este es el modelo clave que transforma la aplicación en colaborativa:

```python
class ProyectoUsuario(SQLModel, table=True):
    id_proyecto_usuario: int | None = Field(default=None, primary_key=True)
    id_proyecto: int = Field(foreign_key="proyecto.id_proyecto")
    id_usuario: int = Field(foreign_key="usuario.id_usuario")
    rol_proyecto: str = Field(default="viewer")  # owner, editor, viewer
```

**Campos**:
- `id_proyecto_usuario`: Clave primaria autoincrementalde la relación.
- `id_proyecto`: Foreign key hacia Proyecto.
- `id_usuario`: Foreign key hacia Usuario.
- `rol_proyecto`: Rol del usuario en este proyecto específico.

**Roles de proyecto**:
- `owner`: Control total del proyecto (editar, borrar, gestionar miembros).
- `editor`: Puede editar el proyecto y gestionar tareas.
- `viewer`: Solo puede ver el proyecto y comentar.

Este diseño permite que el mismo usuario tenga diferentes roles en diferentes proyectos.

### 4.4 `app/schemas/` — Esquemas (DTOs)

Los schemas actúan como objetos de transferencia de datos (DTOs) entre el cliente y la API. No representan tablas; su único propósito es definir qué datos se aceptan y qué datos se exponen. El patrón utilizado en todos los recursos es consistente:

- **`XxxCreate`**: campos requeridos para crear un recurso. Nunca expone datos internos como `hashed_password` o claves primarias.
- **`XxxResponse`**: campos que se devuelven al cliente. Hereda de `XxxCreate` cuando los campos de respuesta son un superconjunto.
- **`XxxUpdate`**: todos los campos son opcionales (`str | None = None`), lo que permite actualizaciones parciales (PATCH).

En `UsuarioCreate` y `UsuarioUpdate`, el campo `password` incluye validaciones de longitud que se importan directamente de `app.core.security` (`PASSWORD_MIN_LENGTH=8` y `PASSWORD_MAX_LENGTH=128`), lo que garantiza que los límites de contraseña estén centralizados. Las validaciones se aplican usando `Field(min_length=PASSWORD_MIN_LENGTH, max_length=PASSWORD_MAX_LENGTH)`.

### 4.5 `app/services/` — Capa de servicio

Cada servicio es una clase que recibe la sesión de base de datos en su constructor mediante inyección de dependencias. Los servicios básicos implementan operaciones CRUD estándar:

- `create(data)`: construye la entidad ORM desde el schema, la persiste y devuelve un schema de respuesta.
- `get_all()`: devuelve todos los registros.
- `get_by_id(id)`: busca por clave primaria y lanza `HTTPException 404` si no existe.
- `update(id, data)`: aplica solo los campos presentes en el payload usando `model_dump(exclude_unset=True)` y `setattr` en bucle, lo que implementa correctamente la semántica de PATCH.
- `delete(id)`: elimina el registro y devuelve un mensaje de confirmación.

#### UsuarioService

`UsuarioService` tiene un método adicional `get_by_email(email)` utilizado por el sistema de autenticación. Además, en los métodos `create` y `update`, transforma la contraseña en texto plano recibida del schema en un hash bcrypt_sha256 antes de persistirla en el campo `hashed_password`. El servicio captura excepciones `ValueError` de la función `hash_password()` y las convierte en respuestas HTTP 400 con mensajes claros. También valida que el rol exista antes de crear o actualizar un usuario, lanzando `HTTPException 400` si el rol especificado no se encuentra. Usa manejo de `IntegrityError` de SQLAlchemy para capturar conflictos de integridad (como emails duplicados) y devuelve respuestas HTTP 400 controladas.

#### ProyectoService (Completamente refactorizado para colaboración)

`ProyectoService` es el corazón del sistema colaborativo. Ha sido completamente refactorizado con **13 métodos** para gestionar proyectos compartidos con roles y permisos granulares:

**Métodos CRUD básicos con permisos**:

1. **`create(proyecto_data, user_id)`**: Crea un proyecto y añade automáticamente al creador como miembro con rol `owner` en la tabla `ProyectoUsuario`. Esta atomicidad garantiza que todo proyecto tenga al menos un owner.

2. **`get_all()`**: Devuelve todos los proyectos. Solo accesible para admin global.

3. **`get_by_user(user_id)`**: Devuelve únicamente los proyectos donde el usuario es miembro, realizando un JOIN con `ProyectoUsuario`. Usuarios normales solo ven sus proyectos.

4. **`get_by_id(id)`**: Busca un proyecto por ID y lanza 404 si no existe.

5. **`get_by_id_for_user(id, user)`**: Devuelve un proyecto solo si el usuario tiene permisos para verlo. Admin global puede ver cualquier proyecto; usuarios normales solo si son miembros. Lanza 403 si no tiene acceso.

6. **`update(id, proyecto_data, user)`**: Actualiza un proyecto. Solo admin global, owner o editor pueden editar. Viewer recibe 403.

7. **`delete(id, user)`**: Elimina un proyecto. Solo admin global u owner pueden borrar. Editor y viewer reciben 403. Al borrar el proyecto, se eliminan en cascada todas sus tareas, comentarios y membresías.

**Métodos de gestión de miembros**:

8. **`add_member(project_id, member_data, user)`**: Añade un nuevo miembro al proyecto. Solo admin u owner pueden añadir miembros. Valida que:
   - El proyecto existe
   - El usuario a añadir existe
   - El usuario no esté ya como miembro (evita duplicados)
   - El rol sea válido (owner/editor/viewer)

9. **`remove_member(project_id, user_id_to_remove, user)`**: Elimina un miembro del proyecto. Solo admin u owner pueden quitar miembros. Implementa protección crítica: no permite eliminar el último owner del proyecto, garantizando que siempre haya al menos un propietario.

10. **`update_member_role(project_id, member_id, rol_proyecto, user)`**: Cambia el rol de un miembro. Solo admin u owner pueden cambiar roles. Si se intenta cambiar el rol del último owner a otro rol, lanza 400 para prevenir dejar el proyecto sin dueño.

11. **`get_project_members(project_id, user)`**: Lista todos los miembros del proyecto con sus roles. Admin o cualquier miembro del proyecto puede ver la lista.

**Métodos auxiliares de permisos**:

12. **`get_user_project_role(project_id, user_id)`**: Devuelve el rol del usuario en el proyecto (`owner`, `editor`, `viewer`) o `None` si no es miembro. Método fundamental usado por otros servicios para verificar permisos.

13. **`is_project_member(project_id, user_id)`**: Devuelve `True` si el usuario es miembro del proyecto, `False` en caso contrario.

14. **`has_project_permission(project_id, user, allowed_roles)`**: Método central de autorización. Verifica si el usuario tiene permiso en el proyecto según una lista de roles permitidos. Admin global siempre devuelve `True`. Para usuarios normales, obtiene su rol en el proyecto y verifica si está en `allowed_roles`.

**Ejemplo de uso del sistema de permisos**:

```python
# En el servicio
if not self.has_project_permission(id, user, ["owner", "editor"]):
    raise HTTPException(403, "No tienes permiso para editar")
```

#### TareaService (Actualizado con control de permisos)

`TareaService` ha sido actualizado para respetar los permisos del proyecto:

- **Todos los métodos ahora reciben `user`** como parámetro para validar permisos.

- **Métodos privados de verificación**:
  - `_get_user_project_role(project_id, user_id)`: Obtiene el rol del usuario en el proyecto de la tarea.
  - `_can_modify_task(project_id, user)`: Verifica si el usuario puede modificar tareas (admin, owner o editor).
  - `_can_view_task(project_id, user)`: Verifica si el usuario puede ver tareas (admin o cualquier miembro).

- **`create(tarea_data, user)`**: Solo owner, editor o admin pueden crear tareas. Viewer recibe 403.

- **`get_all(user)`**: 
  - Admin global: ve todas las tareas del sistema.
  - Usuario normal: solo ve tareas de proyectos donde es miembro (hace JOIN con `ProyectoUsuario`).

- **`get_by_id(id, user)`**: Verifica que el usuario sea miembro del proyecto de la tarea antes de devolverla.

- **`update(id, tarea_data, user)`**: Solo owner, editor o admin pueden editar tareas.

- **`delete(id, user)`**: Solo owner, editor o admin pueden borrar tareas.

#### ComentarioService (Actualizado con control de permisos)

`ComentarioService` implementa permisos similares más restricciones de autoría:

- **Métodos privados**:
  - `_get_user_project_role(project_id, user_id)`: Consulta el rol en el proyecto.
  - `_can_access_task(task_id, user)`: Verifica si el usuario es miembro del proyecto de la tarea.

- **`create(comentario_data, user)`**: Solo miembros del proyecto pueden comentar (owner, editor o viewer).

- **`get_all(user)`**: 
  - Admin global: ve todos los comentarios.
  - Usuario normal: solo comentarios de tareas de proyectos donde es miembro (doble JOIN: Comentario → Tarea → Proyecto → ProyectoUsuario).

- **`get_by_id(id, user)`**: Verifica membresía antes de devolver el comentario.

- **`update(id, comentario_data, user)`**: Solo el autor del comentario o admin global pueden editarlo. Otros miembros reciben 403.

- **`delete(id, user)`**: Solo el autor o admin pueden borrar el comentario.

Esta arquitectura de servicios garantiza que los permisos se verifiquen consistentemente en toda la aplicación, con la lógica centralizada y reutilizable.

### 4.6 `app/routers/` — Capa de presentación (Endpoints)

Los routers son deliberadamente delgados: declaran los endpoints, validan los datos de entrada vía schemas y delegan en los servicios. Cada router tiene un `prefix` y `tags` configurados para la documentación automática de Swagger/OpenAPI.

#### Router de Autenticación (`auth.py`)

Tres endpoints principales:
- `POST /auth/register`: crea un nuevo usuario utilizando `UsuarioService.create()`.
- `POST /auth/login`: valida credenciales usando `OAuth2PasswordRequestForm`, valida la longitud de la contraseña antes de verificarla, y devuelve un token JWT si las credenciales son correctas. El endpoint captura explícitamente errores de longitud de contraseña y devuelve mensajes HTTP 400 claros.
- `GET /auth/me`: devuelve los datos del usuario autenticado actual usando la dependencia `get_current_user`.

#### Router de Proyectos (`proyectos.py`) — Completamente refactorizado

Este router implementa colaboración completa con gestión de miembros. **Todos los endpoints requieren autenticación JWT** vía `Depends(get_current_user)`.

**Endpoints de proyectos**:

1. **`POST /proyectos/`** (response_model=ProyectoResponse)
   - Crea un proyecto nuevo.
   - El usuario autenticado se añade automáticamente como `owner`.
   - Body: `ProyectoCreate` (nombre, descripción, fecha_creacion).

2. **`GET /proyectos/`** (response_model=list[ProyectoResponse])
   - Lista proyectos según permisos:
     - **Admin global**: ve todos los proyectos del sistema.
     - **Usuario normal**: solo ve proyectos donde es miembro.
   - La lógica de filtrado está en `ProyectoService.get_all()` o `get_by_user()`.

3. **`GET /proyectos/{id}`** (response_model=ProyectoResponse)
   - Obtiene un proyecto específico.
   - Solo admin o miembros pueden verlo.
   - No miembros reciben 403 Forbidden.

4. **`PATCH /proyectos/{id}`** (response_model=ProyectoResponse)
   - Actualiza un proyecto.
   - Solo admin, owner o editor pueden editar.
   - Viewer recibe 403.
   - Body: `ProyectoUpdate` (todos los campos opcionales).

5. **`DELETE /proyectos/{id}`** (response_model=dict)
   - Elimina un proyecto y todas sus tareas/comentarios en cascada.
   - Solo admin u owner pueden borrar.
   - Editor y viewer reciben 403.

**Endpoints de gestión de miembros** (NUEVOS):

6. **`GET /proyectos/{id}/miembros`** (response_model=list[ProyectoUsuarioResponse])
   - Lista todos los miembros del proyecto con sus roles.
   - Admin o cualquier miembro puede ver la lista.
   - Devuelve: `[{id_proyecto_usuario, id_proyecto, id_usuario, rol_proyecto}, ...]`

7. **`POST /proyectos/{id}/miembros`** (response_model=ProyectoUsuarioResponse)
   - Añade un nuevo miembro al proyecto.
   - Solo admin u owner pueden añadir miembros.
   - Body: `ProyectoUsuarioCreate` (id_usuario, rol_proyecto).
   - Validaciones:
     - Usuario existe
     - Usuario no está ya como miembro
     - Rol es válido (owner/editor/viewer)

8. **`PATCH /proyectos/{id}/miembros/{id_usuario}`** (response_model=ProyectoUsuarioResponse)
   - Cambia el rol de un miembro existente.
   - Solo admin u owner pueden cambiar roles.
   - Body: `ProyectoUsuarioUpdate` (rol_proyecto).
   - Protección: no permite cambiar el último owner a otro rol.

9. **`DELETE /proyectos/{id}/miembros/{id_usuario}`** (response_model=dict)
   - Elimina un miembro del proyecto.
   - Solo admin u owner pueden quitar miembros.
   - Protección crítica: no permite eliminar el último owner.

**Flujo típico de colaboración**:
```
1. Usuario A crea proyecto → automáticamente es owner
2. Usuario A añade Usuario B como viewer → POST /proyectos/1/miembros
3. Usuario B lista proyectos → GET /proyectos/ → ahora ve el proyecto
4. Usuario A cambia Usuario B a editor → PATCH /proyectos/1/miembros/2
5. Usuario B ahora puede editar tareas del proyecto
```

#### Router de Tareas (`tareas.py`) — Actualizado con autenticación

**Todos los endpoints ahora requieren autenticación** y pasan el usuario al servicio para validar permisos.

- **`POST /tareas/`**: Crea una tarea. Solo owner/editor/admin pueden crear. El servicio verifica que el usuario sea miembro del proyecto especificado en `id_proyecto`.

- **`GET /tareas/`**: Lista tareas según permisos:
  - Admin: todas las tareas del sistema.
  - Usuario normal: solo tareas de proyectos donde es miembro.

- **`GET /tareas/{id}`**: Obtiene una tarea. Verifica que el usuario sea miembro del proyecto de la tarea.

- **`PATCH /tareas/{id}`**: Actualiza una tarea. Solo owner/editor/admin pueden editar.

- **`DELETE /tareas/{id}`**: Elimina una tarea. Solo owner/editor/admin pueden borrar.

#### Router de Comentarios (`comentarios.py`) — Actualizado con autenticación

**Todos los endpoints requieren autenticación** y verifican permisos del proyecto más autoría.

- **`POST /comentarios/`**: Crea un comentario. Cualquier miembro del proyecto puede comentar (owner/editor/viewer).

- **`GET /comentarios/`**: Lista comentarios según permisos:
  - Admin: todos los comentarios.
  - Usuario normal: solo comentarios de proyectos donde es miembro.

- **`GET /comentarios/{id}`**: Obtiene un comentario. Verifica membresía del proyecto.

- **`PATCH /comentarios/{id}`**: Actualiza un comentario. Solo el autor o admin pueden editar.

- **`DELETE /comentarios/{id}`**: Elimina un comentario. Solo el autor o admin pueden borrar.

#### Routers sin autenticación

Los routers de `roles` y `usuarios` siguen el patrón CRUD estándar sin autenticación obligatoria en sus endpoints, aunque en producción deberían protegerse para que solo admin pueda gestionarlos.

---

## 5. Autenticación y seguridad

### Flujo de autenticación

El sistema implementa autenticación basada en **JWT (JSON Web Tokens)** con el esquema **OAuth2 Password Flow**, el estándar más habitual para APIs REST.

**Registro** (`POST /auth/register`):
1. El cliente envía nombre, email, contraseña y rol.
2. `UsuarioService.create()` valida la longitud de la contraseña (mínimo 8, máximo 128 caracteres) y la hashea con `bcrypt_sha256` antes de guardarla.
3. La contraseña en texto plano nunca se persiste ni se registra en ningún log.
4. Si la contraseña no cumple con los requisitos de longitud, se devuelve HTTP 400 con un mensaje descriptivo.

**Login** (`POST /auth/login`):
1. El cliente envía `username` (= email) y `password` como formulario (`application/x-www-form-urlencoded`), que es el formato estándar de OAuth2.
2. Se valida primero la longitud de la contraseña. Si excede el límite, se devuelve HTTP 400 con un mensaje claro: "La contraseña no puede superar 128 caracteres".
3. Se busca el usuario por email. Si no existe o la contraseña no coincide, se lanza `HTTPException 400` con el mensaje "Credenciales inválidas".
4. Si las credenciales son válidas, se genera un JWT con `create_access_token(email)` y se devuelve al cliente.

**Acceso a rutas protegidas**:
1. El cliente incluye el token en la cabecera `Authorization: Bearer <token>`.
2. La dependencia `get_current_user` (en `app/core/auth.py`) intercepta la petición, extrae el token usando `OAuth2PasswordBearer`, lo decodifica con `python-jose` y obtiene el email del campo `sub`.
3. Se busca el usuario en la base de datos usando `UsuarioService.get_by_email()`. Si el token es inválido o el usuario no existe, se lanza `HTTPException 401`.
4. El usuario autenticado se devuelve y está disponible en el endpoint para verificar permisos.

**Verificación de rol global de admin** (Función `is_global_admin`):

La función `is_global_admin(user)` en `app/core/auth.py` verifica si el usuario tiene rol global de administrador:

```python
def is_global_admin(user) -> bool:
    if hasattr(user, 'rol') and user.rol:
        rol_nombre = user.rol.nombre.lower()
        return rol_nombre in ["admin", "administrador"]
    return False
```

Esta función es crucial para:
- Permitir a admins acceder a todos los proyectos sin ser miembros
- Saltarse verificaciones de permisos en servicios
- Gestionar usuarios y roles del sistema

La distinción entre **rol global** (admin/usuario) y **rol de proyecto** (owner/editor/viewer) es fundamental en la arquitectura de permisos.

### Detalles de implementación en `app/core/security.py`

- **Constantes de configuración**:
  - `SECRET_KEY`: clave secreta para firmar los JWT (actualmente hardcodeada, debe externalizarse en producción).
  - `ALGORITHM`: "HS256" (HMAC con SHA-256).
  - `ACCESS_TOKEN_EXPIRE_MINUTES`: 30 minutos.
  - `PASSWORD_MIN_LENGTH`: 8 caracteres.
  - `PASSWORD_MAX_LENGTH`: 128 caracteres.

- **Esquema de hasheo híbrido**: se usa `CryptContext` de `passlib` con dos esquemas:
  - `bcrypt_sha256`: esquema principal que permite contraseñas de hasta 128 caracteres superando el límite técnico de bcrypt puro (72 bytes). Funciona aplicando SHA-256 a la contraseña antes de pasarla a bcrypt, convirtiendo cualquier contraseña en un hash de longitud fija.
  - `bcrypt`: se mantiene como esquema deprecated para retrocompatibilidad con hashes existentes en la base de datos.
  - El parámetro `deprecated="auto"` permite que `passlib` identifique hashes antiguos y los actualice automáticamente al nuevo esquema cuando un usuario se autentica correctamente.

- **Función `validate_password_length(password)`**: valida que la contraseña cumpla con los límites mínimo y máximo. Lanza `ValueError` con mensajes descriptivos en español si no cumple. Esta función se usa tanto en `hash_password()` como en el endpoint de login.

- **Función `hash_password(password)`**: valida la longitud de la contraseña y genera el hash usando `pwd_context.hash()`. Lanza `ValueError` si la contraseña es demasiado corta o larga.

- **Función `verify_password(password, hashed)`**: verifica una contraseña contra su hash. Captura internamente cualquier `ValueError` que pueda lanzar bcrypt y devuelve `False` en lugar de propagar la excepción, evitando errores 500 en producción.

- **Función `create_access_token(sub)`**: genera un JWT con el email del usuario en el campo `sub` y una fecha de expiración. Usa `datetime.now(UTC)` en lugar de `datetime.utcnow()` (deprecado en Python 3.12+).

### Puntos de atención

- La `SECRET_KEY` está hardcodeada en el código (`"clave-super-secreta"`). **En producción debe externalizarse como variable de entorno** para evitar comprometer la seguridad si el código fuente se filtra.
- No se implementa refresh token, por lo que el usuario debe autenticarse de nuevo pasados los 30 minutos.
- El uso de `bcrypt_sha256` permite contraseñas más largas sin romper la compatibilidad con hashes `bcrypt` existentes, ofreciendo una mejor experiencia de usuario.

---

## 5B. Sistema de Roles y Permisos (Arquitectura Colaborativa)

El proyecto implementa un sistema de doble capa de roles que permite colaboración granular tipo Trello:

### Roles Globales (Tabla `rol`)

Determinan permisos a nivel de sistema:

| Rol | Permisos |
|---|---|
| **admin** | Acceso completo a todo el sistema. Puede ver y gestionar todos los proyectos sin ser miembro. Puede crear/editar/borrar usuarios y roles. |
| **usuario** | Usuario normal del sistema. Solo puede ver y gestionar proyectos donde es miembro. |

### Roles de Proyecto (Tabla `proyecto_usuario`)

Determinan permisos dentro de cada proyecto específico. El mismo usuario puede tener diferentes roles en diferentes proyectos:

#### owner (Propietario)

**Permisos completos del proyecto**:
- ✅ Ver el proyecto y todas sus tareas/comentarios
- ✅ Editar metadatos del proyecto (nombre, descripción, fechas)
- ✅ **Borrar el proyecto** (acción destructiva)
- ✅ **Añadir nuevos miembros** al proyecto
- ✅ **Quitar miembros** del proyecto
- ✅ **Cambiar roles** de otros miembros
- ✅ Crear, editar y borrar tareas
- ✅ Crear, editar y borrar comentarios propios
- ✅ Ver comentarios de otros

**Restricciones**:
- No puede eliminarse a sí mismo si es el último owner del proyecto (protección crítica)
- No puede cambiar su propio rol si es el único owner

#### editor (Colaborador con edición)

**Permisos de edición**:
- ✅ Ver el proyecto y todas sus tareas/comentarios
- ✅ Editar metadatos del proyecto
- ✅ Crear, editar y borrar tareas
- ✅ Crear, editar y borrar comentarios propios
- ✅ Ver comentarios de otros

**No puede**:
- ❌ Borrar el proyecto
- ❌ Añadir o quitar miembros
- ❌ Cambiar roles de miembros
- ❌ Editar/borrar comentarios de otros

#### viewer (Espectador)

**Permisos de solo lectura con comentarios**:
- ✅ Ver el proyecto y todas sus tareas
- ✅ Ver todos los comentarios
- ✅ Crear comentarios propios
- ✅ Editar y borrar sus propios comentarios

**No puede**:
- ❌ Editar metadatos del proyecto
- ❌ Crear, editar o borrar tareas
- ❌ Borrar el proyecto
- ❌ Gestionar miembros
- ❌ Cambiar roles
- ❌ Editar/borrar comentarios de otros

### Tabla de Permisos Completa

| Acción | admin (global) | owner | editor | viewer | no miembro |
|---|---|---|---|---|---|
| Ver proyecto | ✅ | ✅ | ✅ | ✅ | ❌ |
| Editar proyecto | ✅ | ✅ | ✅ | ❌ | ❌ |
| Borrar proyecto | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ver tareas | ✅ | ✅ | ✅ | ✅ | ❌ |
| Crear tareas | ✅ | ✅ | ✅ | ❌ | ❌ |
| Editar tareas | ✅ | ✅ | ✅ | ❌ | ❌ |
| Borrar tareas | ✅ | ✅ | ✅ | ❌ | ❌ |
| Ver comentarios | ✅ | ✅ | ✅ | ✅ | ❌ |
| Crear comentarios | ✅ | ✅ | ✅ | ✅ | ❌ |
| Editar comentarios propios | ✅ | ✅ | ✅ | ✅ | ❌ |
| Editar comentarios de otros | ✅ | ❌ | ❌ | ❌ | ❌ |
| Añadir miembros | ✅ | ✅ | ❌ | ❌ | ❌ |
| Quitar miembros | ✅ | ✅ | ❌ | ❌ | ❌ |
| Cambiar roles | ✅ | ✅ | ❌ | ❌ | ❌ |

### Implementación Técnica de Permisos

**Verificación en cascada**:

1. **Nivel 1 - Autenticación**: `get_current_user()` verifica que el token JWT sea válido.
2. **Nivel 2 - Rol global**: `is_global_admin()` verifica si es admin (acceso total).
3. **Nivel 3 - Membresía**: `is_project_member()` verifica si es miembro del proyecto.
4. **Nivel 4 - Rol de proyecto**: `has_project_permission()` verifica el rol específico.

**Ejemplo de flujo de verificación en ProyectoService.update()**:

```python
def update(self, id: int, proyecto_data: ProyectoUpdate, user):
    proyecto = self.get_by_id(id)  # Verifica que existe (404 si no)
    
    # Verificar permisos: admin, owner o editor pueden editar
    if not self.has_project_permission(id, user, ["owner", "editor"]):
        raise HTTPException(403, "No tienes permiso para editar")
    
    # ... aplicar cambios ...
```

**Método central `has_project_permission()`**:

```python
def has_project_permission(
    self, project_id: int, user, allowed_roles: list[str]
) -> bool:
    # Admin global siempre tiene permiso
    if is_global_admin(user):
        return True
    
    # Obtener rol del usuario en el proyecto
    rol = self.get_user_project_role(project_id, user.id_usuario)
    
    # Verificar si está en los roles permitidos
    return rol in allowed_roles if rol else False
```

### Códigos de Estado HTTP para Autorización

- **401 Unauthorized**: Token JWT inválido, expirado o no proporcionado.
- **403 Forbidden**: Usuario autenticado pero sin permisos para la acción.
- **404 Not Found**: Recurso no existe o el usuario no tiene acceso (también oculta existencia).

### Protecciones Críticas Implementadas

1. **Protección de último owner**: No se permite eliminar o cambiar el rol del último owner de un proyecto, garantizando que siempre haya al menos un propietario.

2. **Validación de membresía**: Todas las operaciones sobre tareas y comentarios verifican que el usuario sea miembro del proyecto antes de proceder.

3. **Prevención de duplicados**: No se permite añadir dos veces al mismo usuario como miembro de un proyecto.

4. **Validación de roles**: Solo se permiten los valores `owner`, `editor` y `viewer` en `rol_proyecto`.

5. **Cascade delete**: Al eliminar un proyecto, se eliminan automáticamente todas sus membresías, tareas y comentarios, manteniendo integridad referencial.

### Escenario de Ejemplo

```
Usuario A (admin global):
├─ Proyecto 1: owner
├─ Proyecto 2: no miembro (pero puede acceder por ser admin)
└─ Puede ver TODOS los proyectos del sistema

Usuario B (usuario normal):
├─ Proyecto 1: editor
├─ Proyecto 3: viewer
└─ Solo ve Proyectos 1 y 3 (donde es miembro)

Usuario C (usuario normal):
├─ Proyecto 3: owner
└─ Solo ve Proyecto 3
    ├─ Puede añadir Usuario D como member
    ├─ Puede cambiar rol de Usuario B de viewer a editor
    └─ Puede quitar a Usuario B del proyecto
```

Este sistema garantiza colaboración segura y escalable donde cada usuario solo ve y gestiona lo que le corresponde.

---

## 6. Configuración y variables de entorno

El archivo de referencia es `app/core/config.py`. Las variables admitidas son:

| Variable | Descripción | Valor por defecto |
|---|---|---|
| `DATABASE_URL` | Cadena de conexión completa a PostgreSQL | `postgresql+psycopg2://admin:admin@localhost:5432/gestion` |
| `DEBUG` | Activa el log de queries SQL en consola | `False` |
| `APP_NAME` | Nombre de la aplicación devuelto en el endpoint raíz | `gestion-proyectos-tareas-backend` |

Las variables se pueden definir en un archivo `.env` en la raíz del proyecto (compatible gracias a `pydantic-settings`) o como variables de entorno del sistema/contenedor.

En el `docker-compose.yml`, las variables `DATABASE_URL` y `DEBUG` se pasan directamente al contenedor `api`, apuntando `DATABASE_URL` al servicio `db` usando el nombre del contenedor como hostname (`@db:5432`), que es la forma estándar de comunicación entre servicios en Docker Compose.

---

## 7. Estilo de código y calidad

El proyecto utiliza **flake8** como linter de sintaxis y estilo, aplicando las convenciones de **PEP 8**:

- Líneas de máximo 79 caracteres.
- Dos líneas en blanco entre funciones y clases de nivel superior.
- Espacios después de las comas en los argumentos.
- Una línea en blanco al final de cada archivo.
- Sin imports redundantes o sin usar.

El código es consistente en su estructura: todos los routers, servicios y schemas siguen exactamente el mismo patrón, lo que facilita la lectura y el onboarding. Los nombres de variables, clases y endpoints están en español, lo que refleja el dominio del negocio del proyecto académico.

El uso de type hints de Python moderno (p. ej. `str | None` en lugar de `Optional[str]`) indica compatibilidad con Python 3.10+ y sigue las recomendaciones actuales del lenguaje.

---

## 8. Dependencias externas

| Librería | Uso | Acoplamiento |
|---|---|---|
| **FastAPI** | Framework principal: enrutado, validación, OpenAPI, inyección de dependencias | Alto — es el núcleo del proyecto |
| **SQLModel** | ORM y validación combinados. Fusión de SQLAlchemy + Pydantic | Alto — los modelos y schemas dependen de él directamente |
| **SQLAlchemy** | Motor de base de datos subyacente a SQLModel | Medio — se usa indirectamente, salvo en `create_engine` y `Session` |
| **psycopg2-binary** | Driver de PostgreSQL para Python | Medio — solo importa si se cambia de base de datos |
| **python-jose[cryptography]** | Codificación/decodificación de JWT con soporte criptográfico | Medio — aislado en `security.py` y `auth.py` |
| **passlib[bcrypt]** | Context de hasheo de contraseñas, versión 1.7.4 fijada | Medio — aislado en `security.py` |
| **bcrypt** | Backend de hasheo bcrypt, versión 4.0.1 fijada por compatibilidad con passlib 1.7.4 | Medio — dependencia de passlib, uso indirecto |
| **pydantic-settings** | Lectura de variables de entorno en `Settings` | Bajo — aislado en `config.py` |
| **python-multipart** | Necesario para recibir formularios (`OAuth2PasswordRequestForm`) | Bajo — dependencia implícita de FastAPI |
| **uvicorn** | Servidor ASGI para ejecutar la aplicación | Alto en despliegue, nulo en código |
| **pytest + httpx** | Framework de tests y cliente HTTP asíncrono para testing | Solo en desarrollo/CI |

---

## 9. Puntos fuertes del diseño

**Separación clara de responsabilidades (Layered Architecture)**: el proyecto distingue correctamente entre la capa de presentación (routers), la capa de negocio (services) y la capa de datos (models). Los routers no contienen lógica de negocio; los modelos no contienen lógica de negocio. Esto hace el código mantenible y testeable de forma independiente.

**Inyección de dependencias nativa de FastAPI**: el uso de `Depends()` para inyectar tanto la sesión de base de datos como los servicios y el usuario autenticado es idiomático y correcto. Permite sustituir dependencias fácilmente en los tests sin modificar el código de producción.

**Prevención de errores 500 en autenticación**: las funciones de seguridad (`verify_password`, `hash_password`) capturan excepciones internas y las convierten en respuestas HTTP controladas (400 o credenciales inválidas), evitando que errores de validación lleguen al usuario como errores 500 internos del servidor.

**Validación de integridad referencial**: `UsuarioService` valida explícitamente que el rol exista antes de crear o actualizar un usuario, y captura errores de `IntegrityError` de SQLAlchemy para devolver mensajes HTTP 400 descriptivos en lugar de errores genéricos.

**Gestión de contraseñas segura y escalable**: el uso de `bcrypt_sha256` como esquema principal permite contraseñas de hasta 128 caracteres mientras mantiene compatibilidad con hashes `bcrypt` legacy, ofreciendo una mejor experiencia de usuario sin comprometer la seguridad.

**Prevención de imports circulares con `TYPE_CHECKING`**: los modelos tienen relaciones bidireccionales entre sí, lo que generaría imports circulares. El uso de `if TYPE_CHECKING:` para esos imports y el uso de strings como anotaciones de tipo (`"Tarea"`, `"Usuario"`) es la solución correcta y recomendada en Python.

**Actualizaciones parciales correctas con PATCH**: el uso de `model_dump(exclude_unset=True)` asegura que solo los campos incluidos en el cuerpo de la petición se actualicen, respetando la semántica HTTP de PATCH en lugar de PUT (que reemplazaría todos los campos).

**Sistema de permisos granular y escalable**: La implementación de roles por proyecto mediante la tabla intermedia `ProyectoUsuario` permite colaboración flexible donde el mismo usuario puede tener diferentes niveles de acceso en diferentes proyectos. Esto es escalable y permite casos de uso complejos sin modificar la estructura de datos.

**Protecciones críticas de integridad**: El sistema implementa validaciones que previenen estados inconsistentes:
- No permite eliminar el último owner de un proyecto (garantiza gobernanza)
- No permite añadir dos veces al mismo usuario como miembro (evita duplicados)
- Validación de roles permitidos (owner/editor/viewer)
- Cascade delete automático de membresías, tareas y comentarios al borrar un proyecto

**Centralización de lógica de autorización**: El método `has_project_permission()` en `ProyectoService` centraliza toda la verificación de permisos, reutilizándose en los otros servicios (`TareaService`, `ComentarioService`). Esto evita duplicación de código y garantiza consistencia.

**Separación clara entre rol global y rol de proyecto**: La arquitectura distingue correctamente entre:
- `Rol` (tabla): roles a nivel sistema (admin/usuario) que determinan acceso general
- `rol_proyecto` (campo en ProyectoUsuario): roles dentro de cada proyecto (owner/editor/viewer)

Esta separación permite control fino de acceso sin mezclar concerns.

**Admin global con bypass inteligente**: El sistema verifica primero si el usuario es admin global en todas las operaciones con `is_global_admin()`, permitiendo acceso administrativo sin necesidad de añadir al admin como miembro de cada proyecto. Esto facilita tareas de mantenimiento y supervisión.

**Manejo de errores con códigos HTTP semánticamente correctos**:
- `401 Unauthorized`: problemas de autenticación (token inválido)
- `403 Forbidden`: usuario autenticado pero sin permisos
- `404 Not Found`: recurso no existe o usuario no tiene acceso (también oculta existencia cuando no debe verse)
- `400 Bad Request`: datos inválidos, roles incorrectos, duplicados

Esta distinción clara mejora la experiencia del cliente y facilita debugging.

**Eliminación en cascada declarativa**: la relación entre proyectos y tareas, y entre tareas y comentarios, tienen `cascade_delete=True`, lo que garantiza la integridad referencial automáticamente a nivel del ORM sin necesidad de lógica manual.

**Contenerización completa**: el `Dockerfile` y `docker-compose.yml` hacen que el proyecto sea completamente reproducible en cualquier entorno sin dependencias del sistema operativo local.

**Documentación automática**: FastAPI genera automáticamente una interfaz Swagger UI en `/docs` y ReDoc en `/redoc`, incluyendo todos los schemas de entrada/salida, lo que facilita la colaboración con equipos de frontend o QA.

---

## 10. Mejoras recientes y evolución del proyecto

### 10.1 Resolución del problema bcrypt/passlib

**Contexto del problema**: En versiones anteriores, el proyecto experimentaba errores 500 durante el login debido a una incompatibilidad entre `passlib` y versiones recientes de `bcrypt` (4.1.x+), junto con el límite técnico de bcrypt puro de 72 bytes para contraseñas.

**Solución implementada**:
1. **Fijación de versiones compatibles**: Se fijaron `passlib==1.7.4` y `bcrypt==4.0.1` en `requirements.txt` para garantizar compatibilidad estable.
2. **Esquema híbrido bcrypt_sha256 + bcrypt**: Se configuró `CryptContext` con dos esquemas:
   - `bcrypt_sha256`: esquema principal que supera el límite de 72 bytes aplicando SHA-256 a la contraseña antes del hash bcrypt.
   - `bcrypt`: mantenido como deprecated para compatibilidad con hashes existentes.
3. **Límites aumentados y centralizados**: Se aumentó el límite de contraseña de 72 a 128 caracteres, centralizando las constantes `PASSWORD_MIN_LENGTH` y `PASSWORD_MAX_LENGTH` en `security.py`.
4. **Validación previa en login**: Se añadió validación de longitud de contraseña en el endpoint de login antes de verificar credenciales, devolviendo mensajes HTTP 400 claros.
5. **Manejo robusto de errores**: `verify_password()` captura internamente `ValueError` y devuelve `False` en lugar de propagar excepciones, evitando errores 500.

**Beneficios**:
- Experiencia de usuario mejorada con mensajes de error claros y descriptivos.
- Mayor seguridad al permitir contraseñas más largas.
- Compatibilidad retroactiva con usuarios existentes.
- Estabilidad en producción sin errores 500 inesperados.

### 10.2 Compatibilidad con Python 3.12

El proyecto utiliza las últimas características de Python 3.12:
- **Type hints modernos**: uso de `str | None` en lugar de `Optional[str]`.
- **Timezone-aware datetimes**: uso de `datetime.now(UTC)` en lugar de `datetime.utcnow()` deprecado.
- **Importación de `UTC` directamente desde `datetime`**: disponible desde Python 3.11+.

### 10.3 Calidad de código

Se aplica **flake8** como linter estándar con las convenciones PEP 8:
- Líneas de máximo 79 caracteres.
- Sin líneas en blanco al final de archivos.
- Imports ordenados y sin redundancias.
- Dos líneas en blanco entre definiciones de nivel superior.

El proyecto pasa todos los tests de calidad sin warnings ni errores.

### 10.4 Mejoras futuras recomendadas

A pesar de que el proyecto está funcional y cumple con su propósito, existen algunos warnings de deprecación que deberían abordarse en futuras iteraciones:

1. **Migración de `on_event` a lifespan handlers**: FastAPI ha deprecado el uso de `@app.on_event("startup")` en favor de los nuevos lifespan event handlers. Esto permitiría un manejo más limpio de eventos de inicio y cierre de la aplicación.

2. **Actualización de `pydantic-settings`**: La clase `Settings` en `config.py` utiliza `class Config` que está deprecado en Pydantic V2. Debería migrarse a `ConfigDict` según las guías de migración de Pydantic V2.

3. **Externalización de la SECRET_KEY**: La clave secreta para JWT debe moverse a variables de entorno en lugar de estar hardcodeada en el código fuente.

4. **Implementación de refresh tokens**: Para mejorar la experiencia de usuario, se podría implementar un sistema de refresh tokens que evite que los usuarios tengan que autenticarse cada 30 minutos.


Estas mejoras no son críticas para el funcionamiento actual pero aumentarían la robustez y mantenibilidad del proyecto a largo plazo.

---

## 11. Cómo ejecutar y desarrollar el proyecto

### Opción A — Con Docker Compose (recomendado)

Este modo levanta tanto la base de datos PostgreSQL como la API en contenedores aislados.

```bash
# Construir las imágenes y levantar los servicios
docker-compose up --build

# Detener los servicios
docker-compose down

# Detener y eliminar los volúmenes (borra los datos de la BD)
docker-compose down -v
```

La API estará disponible en `http://localhost:8000`.

### Opción B — En local con entorno virtual

```bash
# Crear y activar el entorno virtual
python -m venv venv
.\venv\Scripts\Activate.ps1       # Windows PowerShell
# source venv/bin/activate         # Linux / macOS

# Instalar dependencias
pip install -r requirements.txt

# Crear el archivo de entorno (opcional, tiene valores por defecto)
# Crear .env con DATABASE_URL=postgresql+psycopg2://admin:admin@localhost:5432/gestion

# Iniciar la aplicación
uvicorn main:app --reload
```

> Se requiere tener una instancia de PostgreSQL accesible con los parámetros de conexión configurados.

### Ejecutar los tests

```bash
pytest tests/
```

Los tests actuales incluyen:
- `tests/test_main.py`: comprueba los endpoints `GET /` y `GET /health` mediante el `TestClient` de FastAPI, que permite hacer peticiones HTTP sin levantar un servidor real.
- `tests/test_security.py`: prueba el sistema de hashing y verificación de contraseñas, incluyendo:
  - Hash y verificación normal de contraseñas.
  - Soporte para contraseñas al límite máximo permitido (128 caracteres).
  - Validación de mensajes de error claros cuando se excede el límite.
  - Compatibilidad con hashes legacy de `bcrypt` puro.

### Acceder a la documentación interactiva

Con la aplicación en ejecución, abrir en el navegador:

- **Swagger UI**: `http://localhost:8000/docs`
- **ReDoc**: `http://localhost:8000/redoc`

### Comandos de calidad de código

```bash
# Verificar el estilo con flake8
flake8 app/

# Ejecutar los tests con salida detallada
pytest tests/ -v
```

---

*Documento generado el 30 de abril de 2026 — Proyecto TFG: Gestión de Proyectos y Tareas (Backend)*

**Última actualización**: 30 de abril de 2026 — Sistema refactorizado a arquitectura colaborativa tipo Trello con proyectos compartidos y roles granulares.

**Historial de cambios principales**:
- **30-04-2026**: Refactorización completa a sistema colaborativo. Implementación de `ProyectoUsuario`, roles por proyecto (owner/editor/viewer), gestión de miembros, y control de permisos granular en todos los servicios.
- **28-04-2026**: Mejoras en seguridad: sistema de contraseñas bcrypt_sha256, límites aumentados a 128 caracteres, compatibilidad Python 3.12.

Para detalles completos sobre la refactorización colaborativa, consultar: `REFACTORIZACION_TRELLO.md`

