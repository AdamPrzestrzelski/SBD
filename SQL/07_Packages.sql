-- ============================================
-- CarRent DB - Pakiety PL/SQL
-- ============================================

-- ==========================================
-- PKG_RENTAL - Zarządzanie wypożyczeniami
-- ==========================================

CREATE OR REPLACE PACKAGE PKG_RENTAL AS
    PROCEDURE create_rental(
        p_client_id    IN NUMBER,
        p_car_id       IN NUMBER,
        p_start_date   IN DATE,
        p_end_date     IN DATE,
        p_rental_id    OUT NUMBER
    );

    PROCEDURE cancel_rental(
        p_rental_id IN NUMBER
    );

    PROCEDURE extend_rental(
        p_rental_id    IN NUMBER,
        p_new_end_date IN DATE
    );

    PROCEDURE complete_rental(
        p_rental_id IN NUMBER
    );
END PKG_RENTAL;
/

CREATE OR REPLACE PACKAGE BODY PKG_RENTAL AS

    PROCEDURE create_rental(
        p_client_id    IN NUMBER,
        p_car_id       IN NUMBER,
        p_start_date   IN DATE,
        p_end_date     IN DATE,
        p_rental_id    OUT NUMBER
    )
    IS
        v_days        NUMBER;
        v_total_price NUMBER(10,2);
        v_available   NUMBER;
    BEGIN
        -- Sprawdź dostępność
        v_available := check_car_availability(p_car_id, p_start_date, p_end_date);
        IF v_available = 0 THEN
            RAISE_APPLICATION_ERROR(-20010, 'Samochód nie jest dostępny w podanym terminie.');
        END IF;

        -- Oblicz cenę
        v_days := p_end_date - p_start_date;
        v_total_price := calculate_rental_price(p_car_id, v_days, p_client_id);

        -- Utwórz wypożyczenie
        INSERT INTO RENTALS (CLIENT_ID, CAR_ID, START_DATE, END_DATE, TOTAL_PRICE, STATUS)
        VALUES (p_client_id, p_car_id, p_start_date, p_end_date, v_total_price, 'ACTIVE')
        RETURNING RENTAL_ID INTO p_rental_id;

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END create_rental;

    PROCEDURE cancel_rental(
        p_rental_id IN NUMBER
    )
    IS
    BEGIN
        UPDATE RENTALS
        SET STATUS = 'CANCELLED',
            ACTUAL_END_DATE = SYSDATE
        WHERE RENTAL_ID = p_rental_id
          AND STATUS = 'ACTIVE';

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20011, 'Nie znaleziono aktywnego wypożyczenia o ID: ' || p_rental_id);
        END IF;

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END cancel_rental;

    PROCEDURE extend_rental(
        p_rental_id    IN NUMBER,
        p_new_end_date IN DATE
    )
    IS
        v_car_id      NUMBER;
        v_old_end     DATE;
        v_client_id   NUMBER;
        v_extra_days  NUMBER;
        v_extra_price NUMBER(10,2);
    BEGIN
        -- Pobierz dane wypożyczenia
        SELECT CAR_ID, END_DATE, CLIENT_ID
        INTO v_car_id, v_old_end, v_client_id
        FROM RENTALS
        WHERE RENTAL_ID = p_rental_id AND STATUS = 'ACTIVE';

        IF p_new_end_date <= v_old_end THEN
            RAISE_APPLICATION_ERROR(-20012, 'Nowa data musi być późniejsza niż obecna data końca.');
        END IF;

        -- Oblicz dodatkowy koszt
        v_extra_days := p_new_end_date - v_old_end;
        v_extra_price := calculate_rental_price(v_car_id, v_extra_days, v_client_id);

        UPDATE RENTALS
        SET END_DATE = p_new_end_date,
            TOTAL_PRICE = TOTAL_PRICE + v_extra_price
        WHERE RENTAL_ID = p_rental_id;

        COMMIT;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20011, 'Nie znaleziono aktywnego wypożyczenia o ID: ' || p_rental_id);
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END extend_rental;

    PROCEDURE complete_rental(
        p_rental_id IN NUMBER
    )
    IS
    BEGIN
        UPDATE RENTALS
        SET STATUS = 'COMPLETED',
            ACTUAL_END_DATE = SYSDATE
        WHERE RENTAL_ID = p_rental_id
          AND STATUS = 'ACTIVE';

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20011, 'Nie znaleziono aktywnego wypożyczenia o ID: ' || p_rental_id);
        END IF;

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END complete_rental;

