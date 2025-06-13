namespace Voter.DataAccess.Exceptions
{
    public class SaveFailedException : Exception
    {
        public SaveFailedException() : base("Save operation failed")
        {
        }

        public SaveFailedException(string message) : base(message)
        {
        }

        public SaveFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
