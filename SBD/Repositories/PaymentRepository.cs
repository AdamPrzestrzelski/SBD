using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class PaymentRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Payment> GetByRentalId(int rentalId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT p.PAYMENT_ID, p.RENTAL_ID, p.AMOUNT, p.PAYMENT_DATE, " +
                "p.STATUS_ID, p.INSTALLMENT_NO, p.TOTAL_INSTALLMENTS, p.PAYMENT_METHOD, " +
                "ps.NAME AS STATUS_NAME " +
                "FROM PAYMENTS p " +
                "JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID " +
                "WHERE p.RENTAL_ID = @rentalId " +
                "ORDER BY p.INSTALLMENT_NO",
                new SqlParameter("@rentalId", rentalId));

            return MapPayments(dt);
        }

        public List<Payment> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT p.PAYMENT_ID, p.RENTAL_ID, p.AMOUNT, p.PAYMENT_DATE, " +
                "p.STATUS_ID, p.INSTALLMENT_NO, p.TOTAL_INSTALLMENTS, p.PAYMENT_METHOD, " +
                "ps.NAME AS STATUS_NAME " +
                "FROM PAYMENTS p " +
                "JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID " +
                "ORDER BY p.PAYMENT_DATE DESC"); // W SQL Server nie ma "NULLS LAST" bezpośrednio w ORDER BY, zazwyczaj robimy order by (case when PAYMENT_DATE is null then 1 else 0 end), PAYMENT_DATE DESC

            return MapPayments(dt);
        }

        public List<Payment> GetPending()
        {
            var dt = _db.ExecuteQuery(
                "SELECT p.PAYMENT_ID, p.RENTAL_ID, p.AMOUNT, p.PAYMENT_DATE, " +
                "p.STATUS_ID, p.INSTALLMENT_NO, p.TOTAL_INSTALLMENTS, p.PAYMENT_METHOD, " +
                "ps.NAME AS STATUS_NAME " +
                "FROM PAYMENTS p " +
                "JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID " +
                "WHERE ps.NAME = 'PENDING' " +
                "ORDER BY p.RENTAL_ID");

            return MapPayments(dt);
        }

        /// <summary>
        /// Przetwarza płatność.
        /// </summary>
        public void ProcessPayment(int rentalId, decimal amount, string paymentMethod = "CARD")
        {
            _db.ExecuteProcedure("sp_ProcessPayment",
                new SqlParameter("@p_rental_id", rentalId),
                new SqlParameter("@p_amount", amount),
                new SqlParameter("@p_payment_method", paymentMethod));
        }

        /// <summary>
        /// Tworzy plan ratalny.
        /// </summary>
        public void CreateInstallmentPlan(int rentalId, int numberOfInstallments)
        {
            _db.ExecuteProcedure("sp_CalculateInstallments",
                new SqlParameter("@p_rental_id", rentalId),
                new SqlParameter("@p_num_installments", numberOfInstallments));
        }

        /// <summary>
        /// Oznacza konkretną płatność jako opłaconą.
        /// </summary>
        public void MarkAsPaid(int paymentId)
        {
            _db.ExecuteNonQuery(
                "UPDATE PAYMENTS SET STATUS_ID = (SELECT STATUS_ID FROM PAYMENT_STATUSES WHERE NAME = 'PAID'), " +
                "PAYMENT_DATE = GETDATE() WHERE PAYMENT_ID = @id",
                new SqlParameter("@id", paymentId));
        }

        private List<Payment> MapPayments(DataTable dt)
        {
            var payments = new List<Payment>();
            foreach (DataRow row in dt.Rows)
            {
                payments.Add(new Payment
                {
                    PaymentId = Convert.ToInt32(row["PAYMENT_ID"]),
                    RentalId = Convert.ToInt32(row["RENTAL_ID"]),
                    Amount = Convert.ToDecimal(row["AMOUNT"]),
                    PaymentDate = row["PAYMENT_DATE"] != DBNull.Value ? Convert.ToDateTime(row["PAYMENT_DATE"]) : (DateTime?)null,
                    StatusId = Convert.ToInt32(row["STATUS_ID"]),
                    InstallmentNo = Convert.ToInt32(row["INSTALLMENT_NO"]),
                    TotalInstallments = Convert.ToInt32(row["TOTAL_INSTALLMENTS"]),
                    PaymentMethod = row["PAYMENT_METHOD"]?.ToString(),
                    StatusName = row["STATUS_NAME"]?.ToString()
                });
            }
            return payments;
        }
    }
}
