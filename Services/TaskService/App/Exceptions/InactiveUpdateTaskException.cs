namespace TaskService.App.Exceptions
{
    public class InactiveUpdateTaskException : Exception
    {
        public InactiveUpdateTaskException() { }
        public InactiveUpdateTaskException(string message) : base(message) { }
        public InactiveUpdateTaskException(string message, Exception innerException) : base(message, innerException) { }
    }
}
