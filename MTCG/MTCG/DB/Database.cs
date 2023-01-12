using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;


namespace MTCG.DB
{
    internal class Database
    {
        private readonly string _connectionString;

        public Database()
        {
            _connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=MTGCdb;";
        }
        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteQuery(string sql)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }
    }
}
