using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private const int MaxRequests = 5;
    private const int Seconds = 1;
    private static TimeLimits timeLimits;

    static void Main(string[] args)
    {
        timeLimits = new TimeLimits(MaxRequests, Seconds);
        Task.Delay(1000).Wait();

        // waiting for six seconds
        // Task.Delay(6000).Wait();

        // test slow start
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: Executing 2 threads");
        for (int i = 0; i < 2; i++)
        {
            Task.Delay(50).Wait();
            SomeProcess($"SlowStart[{i}]");
        }
        Task.Delay(50).Wait();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: Waiting 2 seconds");
        Task.Delay((Seconds * 1000) * 2).Wait();
        for (int i = 2; i < 10; i++)
        {
            Task.Delay(50).Wait();
            SomeProcess($"SlowStart[{i}]");
        }
        Task.Delay(5000).Wait();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: Finished!");
        Task.Delay(50).Wait();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: Starting batch process . . .");
        Task.Delay(50).Wait();
        // test batch
        for (int i = 0; i < 100; i++)
        {
            Task.Delay(50).Wait();
            SomeProcess($"Batch[{i}]");
        }

        // waiting for 30 seconds
        Task.Delay(20000).Wait();
        timeLimits.Dispose();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: done.");
    }

    public static Task SomeProcess(string cod)
    => Task.Run(async () =>
    {
        await timeLimits.Wait(cod);
        Task.Delay(200).Wait();
    });


}

public class TimeLimits : IDisposable
{
    private readonly SemaphoreSlim sem;
    private readonly int limit;
    private readonly Timer timer;
    private int counter;

    public TimeLimits(int size, int seconds)
    {
        counter = 0;
        limit = size;
        sem = new SemaphoreSlim(size, size);
        timer = new Timer(
                    callback: new TimerCallback(TimerTask),
                    state: null,
                    dueTime: 0,
                    period: seconds * 1000);
    }
    private void TimerTask(object? timerState)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] (timer): {counter} tasks executed. Refreshing . . .");
        counter = 0;
        if (sem.CurrentCount != limit)
            sem.Release(limit - sem.CurrentCount);
    }

    public async Task Wait(string cod)
    {
        await sem.WaitAsync();

        counter++;
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}]: Counter incremented: {counter} %%%% ID: {cod}");
    }

    public void Dispose()
    {
        sem.Dispose();
        timer.Dispose();
    }
}