-- ============================================
-- CarRent DB - Procedury
-- ============================================

-- generate_monthly_report - generuje raport przychodów za dany miesiąc
CREATE OR REPLACE PROCEDURE generate_monthly_report(
    p_month IN NUMBER,
    p_year  IN NUMBER
)
IS
    v_total_rentals   NUMBER;
    v_total_revenue   NUMBER(12,2);
    v_avg_price       NUMBER(10,2);
    v_active_cars     NUMBER;
    v_new_clients     NUMBER;
    v_total_penalties  NUMBER;
BEGIN
    -- Liczba wypożyczeń
    SELECT COUNT(*) INTO v_total_rentals
    FROM RENTALS
    WHERE EXTRACT(MONTH FROM START_DATE) = p_month
      AND EXTRACT(YEAR FROM START_DATE) = p_year;

    -- Przychody
    SELECT NVL(SUM(p.AMOUNT), 0), NVL(AVG(p.AMOUNT), 0)
    INTO v_total_revenue, v_avg_price
    FROM PAYMENTS p
    JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID
    WHERE ps.NAME = 'PAID'
      AND EXTRACT(MONTH FROM p.PAYMENT_DATE) = p_month
      AND EXTRACT(YEAR FROM p.PAYMENT_DATE) = p_year;

    -- Aktywne samochody (z wypożyczeniem w danym miesiącu)
    SELECT COUNT(DISTINCT CAR_ID) INTO v_active_cars
    FROM RENTALS
    WHERE EXTRACT(MONTH FROM START_DATE) = p_month
      AND EXTRACT(YEAR FROM START_DATE) = p_year;

    -- Nowi klienci
    SELECT COUNT(*) INTO v_new_clients
    FROM CLIENTS
    WHERE EXTRACT(MONTH FROM CREATED_AT) = p_month
      AND EXTRACT(YEAR FROM CREATED_AT) = p_year;

    -- Kary
    SELECT COUNT(*) INTO v_total_penalties
    FROM PENALTIES
    WHERE EXTRACT(MONTH FROM CREATED_AT) = p_month
      AND EXTRACT(YEAR FROM CREATED_AT) = p_year;

    -- Wyświetlenie raportu
    DBMS_OUTPUT.PUT_LINE('========================================');
    DBMS_OUTPUT.PUT_LINE('  RAPORT MIESIĘCZNY: ' || p_month || '/' || p_year);
    DBMS_OUTPUT.PUT_LINE('========================================');
    DBMS_OUTPUT.PUT_LINE('Wypożyczenia:       ' || v_total_rentals);
    DBMS_OUTPUT.PUT_LINE('Przychody:          ' || TO_CHAR(v_total_revenue, '999,999.99') || ' PLN');
    DBMS_OUTPUT.PUT_LINE('Średnia płatność:   ' || TO_CHAR(v_avg_price, '999,999.99') || ' PLN');
    DBMS_OUTPUT.PUT_LINE('Aktywne samochody:  ' || v_active_cars);
    DBMS_OUTPUT.PUT_LINE('Nowi klienci:       ' || v_new_clients);
    DBMS_OUTPUT.PUT_LINE('Naliczone kary:     ' || v_total_penalties);
    DBMS_OUTPUT.PUT_LINE('========================================');
END;
/

-- archive_old_rentals - archiwizacja starych wypożyczeń
CREATE OR REPLACE PROCEDURE archive_old_rentals(
    p_cutoff_date IN DATE
)
IS
    v_archived_count NUMBER := 0;
BEGIN
    -- Oznacz stare zakończone wypożyczenia
    UPDATE RENTALS
    SET STATUS = 'ARCHIVED'
    WHERE STATUS = 'COMPLETED'
      AND ACTUAL_END_DATE < p_cutoff_date;

    v_archived_count := SQL%ROWCOUNT;

    DBMS_OUTPUT.PUT_LINE('Zarchiwizowano ' || v_archived_count || ' wypożyczeń starszych niż ' ||
                         TO_CHAR(p_cutoff_date, 'YYYY-MM-DD'));

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/

-- block_high_risk_clients - blokada klientów z dużą liczbą kar
CREATE OR REPLACE PROCEDURE block_high_risk_clients(
    p_penalty_threshold IN NUMBER DEFAULT 5
)
IS
    v_blocked_count NUMBER := 0;
    CURSOR c_risky_clients IS
        SELECT cl.CLIENT_ID, cl.FIRST_NAME || ' ' || cl.LAST_NAME AS CLIENT_NAME,
               COUNT(p.PENALTY_ID) AS PENALTY_COUNT
        FROM CLIENTS cl
        JOIN PENALTIES p ON cl.CLIENT_ID = p.CLIENT_ID
        WHERE cl.IS_BLOCKED = 0
        GROUP BY cl.CLIENT_ID, cl.FIRST_NAME, cl.LAST_NAME
        HAVING COUNT(p.PENALTY_ID) >= p_penalty_threshold;
BEGIN
    FOR rec IN c_risky_clients LOOP
        UPDATE CLIENTS
        SET IS_BLOCKED = 1,
            PENALTY_MULTIPLIER = 1.50
        WHERE CLIENT_ID = rec.CLIENT_ID;

        v_blocked_count := v_blocked_count + 1;

        DBMS_OUTPUT.PUT_LINE('Zablokowano: ' || rec.CLIENT_NAME ||
                             ' (kary: ' || rec.PENALTY_COUNT || ')');
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Łącznie zablokowano ' || v_blocked_count || ' klientów.');

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
