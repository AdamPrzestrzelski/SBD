using System;
using System.Data;
using System.Data.SqlClient;
using SBD.Database;

namespace SBD.Repositories
{
    public class ReportRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public DataTable GetMonthlyRevenue()
        {
            return _db.ExecuteQuery(
                "SELECT REVENUE_YEAR, REVENUE_MONTH, TOTAL_RENTALS, TOTAL_PAYMENTS, " +
                "TOTAL_REVENUE, AVG_PAYMENT FROM V_MONTHLY_REVENUE");
        }

        public DataTable GetMonthlyRevenue(int month, int year)
        {
            return _db.ExecuteQuery(
                "SELECT REVENUE_YEAR, REVENUE_MONTH, TOTAL_RENTALS, TOTAL_PAYMENTS, " +
                "TOTAL_REVENUE, AVG_PAYMENT FROM V_MONTHLY_REVENUE " +
                "WHERE REVENUE_MONTH = @month AND REVENUE_YEAR = @year",
                new SqlParameter("@month", month),
                new SqlParameter("@year", year));
        }

        public DataTable GetCarUtilization()
        {
            return _db.ExecuteQuery(
                "SELECT CAR_ID, CAR_NAME, PLATE_NUMBER, CATEGORY_NAME, BRANCH_NAME, " +
                "CURRENT_STATUS, TOTAL_RENTALS, TOTAL_RENTAL_DAYS, TOTAL_REVENUE " +
                "FROM V_CAR_UTILIZATION ORDER BY TOTAL_RENTAL_DAYS DESC");
        }

        public DataTable GetHighRiskClients()
        {
            return _db.ExecuteQuery(
                "SELECT CLIENT_ID, CLIENT_NAME, EMAIL, IS_BLOCKED, " +
                "PENALTY_MULTIPLIER, PENALTY_COUNT, TOTAL_PENALTY_AMOUNT " +
                "FROM V_HIGH_RISK_CLIENTS");
        }

        public DataTable GetAvailableCars()
        {
            return _db.ExecuteQuery(
                "SELECT CAR_ID, BRAND, MODEL, YEAR, PLATE_NUMBER, DAILY_RATE, " +
                "COLOR, SEATS, MILEAGE, CATEGORY_NAME, BRANCH_NAME, BRANCH_CITY " +
                "FROM V_AVAILABLE_CARS ORDER BY DAILY_RATE");
        }

        public DataTable GetChangeHistory(string tableName = null, int? recordId = null, int topN = 50)
        {
            string sql = $"SELECT TOP {topN} HISTORY_ID, TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, " +
                         "CHANGE_DATE, CHANGED_BY, OPERATION_TYPE " +
                         "FROM CHANGE_HISTORY WHERE 1=1 ";

            var parameters = new System.Collections.Generic.List<SqlParameter>();

            if (!string.IsNullOrEmpty(tableName))
            {
                sql += "AND TABLE_NAME = @tableName ";
                parameters.Add(new SqlParameter("@tableName", tableName));
            }

            if (recordId.HasValue)
            {
                sql += "AND RECORD_ID = @recordId ";
                parameters.Add(new SqlParameter("@recordId", recordId.Value));
            }

            sql += "ORDER BY CHANGE_DATE DESC";

            return _db.ExecuteQuery(sql, parameters.ToArray());
        }

        public DataTable GetSystemSummary()
        {
            return _db.ExecuteQuery(
                "SELECT " +
                "(SELECT COUNT(*) FROM CLIENTS) AS TOTAL_CLIENTS, " +
                "(SELECT COUNT(*) FROM CARS) AS TOTAL_CARS, " +
                "(SELECT COUNT(*) FROM CARS WHERE STATUS = 'AVAILABLE') AS AVAILABLE_CARS, " +
                "(SELECT COUNT(*) FROM RENTALS WHERE STATUS = 'ACTIVE') AS ACTIVE_RENTALS, " +
                "(SELECT COUNT(*) FROM RESERVATIONS r JOIN RESERVATION_STATUSES rs ON r.STATUS_ID = rs.STATUS_ID WHERE rs.NAME = 'ACTIVE') AS ACTIVE_RESERVATIONS, " +
                "(SELECT ISNULL(SUM(p.AMOUNT), 0) FROM PAYMENTS p JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID WHERE ps.NAME = 'PAID') AS TOTAL_REVENUE, " +
                "(SELECT COUNT(*) FROM PENALTIES) AS TOTAL_PENALTIES, " +
                "(SELECT COUNT(*) FROM CLIENTS WHERE IS_BLOCKED = 1) AS BLOCKED_CLIENTS, " +
                "(SELECT COUNT(*) FROM BRANCHES) AS TOTAL_BRANCHES, " +
                "(SELECT COUNT(*) FROM EMPLOYEES) AS TOTAL_EMPLOYEES");
        }
    }
}
