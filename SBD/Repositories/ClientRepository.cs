using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class ClientRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Client> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT CLIENT_ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, " +
                "ADDRESS, CITY, POSTAL_CODE, IS_BLOCKED, PENALTY_MULTIPLIER, CREATED_AT, UPDATED_AT " +
                "FROM CLIENTS ORDER BY CLIENT_ID");

            return MapClients(dt);
        }

        public Client GetById(int clientId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT CLIENT_ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, " +
                "ADDRESS, CITY, POSTAL_CODE, IS_BLOCKED, PENALTY_MULTIPLIER, CREATED_AT, UPDATED_AT " +
                "FROM CLIENTS WHERE CLIENT_ID = :id",
                new OracleParameter("id", clientId));

            var list = MapClients(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Client> Search(string searchTerm)
        {
            var dt = _db.ExecuteQuery(
                "SELECT CLIENT_ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, " +
                "ADDRESS, CITY, POSTAL_CODE, IS_BLOCKED, PENALTY_MULTIPLIER, CREATED_AT, UPDATED_AT " +
                "FROM CLIENTS WHERE UPPER(FIRST_NAME || ' ' || LAST_NAME) LIKE UPPER(:term) " +
                "OR UPPER(EMAIL) LIKE UPPER(:term) " +
                "ORDER BY LAST_NAME",
                new OracleParameter("term", $"%{searchTerm}%"));

            return MapClients(dt);
        }

        public int Insert(Client client)
        {
            var result = _db.ExecuteScalar(
                "INSERT INTO CLIENTS (FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, ADDRESS, CITY, POSTAL_CODE, PASSWORD_HASH) " +
                "VALUES (:fname, :lname, :email, :phone, :pesel, :addr, :city, :postal, :pass) " +
                "RETURNING CLIENT_ID INTO :id",
                new OracleParameter("fname", client.FirstName),
                new OracleParameter("lname", client.LastName),
                new OracleParameter("email", client.Email),
                new OracleParameter("phone", (object)client.Phone ?? DBNull.Value),
                new OracleParameter("pesel", (object)client.Pesel ?? DBNull.Value),
                new OracleParameter("addr", (object)client.Address ?? DBNull.Value),
                new OracleParameter("city", (object)client.City ?? DBNull.Value),
                new OracleParameter("postal", (object)client.PostalCode ?? DBNull.Value),
                new OracleParameter("pass", client.PasswordHash ?? "default_hash"),
                new OracleParameter("id", OracleDbType.Decimal) { Direction = ParameterDirection.Output });

            // Użyj ExecuteNonQuery z RETURNING
            using (var conn = _db.GetConnection())
            using (var cmd = new OracleCommand(
                "INSERT INTO CLIENTS (FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, ADDRESS, CITY, POSTAL_CODE, PASSWORD_HASH) " +
                "VALUES (:fname, :lname, :email, :phone, :pesel, :addr, :city, :postal, :pass) " +
                "RETURNING CLIENT_ID INTO :id", conn))
            {
                cmd.Parameters.Add("fname", client.FirstName);
                cmd.Parameters.Add("lname", client.LastName);
                cmd.Parameters.Add("email", client.Email);
                cmd.Parameters.Add("phone", (object)client.Phone ?? DBNull.Value);
                cmd.Parameters.Add("pesel", (object)client.Pesel ?? DBNull.Value);
                cmd.Parameters.Add("addr", (object)client.Address ?? DBNull.Value);
                cmd.Parameters.Add("city", (object)client.City ?? DBNull.Value);
                cmd.Parameters.Add("postal", (object)client.PostalCode ?? DBNull.Value);
                cmd.Parameters.Add("pass", client.PasswordHash ?? "default_hash");

                var idParam = new OracleParameter("id", OracleDbType.Decimal)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(idParam);
                cmd.ExecuteNonQuery();

                return Convert.ToInt32(idParam.Value.ToString());
            }
        }

        public void Update(Client client)
        {
            _db.ExecuteNonQuery(
                "UPDATE CLIENTS SET FIRST_NAME = :fname, LAST_NAME = :lname, EMAIL = :email, " +
                "PHONE = :phone, ADDRESS = :addr, CITY = :city, POSTAL_CODE = :postal " +
                "WHERE CLIENT_ID = :id",
                new OracleParameter("fname", client.FirstName),
                new OracleParameter("lname", client.LastName),
                new OracleParameter("email", client.Email),
                new OracleParameter("phone", (object)client.Phone ?? DBNull.Value),
                new OracleParameter("addr", (object)client.Address ?? DBNull.Value),
                new OracleParameter("city", (object)client.City ?? DBNull.Value),
                new OracleParameter("postal", (object)client.PostalCode ?? DBNull.Value),
                new OracleParameter("id", client.ClientId));
        }

        public void Delete(int clientId)
        {
            _db.ExecuteNonQuery(
                "DELETE FROM CLIENTS WHERE CLIENT_ID = :id",
                new OracleParameter("id", clientId));
        }

        public void BlockClient(int clientId)
        {
            _db.ExecuteNonQuery(
                "UPDATE CLIENTS SET IS_BLOCKED = 1, PENALTY_MULTIPLIER = 1.50 WHERE CLIENT_ID = :id",
                new OracleParameter("id", clientId));
        }

        public void UnblockClient(int clientId)
        {
            _db.ExecuteNonQuery(
                "UPDATE CLIENTS SET IS_BLOCKED = 0, PENALTY_MULTIPLIER = 1.00 WHERE CLIENT_ID = :id",
                new OracleParameter("id", clientId));
        }

        public List<Client> GetBlockedClients()
        {
            var dt = _db.ExecuteQuery(
                "SELECT CLIENT_ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, " +
                "ADDRESS, CITY, POSTAL_CODE, IS_BLOCKED, PENALTY_MULTIPLIER, CREATED_AT, UPDATED_AT " +
                "FROM CLIENTS WHERE IS_BLOCKED = 1 ORDER BY LAST_NAME");

            return MapClients(dt);
        }

        private List<Client> MapClients(DataTable dt)
        {
            var clients = new List<Client>();
            foreach (DataRow row in dt.Rows)
            {
                clients.Add(new Client
                {
                    ClientId = Convert.ToInt32(row["CLIENT_ID"]),
                    FirstName = row["FIRST_NAME"].ToString(),
                    LastName = row["LAST_NAME"].ToString(),
                    Email = row["EMAIL"].ToString(),
                    Phone = row["PHONE"]?.ToString(),
                    Pesel = row["PESEL"]?.ToString(),
                    Address = row["ADDRESS"]?.ToString(),
                    City = row["CITY"]?.ToString(),
                    PostalCode = row["POSTAL_CODE"]?.ToString(),
                    IsBlocked = Convert.ToInt32(row["IS_BLOCKED"]) == 1,
                    PenaltyMultiplier = Convert.ToDecimal(row["PENALTY_MULTIPLIER"]),
                    CreatedAt = Convert.ToDateTime(row["CREATED_AT"]),
                    UpdatedAt = Convert.ToDateTime(row["UPDATED_AT"])
                });
            }
            return clients;
        }
    }
}
