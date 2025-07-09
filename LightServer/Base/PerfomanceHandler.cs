using System.Diagnostics;
using LightServer.Utils;

public class PerfomanceHandler
{
    private readonly Process process;
    private readonly int processorCount;
    private TimeSpan lastTotalProcessorTime;
    private DateTime lastCheckTime;

    public PerfomanceHandler()
    {
        process = Process.GetCurrentProcess();
        processorCount = Environment.ProcessorCount;
        lastTotalProcessorTime = process.TotalProcessorTime;
        lastCheckTime = DateTime.UtcNow;
    }

    public void StartLogging(int intervalMs = 1000)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                Log();
                await Task.Delay(intervalMs);
            }
        });
    }

    private void Log()
    {
        process.Refresh();

        // RAM в мегабайтах
        float ramMB = process.WorkingSet64 / (1024f * 1024f);

        // CPU%
        var now = DateTime.UtcNow;
        var cpuTime = process.TotalProcessorTime;
        var deltaCpu = (cpuTime - lastTotalProcessorTime).TotalMilliseconds;
        var deltaTime = (now - lastCheckTime).TotalMilliseconds;

        float cpuUsage = (float)(deltaCpu / (deltaTime * processorCount)) * 100f;

        lastTotalProcessorTime = cpuTime;
        lastCheckTime = now;

        LogUtils.LogHard($"CPU: {cpuUsage:F1}%  |  RAM: {ramMB:F1} MB");
    }
}
