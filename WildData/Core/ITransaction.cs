using System;
using System.Data;

namespace ModernRoute.WildData.Core
{
    /// <summary>
    /// Data access transaction.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Session associated with transaction.
        /// </summary>
        IBaseSession Session { get; }

        /// <summary>
        /// Transaction isolation level.
        /// </summary>
        IsolationLevel IsolationLevel { get; }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks the transaction.
        /// </summary>
        void Rollback();
    }
}
