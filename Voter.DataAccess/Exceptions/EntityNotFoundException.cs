namespace Voter.DataAccess.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base("Entity was not found")
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EntityNotFoundException(string entityName, object id)
            : base($"{entityName} with ID {id} was not found")
        {
        }
    }
}
