# App Gestión de Proyectos y Tareas (Frontend)

Aplicación cliente multiplataforma desarrollada en **.NET MAUI** para la gestión de proyectos, tareas y usuarios.

Este proyecto forma parte del **Trabajo de Fin de Grado del CFGS Desarrollo de Aplicaciones Multiplataforma (DAM)** y consume una API REST desarrollada en FastAPI.

---

# Descripción

La aplicación permite a los usuarios interactuar con el sistema de gestión de proyectos desde una interfaz moderna y visual.

A través de la app se pueden:

- visualizar proyectos  
- gestionar tareas  
- consultar información del equipo  
- interactuar con la API  

El objetivo es ofrecer una experiencia intuitiva para gestionar el trabajo en equipo.

---

# Tecnologías utilizadas

| Componente           | Tecnología |
|--------------------|----------|
| Lenguaje           | C# |
| Framework UI       | .NET MAUI |
| Arquitectura       | MVVM |
| Entorno desarrollo | Visual Studio |
| Comunicación API   | HTTP (REST) |

---


# Arquitectura

La aplicación sigue el patrón **MVVM (Model - View - ViewModel)**:


View → Interfaz de usuario (XAML)
ViewModel → Lógica de presentación
Model → Representación de datos
Services → Comunicación con la API


---

# Estructura del proyecto

```
GestionTareas
│
├── Model → Clases de datos (Proyecto, Tarea, Usuario...)
├── View → Interfaces XAML
├── ViewModel → Lógica de las vistas
├── Services → Consumo de la API
├── Resources → Estilos, colores, imágenes
└── AppShell.xaml → Navegación de la app
```

---

# Funcionalidades principales

## Proyectos

- visualizar lista de proyectos  
- acceder al detalle de cada proyecto  

## Tareas

- visualizar tareas  
- consultar estado  
- ver detalles  

## Interfaz

- diseño moderno  
- navegación mediante AppShell  
- componentes reutilizables  

---

# Requisitos

- Visual Studio 2022 o superior  
- .NET MAUI instalado  
- Emulador o dispositivo físico  

---

# Instalación

Clonar el repositorio:

```bash
git clone https://github.com/Irenecg22/GestionTareas.git
cd GestionTareas
```

Abrir el proyecto en Visual Studio.

---

# Ejecución

1. Seleccionar dispositivo (Android, Windows, etc.)
2. Ejecutar el proyecto

La aplicación se conecta a la API en:

```
http://localhost:8000
```

Asegúrate de que el backend esté en ejecución.

---

# Conexión con la API

La app utiliza servicios para consumir la API REST.

Ejemplo:

- ProyectoService  
- TareaService  
- UserService  

Estos servicios realizan llamadas HTTP para obtener y enviar datos.

---

# Estado del proyecto

- Estructura base implementada  
- Navegación configurada  
- Consumo de API en desarrollo  
- Mejora estética en progreso

## Autores

* Jhojahn Sebastian Ramirez Marin
* Irene Gomez Cañada
