using MongoDB.Driver;

namespace Contact.API
{
    public class AppSettings
    {
        public string MongoConnectionString { get; set; }
        public string ContactDatabaseName { get; set; }
    }
}