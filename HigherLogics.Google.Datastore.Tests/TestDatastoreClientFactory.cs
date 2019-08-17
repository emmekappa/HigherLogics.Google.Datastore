using Google.Cloud.Datastore.V1;
using Grpc.Core;

namespace HigherLogics.Google.Datastore.Tests
{
    public class TestDatastoreClientFactory
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
    }
}