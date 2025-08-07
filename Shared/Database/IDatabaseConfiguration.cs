namespace Database
{
    public interface IDatabaseConfiguration
    {
        public string GetConnectionString();
        public bool Migrate();
    }
}
