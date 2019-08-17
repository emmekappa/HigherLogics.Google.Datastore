using Google.Cloud.Datastore.V1;
using Xunit;

namespace HigherLogics.Google.Datastore.Tests
{
    public class QueryTests
    {
        [Fact]
        public void asd()
        {
            var db = TestDatastoreClientFactory.Create();
            var query = db.CreateQuery<SimpleWithEntityField>();
            var queryResults = db.RunQueryLazily(query);
            foreach (var result in queryResults)
            {
                
                
            }
        }
    }
}