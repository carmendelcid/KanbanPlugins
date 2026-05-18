using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace KanbanPlugins
{
    // 1. OBLIGATORIO PL-400: Todo plugin debe implementar la interfaz IPlugin
    public class TaskLimitPlugin : IPlugin
    {
        // 2. El método Execute es el motor principal que Dataverse ejecutará
        public void Execute(IServiceProvider serviceProvider)
        {
            // A. Extraer el contexto (quién hace la acción, en qué tabla, etc.)
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // B. Crear el servicio de Dataverse (nuestra conexión a la base de datos)
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // C. Seguridad: Validar que estamos trabajando con una tabla (Entity) y que trae datos ("Target")
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // D. LATE BOUND (Tip PL-400): Usamos Entity genérico en lugar de clases específicas
                Entity task = (Entity)context.InputParameters["Target"];

                // Si no es la tabla de tareas, abortamos y no hacemos nada
                if (task.LogicalName != "task") return;

                // E. Comprobar si se está moviendo a "En Proceso"
                // Nota: Usamos el campo "category" por el truco que hicimos en la Fase 5
                if (task.Attributes.Contains("category") && task["category"].ToString() == "En Proceso")
                {
                    // F. LA CONSULTA (QueryExpression): Buscar cuántas tareas "En Proceso" tiene el usuario
                    QueryExpression query = new QueryExpression("task");
                    query.ColumnSet = new ColumnSet("activityid"); // Solo traemos el ID para que la consulta vuele

                    // Condición 1: Que la categoría sea "En Proceso"
                    query.Criteria.AddCondition("category", ConditionOperator.Equal, "En Proceso");
                    // Condición 2: Que el dueño de la tarea sea el usuario actual
                    query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, context.UserId);

                    // Ejecutar la consulta contra Dataverse
                    EntityCollection results = service.RetrieveMultiple(query);

                    // G. LA REGLA DE NEGOCIO (El jefe hablando)
                    if (results.Entities.Count >= 3)
                    {
                        // Si tiene 3 o más, lanzamos un error y Dataverse CANCELARÁ el guardado
                        throw new InvalidPluginExecutionException("¡Límite alcanzado! No puedes gestionar más de 3 tareas 'En Proceso' a la vez. ¡Termina una primero!");
                    }
                }
            }
        }
    }
}