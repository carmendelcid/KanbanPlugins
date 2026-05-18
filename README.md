# ⚙️ Kanban Board - Dataverse Plugin (Backend)

## 📖 Descripción
Este repositorio contiene la lógica de negocio (Backend) del proyecto principal "Kanban Board Inteligente". Está desarrollado en **C# (.NET)** y diseñado para ejecutarse como un Plugin síncrono dentro de Microsoft Dataverse.

## 🛡️ Reglas de Negocio Implementadas
El plugin actúa como un guardián de seguridad en la fase *Pre-Operation* de Dataverse. Su función principal es **limitar el trabajo en curso (WIP)**:
- Escanea las tareas asignadas al usuario actual (`context.UserId`).
- Si el usuario intenta mover una tarea a la columna "En Proceso" y ya tiene 3 o más tareas en ese estado, el plugin intercepta la transacción.
- Lanza un `InvalidPluginExecutionException` bloqueando el movimiento para garantizar la productividad y evitar cuellos de botella.

## 🛠️ Tecnologías
- C# (.NET Framework 4.6.2)
- Microsoft.CrmSdk.CoreAssemblies (Late-Bound)
