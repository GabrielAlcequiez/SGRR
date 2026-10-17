using SGRR.Data.Common.Enums;
using SGRR.Data.Repositories;

namespace SGRR.Test.Repositories;

public class CircularActivityLogRepositoryTests
{
    [Fact]
    public void AddLog_KeepsOnlyLatest100LogsInNewestFirstOrder()
    {
        var repository = new CircularActivityLogRepository();

        for (var index = 0; index <= 100; index++)
        {
            repository.AddLog(LogSeverity.Info, $"event-{index}");
        }

        var logs = repository.GetLast100Logs().ToArray();

        Assert.Equal(100, logs.Length);
        Assert.Equal("event-100", logs[0].Message);
        Assert.Equal("event-1", logs[^1].Message);
    }
}
