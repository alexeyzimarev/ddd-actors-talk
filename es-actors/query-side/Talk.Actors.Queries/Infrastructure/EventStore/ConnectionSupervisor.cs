using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using ILogger = Serilog.ILogger;

namespace Talk.Actors.Queries.Infrastructure.EventStore
{
    public class ConnectionSupervisor
    {
        static readonly ILogger Log = Serilog.Log.ForContext<ConnectionSupervisor>();

        readonly IEventStoreConnection _connection;
        readonly Action _onConnected;
        readonly Action<string> _onDisconnected;
        readonly Action<Exception> _onError;
        readonly Action _onFatalFailure;

        public ConnectionSupervisor(
            IEventStoreConnection connection,
            Action onFatalFailure,
            Action onConnected = null,
            Action<string> onDisconnected = null,
            Action<Exception> onError = null)
        {
            _connection = connection;
            _onFatalFailure = onFatalFailure;
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;
            _onError = onError;
        }

        public void Initialize()
        {
            _connection.Connected += OnConnected;
            _connection.Closed += OnConnectionClosed;
            _connection.ErrorOccurred += OnConnectionErrorOccurred;

            Log.Debug("EventStore connection monitor started for: " + _connection.ConnectionName);
        }

        public void Shutdown()
        {
            _connection.Connected -= OnConnected;
            _connection.Closed -= OnConnectionClosed;
            _connection.ErrorOccurred -= OnConnectionErrorOccurred;

            Log.Debug("EventStore connection monitor stopped for: " + _connection.ConnectionName);
        }

        void OnConnected(object sender, ClientConnectionEventArgs args)
        {
            Log.Information(
                "EventStore connection with id = {ConnectionId} successfully connected to {endpoint}",
                args.Connection.ConnectionName,
                args.RemoteEndPoint.ToString()
            );
            _onConnected?.Invoke();
        }

        void OnConnectionClosed(object sender, ClientClosedEventArgs args)
        {
            Log.Information(
                "EventStore connection with id = {ConnectionId} was closed due to {ConnectionClosedReason}",
                args.Connection.ConnectionName,
                args.Reason);
            _onDisconnected?.Invoke(args.Reason);
        }

        void OnConnectionErrorOccurred(object sender, ClientErrorEventArgs args)
        {
            Log.Error(args.Exception,
                "EventStore connection with id = {ConnectionId} error occured",
                args.Connection.ConnectionName);

            _onError?.Invoke(args.Exception);

            var exception = (args.Exception as AggregateException)?.GetBaseException() ?? args.Exception;
            switch (exception)
            {
                case RetriesLimitReachedException retriesLimitReached:
                    Log.Fatal(
                        "EventStore connection's limit of reconnection or operation retries reached. " +
                        "Stopping service...",
                        retriesLimitReached);
                    _onFatalFailure();
                    break;
                case ClusterException clusterException:
                    Log.Fatal(
                        "EventStore connection could not establish link with EventStore cluster. " +
                        "Maximum number of cluster connection attempts reached. " +
                        "Stopping service...",
                        clusterException);
                    _onFatalFailure();
                    break;
                default:
                    Log.Warning(exception, "");
                    break;
            }
        }
    }
}