END PKG_RENTAL;
/

-- ==========================================
-- PKG_PAYMENT - Zarządzanie płatnościami
-- ==========================================

CREATE OR REPLACE PACKAGE PKG_PAYMENT AS
    PROCEDURE process_payment(
        p_rental_id      IN NUMBER,
        p_amount         IN NUMBER,
        p_payment_method IN VARCHAR2 DEFAULT 'CARD'
    );

    PROCEDURE calculate_installments(
        p_rental_id        IN NUMBER,
        p_num_installments IN NUMBER
    );
END PKG_PAYMENT;
/

CREATE OR REPLACE PACKAGE BODY PKG_PAYMENT AS

    PROCEDURE process_payment(
        p_rental_id      IN NUMBER,
        p_amount         IN NUMBER,
        p_payment_method IN VARCHAR2 DEFAULT 'CARD'
    )
    IS
        v_paid_status_id NUMBER;
    BEGIN
        SELECT STATUS_ID INTO v_paid_status_id
        FROM PAYMENT_STATUSES WHERE NAME = 'PAID';

        INSERT INTO PAYMENTS (RENTAL_ID, AMOUNT, PAYMENT_DATE, STATUS_ID, PAYMENT_METHOD)
        VALUES (p_rental_id, p_amount, SYSDATE, v_paid_status_id, p_payment_method);

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END process_payment;

    PROCEDURE calculate_installments(
        p_rental_id        IN NUMBER,
        p_num_installments IN NUMBER
    )
    IS
        v_total_price    NUMBER(10,2);
        v_installment    NUMBER(10,2);
        v_pending_status NUMBER;
    BEGIN
        SELECT TOTAL_PRICE INTO v_total_price
        FROM RENTALS WHERE RENTAL_ID = p_rental_id;

        SELECT STATUS_ID INTO v_pending_status
        FROM PAYMENT_STATUSES WHERE NAME = 'PENDING';

        v_installment := ROUND(v_total_price / p_num_installments, 2);

        FOR i IN 1..p_num_installments LOOP
            INSERT INTO PAYMENTS (RENTAL_ID, AMOUNT, STATUS_ID, INSTALLMENT_NO, TOTAL_INSTALLMENTS)
            VALUES (p_rental_id, v_installment, v_pending_status, i, p_num_installments);
        END LOOP;

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END calculate_installments;

END PKG_PAYMENT;
/

-- ==========================================
-- PKG_REPORTS - Raporty
-- ==========================================

CREATE OR REPLACE PACKAGE PKG_REPORTS AS
    PROCEDURE monthly_revenue(
        p_month IN NUMBER,
        p_year  IN NUMBER
    );

    PROCEDURE fleet_usage;

    PROCEDURE penalty_statistics;
END PKG_REPORTS;
/

