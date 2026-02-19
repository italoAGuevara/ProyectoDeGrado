namespace API.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string name, object key) : base($"Entity \"{name}\" ({key}) has a conflict.")
        {
        }

        public ConflictException(string name, object key, bool exist) : base($"Entity \"{name}\" ({key}) exist is \"{exist}\".")
        {
        }
    }
}
