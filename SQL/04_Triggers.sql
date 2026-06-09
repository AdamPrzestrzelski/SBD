-- ============================================
-- CarRent DB - Triggery
-- ============================================

-- ==========================================
-- TRIGGERY HISTORII ZMIAN
-- ==========================================

-- TRG_RENTAL_HISTORY - zapis historii zmian wypożyczeń
CREATE OR REPLACE TRIGGER TRG_RENTAL_HISTORY
AFTER INSERT OR UPDATE OR DELETE ON RENTALS
FOR EACH ROW
DECLARE
    v_operation VARCHAR2(10);
    v_old_vals  CLOB;
    v_new_vals  CLOB;
    v_record_id NUMBER;
BEGIN
    IF INSERTING THEN
        v_operation := 'INSERT';
        v_record_id := :NEW.RENTAL_ID;
        v_new_vals := 'CLIENT_ID=' || :NEW.CLIENT_ID ||
                      ', CAR_ID=' || :NEW.CAR_ID ||
                      ', START_DATE=' || TO_CHAR(:NEW.START_DATE, 'YYYY-MM-DD') ||
                      ', END_DATE=' || TO_CHAR(:NEW.END_DATE, 'YYYY-MM-DD') ||
                      ', TOTAL_PRICE=' || :NEW.TOTAL_PRICE ||
                      ', STATUS=' || :NEW.STATUS;
    ELSIF UPDATING THEN
        v_operation := 'UPDATE';
        v_record_id := :OLD.RENTAL_ID;
        v_old_vals := 'CLIENT_ID=' || :OLD.CLIENT_ID ||
                      ', CAR_ID=' || :OLD.CAR_ID ||
                      ', START_DATE=' || TO_CHAR(:OLD.START_DATE, 'YYYY-MM-DD') ||
                      ', END_DATE=' || TO_CHAR(:OLD.END_DATE, 'YYYY-MM-DD') ||
                      ', TOTAL_PRICE=' || :OLD.TOTAL_PRICE ||
                      ', STATUS=' || :OLD.STATUS;
        v_new_vals := 'CLIENT_ID=' || :NEW.CLIENT_ID ||
                      ', CAR_ID=' || :NEW.CAR_ID ||
                      ', START_DATE=' || TO_CHAR(:NEW.START_DATE, 'YYYY-MM-DD') ||
                      ', END_DATE=' || TO_CHAR(:NEW.END_DATE, 'YYYY-MM-DD') ||
                      ', TOTAL_PRICE=' || :NEW.TOTAL_PRICE ||
                      ', STATUS=' || :NEW.STATUS;
    ELSIF DELETING THEN
        v_operation := 'DELETE';
        v_record_id := :OLD.RENTAL_ID;
        v_old_vals := 'CLIENT_ID=' || :OLD.CLIENT_ID ||
                      ', CAR_ID=' || :OLD.CAR_ID ||
                      ', START_DATE=' || TO_CHAR(:OLD.START_DATE, 'YYYY-MM-DD') ||
                      ', END_DATE=' || TO_CHAR(:OLD.END_DATE, 'YYYY-MM-DD') ||
                      ', TOTAL_PRICE=' || :OLD.TOTAL_PRICE ||
                      ', STATUS=' || :OLD.STATUS;
    END IF;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, OPERATION_TYPE)
    VALUES ('RENTALS', v_record_id, v_old_vals, v_new_vals, v_operation);
END;
/

-- TRG_PAYMENT_HISTORY - zapis historii zmian płatności
CREATE OR REPLACE TRIGGER TRG_PAYMENT_HISTORY
AFTER INSERT OR UPDATE OR DELETE ON PAYMENTS
FOR EACH ROW
DECLARE
    v_operation VARCHAR2(10);
    v_old_vals  CLOB;
    v_new_vals  CLOB;
    v_record_id NUMBER;
