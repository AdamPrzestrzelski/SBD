using System;
using System.Data;
using System.Data.SqlClient;

namespace SBD.Database
{
    /// <summary>
    /// Klasa zarządzająca połączeniem z bazą danych SQL Server.
    /// Wzorzec Singleton zapewnia jedną instancję dla całej aplikacji.
    /// </summary>
    public sealed class DbConnection
    {
        private static readonly Lazy<DbConnection> _instance = new Lazy<DbConnection>(() => new DbConnection());
        public static DbConnection Instance => _instance.Value;

        public static string ConnectionString { get; set; }

        private readonly string _connectionString;

        private DbConnection()
        {
            _connectionString = ConnectionString ?? "Server=(localdb)\\MSSQLLocalDB;Database=CarRentDB;Trusted_Connection=True;";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Zwraca wynik jako DataTable (zazwyczaj z SELECT).
        /// </summary>
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Wykonuje polecenie nie zwracające wierszy (INSERT, UPDATE, DELETE).
        /// Zwraca liczbę zmienionych wierszy.
        /// </summary>
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Wykonuje polecenie zwracające pojedynczą wartość (np. COUNT(*), funkcja skalarana).
        /// </summary>
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Pomocnicza metoda do wykonywania procedur składowanych.
        /// </summary>
        public void ExecuteProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(procedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Testuje połączenie z bazą danych.
        /// </summary>
        public void TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Pomyślnie połączono z bazą danych SQL Server!");
                    
                    // Pokazanie wersji serwera
                    using (var cmd = new SqlCommand("SELECT @@VERSION", connection))
                    {
                        var version = cmd.ExecuteScalar();
                        Console.WriteLine($"Wersja serwera: {version}");
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Błąd połączenia z bazą danych: " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}
