using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class ClientRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Client> GetAll()
        {
            var dt = _db.ExecuteQuery("SELECT * FROM CLIENTS ORDER BY LAST_NAME, FIRST_NAME");
            return MapClients(dt);
        }

        public Client GetById(int clientId)
        {
            var dt = _db.ExecuteQuery("SELECT * FROM CLIENTS WHERE CLIENT_ID = @id", 
                new SqlParameter("@id", clientId));
                
            var list = MapClients(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Client> Search(string term)
        {
            var dt = _db.ExecuteQuery(
                "SELECT * FROM CLIENTS WHERE " +
                "FIRST_NAME LIKE '%' + @term + '%' OR " +
                "LAST_NAME LIKE '%' + @term + '%' OR " +
                "EMAIL LIKE '%' + @term + '%' " +
                "ORDER BY LAST_NAME",
                new SqlParameter("@term", term));
            return MapClients(dt);
        }

        public List<Client> GetBlockedClients()
        {
            var dt = _db.ExecuteQuery("SELECT * FROM CLIENTS WHERE IS_BLOCKED = 1 ORDER BY LAST_NAME");
            return MapClients(dt);
        }

        public int Insert(Client client)
        {
            // Opcja RETURNING w SQL Server to np. SCOPE_IDENTITY() lub OUTPUT Inserted.CLIENT_ID
            var result = _db.ExecuteScalar(
                "INSERT INTO CLIENTS (FIRST_NAME, LAST_NAME, EMAIL, PHONE, PESEL, ADDRESS, CITY, POSTAL_CODE, PASSWORD_HASH) " +
                "OUTPUT Inserted.CLIENT_ID " +
                "VALUES (@fname, @lname, @email, @phone, @pesel, @address, @city, @postal, @pass)",
                new SqlParameter("@fname", client.FirstName),
                new SqlParameter("@lname", client.LastName),
                new SqlParameter("@email", client.Email),
                new SqlParameter("@phone", (object)client.Phone ?? DBNull.Value),
                new SqlParameter("@pesel", (object)client.Pesel ?? DBNull.Value),
                new SqlParameter("@address", (object)client.Address ?? DBNull.Value),
                new SqlParameter("@city", (object)client.City ?? DBNull.Value),
                new SqlParameter("@postal", (object)client.PostalCode ?? DBNull.Value),
                new SqlParameter("@pass", client.PasswordHash ?? "default_hash"));
                
            return Convert.ToInt32(result);
        }

        public void Update(Client client)
        {
            _db.ExecuteNonQuery(
                "UPDATE CLIENTS SET FIRST_NAME = @fname, LAST_NAME = @lname, EMAIL = @email, " +
                "PHONE = @phone, ADDRESS = @address, CITY = @city " +
                "WHERE CLIENT_ID = @id",
                new SqlParameter("@fname", client.FirstName),
                new SqlParameter("@lname", client.LastName),
                new SqlParameter("@email", client.Email),
                new SqlParameter("@phone", (object)client.Phone ?? DBNull.Value),
                new SqlParameter("@address", (object)client.Address ?? DBNull.Value),
                new SqlParameter("@city", (object)client.City ?? DBNull.Value),
                new SqlParameter("@id", client.ClientId));
        }

        public void Delete(int clientId)
        {
            _db.ExecuteNonQuery("DELETE FROM CLIENTS WHERE CLIENT_ID = @id", 
                new SqlParameter("@id", clientId));
        }

        public void BlockClient(int clientId)
        {
            _db.ExecuteProcedure("sp_BlockClient", new SqlParameter("@p_client_id", clientId));
        }

        public void UnblockClient(int clientId)
        {
            _db.ExecuteNonQuery("UPDATE CLIENTS SET IS_BLOCKED = 0, PENALTY_MULTIPLIER = 1.0 WHERE CLIENT_ID = @id",
                new SqlParameter("@id", clientId));
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
                    RegistrationDate = Convert.ToDateTime(row["REGISTRATION_DATE"]),
                    IsBlocked = Convert.ToInt32(row["IS_BLOCKED"]) == 1,
                    PenaltyMultiplier = Convert.ToDecimal(row["PENALTY_MULTIPLIER"])
                });
            }
            return clients;
        }
    }
}
