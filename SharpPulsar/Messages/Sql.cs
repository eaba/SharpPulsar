﻿using System;
using SharpPulsar.Presto;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Client;

namespace SharpPulsar.Messages
{
    public sealed class SqlQuery : ISqlQuery
    {
        public SqlQuery(ClientOptions options, Action<Exception> exceptionHandler, Action<string> log)
        {
            ExceptionHandler = exceptionHandler;
            ClientOptions = options;
            Log = log;
        }

        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public ClientOptions ClientOptions { get; }
    }
    internal sealed class SqlSession 
    {
        public SqlSession(ClientSession session, ClientOptions options, Action<Exception> exceptionHandler, Action<string> log)
        {
            ExceptionHandler = exceptionHandler;
            ClientSession = session;
            Log = log;
            ClientOptions = options;
        }

        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public ClientSession ClientSession { get; }
        public ClientOptions ClientOptions { get; }
    }
}
