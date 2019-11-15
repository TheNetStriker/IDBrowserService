using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Data.PostgresHelpers
{
    public class SqlGenerationHelper : NpgsqlSqlGenerationHelper
    {
        public SqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies) { }

        public override string DelimitIdentifier(string identifier) => identifier.Contains(".") ? base.DelimitIdentifier(identifier) : identifier;
    }
}
