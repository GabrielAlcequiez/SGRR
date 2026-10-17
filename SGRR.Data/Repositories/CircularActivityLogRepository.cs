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
        lock (_lock)
        {
            if (_logs.Count >= MaxLogs)
            {
                _logs.Dequeue();
            }
            _logs.Enqueue(new SystemActivityLog(severity, message));
        }
    }

    public IEnumerable<SystemActivityLog> GetLast100Logs()
    {
        lock (_lock)
        {
            return _logs.Reverse().ToArray();
        }
    }
}