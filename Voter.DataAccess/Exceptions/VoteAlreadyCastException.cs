namespace Voter.DataAccess.Exceptions
{
    public class VoteAlreadyCastException : Exception
    {
        public VoteAlreadyCastException() : base("User has already voted on this poll")
        {
        }

        public VoteAlreadyCastException(string message) : base(message)
        {
        }

        public VoteAlreadyCastException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
