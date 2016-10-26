using System;

namespace ModernRoute.WildData.Npgsql.Core
{
    public class BaseRepository
    {
        public BaseRepository(BaseSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            Session = session;
        }

        protected BaseSession Session
        {
            get;
            private set;
        }
    }
}
