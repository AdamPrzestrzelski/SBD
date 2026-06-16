-- 10_Drop_All.sql w SQL Server
-- Najpierw procedury, funkcje, widoki, triggery, potem tabele

DECLARE @sql NVARCHAR(MAX) = N'';

-- Triggery (w SQL Server należą do tabel, więc dropujemy tabele, ale można osobno)
-- Widoki
SELECT @sql += N'DROP VIEW ' + QUOTENAME(name) + N';' + CHAR(13) FROM sys.views;
EXEC sp_executesql @sql;
SET @sql = N'';

-- Funkcje
SELECT @sql += N'DROP FUNCTION ' + QUOTENAME(name) + N';' + CHAR(13) FROM sys.objects WHERE type_desc LIKE '%FUNCTION%';
EXEC sp_executesql @sql;
SET @sql = N'';

-- Procedury
SELECT @sql += N'DROP PROCEDURE ' + QUOTENAME(name) + N';' + CHAR(13) FROM sys.procedures;
EXEC sp_executesql @sql;
SET @sql = N'';

-- Tabele (najpierw FK drop, potem drop tabel)
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';' + CHAR(13)
FROM sys.foreign_keys;
EXEC sp_executesql @sql;
SET @sql = N'';

SELECT @sql += N'DROP TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(name) + N';' + CHAR(13)
FROM sys.tables;
EXEC sp_executesql @sql;

-- Role
DECLARE @role NVARCHAR(128);
DECLARE cur CURSOR FOR SELECT name FROM sys.database_principals WHERE type = 'R' AND is_fixed_role = 0 AND principal_id > 0;
OPEN cur;
FETCH NEXT FROM cur INTO @role;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP ROLE ' + @role);
    FETCH NEXT FROM cur INTO @role;
END
CLOSE cur;
DEALLOCATE cur;
