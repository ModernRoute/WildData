using System;
using System.Data;

namespace ModernRoute.WildData.Core
{
    /// <summary>
    /// Data access session.
    /// </summary>
    public interface IBaseSession : IDisposable
    {
        /// <summary>
        /// Begins transaction.
        /// </summary>
        /// <param name="level">Transaction isolation level.</param>
        /// <returns>Created transaction</returns>
        /// <exception cref="NotSupportedException">When <paramref name="level"/> is not supported.</exception>
        ITransaction BeginTransaction(IsolationLevel level = IsolationLevel.Serializable);

        /// <summary>
        /// Associated transaction. Can be null.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs only in setter when user is trying 
        /// to associate foreign transaction.</exception>
        ITransaction Transaction { get; set; }
    }
}
