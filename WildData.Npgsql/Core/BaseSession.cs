using ModernRoute.WildData.Core;
using ModernRoute.WildData.Npgsql.Resources;
using Npgsql;
using System;
using System.Data;

namespace ModernRoute.WildData.Npgsql.Core
{
    public abstract class BaseSession : IBaseSession
    {
        public BaseSession(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            bool exception = false;

            try
            {
                Connection = new NpgsqlConnection(connectionString);
                Connection.Open();
            }
            catch
            {
                exception = true;
                throw;
            }
            finally
            {
                if (exception)
                {
                    Dispose(true);
                }
            }
        }

        public ITransaction BeginTransaction(IsolationLevel level)
        {
            CheckDisposed();

            return new Transaction(this, level);
        }

        public NpgsqlCommand CreateCommand()
        {
            CheckDisposed();

            NpgsqlCommand command = Connection.CreateCommand();

            command.Connection = Connection;

            if (InternalTransaction != null)
            {
                command.Transaction = InternalTransaction.PostgreSqlTransaction;
            }

            return command;
        }

        public ITransaction Transaction
        {
            get
            {
                return InternalTransaction;
            }
            set
            {
                if (value == null)
                {
                    InternalTransaction = null;
                    return;
                }

                if (!(value is Transaction))
                {
                    throw new InvalidOperationException(Strings.CannotAssociateForeignTransactionWithSession);
                }

                Transaction transaction = (Transaction)value;

                if (transaction.Session != this)
                {
                    throw new InvalidOperationException(Strings.TransactionHasBeenCreatedInAnotherSession);
                }

                InternalTransaction = transaction;
            }
        }

        internal NpgsqlConnection Connection
        {
            get;
            private set;
        }

        internal Transaction InternalTransaction;

        #region IDisposable Support
        private bool _Disposed = false;

        private void CheckDisposed()
        {
            if (_Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    if (Connection != null)
                    {
                        Connection.Dispose();
                        Connection = null;
                    }
                }

                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion 
    }
}
