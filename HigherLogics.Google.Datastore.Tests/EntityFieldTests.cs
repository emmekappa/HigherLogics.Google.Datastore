using System.ComponentModel.DataAnnotations;
using Google.Cloud.Datastore.V1;
using Xunit;

namespace HigherLogics.Google.Datastore.Tests
{
    class SimpleWithEntityField
    {
        [Key]
        public long Bar { get; set; }
        
        [EntityField("_baz")]
        public string Baz { get; set; }
    }

    class ComplexWithEntityField
    {
        [Key]
        public long Key { get; set; }
        
        [EntityField("firstNested")]
        public SimpleWithEntityField Nested1 { get; set; }
    }
    
    class FKClassWithEntityField
    {
        [Key]
        public long Id { get; set; }
        [EntityField("fkSimple")]
        public FK<SimpleWithEntityField> Simple { get; set; }
    }
    
    public class EntityFieldTests
    {
        [Fact]
        public static void SimpleTests()
        {
            var x = new SimpleWithEntityField { Bar = 99, Baz = "hello world!" };
            var e = Entity<SimpleWithEntityField>.To(new Entity(), x);
            var y = Entity<SimpleWithEntityField>.From(new SimpleWithEntityField(), e);
            Assert.Equal(x.Bar, y.Bar);
            Assert.Equal(x.Baz, y.Baz);
            Assert.Equal(x.Bar, e.Key.Id());
            Assert.Equal(x.Baz, e["_baz"]);
            Assert.NotEqual(x.Bar, e["_baz"].IntegerValue);
            Assert.NotEqual(x.Baz, e.Key.ToString());
        }
        
        [Fact]
        public static void ComplexWithEntityFieldTests()
        {
            var x = new ComplexWithEntityField() {Key = 10, Nested1 = new SimpleWithEntityField() {Baz = "hey!"}};
            var e = Entity<ComplexWithEntityField>.To(new Entity(), x);
            var y = Entity<ComplexWithEntityField>.From(new ComplexWithEntityField(), e);
            Assert.Equal(x.Key, y.Key);
            Assert.Equal(x.Nested1.Baz, y.Nested1.Baz);
            Assert.Equal(x.Nested1.Bar, y.Nested1.Bar);
            Assert.Equal(x.Key, e.Key.Id());
            Assert.Equal(x.Nested1.Baz, e["firstNested"].EntityValue["_baz"]);
        }
        
        [Fact]
        public static void FKTestWithEntityFieldTests()
        {
            var x = new FKClassWithEntityField
            {
                Id = 31337,
                Simple = new FK<SimpleWithEntityField>(new SimpleWithEntityField
                {
                    Bar = 33,
                    Baz = "hello world!",
                }),
            };
            var e = Entity<FKClassWithEntityField>.To(new Entity(), x);
            var rt = Entity<FKClassWithEntityField>.From(new FKClassWithEntityField(), e);
            Assert.Equal(x.Id, rt.Id);
            Assert.Equal(x.Simple.Key, rt.Simple.Key);
            Assert.Equal(x.Simple.Value.Bar, e["fkSimple"].KeyValue.Path[0].Id);
        }
    }
}