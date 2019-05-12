using System.Data;

namespace SaaSOvation.Common.Port.Adapters.Persistence
{
    public class JoinOn
    {
        private object currentLeftQualifier;
        private readonly string leftKey;
        private readonly string rightKey;

        public JoinOn()
        {
        }

        public JoinOn(string leftKey, string rightKey)
        {
            this.leftKey = leftKey;
            this.rightKey = rightKey;
        }

        public bool IsSpecified => leftKey != null && rightKey != null;

        public bool HasCurrentLeftQualifier(IDataReader dataReader)
        {
            try
            {
                var columnValue = dataReader.GetValue(dataReader.GetOrdinal(leftKey));
                if (columnValue == null) return false;
                return columnValue.Equals(currentLeftQualifier);
            }
            catch
            {
                return false;
            }
        }

        public bool IsJoinedOn(IDataReader dataReader)
        {
            var leftColumn = default(object);
            var rightColumn = default(object);
            try
            {
                if (IsSpecified)
                {
                    leftColumn = dataReader.GetValue(dataReader.GetOrdinal(leftKey));
                    rightColumn = dataReader.GetValue(dataReader.GetOrdinal(rightKey));
                }
            }
            catch
            {
                // ignore
            }

            return leftColumn != null && rightColumn != null;
        }

        public void SaveCurrentLeftQualifier(string columnName, object columnValue)
        {
            if (columnName.Equals(leftKey)) currentLeftQualifier = columnValue;
        }
    }
}