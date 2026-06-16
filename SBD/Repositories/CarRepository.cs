using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.COLOR, c.SEATS, c.MILEAGE, c.STATUS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "ORDER BY c.BRAND, c.MODEL");

            return MapCars(dt);
        }

        public List<Car> GetAvailable()
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.COLOR, c.SEATS, c.MILEAGE, c.STATUS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.STATUS = 'AVAILABLE' " +
                "ORDER BY c.DAILY_RATE");

            return MapCars(dt);
        }

        public Car GetById(int carId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.COLOR, c.SEATS, c.MILEAGE, c.STATUS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.CAR_ID = @id",
                new SqlParameter("@id", carId));

            var list = MapCars(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Car> SearchByCategory(int categoryId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.COLOR, c.SEATS, c.MILEAGE, c.STATUS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.CATEGORY_ID = @catId",
                new SqlParameter("@catId", categoryId));

            return MapCars(dt);
        }

        public List<Car> SearchByBranch(int branchId)
        {
            var dt = _db.ExecuteQuery(
                "SELECT c.CAR_ID, c.BRAND, c.MODEL, c.YEAR, c.PLATE_NUMBER, c.VIN, " +
                "c.DAILY_RATE, c.CATEGORY_ID, c.BRANCH_ID, c.COLOR, c.SEATS, c.MILEAGE, c.STATUS, " +
                "cat.NAME AS CATEGORY_NAME, b.NAME AS BRANCH_NAME " +
                "FROM CARS c " +
                "JOIN CAR_CATEGORIES cat ON c.CATEGORY_ID = cat.CATEGORY_ID " +
                "JOIN BRANCHES b ON c.BRANCH_ID = b.BRANCH_ID " +
                "WHERE c.BRANCH_ID = @branchId",
                new SqlParameter("@branchId", branchId));

            return MapCars(dt);
        }

        public void Insert(Car car)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO CARS (BRAND, MODEL, YEAR, PLATE_NUMBER, VIN, DAILY_RATE, " +
                "CATEGORY_ID, BRANCH_ID, COLOR, SEATS, MILEAGE, STATUS) " +
                "VALUES (@brand, @model, @year, @plate, @vin, @rate, @catId, @branchId, " +
                "@color, @seats, @mileage, 'AVAILABLE')",
                new SqlParameter("@brand", car.Brand),
                new SqlParameter("@model", car.Model),
                new SqlParameter("@year", car.Year),
                new SqlParameter("@plate", car.PlateNumber),
                new SqlParameter("@vin", car.Vin),
                new SqlParameter("@rate", car.DailyRate),
                new SqlParameter("@catId", car.CategoryId),
                new SqlParameter("@branchId", car.BranchId),
                new SqlParameter("@color", (object)car.Color ?? DBNull.Value),
                new SqlParameter("@seats", car.Seats),
                new SqlParameter("@mileage", car.Mileage));
        }

        public void Update(Car car)
        {
            _db.ExecuteNonQuery(
                "UPDATE CARS SET BRAND = @brand, MODEL = @model, DAILY_RATE = @rate, " +
                "STATUS = @status, MILEAGE = @mileage " +
                "WHERE CAR_ID = @id",
                new SqlParameter("@brand", car.Brand),
                new SqlParameter("@model", car.Model),
                new SqlParameter("@rate", car.DailyRate),
                new SqlParameter("@status", car.Status),
                new SqlParameter("@mileage", car.Mileage),
                new SqlParameter("@id", car.CarId));
        }

        public void Delete(int carId)
        {
            _db.ExecuteNonQuery("DELETE FROM CARS WHERE CAR_ID = @id", new SqlParameter("@id", carId));
        }

        public List<string> GetCategories()
        {
            var dt = _db.ExecuteQuery("SELECT CATEGORY_ID, NAME FROM CAR_CATEGORIES ORDER BY CATEGORY_ID");
            var list = new List<string>();
            foreach (DataRow row in dt.Rows)
                list.Add($"{row["CATEGORY_ID"]} - {row["NAME"]}");
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
                    Color = row["COLOR"]?.ToString(),
                    Seats = Convert.ToInt32(row["SEATS"]),
                    Mileage = Convert.ToInt32(row["MILEAGE"]),
                    Status = row["STATUS"].ToString(),
                    CategoryName = row["CATEGORY_NAME"]?.ToString(),
                    BranchName = row["BRANCH_NAME"]?.ToString()
                });
            }
            return cars;
        }
    }
}
