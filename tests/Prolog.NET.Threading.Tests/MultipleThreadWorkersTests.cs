namespace Prolog.NET.Threading.Tests;

public class MultipleThreadWorkersTests
{
    [Fact]
    public void MultipleThreadWorkers_ShouldHaveDifferentWorkerThreads()
    {
        // Arrange
        ThreadWorker worker1 = new();
        ThreadWorker worker2 = new();
        int testThread = Environment.CurrentManagedThreadId;

        // Act
        int workerThread1 = -1;
        worker1.AddJob(() => workerThread1 = Environment.CurrentManagedThreadId);
        worker1.Dispose();
        int workerThread2 = -1;
        worker2.AddJob(() => workerThread2 = Environment.CurrentManagedThreadId);
        worker2.Dispose();

        // Assert
        Assert.NotEqual(-1, workerThread1);
        Assert.NotEqual(testThread, workerThread1);
        Assert.NotEqual(-1, workerThread2);
        Assert.NotEqual(testThread, workerThread2);
        Assert.NotEqual(workerThread1, workerThread2);
    }
}
