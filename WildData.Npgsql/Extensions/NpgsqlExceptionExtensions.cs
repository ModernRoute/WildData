using ModernRoute.WildData.Core;
using Npgsql;
using System;

namespace ModernRoute.WildData.Npgsql.Extensions
{
    static class NpgsqlExceptionExtensions
    {
        // Found here: http://www.postgresql.org/docs/9.4/static/errcodes-appendix.html
        private const string _IntegrityConstraintViolation = "23000";
        private const string _RaiseException = "P0001";

        private const string _VersionConflictHint = "VersionConflict";

        public static bool TryConvertToWriteResult(this NpgsqlException exception, out WriteResult value)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            
            value = null;

            PostgresException postgresException = exception as PostgresException;

            if (postgresException == null)
            {
                return false;
            }

            switch (postgresException.SqlState)
            { 
                case _IntegrityConstraintViolation:
                    value = new WriteResult(WriteResultType.ConstraintFailed, postgresException.Message);
                    break;
                case _RaiseException:
                    if (postgresException.Hint == _VersionConflictHint)
                    {
                        value = new WriteResult(WriteResultType.Conflict);
                    }
                    break;
            }

            return value != null;
        }     
    }
}
