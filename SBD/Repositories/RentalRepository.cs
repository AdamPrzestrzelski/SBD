using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
                "SELECT r.RENTAL_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.ACTUAL_RETURN_DATE, r.STATUS, r.TOTAL_PRICE, r.START_MILEAGE, r.END_MILEAGE, " +
                "cl.FIRST_NAME + ' ' + cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND + ' ' + c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "ORDER BY r.START_DATE DESC");

            return MapRentals(dt);
        }

        public List<Rental> GetActive()
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.ACTUAL_RETURN_DATE, r.STATUS, r.TOTAL_PRICE, r.START_MILEAGE, r.END_MILEAGE, " +
                "cl.FIRST_NAME + ' ' + cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND + ' ' + c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.STATUS = 'ACTIVE' " +
                "ORDER BY r.START_DATE");

            return MapRentals(dt);
        }

        public List<Rental> GetByClientId(int clientId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.ACTUAL_RETURN_DATE, r.STATUS, r.TOTAL_PRICE, r.START_MILEAGE, r.END_MILEAGE, " +
                "cl.FIRST_NAME + ' ' + cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND + ' ' + c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.CLIENT_ID = @clientId " +
                "ORDER BY r.START_DATE DESC",
                new SqlParameter("@clientId", clientId));

            return MapRentals(dt);
        }

        public Rental GetById(int rentalId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RENTAL_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.ACTUAL_RETURN_DATE, r.STATUS, r.TOTAL_PRICE, r.START_MILEAGE, r.END_MILEAGE, " +
                "cl.FIRST_NAME + ' ' + cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND + ' ' + c.MODEL AS CAR_NAME, c.PLATE_NUMBER " +
                "FROM RENTALS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "WHERE r.RENTAL_ID = @id",
                new SqlParameter("@id", rentalId));

            var list = MapRentals(dt);
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Tworzy wypożyczenie przez zapytanie.
        /// Używamy Output Inserted.RENTAL_ID, by pobrać nowe ID w SQL Server.
        /// </summary>
        public int CreateRental(int clientId, int carId, DateTime startDate, DateTime endDate)
        {
            // By symulować zachowanie procedury, wywołamy po prostu nową procedurę składowaną
            // i z użyciem parametru wyjściowego lub po prostu zapytaniem SELECT zwrócimy ID
            using (var conn = _db.GetConnection())
            using (var cmd = new SqlCommand("sp_CreateRental", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@p_client_id", clientId));
                cmd.Parameters.Add(new SqlParameter("@p_car_id", carId));
                cmd.Parameters.Add(new SqlParameter("@p_start_date", startDate));
                cmd.Parameters.Add(new SqlParameter("@p_end_date", endDate));
                
                var outParam = new SqlParameter("@p_rental_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outParam);

                conn.Open();
                cmd.ExecuteNonQuery();
                return (int)outParam.Value;
            }
        }

        public void CompleteRental(int rentalId)
        {
            _db.ExecuteProcedure("sp_CompleteRental",
                new SqlParameter("@p_rental_id", rentalId));
        }

        public void CancelRental(int rentalId)
        {
            _db.ExecuteProcedure("sp_CancelRental",
                new SqlParameter("@p_rental_id", rentalId));
        }

        public void ExtendRental(int rentalId, DateTime newEndDate)
        {
            _db.ExecuteProcedure("sp_ExtendRental",
                new SqlParameter("@p_rental_id", rentalId),
                new SqlParameter("@p_new_end_date", newEndDate));
        }

        private List<Rental> MapRentals(DataTable dt)
        {
            var rentals = new List<Rental>();
            foreach (DataRow row in dt.Rows)
            {
                rentals.Add(new Rental
                {
                    RentalId = Convert.ToInt32(row["RENTAL_ID"]),
                    ClientId = Convert.ToInt32(row["CLIENT_ID"]),
                    CarId = Convert.ToInt32(row["CAR_ID"]),
                    StartDate = Convert.ToDateTime(row["START_DATE"]),
                    EndDate = Convert.ToDateTime(row["END_DATE"]),
                    ActualReturnDate = row["ACTUAL_RETURN_DATE"] != DBNull.Value ? Convert.ToDateTime(row["ACTUAL_RETURN_DATE"]) : (DateTime?)null,
                    Status = row["STATUS"].ToString(),
                    TotalPrice = row["TOTAL_PRICE"] != DBNull.Value ? Convert.ToDecimal(row["TOTAL_PRICE"]) : 0,
                    StartMileage = Convert.ToInt32(row["START_MILEAGE"]),
                    EndMileage = row["END_MILEAGE"] != DBNull.Value ? Convert.ToInt32(row["END_MILEAGE"]) : (int?)null,
                    ClientName = row["CLIENT_NAME"]?.ToString(),
                    CarName = row["CAR_NAME"]?.ToString(),
                    PlateNumber = row["PLATE_NUMBER"]?.ToString()
                });
            }
            return rentals;
        }
    }
}
