namespace ModernRoute.WildData.Core
{
    public class WriteResult
    {
        public WriteResultType ResultType
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public WriteResult(WriteResultType resultType, string message = null)
        {
            ResultType = resultType;
            Message = message;
        }

        public static WriteResult NotFound(string message = null)
        {
            return new WriteResult(WriteResultType.NotFound,message);
        }

        public static WriteResult Ok(string message = null)
        {
            return new WriteResult(WriteResultType.Ok, message);
        }

        public static WriteResult ConstraintFailed(string message = null)
        {
            return new WriteResult(WriteResultType.ConstraintFailed, message);
        }

        public static WriteResult Conflict(string message = null)
        {
            return new WriteResult(WriteResultType.Conflict, message);
        }

        public static WriteResult Unknown(string message = null)
        {
            return new WriteResult(WriteResultType.Unknown, message);
        }        

        public static WriteResult ForSingleRow(int rowsAffected)
        {
            if (rowsAffected == 1)
            {
                return Ok();
            }

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return Unknown();
        }
    }
}
