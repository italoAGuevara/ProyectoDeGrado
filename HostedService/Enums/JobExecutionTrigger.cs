namespace HostedService.Enums;

/// <summary>Origen de la ejecución del trabajo (persistido en historial).</summary>
public enum JobExecutionTrigger
{
    Manual = 0,
    Programada = 1
}
