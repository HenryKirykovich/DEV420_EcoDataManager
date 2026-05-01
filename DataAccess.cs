using System;
using System.Data;
using System.Data.SqlClient;


namespace EcoData_Manager
{
    public static class DataAccess
    {
        private static string GetConnectionString()
        {
            return @"Server=(localdb)\MSSQLLocalDB;Database=EcoManagementDB;Trusted_Connection=True;";
        }

        public static DataTable ExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                conn.Open();
                da.Fill(dt);
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static void BulkInsert(DataTable table, string destinationTable)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(destinationTable)) throw new ArgumentNullException(nameof(destinationTable));

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Find identity columns and remove them from a copy of the table
                var identityCols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA+'.'+TABLE_NAME), COLUMN_NAME, 'IsIdentity')=1",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@t", destinationTable);
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read()) identityCols.Add(rdr.GetString(0));
                }

                // Build a clean DataTable without identity columns
                DataTable insertTable = table.Clone();
                foreach (var idCol in identityCols)
                    if (insertTable.Columns.Contains(idCol))
                        insertTable.Columns.Remove(idCol);

                foreach (DataRow srcRow in table.Rows)
                {
                    DataRow newRow = insertTable.NewRow();
                    foreach (DataColumn col in insertTable.Columns)
                        newRow[col.ColumnName] = srcRow[col.ColumnName];
                    insertTable.Rows.Add(newRow);
                }

                using (var bulk = new SqlBulkCopy(conn))
                {
                    bulk.DestinationTableName = destinationTable;
                    bulk.WriteToServer(insertTable);
                }
            }
        }

        // TODO: add parameterized helpers and async versions as needed
    }
}
