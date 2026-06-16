-- 08_Roles.sql w SQL Server

-- Tworzenie ról (tylko na poziomie bazy danych)
-- Zauważ, że żeby dodać użytkownika do roli, należy najpierw go stworzyć z poziomu serwera (LOGIN),
-- a potem z poziomu bazy danych (USER). Ten skrypt tworzy tylko ramy uprawnień (role).

CREATE ROLE Admin;
CREATE ROLE Employee;
CREATE ROLE Client;
CREATE ROLE Analyst;

-- ===================================================================
-- UPRAWNIENIA ROLI Admin
-- ===================================================================
-- Ma pełny dostęp do wszystkich tabel i widoków w schemacie dbo
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO Admin;
GRANT EXECUTE ON SCHEMA::dbo TO Admin;

-- ===================================================================
-- UPRAWNIENIA ROLI Employee (Pracownik wypożyczalni)
-- ===================================================================
GRANT SELECT, INSERT, UPDATE, DELETE ON CARS TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON CLIENTS TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON RENTALS TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON RESERVATIONS TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON PAYMENTS TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON PENALTIES TO Employee;
GRANT SELECT, INSERT, UPDATE, DELETE ON SERVICES TO Employee;

-- Pracownik ma dostęp do wszystkich widoków (może przeglądać statystyki itd.)
GRANT SELECT ON V_AVAILABLE_CARS TO Employee;
GRANT SELECT ON V_CLIENT_RENTALS TO Employee;
GRANT SELECT ON V_CAR_UTILIZATION TO Employee;
GRANT SELECT ON V_MONTHLY_REVENUE TO Employee;
GRANT SELECT ON V_HIGH_RISK_CLIENTS TO Employee;

-- ===================================================================
-- UPRAWNIENIA ROLI Client (Klient logujący się z poziomu aplikacji klienckiej)
-- ===================================================================
-- Klient może tylko przeglądać określone informacje, np. dostępne auta i swoje wypożyczenia
-- Poniższe ograniczenie uprawnień zakłada kontrolę dostępu do konkretnych rekordów
-- po stronie aplikacji C#, dlatego nadajemy uprawnienia SELECT do widoków.
GRANT SELECT ON V_AVAILABLE_CARS TO Client;
GRANT SELECT ON V_CLIENT_RENTALS TO Client;
GRANT INSERT ON RESERVATIONS TO Client;
GRANT INSERT ON PAYMENTS TO Client;

-- ===================================================================
-- UPRAWNIENIA ROLI Analyst (Analityk biznesowy)
-- ===================================================================
-- Analityk widzi wszystkie dane, ale nie może ich edytować
GRANT SELECT ON SCHEMA::dbo TO Analyst;
