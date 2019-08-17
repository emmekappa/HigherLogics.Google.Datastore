using System;

namespace HigherLogics.Google.Datastore
{
    internal class EntityFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public EntityFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}