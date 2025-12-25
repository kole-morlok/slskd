// <copyright file="Z12092025_TransferLocalPathMigration.cs" company="slskd Team">
//     Copyright (c) slskd Team. All rights reserved.
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU Affero General Public License as published
//     by the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU Affero General Public License for more details.
//
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see https://www.gnu.org/licenses/.
// </copyright>

namespace slskd.Migrations;

using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Serilog;

/// <summary>
///     Updates the Transfers table to add the LocalPath column.
/// </summary>
public class Z12092025_TransferLocalPathMigration : IMigration
{
    public Z12092025_TransferLocalPathMigration(ConnectionStringDictionary connectionStrings)
    {
        ConnectionString = connectionStrings[Database.Transfers];
    }

    private ILogger Log { get; } = Serilog.Log.ForContext<Z12092025_TransferLocalPathMigration>();
    private string ConnectionString { get; }

    public bool NeedsToBeApplied()
    {
        // check to see if the LocalPath column exists in the Transfers table
        var schema = SchemaInspector.GetDatabaseSchema(ConnectionString);
        var transfers = schema["Transfers"];

        var localPathExists = transfers.Any(c => c.Name.Equals("LocalPath", StringComparison.OrdinalIgnoreCase));

        if (localPathExists)
        {
            return false;
        }

        return true;
    }

    public void Apply()
    {
        if (!NeedsToBeApplied())
        {
            Log.Information("> Migration {Name} is not necessary or has already been applied", nameof(Z12092025_TransferLocalPathMigration));
            return;
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            Log.Information("> Adding LocalPath column to the Transfers table...");

            var command = new SqliteCommand(@"
                ALTER TABLE Transfers ADD COLUMN LocalPath TEXT
            ", connection, transaction);
            command.ExecuteNonQuery();

            Log.Information("> LocalPath column added");
            transaction.Commit();
            Log.Information("> Done!");
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
