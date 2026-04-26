namespace Prolog.NET.Threading.Tests;

public class SingleThreadWorkerTests
{
    [Fact]
    public void ThreadWorker_WhenDisposedWithEnqueuedJob_ShouldFinishJob()
    {
        // Arrange
        ThreadWorker worker = new();

        // Act
        bool finished = false;
        worker.AddJob(() => finished = true);
        worker.Dispose();

        // Assert
        Assert.True(finished);
    }

    [Fact]
    public void ThreadWorker_WhenDisposedWithMultipleEnqueuedJobs_ShouldFinishJobs()
    {
        // Arrange
        ThreadWorker worker = new();

        // Act
        bool finished1 = false;
        worker.AddJob(() => finished1 = true);
        bool finished2 = false;
        worker.AddJob(() => finished2 = true);
        bool finished3 = false;
        worker.AddJob(() => finished3 = true);
        worker.Dispose();

        // Assert
        Assert.True(finished1);
        Assert.True(finished2);
        Assert.True(finished3);
    }

    [Fact]
    public void ThreadWorker_WhenJobIsAdded_ShouldRunOnDifferentThread()
    {
        // Arrange
        ThreadWorker worker = new();
        int testThread = Environment.CurrentManagedThreadId;

        // Act
        int workerThread = -1;
        worker.AddJob(() => workerThread = Environment.CurrentManagedThreadId);
        worker.Dispose();

        // Assert
        Assert.NotEqual(-1, workerThread);
        Assert.NotEqual(testThread, workerThread);
    }

    [Fact]
    public void ThreadWorker_WhenMultipleJobsAreAdded_ShouldRunOnSameWorkerThread()
    {
        // Arrange
        ThreadWorker worker = new();
        int testThread = Environment.CurrentManagedThreadId;

        // Act
        int jobThread1 = -1;
        worker.AddJob(() => jobThread1 = Environment.CurrentManagedThreadId);
        int jobThread2 = -1;
        worker.AddJob(() => jobThread2 = Environment.CurrentManagedThreadId);
        worker.Dispose();

        // Assert
        Assert.NotEqual(-1, jobThread1);
        Assert.NotEqual(testThread, jobThread1);
        Assert.NotEqual(-1, jobThread2);
        Assert.NotEqual(testThread, jobThread2);
        Assert.Equal(jobThread1, jobThread2);
    }
}
