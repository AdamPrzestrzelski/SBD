using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class RentalRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Rental> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, " +
                "r.START_DATE, r.END_DATE, r.ACTUAL_END_DATE, r.TOTAL_PRICE, r.STATUS, r.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "ORDER BY r.CREATED_AT DESC");

            return MapRentals(dt);
        }

        public Rental GetById(int rentalId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, " +
                "r.START_DATE, r.END_DATE, r.ACTUAL_END_DATE, r.TOTAL_PRICE, r.STATUS, r.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.RENTAL_ID = :id",
                new OracleParameter("id", rentalId));

            var list = MapRentals(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Rental> GetByClientId(int clientId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, " +
                "r.START_DATE, r.END_DATE, r.ACTUAL_END_DATE, r.TOTAL_PRICE, r.STATUS, r.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.CLIENT_ID = :clientId " +
                "ORDER BY r.START_DATE DESC",
                new OracleParameter("clientId", clientId));

            return MapRentals(dt);
        }

        public List<Rental> GetActive()
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, " +
                "r.START_DATE, r.END_DATE, r.ACTUAL_END_DATE, r.TOTAL_PRICE, r.STATUS, r.CREATED_AT, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.STATUS = 'ACTIVE' " +
                "ORDER BY r.END_DATE");

            return MapRentals(dt);
        }

        /// <summary>
        /// Tworzy nowe wypożyczenie przez pakiet PKG_RENTAL.
        /// </summary>
        public int CreateRental(int clientId, int carId, DateTime startDate, DateTime endDate)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OracleCommand("PKG_RENTAL.CREATE_RENTAL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_client_id", OracleDbType.Int32).Value = clientId;
                cmd.Parameters.Add("p_car_id", OracleDbType.Int32).Value = carId;
                cmd.Parameters.Add("p_start_date", OracleDbType.Date).Value = startDate;
                cmd.Parameters.Add("p_end_date", OracleDbType.Date).Value = endDate;

                var rentalIdParam = new OracleParameter("p_rental_id", OracleDbType.Decimal)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(rentalIdParam);

                cmd.ExecuteNonQuery();
                return Convert.ToInt32(rentalIdParam.Value.ToString());
            }
        }

        /// <summary>
        /// Anuluje wypożyczenie przez pakiet PKG_RENTAL.
        /// </summary>
        public void CancelRental(int rentalId)
        {
            _db.ExecuteProcedure("PKG_RENTAL.CANCEL_RENTAL",
                new OracleParameter("p_rental_id", rentalId));
        }

        /// <summary>
        /// Przedłuża wypożyczenie przez pakiet PKG_RENTAL.
        /// </summary>
        public void ExtendRental(int rentalId, DateTime newEndDate)
        {
            _db.ExecuteProcedure("PKG_RENTAL.EXTEND_RENTAL",
                new OracleParameter("p_rental_id", rentalId),
                new OracleParameter("p_new_end_date", newEndDate));
        }

        /// <summary>
        /// Kończy wypożyczenie przez pakiet PKG_RENTAL.
        /// </summary>
        public void CompleteRental(int rentalId)
        {
            _db.ExecuteProcedure("PKG_RENTAL.COMPLETE_RENTAL",
                new OracleParameter("p_rental_id", rentalId));
        }

        private List<Rental> MapRentals(DataTable dt)
        {
            var rentals = new List<Rental>();
            foreach (DataRow row in dt.Rows)
            {
                rentals.Add(new Rental
                {
                    RentalId = Convert.ToInt32(row["RENTAL_ID"]),
                    ReservationId = row["RESERVATION_ID"] != DBNull.Value ? Convert.ToInt32(row["RESERVATION_ID"]) : (int?)null,
                    ClientId = Convert.ToInt32(row["CLIENT_ID"]),
                    CarId = Convert.ToInt32(row["CAR_ID"]),
                    StartDate = Convert.ToDateTime(row["START_DATE"]),
                    EndDate = Convert.ToDateTime(row["END_DATE"]),
                    ActualEndDate = row["ACTUAL_END_DATE"] != DBNull.Value ? Convert.ToDateTime(row["ACTUAL_END_DATE"]) : (DateTime?)null,
                    TotalPrice = Convert.ToDecimal(row["TOTAL_PRICE"]),
                    Status = row["STATUS"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["CREATED_AT"]),
                    ClientName = row["CLIENT_NAME"]?.ToString(),
                    CarName = row["CAR_NAME"]?.ToString(),
                    PlateNumber = row["PLATE_NUMBER"]?.ToString()
                });
            }
            return rentals;
        }
    }
}
