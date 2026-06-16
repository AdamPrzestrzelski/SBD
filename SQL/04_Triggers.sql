-- 04_Triggers.sql w SQL Server

GO
CREATE OR ALTER TRIGGER trg_Clients_History
ON CLIENTS
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @op NVARCHAR(20) = CASE WHEN EXISTS(SELECT * FROM inserted) THEN 'UPDATE' ELSE 'DELETE' END;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, CHANGED_BY, OPERATION_TYPE)
    SELECT 
        'CLIENTS',
        d.CLIENT_ID,
        'Name: ' + d.FIRST_NAME + ' ' + d.LAST_NAME + ', Email: ' + d.EMAIL,
        CASE WHEN @op = 'UPDATE' THEN 'Name: ' + i.FIRST_NAME + ' ' + i.LAST_NAME + ', Email: ' + i.EMAIL ELSE NULL END,
        SUSER_SNAME(),
        @op
    FROM deleted d
    LEFT JOIN inserted i ON d.CLIENT_ID = i.CLIENT_ID;
END;
GO

CREATE OR ALTER TRIGGER trg_Cars_History
ON CARS
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @op NVARCHAR(20) = CASE WHEN EXISTS(SELECT * FROM inserted) THEN 'UPDATE' ELSE 'DELETE' END;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, CHANGED_BY, OPERATION_TYPE)
    SELECT 
        'CARS',
        d.CAR_ID,
        'Status: ' + d.STATUS + ', Mileage: ' + CAST(d.MILEAGE AS NVARCHAR),
        CASE WHEN @op = 'UPDATE' THEN 'Status: ' + i.STATUS + ', Mileage: ' + CAST(i.MILEAGE AS NVARCHAR) ELSE NULL END,
        SUSER_SNAME(),
        @op
    FROM deleted d
    LEFT JOIN inserted i ON d.CAR_ID = i.CAR_ID;
END;
GO

CREATE OR ALTER TRIGGER trg_Rental_Validation
ON RENTALS
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @client_id INT, @car_id INT, @start_date DATETIME, @end_date DATETIME;
    DECLARE @is_blocked INT;
    DECLARE @is_avail INT;

    -- Prosty kursor dla uproszczenia (zazwyczaj insert to 1 wiersz)
    DECLARE cur CURSOR FOR SELECT CLIENT_ID, CAR_ID, START_DATE, END_DATE FROM inserted;
    OPEN cur;
    FETCH NEXT FROM cur INTO @client_id, @car_id, @start_date, @end_date;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @is_blocked = IS_BLOCKED FROM CLIENTS WHERE CLIENT_ID = @client_id;
        IF @is_blocked = 1
        BEGIN
            RAISERROR('Klient jest zablokowany i nie może wypożyczyć samochodu.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        SET @is_avail = dbo.fn_CheckCarAvailability(@car_id, @start_date, @end_date);
        IF @is_avail = 0
        BEGIN
            RAISERROR('Samochód nie jest dostępny w wybranym terminie.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO RENTALS (CLIENT_ID, CAR_ID, START_DATE, END_DATE, ACTUAL_RETURN_DATE, STATUS, TOTAL_PRICE, START_MILEAGE, END_MILEAGE)
        SELECT CLIENT_ID, CAR_ID, START_DATE, END_DATE, ACTUAL_RETURN_DATE, STATUS, TOTAL_PRICE, START_MILEAGE, END_MILEAGE
        FROM inserted WHERE CLIENT_ID = @client_id AND CAR_ID = @car_id AND START_DATE = @start_date;

        FETCH NEXT FROM cur INTO @client_id, @car_id, @start_date, @end_date;
    END
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

CREATE OR ALTER TRIGGER trg_Update_Car_Status
ON RENTALS
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Aktualizuj status na RENTED jeśli wynajem zaczyna się dzisiaj i jest aktywny
    UPDATE CARS
    SET STATUS = 'RENTED'
    WHERE CAR_ID IN (
        SELECT CAR_ID FROM inserted WHERE STATUS = 'ACTIVE' AND CAST(START_DATE AS DATE) <= CAST(GETDATE() AS DATE)
    );

    -- Aktualizuj status na AVAILABLE jeśli wynajem został anulowany lub zakończony
    UPDATE CARS
    SET STATUS = 'AVAILABLE'
    WHERE CAR_ID IN (
        SELECT CAR_ID FROM inserted WHERE STATUS IN ('COMPLETED', 'CANCELLED')
    );
END;
GO

CREATE OR ALTER TRIGGER trg_Penalty_Check
ON PENALTIES
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @client_id INT;
    DECLARE @penalty_count INT;

    DECLARE cur CURSOR FOR SELECT DISTINCT CLIENT_ID FROM inserted;
    OPEN cur;
    FETCH NEXT FROM cur INTO @client_id;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @penalty_count = COUNT(*) FROM PENALTIES WHERE CLIENT_ID = @client_id;

        IF @penalty_count >= 5
        BEGIN
            UPDATE CLIENTS SET IS_BLOCKED = 1 WHERE CLIENT_ID = @client_id;
        END
        ELSE IF @penalty_count >= 3
        BEGIN
            UPDATE CLIENTS SET PENALTY_MULTIPLIER = 1.50 WHERE CLIENT_ID = @client_id;
        END

        FETCH NEXT FROM cur INTO @client_id;
    END
    CLOSE cur;
    DEALLOCATE cur;
END;
GO
