using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using SBD.Database;
using SBD.Models;

namespace SBD.Repositories
{
    public class CarRepository
    {
        private readonly DbConnection _db = DbConnection.Instance;

        public List<Car> GetAll()
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.STATUS, c.MILEAGE, c.COLOR, c.SEATS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "ORDER BY c.CAR_ID");

            return MapCars(dt);
        }

        public Car GetById(int carId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.STATUS, c.MILEAGE, c.COLOR, c.SEATS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.CAR_ID = :id",
                new OracleParameter("id", carId));

            var list = MapCars(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Car> GetAvailable()
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.STATUS, c.MILEAGE, c.COLOR, c.SEATS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.STATUS = 'AVAILABLE' " +
                "ORDER BY c.DAILY_RATE");

            return MapCars(dt);
        }

        public List<Car> SearchByCategory(int categoryId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.STATUS, c.MILEAGE, c.COLOR, c.SEATS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.CATEGORY_ID = :catId AND c.STATUS = 'AVAILABLE' " +
                "ORDER BY c.DAILY_RATE",
                new OracleParameter("catId", categoryId));

            return MapCars(dt);
        }

        public List<Car> SearchByBranch(int branchId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.STATUS, c.MILEAGE, c.COLOR, c.SEATS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.BRANCH_ID = :brId AND c.STATUS = 'AVAILABLE' " +
                "ORDER BY c.DAILY_RATE",
                new OracleParameter("brId", branchId));

            return MapCars(dt);
        }

        public void Insert(Car car)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO CARS (BRAND, MODEL, YEAR, PLATE_NUMBER, VIN, DAILY_RATE, " +
                "CATEGORY_ID, BRANCH_ID, STATUS, MILEAGE, COLOR, SEATS) " +
                "VALUES (:brand, :model, :year, :plate, :vin, :rate, :catId, :brId, :status, :mileage, :color, :seats)",
                new OracleParameter("brand", car.Brand),
                new OracleParameter("model", car.Model),
                new OracleParameter("year", car.Year),
                new OracleParameter("plate", car.PlateNumber),
                new OracleParameter("vin", car.Vin),
                new OracleParameter("rate", car.DailyRate),
                new OracleParameter("catId", car.CategoryId),
                new OracleParameter("brId", car.BranchId),
                new OracleParameter("status", car.Status),
                new OracleParameter("mileage", car.Mileage),
                new OracleParameter("color", (object)car.Color ?? DBNull.Value),
                new OracleParameter("seats", car.Seats));
        }

        public void Update(Car car)
        {
            _db.ExecuteNonQuery(
                "UPDATE CARS SET BRAND = :brand, MODEL = :model, YEAR = :year, " +
                "DAILY_RATE = :rate, CATEGORY_ID = :catId, BRANCH_ID = :brId, " +
                "STATUS = :status, MILEAGE = :mileage, COLOR = :color, SEATS = :seats " +
                "WHERE CAR_ID = :id",
                new OracleParameter("brand", car.Brand),
                new OracleParameter("model", car.Model),
                new OracleParameter("year", car.Year),
                new OracleParameter("rate", car.DailyRate),
                new OracleParameter("catId", car.CategoryId),
                new OracleParameter("brId", car.BranchId),
                new OracleParameter("status", car.Status),
                new OracleParameter("mileage", car.Mileage),
                new OracleParameter("color", (object)car.Color ?? DBNull.Value),
                new OracleParameter("seats", car.Seats),
                new OracleParameter("id", car.CarId));
        }

        public void Delete(int carId)
        {
            _db.ExecuteNonQuery("DELETE FROM CARS WHERE CAR_ID = :id",
                new OracleParameter("id", carId));
        }

        public List<CarCategory> GetCategories()
        {
            var dt = _db.ExecuteQuery("SELECT CATEGORY_ID, NAME, DESCRIPTION FROM CAR_CATEGORIES ORDER BY NAME");
            var list = new List<CarCategory>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CarCategory
                {
                    CategoryId = Convert.ToInt32(row["CATEGORY_ID"]),
                    Name = row["NAME"].ToString(),
                    Description = row["DESCRIPTION"]?.ToString()
                });
            }
            return list;
        }

        private List<Car> MapCars(DataTable dt)
        {
            var cars = new List<Car>();
            foreach (DataRow row in dt.Rows)
            {
                cars.Add(new Car
                {
                    CarId = Convert.ToInt32(row["CAR_ID"]),
                    Brand = row["BRAND"].ToString(),
                    Model = row["MODEL"].ToString(),
                    Year = Convert.ToInt32(row["YEAR"]),
                    PlateNumber = row["PLATE_NUMBER"].ToString(),
                    Vin = row["VIN"].ToString(),
                    DailyRate = Convert.ToDecimal(row["DAILY_RATE"]),
                    CategoryId = Convert.ToInt32(row["CATEGORY_ID"]),
                    BranchId = Convert.ToInt32(row["BRANCH_ID"]),
                    Status = row["STATUS"].ToString(),
                    Mileage = Convert.ToInt32(row["MILEAGE"]),
                    Color = row["COLOR"]?.ToString(),
                    Seats = Convert.ToInt32(row["SEATS"]),
                    CategoryName = row["CATEGORY_NAME"]?.ToString(),
                    BranchName = row["BRANCH_NAME"]?.ToString()
                });
            }
            return cars;
        }
    }
}