BEGIN
    IF INSERTING THEN
        v_operation := 'INSERT';
        v_record_id := :NEW.PAYMENT_ID;
        v_new_vals := 'RENTAL_ID=' || :NEW.RENTAL_ID ||
                      ', AMOUNT=' || :NEW.AMOUNT ||
                      ', STATUS_ID=' || :NEW.STATUS_ID;
    ELSIF UPDATING THEN
        v_operation := 'UPDATE';
        v_record_id := :OLD.PAYMENT_ID;
        v_old_vals := 'RENTAL_ID=' || :OLD.RENTAL_ID ||
                      ', AMOUNT=' || :OLD.AMOUNT ||
                      ', STATUS_ID=' || :OLD.STATUS_ID;
        v_new_vals := 'RENTAL_ID=' || :NEW.RENTAL_ID ||
                      ', AMOUNT=' || :NEW.AMOUNT ||
                      ', STATUS_ID=' || :NEW.STATUS_ID;
    ELSIF DELETING THEN
        v_operation := 'DELETE';
        v_record_id := :OLD.PAYMENT_ID;
        v_old_vals := 'RENTAL_ID=' || :OLD.RENTAL_ID ||
                      ', AMOUNT=' || :OLD.AMOUNT ||
                      ', STATUS_ID=' || :OLD.STATUS_ID;
    END IF;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, OPERATION_TYPE)
    VALUES ('PAYMENTS', v_record_id, v_old_vals, v_new_vals, v_operation);
END;
/

-- TRG_CLIENT_HISTORY - zapis historii zmian klientów
CREATE OR REPLACE TRIGGER TRG_CLIENT_HISTORY
AFTER UPDATE ON CLIENTS
FOR EACH ROW
DECLARE
    v_old_vals CLOB;
    v_new_vals CLOB;
BEGIN
    v_old_vals := 'FIRST_NAME=' || :OLD.FIRST_NAME ||
                  ', LAST_NAME=' || :OLD.LAST_NAME ||
                  ', EMAIL=' || :OLD.EMAIL ||
                  ', IS_BLOCKED=' || :OLD.IS_BLOCKED ||
                  ', PENALTY_MULTIPLIER=' || :OLD.PENALTY_MULTIPLIER;
    v_new_vals := 'FIRST_NAME=' || :NEW.FIRST_NAME ||
                  ', LAST_NAME=' || :NEW.LAST_NAME ||
                  ', EMAIL=' || :NEW.EMAIL ||
                  ', IS_BLOCKED=' || :NEW.IS_BLOCKED ||
                  ', PENALTY_MULTIPLIER=' || :NEW.PENALTY_MULTIPLIER;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, OPERATION_TYPE)
    VALUES ('CLIENTS', :OLD.CLIENT_ID, v_old_vals, v_new_vals, 'UPDATE');
END;
/

-- TRG_CAR_HISTORY - zapis historii zmian samochodów
CREATE OR REPLACE TRIGGER TRG_CAR_HISTORY
AFTER UPDATE ON CARS
FOR EACH ROW
DECLARE
    v_old_vals CLOB;
    v_new_vals CLOB;
BEGIN
    v_old_vals := 'BRAND=' || :OLD.BRAND ||
                  ', MODEL=' || :OLD.MODEL ||
                  ', STATUS=' || :OLD.STATUS ||
                  ', DAILY_RATE=' || :OLD.DAILY_RATE ||
                  ', MILEAGE=' || :OLD.MILEAGE;
    v_new_vals := 'BRAND=' || :NEW.BRAND ||
                  ', MODEL=' || :NEW.MODEL ||
                  ', STATUS=' || :NEW.STATUS ||
                  ', DAILY_RATE=' || :NEW.DAILY_RATE ||
                  ', MILEAGE=' || :NEW.MILEAGE;

    INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, NEW_VALUES, OPERATION_TYPE)
    VALUES ('CARS', :OLD.CAR_ID, v_old_vals, v_new_vals, 'UPDATE');
END;
/

-- ==========================================
-- TRIGGERY WALIDACYJNE I STATUSOWE
-- ==========================================

-- TRG_VALIDATE_RENTAL - walidacja dostępności auta przed wypożyczeniem
CREATE OR REPLACE TRIGGER TRG_VALIDATE_RENTAL
BEFORE INSERT ON RENTALS
FOR EACH ROW
DECLARE
    v_car_status    VARCHAR2(20);
    v_is_blocked    NUMBER(1);
    v_overlap_count NUMBER;
