using System.Linq;
using Google.Cloud.Datastore.V1;

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
            var datastoreDbBuilder = new DatastoreDbBuilder()
            {
                ProjectId = projectId,
                EmulatorDetection = EmulatorDetection.EmulatorOnly,
            };
            return datastoreDbBuilder.Build();
            /*var client = DatastoreClient.Create(new Channel(emulatorHost, emulatorPort, ChannelCredentials.Insecure));
            return DatastoreDb.Create(projectId, namespaceId, client);*/
        }
        
        public static void DeleteAllEntitiesOfKind<T>() where T : class
        {
            var db = Create();
            var query = db.CreateQuery<T>();
            var queryResult = db.RunQuery(query).Entities.ToList();
            //var queryResult = db.RunQuery(db.CreateQuery<T>()).Entities<T>();
            
            if (!queryResult.Any())
                return;

            using (var transaction = db.BeginTransaction())
            {

                transaction.Delete(queryResult);
                //transaction.Delete<T>(queryResult);
                transaction.Commit();
            }
        }
    }
}