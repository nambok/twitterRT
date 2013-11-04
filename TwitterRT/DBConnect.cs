using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace TwitterRT
{
    class DBConnect
    {
        private static MySqlConnection connection;

        public static void SetServerPrefences(string server, string user, string password, string database)
        {
            string connectionString = "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};";
            connection = new MySqlConnection(System.String.Format(connectionString, server, database, user, password));
        }

        private static void Connect()
        {
            connection.Open();
        }
        private static void Disconnect()
        {
            connection.Close();
        }

        public static DataTable Select(string query, MysqlParameterCollection collection)
        {
            try
            {
                Connect();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (collection != null)
                {
                    foreach (MySqlParameter param in collection.Parameters)
                        cmd.Parameters.Add(param);
                }
                MySqlDataReader resultSet = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(resultSet);
                Disconnect();
                return dt;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("\nDB ERROR: {0}", ex.Message);
                throw;
            }
        }

        public static long Insert(string query, MysqlParameterCollection collection)
        {
            try
            {
                Connect();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (collection != null)
                {
                    foreach (MySqlParameter param in collection.Parameters)
                        cmd.Parameters.Add(param);
                }
                cmd.ExecuteNonQuery();
                Disconnect();
                return cmd.LastInsertedId;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("\nDB ERROR: {0}", ex.Message);
                throw;
            }
        }
        public static void ExecuteQuery(string query, MysqlParameterCollection collection)
        {
            try
            {
                Connect();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (collection != null)
                {
                    foreach (MySqlParameter param in collection.Parameters)
                        cmd.Parameters.Add(param);
                }
                cmd.ExecuteNonQuery();
                Disconnect();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("\nDB ERROR: {0}", ex.Message);
                throw;
            }
        }
    }

    public class MysqlParameterCollection
    {
        private List<MySqlParameter> parameters = new List<MySqlParameter>();

        public List<MySqlParameter> Parameters { get { return parameters; } }

        public void Add(string parameterName, object value)
        {
            parameters.Add(new MySqlParameter(parameterName, value));
        }

        public void Clear()
        {
            parameters.Clear();
        }
    }
}
