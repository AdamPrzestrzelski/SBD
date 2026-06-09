using System;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace SBD.Database
{
    /// <summary>
    /// Singleton zarządzający połączeniem z bazą danych Oracle.
    /// </summary>
    public class DbConnection
    {
        private static DbConnection _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        private DbConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["OracleDb"]?.ConnectionString
                ?? throw new ConfigurationErrorsException(
                    "Brak connection stringa 'OracleDb' w App.config. " +
                    "Dodaj: <add name=\"OracleDb\" connectionString=\"...\" />");
        }

        public static DbConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new DbConnection();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Zwraca nowe otwarte połączenie z bazą danych.
        /// Pamiętaj o zamknięciu (using).
        /// </summary>
        public OracleConnection GetConnection()
        {
            var conn = new OracleConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Wykonuje zapytanie SELECT i zwraca DataTable z wynikami.
        /// </summary>
        public DataTable ExecuteQuery(string sql, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using (var adapter = new OracleDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Wykonuje zapytanie INSERT/UPDATE/DELETE i zwraca liczbę zmienionych wierszy.
        /// </summary>
        public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Wykonuje zapytanie i zwraca wartość skalarną (pierwsza kolumna, pierwszy wiersz).
        /// </summary>
        public object ExecuteScalar(string sql, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Wykonuje procedurę składowaną.
        /// </summary>
        public void ExecuteProcedure(string procedureName, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new OracleCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Wykonuje funkcję i zwraca wynik.
        /// </summary>
        public object ExecuteFunction(string functionName, OracleDbType returnType, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new OracleCommand(functionName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var returnParam = new OracleParameter("RETURN_VALUE", returnType)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                cmd.Parameters.Add(returnParam);

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();
                return returnParam.Value;
            }
        }

        /// <summary>
        /// Testuje połączenie z bazą danych.
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    Console.WriteLine("Połączenie z bazą danych Oracle nawiązane pomyślnie.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd połączenia z bazą danych: {ex.Message}");
                return false;
            }
        }
    }
}
