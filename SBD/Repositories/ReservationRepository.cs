using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class ReservationRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Reservation> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.STATUS_ID, r.CREATED_AT, r.NOTES, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, " +
                "rs.NAME AS STATUS_NAME " +
                "FROM RESERVATIONS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "JOIN RESERVATION_STATUSES rs ON r.STATUS_ID = rs.STATUS_ID " +
                "ORDER BY r.CREATED_AT DESC");

            return MapReservations(dt);
        }

        public List<Reservation> GetByClientId(int clientId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT r.RESERVATION_ID, r.CLIENT_ID, r.CAR_ID, r.START_DATE, r.END_DATE, " +
                "r.STATUS_ID, r.CREATED_AT, r.NOTES, " +
                "cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME, " +
                "c.BRAND || ' ' || c.MODEL AS CAR_NAME, " +
                "rs.NAME AS STATUS_NAME " +
                "FROM RESERVATIONS r " +
                "JOIN CLIENTS cl ON r.CLIENT_ID = cl.CLIENT_ID " +
                "JOIN CARS c ON r.CAR_ID = c.CAR_ID " +
                "JOIN RESERVATION_STATUSES rs ON r.STATUS_ID = rs.STATUS_ID " +
                "WHERE r.CLIENT_ID = :clientId " +
                "ORDER BY r.START_DATE DESC",
                new OracleParameter("clientId", clientId));

            return MapReservations(dt);
        }

        public void Insert(Reservation reservation)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO RESERVATIONS (CLIENT_ID, CAR_ID, START_DATE, END_DATE, STATUS_ID, NOTES) " +
                "VALUES (:clientId, :carId, :start, :end, :statusId, :notes)",
                new OracleParameter("clientId", reservation.ClientId),
                new OracleParameter("carId", reservation.CarId),
                new OracleParameter("start", reservation.StartDate),
                new OracleParameter("end", reservation.EndDate),
                new OracleParameter("statusId", reservation.StatusId),
                new OracleParameter("notes", (object)reservation.Notes ?? DBNull.Value));
        }

        public void UpdateStatus(int reservationId, int statusId)
        {
            _db.ExecuteNonQuery(
                "UPDATE RESERVATIONS SET STATUS_ID = :statusId WHERE RESERVATION_ID = :id",
                new OracleParameter("statusId", statusId),
                new OracleParameter("id", reservationId));
        }

        public void Delete(int reservationId)
        {
            _db.ExecuteNonQuery(
                "DELETE FROM RESERVATIONS WHERE RESERVATION_ID = :id",
                new OracleParameter("id", reservationId));
        }

        private List<Reservation> MapReservations(DataTable dt)
        {
            var reservations = new List<Reservation>();
            foreach (DataRow row in dt.Rows)
            {
                reservations.Add(new Reservation
                {
                    ReservationId = Convert.ToInt32(row["RESERVATION_ID"]),
                    ClientId = Convert.ToInt32(row["CLIENT_ID"]),
                    CarId = Convert.ToInt32(row["CAR_ID"]),
                    StartDate = Convert.ToDateTime(row["START_DATE"]),
                    EndDate = Convert.ToDateTime(row["END_DATE"]),
                    StatusId = Convert.ToInt32(row["STATUS_ID"]),
                    CreatedAt = Convert.ToDateTime(row["CREATED_AT"]),
                    Notes = row["NOTES"]?.ToString(),
                    ClientName = row["CLIENT_NAME"]?.ToString(),
                    CarName = row["CAR_NAME"]?.ToString(),
                    StatusName = row["STATUS_NAME"]?.ToString()
                });
            }
            return reservations;
        }
    }
}
