using System;
using System.Linq;
using Google.Cloud.Datastore.V1;
using Xunit;
using Xunit.Abstractions;

namespace HigherLogics.Google.Datastore.Tests
{
    public class QueryTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public QueryTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void DebugEnvVar()
        {
            _testOutputHelper.WriteLine($"DATASTORE_EMULATOR_HOST ENV IS = {Environment.GetEnvironmentVariable("DATASTORE_EMULATOR_HOST")}");
        }
        
        [Fact]
        public void FilterEqual()
        {
            var db = TestDatastoreClient.Create();
            TestDatastoreClient.DeleteAllEntitiesOfKind<SimpleWithEntityField>();
            using(var transaction = db.BeginTransaction()) {
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "__tag"
                });
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "__tag"
                });
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "Something else"
                });
                transaction.Commit();
            }
            
            var query = db.CreateQuery<SimpleWithEntityField>();
            query.Filter =
                Filter<SimpleWithEntityField>.Equal(x => x.Baz, "__tag");
            var queryResults = db.RunQueryLazily(query);
            Assert.Equal(2, queryResults.Count());
        }
        
        [Fact]
        public void FilterProperty()
        {
            var db = TestDatastoreClient.Create();
            TestDatastoreClient.DeleteAllEntitiesOfKind<SimpleWithEntityField>();
            using(var transaction = db.BeginTransaction()) {
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "__tag"
                });
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "__tag"
                });
                transaction.Insert(new SimpleWithEntityField
                {
                    Baz = "Something else"
                });
                transaction.Commit();
            }
            
            var query = db.CreateQuery<SimpleWithEntityField>();
            query.Filter =
                Filter<SimpleWithEntityField>.Property(x => x.Baz, "__tag", PropertyFilter.Types.Operator.Equal);
            var queryResults = db.RunQueryLazily(query);
            var entities = queryResults.GetAllResults().Entities.ToList();
            Assert.Equal(2, entities.Count());
        }
    }

   
}