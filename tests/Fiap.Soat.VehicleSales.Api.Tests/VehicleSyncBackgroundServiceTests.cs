using Fiap.Soat.VehicleSales.Api.Shared.BackgroundServices;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Concurrent;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class VehicleSyncBackgroundServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly Mock<IMediator> _mediator = new();
    private readonly ListLogger _logger = new();
    private int _calls;

    [SetUp]
    public void SetUp()
    {
        _mediator.Reset();
        _logger.Entries.Clear();
        _calls = 0;
    }

    [Test]
    public async Task StartAsync_RunsTheSyncImmediatelyWithoutWaitingForTheInterval()
    {
        SetupSync(() => ResponseFactory.Ok(2));
        using var service = CreateService(intervalSeconds: 60);

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => _calls == 1);
        await service.StopAsync(CancellationToken.None);

        Assert.That(_logger.Entries, Has.Some.Matches<LogEntry>(x => x.Level == LogLevel.Debug && x.Message == "Vehicle sync received 2 vehicles"));
    }

    [Test]
    public async Task ExecuteAsync_WhenASyncThrows_LogsTheErrorAndKeepsRunning()
    {
        SetupSync(() => _calls == 1 ? throw new HttpRequestException("Connection refused") : ResponseFactory.Ok(0));
        using var service = CreateService(intervalSeconds: 1);

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => _calls >= 2);
        await service.StopAsync(CancellationToken.None);

        Assert.That(_logger.Entries, Has.Some.Matches<LogEntry>(x =>
            x.Level == LogLevel.Error && x.Exception is HttpRequestException && x.Message == "Vehicle sync failed with an unexpected error"));
    }

    [Test]
    public async Task ExecuteAsync_WhenTheSyncReturnsAFailure_LogsAWarning()
    {
        SetupSync(() => ResponseFactory.Fail<int>("Failed to get vehicle. Error: Service Unavailable"));
        using var service = CreateService(intervalSeconds: 60);

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => _logger.Entries.Any(x => x.Level == LogLevel.Warning));
        await service.StopAsync(CancellationToken.None);

        Assert.That(_logger.Entries, Has.Some.Matches<LogEntry>(x =>
            x.Level == LogLevel.Warning && x.Message == "Vehicle sync failed: Failed to get vehicle. Error: Service Unavailable"));
    }

    [Test]
    public async Task StopAsync_EndsTheLoopWithoutErrors()
    {
        SetupSync(() => ResponseFactory.Ok(0));
        using var service = CreateService(intervalSeconds: 60);

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => _calls == 1);
        await service.StopAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(service.ExecuteTask!.IsCompletedSuccessfully, Is.True);
            Assert.That(_logger.Entries, Has.None.Matches<LogEntry>(x => x.Level >= LogLevel.Warning));
            Assert.That(_logger.Entries, Has.Some.Matches<LogEntry>(x => x.Message == "Vehicle sync stopped"));
        });
    }

    private void SetupSync(Func<Response<int>> result) =>
        _mediator
            .Setup(x => x.Send(It.IsAny<SyncVehiclesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                Interlocked.Increment(ref _calls);
                return result();
            });

    private VehicleSyncBackgroundService CreateService(int intervalSeconds)
    {
        var provider = new ServiceCollection()
            .AddSingleton(_mediator.Object)
            .BuildServiceProvider();

        return new VehicleSyncBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new VehicleSyncOptions { IntervalSeconds = intervalSeconds }),
            _logger);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(Timeout);
        while (!condition())
        {
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class ListLogger : ILogger<VehicleSyncBackgroundService>
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Enqueue(new LogEntry(logLevel, formatter(state, exception), exception));
    }
}
