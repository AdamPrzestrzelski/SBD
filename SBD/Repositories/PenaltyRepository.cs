using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class PenaltyRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Penalty> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT p.PENALTY_ID, p.CLIENT_ID, p.RENTAL_ID, p.REASON, p.AMOUNT, p.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME " +
                "FROM PENALTIES p " +
                "JOIN CLIENTS cl ON p.CLIENT_ID = cl.CLIENT_ID " +
                "ORDER BY p.CREATED_AT DESC");

            return MapPenalties(dt);
        }

        public List<Penalty> GetByClientId(int clientId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT p.PENALTY_ID, p.CLIENT_ID, p.RENTAL_ID, p.REASON, p.AMOUNT, p.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME " +
                "FROM PENALTIES p " +
                "JOIN CLIENTS cl ON p.CLIENT_ID = cl.CLIENT_ID " +
                "WHERE p.CLIENT_ID = :clientId " +
                "ORDER BY p.CREATED_AT DESC",
                new OracleParameter("clientId", clientId));

            return MapPenalties(dt);
        }

        public void Insert(Penalty penalty)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO PENALTIES (CLIENT_ID, RENTAL_ID, REASON, AMOUNT) " +
                "VALUES (:clientId, :rentalId, :reason, :amount)",
                new OracleParameter("clientId", penalty.ClientId),
                new OracleParameter("rentalId", (object)penalty.RentalId ?? DBNull.Value),
                new OracleParameter("reason", penalty.Reason),
                new OracleParameter("amount", penalty.Amount));
        }

        public int GetPenaltyCount(int clientId)
        {
            var result = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM PENALTIES WHERE CLIENT_ID = :clientId",
                new OracleParameter("clientId", clientId));

            return Convert.ToInt32(result);
        }

        public decimal GetTotalPenaltyAmount(int clientId)
        {
            var result = _db.ExecuteScalar(
                "SELECT NVL(SUM(AMOUNT), 0) FROM PENALTIES WHERE CLIENT_ID = :clientId",
                new OracleParameter("clientId", clientId));

            return Convert.ToDecimal(result);
        }

        private List<Penalty> MapPenalties(DataTable dt)
        {
            var penalties = new List<Penalty>();
            foreach (DataRow row in dt.Rows)
            {
                penalties.Add(new Penalty
                {
                    PenaltyId = Convert.ToInt32(row["PENALTY_ID"]),
                    ClientId = Convert.ToInt32(row["CLIENT_ID"]),
                    RentalId = row["RENTAL_ID"] != DBNull.Value ? Convert.ToInt32(row["RENTAL_ID"]) : (int?)null,
                    Reason = row["REASON"].ToString(),
                    Amount = Convert.ToDecimal(row["AMOUNT"]),
                    CreatedAt = Convert.ToDateTime(row["CREATED_AT"]),
                    ClientName = row["CLIENT_NAME"]?.ToString()
                });
            }
            return penalties;
        }
    }
}