BEGIN
    -- Sprawdź status samochodu
    SELECT STATUS INTO v_car_status
    FROM CARS WHERE CAR_ID = :NEW.CAR_ID;

    IF v_car_status != 'AVAILABLE' THEN
        RAISE_APPLICATION_ERROR(-20001,
            'Samochód nie jest dostępny. Aktualny status: ' || v_car_status);
    END IF;

    -- Sprawdź czy klient nie jest zablokowany
    SELECT IS_BLOCKED INTO v_is_blocked
    FROM CLIENTS WHERE CLIENT_ID = :NEW.CLIENT_ID;

    IF v_is_blocked = 1 THEN
        RAISE_APPLICATION_ERROR(-20002,
            'Klient jest zablokowany i nie może wypożyczyć samochodu.');
    END IF;

    -- Sprawdź nakładające się wypożyczenia
    SELECT COUNT(*) INTO v_overlap_count
    FROM RENTALS
    WHERE CAR_ID = :NEW.CAR_ID
      AND STATUS = 'ACTIVE'
      AND START_DATE < :NEW.END_DATE
      AND END_DATE > :NEW.START_DATE;

    IF v_overlap_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20003,
            'Samochód jest już wypożyczony w podanym terminie.');
    END IF;
END;
/

-- TRG_SET_STATUS - zmiana statusu auta po wypożyczeniu
CREATE OR REPLACE TRIGGER TRG_SET_STATUS
AFTER INSERT ON RENTALS
FOR EACH ROW
BEGIN
    IF :NEW.STATUS = 'ACTIVE' THEN
        UPDATE CARS SET STATUS = 'RENTED' WHERE CAR_ID = :NEW.CAR_ID;
    END IF;
END;
/

-- TRG_RETURN_CAR - przywrócenie statusu auta po zakończeniu wypożyczenia
CREATE OR REPLACE TRIGGER TRG_RETURN_CAR
AFTER UPDATE OF STATUS ON RENTALS
FOR EACH ROW
BEGIN
    IF :NEW.STATUS = 'COMPLETED' AND :OLD.STATUS = 'ACTIVE' THEN
        UPDATE CARS SET STATUS = 'AVAILABLE' WHERE CAR_ID = :NEW.CAR_ID;
    ELSIF :NEW.STATUS = 'CANCELLED' AND :OLD.STATUS = 'ACTIVE' THEN
        UPDATE CARS SET STATUS = 'AVAILABLE' WHERE CAR_ID = :NEW.CAR_ID;
    END IF;
END;
/

-- TRG_PENALTY_CHECK - sprawdzenie progu kar klienta
CREATE OR REPLACE TRIGGER TRG_PENALTY_CHECK
AFTER INSERT ON PENALTIES
FOR EACH ROW
DECLARE
    v_penalty_count NUMBER;
BEGIN
    -- Policz kary klienta
    SELECT COUNT(*) INTO v_penalty_count
    FROM PENALTIES
    WHERE CLIENT_ID = :NEW.CLIENT_ID;

    -- Jeśli >= 5 kar → zablokuj klienta
    IF v_penalty_count >= 5 THEN
        UPDATE CLIENTS
        SET IS_BLOCKED = 1,
            PENALTY_MULTIPLIER = 1.50,
            UPDATED_AT = SYSTIMESTAMP
        WHERE CLIENT_ID = :NEW.CLIENT_ID;
    -- Jeśli >= 3 → zwiększ mnożnik ceny
    ELSIF v_penalty_count >= 3 THEN
        UPDATE CLIENTS
        SET PENALTY_MULTIPLIER = 1.00 + (v_penalty_count * 0.10),
            UPDATED_AT = SYSTIMESTAMP
        WHERE CLIENT_ID = :NEW.CLIENT_ID;
    END IF;
END;
/

-- TRG_UPDATE_CLIENT_TIMESTAMP - aktualizacja daty modyfikacji klienta
CREATE OR REPLACE TRIGGER TRG_UPDATE_CLIENT_TIMESTAMP
BEFORE UPDATE ON CLIENTS
FOR EACH ROW
BEGIN
    :NEW.UPDATED_AT := SYSTIMESTAMP;
END;
/
