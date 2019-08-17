using System;
using System.Linq.Expressions;
using System.Reflection;
using Google.Cloud.Datastore.V1;

namespace HigherLogics.Google.Datastore
{
    public static class FilterExtensions
    {
        public static Filter Property<TSource>(this Filter filter, Expression<Func<TSource, object>> projection,
            Value propertyValue, PropertyFilter.Types.Operator @operator) 
        {
            MemberExpression body = (MemberExpression)projection.Body;
            var entityFieldAttribute = body.Member.GetCustomAttribute<EntityFieldAttribute>();
            string name = entityFieldAttribute?.FieldName ?? body.Member.Name;
            return Filter.Property(name, propertyValue, @operator);
        }
        
    }
    
    public static class Filter<T> where T : class
    {
        public static Filter Property<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue, PropertyFilter.Types.Operator @operator)
        {
            MemberExpression body = (MemberExpression)propertySelector.Body;
            var entityFieldAttribute = body.Member.GetCustomAttribute<EntityFieldAttribute>();
            string name = entityFieldAttribute?.FieldName ?? body.Member.Name;
            return Filter.Property(name, Value<TProperty>.To(propertyValue), @operator);
        }

        public static Filter Equal<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue)
        {
            return Property(propertySelector, propertyValue,
                PropertyFilter.Types.Operator.Equal);
        }
        
        public static Filter GreaterThan<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue)
        {
            return Property(propertySelector, propertyValue,
                PropertyFilter.Types.Operator.GreaterThan);
        }
        
        public static Filter LessThan<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue)
        {
            return Property(propertySelector, propertyValue,
                PropertyFilter.Types.Operator.LessThan);
        }
        
        public static Filter GreaterThanOrEqual<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue)
        {
            return Property(propertySelector, propertyValue,
                PropertyFilter.Types.Operator.GreaterThanOrEqual);
        }
        
        public static Filter LessThanOrEqual<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty propertyValue)
        {
            return Property(propertySelector, propertyValue,
                PropertyFilter.Types.Operator.LessThanOrEqual);
        }
    }
}