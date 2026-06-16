# Dokumentacja Projektowa — System Wypożyczalni Samochodów *CarRent*

> **Przedmiot:** Systemy Baz Danych  
> **Technologia DBMS:** Microsoft SQL Server (LocalDB `MSSQLLocalDB`)  
> **Baza danych:** `CarRentDB`  
> **Warstwa aplikacyjna:** ASP.NET Core MVC (.NET 6 / C#)  
> **Autorzy:** Przestrzelski Adam, Sławiński Paweł
---

## Spis treści

1. Opis systemu
2. Architektura projektu
3. Schemat bazy danych — tabele
4. Relacje i klucze obce
5. Diagram ERD (tekstowy)
6. Widoki (Views)
7. Funkcje skalaralne
8. Procedury składowane
9. Triggery
10. Role i uprawnienia
11. Warstwa aplikacyjna — C# ASP.NET Core MVC
12. Kolejność wdrożenia skryptów SQL
13. Dane testowe
14. Znane problemy i decyzje projektowe

---

## 1. Opis systemu

**CarRent** to system zarządzania wypożyczalnią samochodów. Umożliwia:

- Przeglądanie i zarządzanie flotą pojazdów pogrupowanych w kategorie
- Rejestrację i obsługę klientów (z mechanizmem blokowania)
- Tworzenie, przedłużanie, anulowanie i kończenie wypożyczeń
- Obsługę rezerwacji
- Rejestrację płatności (jednorazowych i ratalnych)
- Naliczanie kar (z automatycznym podwyżką mnożnika i blokadą klienta po przekroczeniu progów)
- Zarządzanie serwisem pojazdów
- Audyt zmian w danych klientów i samochodów (tabela historii)
- Raportowanie (widoki analityczne)

System działa w modelu wielooddziałowym — każdy samochód i pracownik są przypisani do konkretnego oddziału.

---

## 2. Architektura projektu

```
SBD/
├── SQL/                        ← Skrypty bazy danych
│   ├── 01_DDL_Tables.sql       ← Definicja tabel
│   ├── 02_DDL_Indexes.sql      ← Indeksy
│   ├── 03_Views.sql            ← Widoki
│   ├── 04_Triggers.sql         ← Triggery
│   ├── 05_Functions.sql        ← Funkcje skalarne
│   ├── 06_Procedures.sql       ← Procedury składowane
│   ├── 08_Roles.sql            ← Role i uprawnienia
│   ├── 09_SeedData.sql         ← Dane testowe
│   └── 10_Drop_All.sql         ← Skrypt czyszczący bazę
└── SBD/                        ← Aplikacja ASP.NET Core MVC
    ├── Controllers/            ← Kontrolery HTTP
    ├── Models/                 ← Modele domenowe C#
    ├── Repositories/           ← Dostęp do danych (ADO.NET)
    ├── Services/               ← Logika biznesowa
    ├── Views/                  ← Szablony Razor (.cshtml)
    ├── Database/               ← Singleton połączenia z DB
    └── appsettings.json        ← Connection string
```

**Connection string** (`appsettings.json`):
```json
"SqlDb": "Server=(localdb)\\MSSQLLocalDB;Database=CarRentDB;Trusted_Connection=True;"
```

---

## 3. Schemat bazy danych — tabele

### 3.1 Tabele słownikowe

#### `CAR_CATEGORIES` — Kategorie samochodów
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `CATEGORY_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator kategorii |
| `NAME` | `NVARCHAR(50)` | NOT NULL | Nazwa (np. EKONOMICZNY, SUV) |
| `DESCRIPTION` | `NVARCHAR(255)` | — | Opis kategorii |

**Dane słownikowe:** EKONOMICZNY, KOMPAKTOWY, SUV, PREMIUM, VAN, SPORTOWY

---

#### `PAYMENT_STATUSES` — Statusy płatności
| Kolumna | Typ | Opis |
|---|---|---|
| `STATUS_ID` | `INT IDENTITY(1,1)` PK | Identyfikator |
| `NAME` | `NVARCHAR(50)` NOT NULL | PENDING / PAID / OVERDUE / CANCELLED / REFUNDED |

---

#### `RESERVATION_STATUSES` — Statusy rezerwacji
| Kolumna | Typ | Opis |
|---|---|---|
| `STATUS_ID` | `INT IDENTITY(1,1)` PK | Identyfikator |
| `NAME` | `NVARCHAR(50)` NOT NULL | ACTIVE / CONFIRMED / CANCELLED / COMPLETED / EXPIRED |

---

#### `SERVICE_STATUSES` — Statusy serwisu
| Kolumna | Typ | Opis |
|---|---|---|
| `STATUS_ID` | `INT IDENTITY(1,1)` PK | Identyfikator |
| `NAME` | `NVARCHAR(50)` NOT NULL | SCHEDULED / IN_PROGRESS / COMPLETED / CANCELLED |

---

### 3.2 Tabele główne

#### `BRANCHES` — Oddziały wypożyczalni
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `BRANCH_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator oddziału |
| `NAME` | `NVARCHAR(100)` | NOT NULL | Nazwa oddziału |
| `ADDRESS` | `NVARCHAR(255)` | NOT NULL | Adres |
| `CITY` | `NVARCHAR(100)` | NOT NULL | Miasto |
| `POSTAL_CODE` | `NVARCHAR(10)` | — | Kod pocztowy |
| `PHONE` | `NVARCHAR(20)` | — | Telefon |
| `EMAIL` | `NVARCHAR(100)` | — | Email |
| `IS_ACTIVE` | `INT DEFAULT 1` | CHECK (0 lub 1) | Czy oddział aktywny |

---

#### `CARS` — Samochody
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `CAR_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `BRAND` | `NVARCHAR(50)` | NOT NULL | Marka |
| `MODEL` | `NVARCHAR(50)` | NOT NULL | Model |
| `YEAR` | `INT` | NOT NULL | Rok produkcji |
| `PLATE_NUMBER` | `NVARCHAR(20)` | NOT NULL, UNIQUE | Numer rejestracyjny |
| `VIN` | `NVARCHAR(17)` | NOT NULL, UNIQUE | Numer VIN |
| `DAILY_RATE` | `DECIMAL(10,2)` | NOT NULL, CHECK > 0 | Stawka dzienna (PLN) |
| `CATEGORY_ID` | `INT` | NOT NULL, FK | Kategoria pojazdu |
| `BRANCH_ID` | `INT` | NOT NULL, FK | Oddział |
| `COLOR` | `NVARCHAR(30)` | — | Kolor |
| `SEATS` | `INT DEFAULT 5` | — | Liczba miejsc |
| `MILEAGE` | `INT DEFAULT 0` | CHECK >= 0 | Przebieg (km) |
| `STATUS` | `NVARCHAR(20) DEFAULT 'AVAILABLE'` | CHECK | AVAILABLE / RENTED / IN_SERVICE / UNAVAILABLE |

---

#### `CLIENTS` — Klienci
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `CLIENT_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `FIRST_NAME` | `NVARCHAR(50)` | NOT NULL | Imię |
| `LAST_NAME` | `NVARCHAR(50)` | NOT NULL | Nazwisko |
| `EMAIL` | `NVARCHAR(100)` | NOT NULL, UNIQUE | Adres e-mail |
| `PHONE` | `NVARCHAR(20)` | — | Telefon |
| `PESEL` | `NVARCHAR(11)` | UNIQUE | PESEL |
| `ADDRESS` | `NVARCHAR(255)` | — | Adres zamieszkania |
| `CITY` | `NVARCHAR(100)` | — | Miasto |
| `POSTAL_CODE` | `NVARCHAR(10)` | — | Kod pocztowy |
| `REGISTRATION_DATE` | `DATETIME DEFAULT GETDATE()` | — | Data rejestracji |
| `PASSWORD_HASH` | `NVARCHAR(255)` | NOT NULL | Hash hasła |
| `IS_BLOCKED` | `INT DEFAULT 0` | CHECK (0 lub 1) | Czy klient zablokowany |
| `PENALTY_MULTIPLIER` | `DECIMAL(3,2) DEFAULT 1.00` | — | Mnożnik ceny (kary) |

> **Logika blokowania:** klient z ≥5 karami zostaje zablokowany automatycznie (trigger); przy ≥3 karach mnożnik wzrasta do 1.50.

---

#### `EMPLOYEES` — Pracownicy
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `EMPLOYEE_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `FIRST_NAME` | `NVARCHAR(50)` | NOT NULL | Imię |
| `LAST_NAME` | `NVARCHAR(50)` | NOT NULL | Nazwisko |
| `EMAIL` | `NVARCHAR(100)` | NOT NULL, UNIQUE | E-mail |
| `PHONE` | `NVARCHAR(20)` | — | Telefon |
| `POSITION` | `NVARCHAR(50)` | NOT NULL | Stanowisko |
| `BRANCH_ID` | `INT` | NOT NULL, FK | Oddział |
| `HIRE_DATE` | `DATETIME DEFAULT GETDATE()` | — | Data zatrudnienia |
| `PASSWORD_HASH` | `NVARCHAR(255)` | NOT NULL | Hash hasła |
| `IS_ACTIVE` | `INT DEFAULT 1` | CHECK (0 lub 1) | Czy aktywny |

---

#### `RENTALS` — Wypożyczenia ⭐ (tabela centralna)
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `RENTAL_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `CLIENT_ID` | `INT` | NOT NULL, FK | Klient |
| `CAR_ID` | `INT` | NOT NULL, FK | Samochód |
| `START_DATE` | `DATETIME` | NOT NULL | Data rozpoczęcia |
| `END_DATE` | `DATETIME` | NOT NULL | Planowana data zakończenia |
| `ACTUAL_RETURN_DATE` | `DATETIME` | — | Faktyczna data zwrotu |
| `STATUS` | `NVARCHAR(20) DEFAULT 'ACTIVE'` | CHECK | ACTIVE / COMPLETED / CANCELLED / EXTENDED |
| `TOTAL_PRICE` | `DECIMAL(10,2)` | — | Całkowita cena |
| `START_MILEAGE` | `INT` | NOT NULL | Przebieg przy wydaniu |
| `END_MILEAGE` | `INT` | — | Przebieg przy zwrocie |

> **Uwaga:** INSERT do tej tabeli jest przechwytywany przez trigger `trg_Rental_Validation` (INSTEAD OF INSERT), który waliduje dostępność samochodu i status klienta przed faktycznym wstawieniem.

---

#### `RESERVATIONS` — Rezerwacje
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `RESERVATION_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `CLIENT_ID` | `INT` | NOT NULL, FK | Klient |
| `CAR_ID` | `INT` | NOT NULL, FK | Samochód |
| `START_DATE` | `DATETIME` | NOT NULL | Data od |
| `END_DATE` | `DATETIME` | NOT NULL | Data do |
| `STATUS_ID` | `INT` | NOT NULL, FK | Status rezerwacji |
| `CREATED_AT` | `DATETIME DEFAULT GETDATE()` | — | Data złożenia |
| `NOTES` | `NVARCHAR(MAX)` | — | Uwagi |

---

#### `PAYMENTS` — Płatności
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `PAYMENT_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `RENTAL_ID` | `INT` | NOT NULL, FK | Wypożyczenie |
| `AMOUNT` | `DECIMAL(10,2)` | NOT NULL, CHECK > 0 | Kwota |
| `PAYMENT_DATE` | `DATETIME` | — | Data zapłaty |
| `STATUS_ID` | `INT` | NOT NULL, FK | Status płatności |
| `INSTALLMENT_NO` | `INT DEFAULT 1` | — | Numer raty |
| `TOTAL_INSTALLMENTS` | `INT DEFAULT 1` | — | Liczba rat |
| `PAYMENT_METHOD` | `NVARCHAR(50)` | — | CARD / CASH / TRANSFER |

---

#### `PENALTIES` — Kary
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `PENALTY_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `CLIENT_ID` | `INT` | NOT NULL, FK | Klient |
| `RENTAL_ID` | `INT` | FK (nullable) | Wypożyczenie (opcjonalne) |
| `REASON` | `NVARCHAR(255)` | NOT NULL | Powód kary |
| `AMOUNT` | `DECIMAL(10,2)` | NOT NULL, CHECK >= 0 | Kwota kary |
| `CREATED_AT` | `DATETIME DEFAULT GETDATE()` | — | Data nałożenia |

---

#### `SERVICES` — Serwis
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `SERVICE_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `CAR_ID` | `INT` | NOT NULL, FK | Samochód |
| `DESCRIPTION` | `NVARCHAR(MAX)` | NOT NULL | Opis usługi |
| `COST` | `DECIMAL(10,2)` | — | Koszt |
| `START_DATE` | `DATETIME` | NOT NULL | Początek serwisu |
| `END_DATE` | `DATETIME` | — | Koniec serwisu |
| `STATUS_ID` | `INT` | NOT NULL, FK | Status |
| `NOTES` | `NVARCHAR(MAX)` | — | Notatki |

---

#### `CHANGE_HISTORY` — Audyt zmian
| Kolumna | Typ | Ograniczenia | Opis |
|---|---|---|---|
| `HISTORY_ID` | `INT IDENTITY(1,1)` | PK | Identyfikator |
| `TABLE_NAME` | `NVARCHAR(50)` | NOT NULL | Nazwa tabeli |
| `RECORD_ID` | `INT` | NOT NULL | ID zmienionego rekordu |
| `OLD_VALUES` | `NVARCHAR(MAX)` | — | Stare wartości (tekst) |
| `NEW_VALUES` | `NVARCHAR(MAX)` | — | Nowe wartości (tekst) |
| `CHANGE_DATE` | `DATETIME2 DEFAULT GETDATE()` | — | Data zmiany |
| `CHANGED_BY` | `NVARCHAR(100)` | — | Login użytkownika SQL |
| `OPERATION_TYPE` | `NVARCHAR(20)` | CHECK | INSERT / UPDATE / DELETE |

> Tabela zapełniana wyłącznie przez triggery historii — nie jest modyfikowana ręcznie.

---

## 4. Relacje i klucze obce

| Tabela podrzędna | Kolumna FK | Tabela nadrzędna | Kolumna PK |
|---|---|---|---|
| `CARS` | `CATEGORY_ID` | `CAR_CATEGORIES` | `CATEGORY_ID` |
| `CARS` | `BRANCH_ID` | `BRANCHES` | `BRANCH_ID` |
| `EMPLOYEES` | `BRANCH_ID` | `BRANCHES` | `BRANCH_ID` |
| `RENTALS` | `CLIENT_ID` | `CLIENTS` | `CLIENT_ID` |
| `RENTALS` | `CAR_ID` | `CARS` | `CAR_ID` |
| `RESERVATIONS` | `CLIENT_ID` | `CLIENTS` | `CLIENT_ID` |
| `RESERVATIONS` | `CAR_ID` | `CARS` | `CAR_ID` |
| `RESERVATIONS` | `STATUS_ID` | `RESERVATION_STATUSES` | `STATUS_ID` |
| `PAYMENTS` | `RENTAL_ID` | `RENTALS` | `RENTAL_ID` |
| `PAYMENTS` | `STATUS_ID` | `PAYMENT_STATUSES` | `STATUS_ID` |
| `PENALTIES` | `CLIENT_ID` | `CLIENTS` | `CLIENT_ID` |
| `PENALTIES` | `RENTAL_ID` | `RENTALS` | `RENTAL_ID` |
| `SERVICES` | `CAR_ID` | `CARS` | `CAR_ID` |
| `SERVICES` | `STATUS_ID` | `SERVICE_STATUSES` | `STATUS_ID` |

---

## 5. Diagram ERD (tekstowy)

```
CAR_CATEGORIES ──< CARS >── BRANCHES
                     │
                     │ (CAR_ID)
                     ↓
CLIENTS ──────────> RENTALS <──────── (wiele do wielu: klient ↔ auto)
   │                  |
   │                  |──< PAYMENTS >── PAYMENT_STATUSES
   │                  └──< PENALTIES (opcjonalnie)
   │
   └──< PENALTIES (bez powiązania z wypożyczeniem)
   │
   └──< RESERVATIONS >── RESERVATION_STATUSES

CARS ──< SERVICES >── SERVICE_STATUSES
CARS ──< RESERVATIONS

CLIENTS/CARS ──(triggery)──> CHANGE_HISTORY

EMPLOYEES >── BRANCHES
```

---

## 6. Widoki (Views)

Wszystkie widoki zdefiniowane w [03_Views.sql](file:///b:/sbd/SBD/SQL/03_Views.sql).

### `V_AVAILABLE_CARS` — Dostępne samochody
Zwraca listę samochodów ze statusem `AVAILABLE`, z dołączonymi danymi kategorii i oddziału.

```sql
WHERE c.STATUS = 'AVAILABLE'
```

**Użycie:** strona główna wyświetlająca flotę do wypożyczenia; uprawnienia dla roli `Client` i `Employee`.

---

### `V_CLIENT_RENTALS` — Wypożyczenia klientów
Łączy dane wypożyczeń z klientami i samochodami. Wylicza:
- `PAID_AMOUNT` — suma wpłat ze statusem PAID
- `PENALTY_AMOUNT` — suma kar przypisanych do wypożyczenia

```sql
ISNULL((SELECT SUM(AMOUNT) FROM PAYMENTS p
        JOIN PAYMENT_STATUSES ps ON p.STATUS_ID = ps.STATUS_ID
        WHERE p.RENTAL_ID = r.RENTAL_ID AND ps.NAME = 'PAID'), 0) AS PAID_AMOUNT
```

---

### `V_MONTHLY_REVENUE` — Przychody miesięczne
Grupuje dane wypłaconych płatności według roku i miesiąca. Zwraca: liczbę wypożyczeń, liczbę płatności, sumę przychodów, średnią płatność.

---

### `V_CAR_UTILIZATION` — Wykorzystanie floty
Dla każdego samochodu oblicza: liczbę wypożyczeń, łączną liczbę dni wynajmu (`DATEDIFF` do `ACTUAL_RETURN_DATE` lub `GETDATE()`), łączny przychód. Uwzględnia statusy ACTIVE, EXTENDED, COMPLETED.

---

### `V_HIGH_RISK_CLIENTS` — Klienci wysokiego ryzyka
Zwraca klientów, którzy mają więcej niż 1 karę LUB łączna kwota kar przekracza 1000 PLN.

```sql
HAVING COUNT(p.PENALTY_ID) > 1 OR ISNULL(SUM(p.AMOUNT), 0) > 1000
```

**Użycie:** dostępny dla ról `Employee` i `Analyst`; niedostępny dla roli `Client`.

---

## 7. Funkcje skalarne

Wszystkie funkcje zdefiniowane w [05_Functions.sql](file:///b:/sbd/SBD/SQL/05_Functions.sql).

### `dbo.fn_CalculateRentalPrice(@p_car_id, @p_days, @p_client_id)`
**Zwraca:** `DECIMAL(10,2)` — całkowita cena wypożyczenia.

**Algorytm:**
```
TOTAL_PRICE = DAILY_RATE × @p_days × PENALTY_MULTIPLIER
```
- Pobiera `DAILY_RATE` z tabeli `CARS`
- Pobiera `PENALTY_MULTIPLIER` z tabeli `CLIENTS` (domyślnie 1.0 jeśli brak)
- Jeśli samochód nie istnieje — zwraca 0

**Wywołanie:** używana w procedurze `sp_CreateRental` do automatycznego wyliczenia ceny.

---

### `dbo.fn_CheckCarAvailability(@p_car_id, @p_start_date, @p_end_date)`
**Zwraca:** `INT` — `1` (dostępny) lub `0` (niedostępny).

**Algorytm — wykrywanie kolizji terminów:**
```sql
(@p_start_date BETWEEN START_DATE AND END_DATE) OR
(@p_end_date   BETWEEN START_DATE AND END_DATE) OR
(START_DATE    BETWEEN @p_start_date AND @p_end_date)
```
Sprawdza aktywne wypożyczenia ze statusem `ACTIVE` lub `EXTENDED`.

**Wywołanie:** używana w triggerze `trg_Rental_Validation`.

---

### `dbo.fn_GetClientMultiplier(@p_client_id)`
**Zwraca:** `DECIMAL(3,2)` — mnożnik klienta lub 1.0 jeśli klient nie istnieje.

---

## 8. Procedury składowane

Wszystkie procedury zdefiniowane w [06_Procedures.sql](file:///b:/sbd/SBD/SQL/06_Procedures.sql).

### `sp_CreateRental` ⭐
**Parametry wejściowe:** `@p_client_id INT`, `@p_car_id INT`, `@p_start_date DATETIME`, `@p_end_date DATETIME`  
**Parametr wyjściowy:** `@p_rental_id INT OUTPUT`

**Działanie:**
1. Oblicza liczbę dni (`DATEDIFF`), minimalnie 1 dzień
2. Wywołuje `fn_CalculateRentalPrice` → wylicza `TOTAL_PRICE`
3. Pobiera aktualny przebieg samochodu jako `START_MILEAGE`
4. Wykonuje `INSERT INTO RENTALS` — **INSERT jest przechwytywany przez trigger `INSTEAD OF INSERT`**
5. Pobiera wygenerowane ID przez `@@IDENTITY` (nie `SCOPE_IDENTITY()` — patrz sekcja 14)

> ⚠️ **Kluczowa decyzja:** użycie `@@IDENTITY` zamiast `SCOPE_IDENTITY()` jest konieczne ze względu na trigger `INSTEAD OF INSERT` na tabeli RENTALS.

---

### `sp_CompleteRental(@p_rental_id)`
Ustawia status wypożyczenia na `COMPLETED` i `ACTUAL_RETURN_DATE = GETDATE()`.

---

### `sp_CancelRental(@p_rental_id)`
Ustawia status wypożyczenia na `CANCELLED`.

---

### `sp_ExtendRental(@p_rental_id, @p_new_end_date)`
Ustawia nową datę końcową i status `EXTENDED`.

---

### `sp_ProcessPayment(@p_rental_id, @p_amount, @p_payment_method)`
Rejestruje płatność ze statusem `PAID` i datą `GETDATE()`.

---

### `sp_CalculateInstallments(@p_rental_id, @p_num_installments)`
Tworzy `N` rekordów płatności ratalnych ze statusem `PENDING`. Każda rata = `ROUND(TOTAL_PRICE / N, 2)`.

---

### `sp_BlockClient(@p_client_id)`
Ręczne zablokowanie klienta — ustawia `IS_BLOCKED = 1`.

---

## 9. Triggery

Wszystkie triggery zdefiniowane w [04_Triggers.sql](file:///b:/sbd/SBD/SQL/04_Triggers.sql).

### `trg_Clients_History` — Audyt klientów
**Tabela:** `CLIENTS` | **Typ:** `AFTER UPDATE, DELETE`

Rejestruje każdą zmianę lub usunięcie klienta w tabeli `CHANGE_HISTORY`. Zapisuje imię, nazwisko i e-mail przed zmianą (`deleted`) i po zmianie (`inserted`). Używa `SUSER_SNAME()` do identyfikacji użytkownika SQL.

```sql
DECLARE @op NVARCHAR(20) = CASE WHEN EXISTS(SELECT * FROM inserted) THEN 'UPDATE' ELSE 'DELETE' END;
```

---

### `trg_Cars_History` — Audyt samochodów
**Tabela:** `CARS` | **Typ:** `AFTER UPDATE, DELETE`

Działa analogicznie do `trg_Clients_History`. Rejestruje zmiany statusu i przebiegu samochodu.

---

### `trg_Rental_Validation` — Walidacja wypożyczenia ⭐
**Tabela:** `RENTALS` | **Typ:** `INSTEAD OF INSERT`

> Jest to najważniejszy i najciekawszy trigger w projekcie.

**Działanie (kursor po każdym wierszu z `inserted`):**
1. Sprawdza, czy klient **nie jest zablokowany** (`IS_BLOCKED = 1` → `RAISERROR` + `ROLLBACK`)
2. Wywołuje `fn_CheckCarAvailability` — sprawdza konflikt terminów (`= 0` → `RAISERROR` + `ROLLBACK`)
3. Jeśli walidacja przejdzie — wykonuje właściwy `INSERT INTO RENTALS`

```sql
CREATE OR ALTER TRIGGER trg_Rental_Validation
ON RENTALS
INSTEAD OF INSERT
```

> ⚠️ **Konsekwencja dla procedury:** ponieważ trigger `INSTEAD OF INSERT` zastępuje oryginalny INSERT, `SCOPE_IDENTITY()` w procedurze `sp_CreateRental` zwracałoby `NULL`. Dlatego procedura używa `@@IDENTITY`, które śledzi ostatni INSERT w całej sesji.

---

### `trg_Update_Car_Status` — Automatyczna aktualizacja statusu auta
**Tabela:** `RENTALS` | **Typ:** `AFTER INSERT, UPDATE`

Automatycznie zmienia status samochodu w zależności od statusu wypożyczenia:
- Wypożyczenie `ACTIVE` + `START_DATE <= GETDATE()` → `CARS.STATUS = 'RENTED'`
- Wypożyczenie `COMPLETED` lub `CANCELLED` → `CARS.STATUS = 'AVAILABLE'`

---

### `trg_Penalty_Check` — Automatyczne blokowanie klientów
**Tabela:** `PENALTIES` | **Typ:** `AFTER INSERT`

Po dodaniu kary sprawdza łączną liczbę kar klienta:
- **≥ 5 kar** → `IS_BLOCKED = 1` (klient zablokowany)
- **≥ 3 kary** → `PENALTY_MULTIPLIER = 1.50` (wyższe stawki)

```sql
IF @penalty_count >= 5
    UPDATE CLIENTS SET IS_BLOCKED = 1 WHERE CLIENT_ID = @client_id;
ELSE IF @penalty_count >= 3
    UPDATE CLIENTS SET PENALTY_MULTIPLIER = 1.50 WHERE CLIENT_ID = @client_id;
```

---

## 10. Role i uprawnienia

Zdefiniowane w [08_Roles.sql](file:///b:/sbd/SBD/SQL/08_Roles.sql).

| Rola | Opis | Uprawnienia |
|---|---|---|
| `Admin` | Administrator systemu | `SELECT, INSERT, UPDATE, DELETE, EXECUTE` na całym schemacie `dbo` |
| `Employee` | Pracownik wypożyczalni | Pełny CRUD na: CARS, CLIENTS, RENTALS, RESERVATIONS, PAYMENTS, PENALTIES, SERVICES + SELECT na wszystkich widokach |
| `Client` | Klient (dostęp przez aplikację) | SELECT na `V_AVAILABLE_CARS`, `V_CLIENT_RENTALS`; INSERT na `RESERVATIONS`, `PAYMENTS` |
| `Analyst` | Analityk biznesowy | SELECT na całym schemacie `dbo` (tylko odczyt) |

> **Uwaga:** Role są tworzone na poziomie bazy danych. Żeby przypisać użytkownika do roli, należy najpierw utworzyć LOGIN na poziomie serwera, następnie USER w bazie, a potem `ALTER ROLE ... ADD MEMBER ...`.

---

## 11. Warstwa aplikacyjna — C# ASP.NET Core MVC

### Wzorzec architektoniczny
Projekt stosuje trójwarstwowy wzorzec: **Controller → Service → Repository**.

```
HTTP Request
    ↓
Controller  (walidacja HTTP, routing, ViewBag)
    ↓
Service     (logika biznesowa, walidacja domenowa)
    ↓
Repository  (ADO.NET, zapytania SQL, mapowanie DataTable → Model)
    ↓
SQL Server (LocalDB)
```

### Połączenie z bazą
Klasa `DbConnection` (Singleton) w katalogu `Database/` zarządza połączeniem.

### Repozytoria

| Plik | Odpowiada za |
|---|---|
| [RentalRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/RentalRepository.cs) | CRUD wypożyczeń, wywołanie `sp_CreateRental`, mapowanie wyników |
| [CarRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/CarRepository.cs) | Lista aut, dostępne auta, szczegóły |
| [ClientRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/ClientRepository.cs) | Lista klientów, wyszukiwanie |
| [PaymentRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/PaymentRepository.cs) | Płatności, raty |
| [PenaltyRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/PenaltyRepository.cs) | Kary klientów |
| [ReservationRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/ReservationRepository.cs) | Rezerwacje |
| [ReportRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/ReportRepository.cs) | Zapytania do widoków analitycznych |
| [BranchRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/BranchRepository.cs) | Oddziały |
| [EmployeeRepository.cs](file:///b:/sbd/SBD/SBD/Repositories/EmployeeRepository.cs) | Pracownicy |

### Serwisy (logika biznesowa)

| Plik | Odpowiada za |
|---|---|
| [RentalService.cs](file:///b:/sbd/SBD/SBD/Services/RentalService.cs) | Walidacja klienta i samochodu przed utworzeniem, zarządzanie cyklem życia wypożyczenia |
| [PaymentService.cs](file:///b:/sbd/SBD/SBD/Services/PaymentService.cs) | Logika płatności i rat |
| [PenaltyService.cs](file:///b:/sbd/SBD/SBD/Services/PenaltyService.cs) | Naliczanie kar, blokowanie klientów |
| [ReportService.cs](file:///b:/sbd/SBD/SBD/Services/ReportService.cs) | Generowanie raportów z widoków |

### Kontrolery

| Plik | Ścieżki URL |
|---|---|
| [RentalsController.cs](file:///b:/sbd/SBD/SBD/Controllers/RentalsController.cs) | `/Rentals/Index`, `/Rentals/Create`, `/Rentals/Complete`, `/Rentals/Cancel`, `/Rentals/Extend` |
| [CarsController.cs](file:///b:/sbd/SBD/SBD/Controllers/CarsController.cs) | `/Cars/*` |
| [ClientsController.cs](file:///b:/sbd/SBD/SBD/Controllers/ClientsController.cs) | `/Clients/*` |
| [PaymentsController.cs](file:///b:/sbd/SBD/SBD/Controllers/PaymentsController.cs) | `/Payments/*` |
| [HomeController.cs](file:///b:/sbd/SBD/SBD/Controllers/HomeController.cs) | `/` |

---

## 12. Kolejność wdrożenia skryptów SQL

Skrypty muszą być uruchamiane w kolejności ze względu na zależności (FK, obiekty zależne od tabel):

```
01_DDL_Tables.sql       ← Najpierw — tworzy tabele
02_DDL_Indexes.sql      ← Indeksy (wymagają tabel)
03_Views.sql            ← Widoki (wymagają tabel)
05_Functions.sql        ← Funkcje (wymagają tabel CARS, CLIENTS, RENTALS)
04_Triggers.sql         ← Triggery (wymagają funkcji fn_CheckCarAvailability)
06_Procedures.sql       ← Procedury (wymagają funkcji fn_CalculateRentalPrice)
08_Roles.sql            ← Role i uprawnienia (wymagają tabel i widoków)
09_SeedData.sql         ← Dane testowe (wymagają wszystkich tabel)
```

> ⚠️ **Przed uruchomieniem `09_SeedData.sql`** — triggery walidacyjne powinny być tymczasowo wyłączone (komentarz w pliku seed), ponieważ dane testowe zawierają wypożyczenia wstawiane bez przejścia przez standardowy proces walidacji.

### Czyszczenie bazy
Skrypt `10_Drop_All.sql` usuwa dynamicznie wszystkie obiekty w odpowiedniej kolejności: widoki → funkcje → procedury → klucze obce → tabele → role.

---

## 13. Dane testowe

Skrypt `09_SeedData.sql` tworzy reprezentatywny zestaw danych:

| Encja | Liczba rekordów |
|---|---|
| Kategorie samochodów | 6 |
| Oddziały | 5 (Warszawa, Kraków, Wrocław, Gdańsk, Poznań) |
| Pracownicy | 10 |
| Klienci | 16 (w tym 1 zablokowany z mnożnikiem 1.50) |
| Samochody | 17 (różne kategorie i statusy) |
| Rezerwacje | 8 |
| Wypożyczenia | 13 (10 zakończonych, 2 aktywne, 1 anulowane) |
| Płatności | 14 (w tym płatność ratalna 3×) |
| Kary | 8 (w tym 5 dla klienta „problematycznego") |
| Serwisy | 4 |

**Klient testowy wysokiego ryzyka:** Grzegorz Problemowy (`CLIENT_ID=16`) — `IS_BLOCKED=1`, `PENALTY_MULTIPLIER=1.50`, 5 kar (łącznie 3850 PLN).

---

## 14. Znane problemy i decyzje projektowe

### Problem: `SCOPE_IDENTITY()` vs `@@IDENTITY` przy triggerze `INSTEAD OF INSERT`

**Symptom:** `Unable to cast object of type 'System.DBNull' to type 'System.Int32'` przy tworzeniu wypożyczenia.

**Przyczyna:** Trigger `trg_Rental_Validation` jest typu `INSTEAD OF INSERT`. Gdy procedura `sp_CreateRental` wykonuje `INSERT INTO RENTALS`, trigger przechwytuje to polecenie i sam wykonuje właściwy INSERT. Przez to `SCOPE_IDENTITY()` w scope procedury zwraca `NULL` — bo oryginalny INSERT procedury nigdy nie trafił do tabeli.

**Rozwiązanie:** Zamiana `SCOPE_IDENTITY()` na `@@IDENTITY` w `sp_CreateRental`:
```sql
-- ŹLE (zwraca NULL przy triggerze INSTEAD OF INSERT)
SET @p_rental_id = SCOPE_IDENTITY();

-- DOBRZE (zwraca ostatnie ID w sesji, niezależnie od scope)
SET @p_rental_id = @@IDENTITY;
```

**Różnica między `SCOPE_IDENTITY()` a `@@IDENTITY`:**
- `SCOPE_IDENTITY()` — zwraca ostatnie auto-generowane ID **w bieżącym zakresie (scope)** — czyli tej samej procedurze/batchu
- `@@IDENTITY` — zwraca ostatnie auto-generowane ID **w całej sesji** — obejmuje też triggery

### Decyzja: Trigger INSTEAD OF zamiast AFTER dla walidacji

Zastosowanie `INSTEAD OF INSERT` zamiast `AFTER INSERT` z `ROLLBACK` pozwala na eleganckie przechwycenie nieprawidłowych danych **zanim** trafią do tabeli, unikając generowania wpisów w logach rollbacków.

### Decyzja: Tabela CHANGE_HISTORY zamiast CDC

Zamiast mechanizmu Change Data Capture (CDC) systemu SQL Server, projekt implementuje własną tabelę audytu zapełnianą triggerami. Daje to pełną kontrolę nad formatem przechowywanych danych i nie wymaga specjalnych uprawnień serwera.