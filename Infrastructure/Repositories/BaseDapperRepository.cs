using System.Data;

namespace Infrastructure.Repositories
{
    public abstract class BaseDapperRepository
    {
        protected const string SelectClause = "SELECT";
        protected const string UpdateClause = "UPDATE";
        protected const string InsertClause = "INSERT";
        protected const string DeleteClause = "DELETE";
        protected const string FromClause = "FROM";
        protected const string WhereClause = "WHERE";
        protected const string WithClause = "WITH";
        protected const string GroupClause = "GROUP BY";
        protected const string Declare = "DECLARE";
        protected const string ExecClause = "EXEC";

        protected static string BuildOrderBySqlUsingInterpolation(string tableName, string sortOrderColumn,
            bool sortOrderDirection)
        {
            var orderBy = $"{tableName}.[CreatedTime]";
            var sortOrder = "DESC";
            if (!string.IsNullOrEmpty(sortOrderColumn))
            {
                orderBy = $"{tableName}.[{sortOrderColumn}]";
            }

            if (!sortOrderDirection)
            {
                sortOrder = "ASC";
            }

            orderBy = $"{orderBy} {sortOrder}";
            return orderBy;
        }

        protected static int GetOffset(int pageSize, int pageNumber)
        {
            return (pageNumber - 1) * pageSize;
        }

        protected static string SearchLike(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm?.Trim()))
            {
                return null;
            }

            string EncodeForLike(string s) => s.Replace("[", "[[]").Replace("%", "[%]");
            return $"%{EncodeForLike(searchTerm.Trim())}%";
        }

        protected static IDbDataParameter CreateDbDataParameters(IDbCommand command, string parameterName, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            return parameter;
        }
    }
}