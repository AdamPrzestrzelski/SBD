using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class BranchRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Branch> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT BRANCH_ID, NAME, ADDRESS, CITY, POSTAL_CODE, PHONE, EMAIL, IS_ACTIVE " +
                "FROM BRANCHES ORDER BY NAME");

            var branches = new List<Branch>();
            foreach (DataRow row in dt.Rows)
            {
                branches.Add(new Branch
                {
                    BranchId = Convert.ToInt32(row["BRANCH_ID"]),
                    Name = row["NAME"].ToString(),
                    Address = row["ADDRESS"].ToString(),
                    City = row["CITY"].ToString(),
                    PostalCode = row["POSTAL_CODE"]?.ToString(),
                    Phone = row["PHONE"]?.ToString(),
                    Email = row["EMAIL"]?.ToString(),
                    IsActive = Convert.ToInt32(row["IS_ACTIVE"]) == 1
                });
            }
            return branches;
        }

        public Branch GetById(int branchId)
        {
            var list = GetAll();
            return list.Find(b => b.BranchId == branchId);
        }

        public void Insert(Branch branch)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO BRANCHES (NAME, ADDRESS, CITY, POSTAL_CODE, PHONE, EMAIL) " +
                "VALUES (@name, @addr, @city, @postal, @phone, @email)",
                new SqlParameter("@name", branch.Name),
                new SqlParameter("@addr", branch.Address),
                new SqlParameter("@city", branch.City),
                new SqlParameter("@postal", (object)branch.PostalCode ?? DBNull.Value),
                new SqlParameter("@phone", (object)branch.Phone ?? DBNull.Value),
                new SqlParameter("@email", (object)branch.Email ?? DBNull.Value));
        }

        public void Update(Branch branch)
        {
            _db.ExecuteNonQuery(
                "UPDATE BRANCHES SET NAME = @name, ADDRESS = @addr, CITY = @city, " +
                "POSTAL_CODE = @postal, PHONE = @phone, EMAIL = @email " +
                "WHERE BRANCH_ID = @id",
                new SqlParameter("@name", branch.Name),
                new SqlParameter("@addr", branch.Address),
                new SqlParameter("@city", branch.City),
                new SqlParameter("@postal", (object)branch.PostalCode ?? DBNull.Value),
                new SqlParameter("@phone", (object)branch.Phone ?? DBNull.Value),
                new SqlParameter("@email", (object)branch.Email ?? DBNull.Value),
                new SqlParameter("@id", branch.BranchId));
        }
    }
}