CREATE OR REPLACE PACKAGE BODY PKG_REPORTS AS

    PROCEDURE monthly_revenue(
        p_month IN NUMBER,
        p_year  IN NUMBER
    )
    IS
    BEGIN
        generate_monthly_report(p_month, p_year);
    END monthly_revenue;

    PROCEDURE fleet_usage
    IS
        CURSOR c_fleet IS
            SELECT CAR_ID, CAR_NAME, PLATE_NUMBER, CATEGORY_NAME,
                   TOTAL_RENTALS, TOTAL_RENTAL_DAYS, TOTAL_REVENUE
            FROM V_CAR_UTILIZATION
            ORDER BY TOTAL_RENTAL_DAYS DESC;
    BEGIN
        DBMS_OUTPUT.PUT_LINE('========================================');
        DBMS_OUTPUT.PUT_LINE('  WYKORZYSTANIE FLOTY');
        DBMS_OUTPUT.PUT_LINE('========================================');

        FOR rec IN c_fleet LOOP
            DBMS_OUTPUT.PUT_LINE(
                rec.CAR_NAME || ' (' || rec.PLATE_NUMBER || ') - ' ||
                rec.CATEGORY_NAME || ' | Wypożyczeń: ' || rec.TOTAL_RENTALS ||
                ' | Dni: ' || rec.TOTAL_RENTAL_DAYS ||
                ' | Przychód: ' || TO_CHAR(rec.TOTAL_REVENUE, '999,999.99') || ' PLN'
            );
        END LOOP;
    END fleet_usage;

    PROCEDURE penalty_statistics
    IS
        CURSOR c_penalties IS
            SELECT CLIENT_ID, CLIENT_NAME, EMAIL, PENALTY_COUNT, TOTAL_PENALTY_AMOUNT,
                   IS_BLOCKED, PENALTY_MULTIPLIER
            FROM V_HIGH_RISK_CLIENTS;
        v_total_penalties NUMBER;
        v_total_amount    NUMBER(12,2);
    BEGIN
        SELECT COUNT(*), NVL(SUM(AMOUNT), 0)
        INTO v_total_penalties, v_total_amount
        FROM PENALTIES;

        DBMS_OUTPUT.PUT_LINE('========================================');
        DBMS_OUTPUT.PUT_LINE('  STATYSTYKI KAR');
        DBMS_OUTPUT.PUT_LINE('========================================');
        DBMS_OUTPUT.PUT_LINE('Łączna liczba kar: ' || v_total_penalties);
        DBMS_OUTPUT.PUT_LINE('Łączna kwota kar:  ' || TO_CHAR(v_total_amount, '999,999.99') || ' PLN');
        DBMS_OUTPUT.PUT_LINE('');
        DBMS_OUTPUT.PUT_LINE('--- Klienci wysokiego ryzyka ---');

        FOR rec IN c_penalties LOOP
            DBMS_OUTPUT.PUT_LINE(
                rec.CLIENT_NAME || ' | Kar: ' || rec.PENALTY_COUNT ||
                ' | Kwota: ' || TO_CHAR(rec.TOTAL_PENALTY_AMOUNT, '999,999.99') || ' PLN' ||
                ' | Zablokowany: ' || CASE rec.IS_BLOCKED WHEN 1 THEN 'TAK' ELSE 'NIE' END ||
                ' | Mnożnik: ' || rec.PENALTY_MULTIPLIER
            );
        END LOOP;
    END penalty_statistics;

END PKG_REPORTS;
/

-- ==========================================
-- PKG_SECURITY - Bezpieczeństwo
-- ==========================================

CREATE OR REPLACE PACKAGE PKG_SECURITY AS
    PROCEDURE log_user_activity(
        p_user_name  IN VARCHAR2,
        p_action     IN VARCHAR2,
        p_table_name IN VARCHAR2,
        p_record_id  IN NUMBER
    );

    FUNCTION validate_user(
        p_email    IN VARCHAR2,
        p_password IN VARCHAR2
    ) RETURN NUMBER; -- zwraca CLIENT_ID lub -1
END PKG_SECURITY;
/

CREATE OR REPLACE PACKAGE BODY PKG_SECURITY AS

    PROCEDURE log_user_activity(
        p_user_name  IN VARCHAR2,
        p_action     IN VARCHAR2,
        p_table_name IN VARCHAR2,
        p_record_id  IN NUMBER
    )
    IS
    BEGIN
        INSERT INTO CHANGE_HISTORY (TABLE_NAME, RECORD_ID, OLD_VALUES, CHANGED_BY, OPERATION_TYPE)
        VALUES (p_table_name, p_record_id, p_action, p_user_name, 'UPDATE');

        COMMIT;
    END log_user_activity;

    FUNCTION validate_user(
        p_email    IN VARCHAR2,
        p_password IN VARCHAR2
    ) RETURN NUMBER
    IS
        v_client_id    NUMBER;
        v_password_hash VARCHAR2(256);
        v_is_blocked   NUMBER(1);
    BEGIN
        SELECT CLIENT_ID, PASSWORD_HASH, IS_BLOCKED
        INTO v_client_id, v_password_hash, v_is_blocked
        FROM CLIENTS
        WHERE EMAIL = p_email;

        -- Proste porównanie (w produkcji: bcrypt)
        IF v_password_hash != p_password THEN
            RETURN -1;
        END IF;

        IF v_is_blocked = 1 THEN
            RETURN -2; -- zablokowany
        END IF;

        RETURN v_client_id;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RETURN -1;
    END validate_user;

END PKG_SECURITY;
/
