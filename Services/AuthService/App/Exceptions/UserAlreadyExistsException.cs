namespace AuthService.App.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public string Username { get; }

        public UserAlreadyExistsException()
            : base("User already exists.")
        {
        }

        public UserAlreadyExistsException(string username)
            : base($"User '{username}' already exists.")
        {
            Username = username;
        }

        public UserAlreadyExistsException(string username, Exception innerException)
            : base($"User '{username}' already exists.", innerException)
        {
            Username = username;
        }
    }
}
