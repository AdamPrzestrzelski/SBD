using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class EmployeeRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Employee> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT e.EMPLOYEE_ID, e.FIRST_NAME, e.LAST_NAME, e.EMAIL, e.PHONE, " +
                "e.POSITION, e.BRANCH_ID, e.HIRE_DATE, e.IS_ACTIVE, " +
                "b.NAME AS BRANCH_NAME " +
                "FROM EMPLOYEES e " +
                "JOIN BRANCHES b ON e.BRANCH_ID = b.BRANCH_ID " +
                "ORDER BY e.LAST_NAME");

            return MapEmployees(dt);
        }

        public Employee GetById(int employeeId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT e.EMPLOYEE_ID, e.FIRST_NAME, e.LAST_NAME, e.EMAIL, e.PHONE, " +
                "e.POSITION, e.BRANCH_ID, e.HIRE_DATE, e.IS_ACTIVE, " +
                "b.NAME AS BRANCH_NAME " +
                "FROM EMPLOYEES e " +
                "JOIN BRANCHES b ON e.BRANCH_ID = b.BRANCH_ID " +
                "WHERE e.EMPLOYEE_ID = @id",
                new SqlParameter("@id", employeeId));

            var list = MapEmployees(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public void Insert(Employee employee)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO EMPLOYEES (FIRST_NAME, LAST_NAME, EMAIL, PHONE, POSITION, BRANCH_ID, PASSWORD_HASH) " +
                "VALUES (@fname, @lname, @email, @phone, @pos, @brId, @pass)",
                new SqlParameter("@fname", employee.FirstName),
                new SqlParameter("@lname", employee.LastName),
                new SqlParameter("@email", employee.Email),
                new SqlParameter("@phone", (object)employee.Phone ?? DBNull.Value),
                new SqlParameter("@pos", employee.Position),
                new SqlParameter("@brId", employee.BranchId),
                new SqlParameter("@pass", employee.PasswordHash ?? "default_hash"));
        }

        public void Update(Employee employee)
        {
            _db.ExecuteNonQuery(
                "UPDATE EMPLOYEES SET FIRST_NAME = @fname, LAST_NAME = @lname, EMAIL = @email, " +
                "PHONE = @phone, POSITION = @pos, BRANCH_ID = @brId " +
                "WHERE EMPLOYEE_ID = @id",
                new SqlParameter("@fname", employee.FirstName),
                new SqlParameter("@lname", employee.LastName),
                new SqlParameter("@email", employee.Email),
                new SqlParameter("@phone", (object)employee.Phone ?? DBNull.Value),
                new SqlParameter("@pos", employee.Position),
                new SqlParameter("@brId", employee.BranchId),
                new SqlParameter("@id", employee.EmployeeId));
        }

        public void Delete(int employeeId)
        {
            _db.ExecuteNonQuery(
                "DELETE FROM EMPLOYEES WHERE EMPLOYEE_ID = @id",
                new SqlParameter("@id", employeeId));
        }

        private List<Employee> MapEmployees(DataTable dt)
        {
            var employees = new List<Employee>();
            foreach (DataRow row in dt.Rows)
            {
                employees.Add(new Employee
                {
                    EmployeeId = Convert.ToInt32(row["EMPLOYEE_ID"]),
                    FirstName = row["FIRST_NAME"].ToString(),
                    LastName = row["LAST_NAME"].ToString(),
                    Email = row["EMAIL"].ToString(),
                    Phone = row["PHONE"]?.ToString(),
                    Position = row["POSITION"].ToString(),
                    BranchId = Convert.ToInt32(row["BRANCH_ID"]),
                    HireDate = Convert.ToDateTime(row["HIRE_DATE"]),
                    IsActive = Convert.ToInt32(row["IS_ACTIVE"]) == 1,
                    BranchName = row["BRANCH_NAME"]?.ToString()
                });
            }
            return employees;
        }
    }
}
