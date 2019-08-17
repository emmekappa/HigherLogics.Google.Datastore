using Google.Cloud.Datastore.V1;
using Grpc.Core;

namespace HigherLogics.Google.Datastore.Tests
{
    public class TestDatastoreClient
    {
        const string emulatorHost = "localhost";
        const int emulatorPort = 8081;
        const string projectId = "mappertests";
        const string namespaceId = "";

        public static DatastoreDb Create()
        {
            var client = DatastoreClient.Create(new Channel(emulatorHost, emulatorPort, ChannelCredentials.Insecure));
            return DatastoreDb.Create(projectId, namespaceId, client);
        }
        
        public static void DeleteAllEntitiesOfKind<T>() where T : class
        {
            var db = Create();
            var queryResult = db.RunQuery(db.CreateQuery<T>()).Entities<T>();
            
            using(var transaction = db.BeginTransaction()) {
                
                transaction.Delete<T>(queryResult);
                transaction.Commit();
            }
        }
    }
}