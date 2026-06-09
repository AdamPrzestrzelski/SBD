-- ============================================
-- CarRent DB - Funkcje
-- ============================================

-- calculate_rental_price - oblicza cenę wypożyczenia z uwzględnieniem kar klienta
CREATE OR REPLACE FUNCTION calculate_rental_price(
    p_car_id    IN NUMBER,
    p_days      IN NUMBER,
    p_client_id IN NUMBER
) RETURN NUMBER
IS
    v_daily_rate          NUMBER(10,2);
    v_penalty_multiplier  NUMBER(3,2);
    v_total_price         NUMBER(10,2);
BEGIN
    -- Pobierz stawkę dzienną
    SELECT DAILY_RATE INTO v_daily_rate
    FROM CARS WHERE CAR_ID = p_car_id;

    -- Pobierz mnożnik kar klienta
    SELECT NVL(PENALTY_MULTIPLIER, 1.00) INTO v_penalty_multiplier
    FROM CLIENTS WHERE CLIENT_ID = p_client_id;

    -- Oblicz cenę: stawka * dni * mnożnik kar
    v_total_price := v_daily_rate * p_days * v_penalty_multiplier;

    RETURN ROUND(v_total_price, 2);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN -1;
END;
/

-- check_car_availability - sprawdza dostępność samochodu w podanym terminie
CREATE OR REPLACE FUNCTION check_car_availability(
    p_car_id     IN NUMBER,
    p_start_date IN DATE,
    p_end_date   IN DATE
) RETURN NUMBER -- 1 = dostępny, 0 = niedostępny
IS
    v_car_status    VARCHAR2(20);
    v_overlap_count NUMBER;
BEGIN
    -- Sprawdź status samochodu
    SELECT STATUS INTO v_car_status
    FROM CARS WHERE CAR_ID = p_car_id;

    IF v_car_status != 'AVAILABLE' THEN
        RETURN 0;
    END IF;

    -- Sprawdź nakładające się wypożyczenia
    SELECT COUNT(*) INTO v_overlap_count
    FROM RENTALS
    WHERE CAR_ID = p_car_id
      AND STATUS = 'ACTIVE'
      AND START_DATE < p_end_date
      AND END_DATE > p_start_date;

    IF v_overlap_count > 0 THEN
        RETURN 0;
    END IF;

    -- Sprawdź nakładające się rezerwacje (aktywne)
    SELECT COUNT(*) INTO v_overlap_count
    FROM RESERVATIONS r
    JOIN RESERVATION_STATUSES rs ON r.STATUS_ID = rs.STATUS_ID
    WHERE r.CAR_ID = p_car_id
      AND rs.NAME = 'ACTIVE'
      AND r.START_DATE < p_end_date
      AND r.END_DATE > p_start_date;

    IF v_overlap_count > 0 THEN
        RETURN 0;
    END IF;

    RETURN 1;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
END;
/

-- calculate_penalty_multiplier - oblicza mnożnik ceny na podstawie kar klienta
CREATE OR REPLACE FUNCTION calculate_penalty_multiplier(
    p_client_id IN NUMBER
) RETURN NUMBER
IS
    v_penalty_count NUMBER;
    v_multiplier    NUMBER(3,2);
BEGIN
    SELECT COUNT(*) INTO v_penalty_count
    FROM PENALTIES
    WHERE CLIENT_ID = p_client_id;

    IF v_penalty_count >= 5 THEN
        v_multiplier := 1.50;
    ELSIF v_penalty_count >= 3 THEN
        v_multiplier := 1.00 + (v_penalty_count * 0.10);
    ELSE
        v_multiplier := 1.00;
    END IF;

    RETURN v_multiplier;
END;
/

-- get_client_penalty_count - zwraca liczbę kar klienta
CREATE OR REPLACE FUNCTION get_client_penalty_count(
    p_client_id IN NUMBER
) RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM PENALTIES WHERE CLIENT_ID = p_client_id;
    RETURN v_count;
END;
/
