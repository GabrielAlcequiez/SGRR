using SGRR.Data.Common.Enums;

namespace SGRR.Data.Entities;

public class SystemActivityLog
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public LogSeverity Severity { get; private set; }
    public string Message { get; private set; }

    public SystemActivityLog(LogSeverity severity, string message)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow; // Guardar siempre en UTC
        Severity = severity;
        Message = message;
    }
}