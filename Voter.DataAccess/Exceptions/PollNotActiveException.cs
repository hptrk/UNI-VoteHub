namespace Voter.DataAccess.Exceptions
{
    public class PollNotActiveException : Exception
    {
        public PollNotActiveException() : base("The poll is not active")
        {
        }

        public PollNotActiveException(string message) : base(message)
        {
        }

        public PollNotActiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
