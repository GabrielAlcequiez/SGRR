using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class CircularActivityLogRepository : ISystemActivityLogRepository
{
    private readonly Queue<SystemActivityLog> _logs = new();
    private readonly object _lock = new();
    private const int MaxLogs = 100;

    public void AddLog(LogSeverity severity, string message)
    {
        var log = new SystemActivityLog(severity, message);

        lock (_lock)
        {
            if (_logs.Count >= MaxLogs)
            {
                _logs.Dequeue(); // Descartar el elemento más antiguo
            }
            _logs.Enqueue(log);
        }
    }

    public IEnumerable<SystemActivityLog> GetLast100Logs()
    {
        lock (_lock)
        {
            // Retorna ordenado desde el más reciente al más antiguo
            return _logs.OrderByDescending(l => l.Timestamp).ToList();
        }
    }
}