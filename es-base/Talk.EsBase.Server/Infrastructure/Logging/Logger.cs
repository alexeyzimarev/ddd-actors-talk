using System;
using Microsoft.Extensions.Logging;

namespace Talk.EsBase.Server.Infrastructure.Logging
{
    public static class Logger
    {
        static ILoggerFactory _loggerFactory;

        public static void UseLoggerFactory(ILoggerFactory loggerFactory)
            => _loggerFactory = loggerFactory;

        public static ILogger ForContext<T>()
            => _loggerFactory.CreateLogger<T>();

        public static ILogger ForContext(Type type)
            => _loggerFactory.CreateLogger(type);
    }
}