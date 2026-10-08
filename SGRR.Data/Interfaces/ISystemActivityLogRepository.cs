using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface ISystemActivityLogRepository
{
    void AddLog(LogSeverity severity, string message);
    IEnumerable<SystemActivityLog> GetLast100Logs();
}