-- 06_Procedures.sql w SQL Server (Połączenie starych procedur i pakietów PL/SQL)

GO
-- Zamiast pakietu PKG_RENTAL
CREATE OR ALTER PROCEDURE sp_CreateRental (
    @p_client_id INT,
    @p_car_id INT,
    @p_start_date DATETIME,
    @p_end_date DATETIME,
    @p_rental_id INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @v_days INT;
    DECLARE @v_total_price DECIMAL(10,2);
    DECLARE @v_mileage INT;

    SET @v_days = DATEDIFF(day, @p_start_date, @p_end_date);
    IF @v_days <= 0 SET @v_days = 1;

    SET @v_total_price = dbo.fn_CalculateRentalPrice(@p_car_id, @v_days, @p_client_id);
    
    SELECT @v_mileage = MILEAGE FROM CARS WHERE CAR_ID = @p_car_id;

    -- Wstawianie (Trigger INSTEAD OF INSERT obsłuży walidację)
    INSERT INTO RENTALS (CLIENT_ID, CAR_ID, START_DATE, END_DATE, TOTAL_PRICE, START_MILEAGE, STATUS)
    VALUES (@p_client_id, @p_car_id, @p_start_date, @p_end_date, @v_total_price, @v_mileage, 'ACTIVE');

    SET @p_rental_id = @@IDENTITY;
END;
GO

CREATE OR ALTER PROCEDURE sp_CompleteRental (
    @p_rental_id INT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RENTALS
    SET STATUS = 'COMPLETED', ACTUAL_RETURN_DATE = GETDATE()
    WHERE RENTAL_ID = @p_rental_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_CancelRental (
    @p_rental_id INT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RENTALS
    SET STATUS = 'CANCELLED'
    WHERE RENTAL_ID = @p_rental_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_ExtendRental (
    @p_rental_id INT,
    @p_new_end_date DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RENTALS
    SET END_DATE = @p_new_end_date, STATUS = 'EXTENDED'
    WHERE RENTAL_ID = @p_rental_id;
END;
GO

-- Zamiast pakietu PKG_PAYMENT
CREATE OR ALTER PROCEDURE sp_ProcessPayment (
    @p_rental_id INT,
    @p_amount DECIMAL(10,2),
    @p_payment_method NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @v_status_id INT;
    SELECT @v_status_id = STATUS_ID FROM PAYMENT_STATUSES WHERE NAME = 'PAID';

    INSERT INTO PAYMENTS (RENTAL_ID, AMOUNT, PAYMENT_DATE, STATUS_ID, PAYMENT_METHOD)
    VALUES (@p_rental_id, @p_amount, GETDATE(), @v_status_id, @p_payment_method);
END;
GO

CREATE OR ALTER PROCEDURE sp_CalculateInstallments (
    @p_rental_id INT,
    @p_num_installments INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @v_total DECIMAL(10,2);
    DECLARE @v_installment_amount DECIMAL(10,2);
    DECLARE @v_status_id INT;
    DECLARE @i INT = 1;

    SELECT @v_total = TOTAL_PRICE FROM RENTALS WHERE RENTAL_ID = @p_rental_id;
    IF @v_total IS NULL RETURN;

    SET @v_installment_amount = ROUND(@v_total / @p_num_installments, 2);
    SELECT @v_status_id = STATUS_ID FROM PAYMENT_STATUSES WHERE NAME = 'PENDING';

    WHILE @i <= @p_num_installments
    BEGIN
        INSERT INTO PAYMENTS (RENTAL_ID, AMOUNT, STATUS_ID, INSTALLMENT_NO, TOTAL_INSTALLMENTS)
        VALUES (@p_rental_id, @v_installment_amount, @v_status_id, @i, @p_num_installments);
        
        SET @i = @i + 1;
    END
END;
GO

-- Stare procedury
CREATE OR ALTER PROCEDURE sp_BlockClient (
    @p_client_id INT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CLIENTS SET IS_BLOCKED = 1 WHERE CLIENT_ID = @p_client_id;
END;
GO
