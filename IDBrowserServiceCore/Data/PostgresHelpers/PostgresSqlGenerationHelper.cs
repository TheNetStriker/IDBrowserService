using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System.Text;

namespace IDBrowserServiceCore.Data.PostgresHelpers
{
    public class PostgresSqlGenerationHelper : NpgsqlSqlGenerationHelper
    {
        public PostgresSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies) { }

        public override string DelimitIdentifier(string identifier)
        {
            return identifier.Contains('.') ? base.DelimitIdentifier(identifier) : identifier;
        }

        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            base.DelimitIdentifier(builder, identifier.ToLower());
        }
    }
}
