-- 05_Functions.sql w SQL Server

GO
-- Funkcja obliczająca cenę wypożyczenia
CREATE OR ALTER FUNCTION fn_CalculateRentalPrice (
    @p_car_id INT,
    @p_days INT,
    @p_client_id INT
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @v_daily_rate DECIMAL(10,2);
    DECLARE @v_multiplier DECIMAL(3,2);
    DECLARE @v_total DECIMAL(10,2);

    -- Pobranie stawki dziennej samochodu
    SELECT @v_daily_rate = DAILY_RATE FROM CARS WHERE CAR_ID = @p_car_id;
    IF @v_daily_rate IS NULL RETURN 0;

    -- Pobranie mnożnika klienta
    SELECT @v_multiplier = PENALTY_MULTIPLIER FROM CLIENTS WHERE CLIENT_ID = @p_client_id;
    IF @v_multiplier IS NULL SET @v_multiplier = 1.0;

    -- Obliczenie
    SET @v_total = @v_daily_rate * @p_days * @v_multiplier;
    RETURN @v_total;
END;
GO

-- Funkcja sprawdzająca dostępność auta
CREATE OR ALTER FUNCTION fn_CheckCarAvailability (
    @p_car_id INT,
    @p_start_date DATETIME,
    @p_end_date DATETIME
)
RETURNS INT
AS
BEGIN
    DECLARE @v_count INT;

    -- Szukamy kolizji
    SELECT @v_count = COUNT(*)
    FROM RENTALS
    WHERE CAR_ID = @p_car_id
      AND STATUS IN ('ACTIVE', 'EXTENDED')
      AND (
          (@p_start_date BETWEEN START_DATE AND END_DATE) OR
          (@p_end_date BETWEEN START_DATE AND END_DATE) OR
          (START_DATE BETWEEN @p_start_date AND @p_end_date)
      );

    IF @v_count > 0
        RETURN 0; -- Niedostępny
    
    RETURN 1; -- Dostępny
END;
GO

-- Funkcja pobierająca mnożnik
CREATE OR ALTER FUNCTION fn_GetClientMultiplier (
    @p_client_id INT
)
RETURNS DECIMAL(3,2)
AS
BEGIN
    DECLARE @v_multiplier DECIMAL(3,2);
    SELECT @v_multiplier = PENALTY_MULTIPLIER FROM CLIENTS WHERE CLIENT_ID = @p_client_id;
    RETURN ISNULL(@v_multiplier, 1.0);
END;
GO
