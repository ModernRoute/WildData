using ModernRoute.WildData.Core;
using Npgsql;
using System;
using System.Data;

namespace ModernRoute.WildData.Npgsql.Core
{
    class Transaction : ITransaction
    {
        public Transaction(BaseSession session, IsolationLevel level)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            _Session = session;

            PostgreSqlTransaction = session.Connection.BeginTransaction(level);
        }

        public IsolationLevel IsolationLevel
        {
            get
            {
                return PostgreSqlTransaction.IsolationLevel;
            }
        }

        public IBaseSession Session
        {
            get { return _Session; }
        }

        public void Commit()
        {
            CheckDisposed();

            PostgreSqlTransaction.Commit();
        }

        public void Rollback()
        {
            CheckDisposed();

            PostgreSqlTransaction.Rollback();
        }

        internal NpgsqlTransaction PostgreSqlTransaction
        {
            get;
            private set;
        }

        private BaseSession _Session;

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
                    if (PostgreSqlTransaction != null)
                    {
                        if (ReferenceEquals(this, _Session.InternalTransaction))
                        {
                            _Session.InternalTransaction = null;
                        }

                        PostgreSqlTransaction.Dispose();
                        PostgreSqlTransaction = null;
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
