using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SaaSOvation.Common.Port.Adapters.Persistence
{
    public class ResultSetObjectMapper<T>
    {
        private readonly string columnPrefix;

        private readonly IDataReader dataReader;
        private readonly JoinOn joinOn;

        public ResultSetObjectMapper(IDataReader dataReader, JoinOn joinOn, string columnPrefix = null)
        {
            this.dataReader = dataReader;
            this.joinOn = joinOn;
            this.columnPrefix = columnPrefix;
        }

        public T MapResultToType()
        {
            var obj = default(T);

            var associationsToMap = new HashSet<string>();

            var fields = typeof(T).GetFields();

            foreach (var field in fields)
            {
                var columnName = FieldNameToColumnName(field.Name);
                var columnIndex = dataReader.GetOrdinal(columnName);
                if (columnIndex >= 0)
                {
                    var columnValue = ColumnValueFrom(columnIndex, field.FieldType);

                    joinOn.SaveCurrentLeftQualifier(columnName, columnValue);

                    field.SetValue(obj, columnValue);
                }
                else
                {
                    var objectPrefix = ToObjectPrefix(columnName);
                    if (!associationsToMap.Contains(objectPrefix) && HasAssociation(objectPrefix))
                        associationsToMap.Add(field.Name);
                }
            }

            if (associationsToMap.Count > 0) MapAssociations(obj, associationsToMap);

            return obj;
        }

        private void MapAssociations(object obj, ISet<string> associationsToMap)
        {
            var mappedCollections = new Dictionary<string, ICollection<object>>();
            while (dataReader.NextResult())
                foreach (var fieldName in associationsToMap)
                {
                    var associationField = typeof(T).GetField(fieldName);
                    var associationFieldType = default(Type);
                    var collection = default(ICollection<object>);

                    if (typeof(IEnumerable).IsAssignableFrom(associationField.FieldType))
                    {
                        if (!mappedCollections.TryGetValue(fieldName, out collection))
                        {
                            collection = CreateCollectionFrom(associationField.FieldType);
                            mappedCollections.Add(fieldName, collection);
                            associationField.SetValue(obj, collection);
                        }

                        var genericType = associationField.FieldType.GetGenericTypeDefinition();
                        associationFieldType = genericType.GetGenericArguments()[0];
                    }
                    else
                    {
                        associationFieldType = associationField.FieldType;
                    }

                    var columnName = FieldNameToColumnName(fieldName);

                    var mapper = new ResultSetObjectMapper<object>(dataReader, joinOn, ToObjectPrefix(columnName));

                    var associationObject = mapper.MapResultToType();

                    if (collection != null)
                        collection.Add(associationObject);
                    else
                        associationField.SetValue(obj, associationObject);
                }
        }

        /// <summary>
        ///     TODO: ensure correctness
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ICollection<object> CreateCollectionFrom(Type type)
        {
            var genericType = type.GetGenericTypeDefinition();
            if (typeof(IList<>).IsAssignableFrom(genericType))
                return new List<object>();
            if (typeof(ISet<>).IsAssignableFrom(genericType))
                return new HashSet<object>();
            return null;
        }

        private bool HasAssociation(string objectPrefix)
        {
            var fieldCount = dataReader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
            {
                var columnName = dataReader.GetName(i);
                if (columnName.StartsWith(objectPrefix) && joinOn.IsJoinedOn(dataReader)) return true;
            }

            return false;
        }

        private string ToObjectPrefix(string columnName)
        {
            return "o_" + columnName + "_";
        }

        private object ColumnValueFrom(int columnIndex, Type columnType)
        {
            switch (Type.GetTypeCode(columnType))
            {
                case TypeCode.Int32:
                    return dataReader.GetInt32(columnIndex);
                case TypeCode.Int64:
                    return dataReader.GetInt64(columnIndex);
                case TypeCode.Boolean:
                    return dataReader.GetBoolean(columnIndex);
                case TypeCode.Int16:
                    return dataReader.GetInt16(columnIndex);
                case TypeCode.Single:
                    return dataReader.GetFloat(columnIndex);
                case TypeCode.Double:
                    return dataReader.GetDouble(columnIndex);
                case TypeCode.Byte:
                    return dataReader.GetByte(columnIndex);
                case TypeCode.Char:
                    return dataReader.GetChar(columnIndex);
                case TypeCode.String:
                    return dataReader.GetString(columnIndex);
                case TypeCode.DateTime:
                    return dataReader.GetDateTime(columnIndex);
                default:
                    throw new InvalidOperationException("Unsupported type.");
            }
        }

        private string FieldNameToColumnName(string fieldName)
        {
            var sb = new StringBuilder();

            if (columnPrefix != null) sb.Append(columnPrefix);

            foreach (var ch in fieldName)
                if (char.IsLetter(ch) && char.IsUpper(ch))
                    sb.Append('_').Append(char.ToLower(ch));
                else
                    sb.Append(ch);

            return sb.ToString();
        }
    }
}