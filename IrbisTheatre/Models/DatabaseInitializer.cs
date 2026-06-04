using Microsoft.EntityFrameworkCore;
using Npgsql;
using IrbisTheatre.Models;

namespace IrbisTheatre;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly string _masterConnectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;

        // Парсим строку подключения для получения имени БД
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        _databaseName = builder.Database;

        // Строка для подключения к postgres (без указания конкретной БД)
        builder.Database = "postgres";
        _masterConnectionString = builder.ToString();
    }

    /// <summary>
    /// Удаляет базу данных (если существует) и создаёт заново
    /// </summary>
    public void RecreateDatabase()
    {
        DropDatabaseIfExists();
        CreateDatabase();
    }

    /// <summary>
    /// Удаляет базу данных если она существует
    /// </summary>
    public void DropDatabaseIfExists()
    {
        using var connection = new NpgsqlConnection(_masterConnectionString);
        connection.Open();

        // Завершаем все подключения к нашей БД перед удалением
        string terminateQuery = $@"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{_databaseName}'
            AND pid <> pg_backend_pid();";

        using var terminateCmd = new NpgsqlCommand(terminateQuery, connection);
        terminateCmd.ExecuteNonQuery();

        // Удаляем базу данных
        string dropQuery = $"DROP DATABASE IF EXISTS \"{_databaseName}\";";
        using var dropCmd = new NpgsqlCommand(dropQuery, connection);
        dropCmd.ExecuteNonQuery();

        Console.WriteLine($"База данных '{_databaseName}' удалена (если существовала)");
    }

    /// <summary>
    /// Создаёт новую базу данных
    /// </summary>
    public void CreateDatabase()
    {
        using var connection = new NpgsqlConnection(_masterConnectionString);
        connection.Open();

        string createQuery = $"CREATE DATABASE \"{_databaseName}\";";
        using var createCmd = new NpgsqlCommand(createQuery, connection);
        createCmd.ExecuteNonQuery();

        Console.WriteLine($"✅ База данных '{_databaseName}' создана");
    }

    /// <summary>
    /// Создаёт все таблицы в указанной базе данных
    /// </summary>
    public void CreateAllTables()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string createTablesSql = @"
            -- Таблица авторов
            CREATE TABLE IF NOT EXISTS ""Authors"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Fio"" VARCHAR(255) NOT NULL,
                ""Country"" VARCHAR(100) NULL,
                ""YearsOfLife"" VARCHAR(50) NULL,
                ""Biography"" TEXT NULL
            );

            -- Таблица жанров
            CREATE TABLE IF NOT EXISTS ""Genres"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(100) NOT NULL UNIQUE
            );

            -- Таблица залов
            CREATE TABLE IF NOT EXISTS ""Halls"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(100) NOT NULL
            );

            -- Таблица свойств
            CREATE TABLE IF NOT EXISTS ""Properties"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(100) NOT NULL UNIQUE
            );

            -- Таблица пьес (спектаклей)
            CREATE TABLE IF NOT EXISTS ""Plays"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Title"" VARCHAR(255) NOT NULL,
                ""Description"" TEXT NULL,
                ""TargetAudience"" VARCHAR(100) NULL,
                ""PremiereDate"" DATE NULL,
                ""AuthorId"" INTEGER NOT NULL REFERENCES ""Authors""(""Id"") ON DELETE RESTRICT,
                ""GenreId"" INTEGER NOT NULL REFERENCES ""Genres""(""Id"") ON DELETE RESTRICT
            );

            -- Таблица ролей
            CREATE TABLE IF NOT EXISTS ""Roles"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(255) NOT NULL,
                ""GenderRequirement"" VARCHAR(20) DEFAULT 'any',
                ""AgeRange"" VARCHAR(50) NULL,
                ""VoiceRequirements"" VARCHAR(255) NULL,
                ""PlayId"" INTEGER NOT NULL REFERENCES ""Plays""(""Id"") ON DELETE CASCADE
            );

            -- Таблица мест
            CREATE TABLE IF NOT EXISTS ""Seats"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""RowNumber"" SMALLINT NOT NULL,
                ""SeatNumber"" SMALLINT NOT NULL,
                ""Category"" VARCHAR(50) NULL,
                ""HallId"" INTEGER NOT NULL REFERENCES ""Halls""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""UK_Seat"" UNIQUE (""HallId"", ""RowNumber"", ""SeatNumber"")
            );

            -- Таблица сотрудников
            CREATE TABLE IF NOT EXISTS ""Employers"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Fio"" VARCHAR(255) NOT NULL,
                ""Gender"" VARCHAR(10) NULL,
                ""BirthDate"" DATE NULL,
                ""Contacts"" TEXT NULL,
                ""Email"" VARCHAR(255) NULL,      -- 👈 ДОБАВИТЬ
                ""Password"" VARCHAR(255) NULL,   -- 👈 ДОБАВИТЬ
                ""Position"" VARCHAR(100) NULL,
                ""Salary"" DECIMAL(10,2) NULL,
                ""Status"" VARCHAR(50) DEFAULT 'работает'
            );

            -- Таблица показов
            CREATE TABLE IF NOT EXISTS ""Performances"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Datetime"" TIMESTAMP NOT NULL,
                ""BasePrice"" DECIMAL(10,2) NOT NULL,
                ""IsPremiere"" BOOLEAN DEFAULT FALSE,
                ""Status"" VARCHAR(50) NULL,
                ""PlayId"" INTEGER NOT NULL REFERENCES ""Plays""(""Id"") ON DELETE RESTRICT
            );

            -- Таблица постановочной группы
            CREATE TABLE IF NOT EXISTS ""ProductionTeams"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""ParticipationType"" TEXT NULL,
                ""ProductionPosition"" VARCHAR(255) NULL,
                ""EmployerId"" INTEGER NOT NULL REFERENCES ""Employers""(""Id"") ON DELETE CASCADE,
                ""RoleId"" INTEGER NOT NULL REFERENCES ""Roles""(""Id"") ON DELETE CASCADE
            );

            -- Таблица билетов
            CREATE TABLE IF NOT EXISTS ""Tickets"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""UniqueNumber"" VARCHAR(50) NOT NULL UNIQUE,
                ""Price"" DECIMAL(10,2) NOT NULL,
                ""Status"" VARCHAR(20) DEFAULT 'free',
                ""PerformanceId"" INTEGER NOT NULL REFERENCES ""Performances""(""Id"") ON DELETE CASCADE,
                ""SeatId"" INTEGER NOT NULL REFERENCES ""Seats""(""Id"") ON DELETE RESTRICT
            );

            -- Таблица связи показов с постановочной группой
            CREATE TABLE IF NOT EXISTS ""PerformanceGroups"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""PerformanceId"" INTEGER NOT NULL REFERENCES ""Performances""(""Id"") ON DELETE CASCADE,
                ""ProductionTeamId"" INTEGER NOT NULL REFERENCES ""ProductionTeams""(""Id"") ON DELETE CASCADE
            );

            -- Таблица свойств сотрудников
            CREATE TABLE IF NOT EXISTS ""EmployerProperties"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Value"" VARCHAR(255) NOT NULL,
                ""EmployerId"" INTEGER NOT NULL REFERENCES ""Employers""(""Id"") ON DELETE CASCADE,
                ""PropertyId"" INTEGER NOT NULL REFERENCES ""Properties""(""Id"") ON DELETE CASCADE
            );

            -- Создание индексов для ускорения запросов
            CREATE INDEX IF NOT EXISTS ""IX_Plays_AuthorId"" ON ""Plays"" (""AuthorId"");
            CREATE INDEX IF NOT EXISTS ""IX_Plays_GenreId"" ON ""Plays"" (""GenreId"");
            CREATE INDEX IF NOT EXISTS ""IX_Roles_PlayId"" ON ""Roles"" (""PlayId"");
            CREATE INDEX IF NOT EXISTS ""IX_Seats_HallId"" ON ""Seats"" (""HallId"");
            CREATE INDEX IF NOT EXISTS ""IX_Performances_PlayId"" ON ""Performances"" (""PlayId"");
            CREATE INDEX IF NOT EXISTS ""IX_Tickets_PerformanceId"" ON ""Tickets"" (""PerformanceId"");
            CREATE INDEX IF NOT EXISTS ""IX_Tickets_SeatId"" ON ""Tickets"" (""SeatId"");
            CREATE INDEX IF NOT EXISTS ""IX_ProductionTeams_EmployerId"" ON ""ProductionTeams"" (""EmployerId"");
            CREATE INDEX IF NOT EXISTS ""IX_ProductionTeams_RoleId"" ON ""ProductionTeams"" (""RoleId"");
            CREATE INDEX IF NOT EXISTS ""IX_PerformanceGroups_PerformanceId"" ON ""PerformanceGroups"" (""PerformanceId"");
            CREATE INDEX IF NOT EXISTS ""IX_PerformanceGroups_ProductionTeamId"" ON ""PerformanceGroups"" (""ProductionTeamId"");
            CREATE INDEX IF NOT EXISTS ""IX_EmployerProperties_EmployerId"" ON ""EmployerProperties"" (""EmployerId"");
            CREATE INDEX IF NOT EXISTS ""IX_EmployerProperties_PropertyId"" ON ""EmployerProperties"" (""PropertyId"");

            
            -- Таблица репетиций
            CREATE TABLE IF NOT EXISTS ""Rehearsals"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Datetime"" TIMESTAMP NOT NULL,
                ""Location"" VARCHAR(100) NOT NULL,
                ""Description"" TEXT NULL,
                ""Status"" VARCHAR(50) DEFAULT 'запланирована',
                ""PlayId"" INTEGER NOT NULL REFERENCES ""Plays""(""Id"") ON DELETE CASCADE
            );

            -- Таблица участников репетиций
            CREATE TABLE IF NOT EXISTS ""RehearsalParticipants"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""RehearsalId"" INTEGER NOT NULL REFERENCES ""Rehearsals""(""Id"") ON DELETE CASCADE,
                ""EmployerId"" INTEGER NOT NULL REFERENCES ""Employers""(""Id"") ON DELETE CASCADE,
                ""Role"" VARCHAR(50) NULL
            );

            -- Индексы
            CREATE INDEX IF NOT EXISTS ""IX_Rehearsals_Datetime"" ON ""Rehearsals"" (""Datetime"");
            CREATE INDEX IF NOT EXISTS ""IX_Rehearsals_PlayId"" ON ""Rehearsals"" (""PlayId"");
            CREATE INDEX IF NOT EXISTS ""IX_RehearsalParticipants_RehearsalId"" ON ""RehearsalParticipants"" (""RehearsalId"");
            CREATE INDEX IF NOT EXISTS ""IX_RehearsalParticipants_EmployerId"" ON ""RehearsalParticipants"" (""EmployerId"");


            -- ТРИГГЕРЫ

            -- 1. ТАБЛИЦА ЖУРНАЛА


            CREATE TABLE IF NOT EXISTS ""Journal"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""TableName"" VARCHAR(100) NOT NULL,
                ""Operation"" VARCHAR(10) NOT NULL,
                ""OperationTime"" TIMESTAMP NOT NULL DEFAULT NOW(),
                ""UserName"" VARCHAR(200) NOT NULL DEFAULT CURRENT_USER
            );


            -- 2. Authors


            CREATE TABLE IF NOT EXISTS ""Authors_Arch"" (LIKE ""Authors"" INCLUDING ALL);
            ALTER TABLE ""Authors_Arch"" ADD COLUMN ""JournalId"" INT;
            ALTER TABLE ""Authors_Arch"" ADD COLUMN ""OperationType"" VARCHAR(10);
            ALTER TABLE ""Authors_Arch"" ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Authors_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Authors', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Authors_Arch"" (""Id"", ""Fio"", ""Country"", ""YearsOfLife"", ""Biography"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Fio"", NEW.""Country"", NEW.""YearsOfLife"", NEW.""Biography"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Authors_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Authors', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Authors_Arch"" (""Id"", ""Fio"", ""Country"", ""YearsOfLife"", ""Biography"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Fio"", NEW.""Country"", NEW.""YearsOfLife"", NEW.""Biography"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Authors_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Authors', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Authors_Arch"" (""Id"", ""Fio"", ""Country"", ""YearsOfLife"", ""Biography"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Fio"", OLD.""Country"", OLD.""YearsOfLife"", OLD.""Biography"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Authors_insert ON ""Authors"";
            CREATE TRIGGER trigger_Authors_insert AFTER INSERT ON ""Authors"" FOR EACH ROW EXECUTE FUNCTION tr_Authors_insert();
            DROP TRIGGER IF EXISTS trigger_Authors_update ON ""Authors"";
            CREATE TRIGGER trigger_Authors_update AFTER UPDATE ON ""Authors"" FOR EACH ROW EXECUTE FUNCTION tr_Authors_update();
            DROP TRIGGER IF EXISTS trigger_Authors_delete ON ""Authors"";
            CREATE TRIGGER trigger_Authors_delete BEFORE DELETE ON ""Authors"" FOR EACH ROW EXECUTE FUNCTION tr_Authors_delete();


            -- 3. Genres


            CREATE TABLE IF NOT EXISTS ""Genres_Arch"" (LIKE ""Genres"" INCLUDING ALL);
            ALTER TABLE ""Genres_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Genres_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Genres', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Genres_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Genres_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Genres', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Genres_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Genres_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Genres', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Genres_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Name"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Genres_insert ON ""Genres"";
            CREATE TRIGGER trigger_Genres_insert AFTER INSERT ON ""Genres"" FOR EACH ROW EXECUTE FUNCTION tr_Genres_insert();
            DROP TRIGGER IF EXISTS trigger_Genres_update ON ""Genres"";
            CREATE TRIGGER trigger_Genres_update AFTER UPDATE ON ""Genres"" FOR EACH ROW EXECUTE FUNCTION tr_Genres_update();
            DROP TRIGGER IF EXISTS trigger_Genres_delete ON ""Genres"";
            CREATE TRIGGER trigger_Genres_delete BEFORE DELETE ON ""Genres"" FOR EACH ROW EXECUTE FUNCTION tr_Genres_delete();


            -- 4. Halls


            CREATE TABLE IF NOT EXISTS ""Halls_Arch"" (LIKE ""Halls"" INCLUDING ALL);
            ALTER TABLE ""Halls_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Halls_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Halls', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Halls_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Halls_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Halls', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Halls_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Halls_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Halls', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Halls_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Name"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Halls_insert ON ""Halls"";
            CREATE TRIGGER trigger_Halls_insert AFTER INSERT ON ""Halls"" FOR EACH ROW EXECUTE FUNCTION tr_Halls_insert();
            DROP TRIGGER IF EXISTS trigger_Halls_update ON ""Halls"";
            CREATE TRIGGER trigger_Halls_update AFTER UPDATE ON ""Halls"" FOR EACH ROW EXECUTE FUNCTION tr_Halls_update();
            DROP TRIGGER IF EXISTS trigger_Halls_delete ON ""Halls"";
            CREATE TRIGGER trigger_Halls_delete BEFORE DELETE ON ""Halls"" FOR EACH ROW EXECUTE FUNCTION tr_Halls_delete();


            -- 5. Properties


            CREATE TABLE IF NOT EXISTS ""Properties_Arch"" (LIKE ""Properties"" INCLUDING ALL);
            ALTER TABLE ""Properties_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Properties_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Properties', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Properties_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Properties_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Properties', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Properties_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Properties_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Properties', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Properties_Arch"" (""Id"", ""Name"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Name"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Properties_insert ON ""Properties"";
            CREATE TRIGGER trigger_Properties_insert AFTER INSERT ON ""Properties"" FOR EACH ROW EXECUTE FUNCTION tr_Properties_insert();
            DROP TRIGGER IF EXISTS trigger_Properties_update ON ""Properties"";
            CREATE TRIGGER trigger_Properties_update AFTER UPDATE ON ""Properties"" FOR EACH ROW EXECUTE FUNCTION tr_Properties_update();
            DROP TRIGGER IF EXISTS trigger_Properties_delete ON ""Properties"";
            CREATE TRIGGER trigger_Properties_delete BEFORE DELETE ON ""Properties"" FOR EACH ROW EXECUTE FUNCTION tr_Properties_delete();


            -- 6. Plays


            CREATE TABLE IF NOT EXISTS ""Plays_Arch"" (LIKE ""Plays"" INCLUDING ALL);
            ALTER TABLE ""Plays_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Plays_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Plays', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Plays_Arch"" (""Id"", ""Title"", ""Description"", ""TargetAudience"", ""PremiereDate"", ""AuthorId"", ""GenreId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Title"", NEW.""Description"", NEW.""TargetAudience"", NEW.""PremiereDate"", NEW.""AuthorId"", NEW.""GenreId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Plays_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Plays', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Plays_Arch"" (""Id"", ""Title"", ""Description"", ""TargetAudience"", ""PremiereDate"", ""AuthorId"", ""GenreId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Title"", NEW.""Description"", NEW.""TargetAudience"", NEW.""PremiereDate"", NEW.""AuthorId"", NEW.""GenreId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Plays_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Plays', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Plays_Arch"" (""Id"", ""Title"", ""Description"", ""TargetAudience"", ""PremiereDate"", ""AuthorId"", ""GenreId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Title"", OLD.""Description"", OLD.""TargetAudience"", OLD.""PremiereDate"", OLD.""AuthorId"", OLD.""GenreId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Plays_insert ON ""Plays"";
            CREATE TRIGGER trigger_Plays_insert AFTER INSERT ON ""Plays"" FOR EACH ROW EXECUTE FUNCTION tr_Plays_insert();
            DROP TRIGGER IF EXISTS trigger_Plays_update ON ""Plays"";
            CREATE TRIGGER trigger_Plays_update AFTER UPDATE ON ""Plays"" FOR EACH ROW EXECUTE FUNCTION tr_Plays_update();
            DROP TRIGGER IF EXISTS trigger_Plays_delete ON ""Plays"";
            CREATE TRIGGER trigger_Plays_delete BEFORE DELETE ON ""Plays"" FOR EACH ROW EXECUTE FUNCTION tr_Plays_delete();



            -- 7. Roles


            CREATE TABLE IF NOT EXISTS ""Roles_Arch"" (LIKE ""Roles"" INCLUDING ALL);
            ALTER TABLE ""Roles_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Roles_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Roles', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Roles_Arch"" (""Id"", ""Name"", ""GenderRequirement"", ""AgeRange"", ""VoiceRequirements"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", NEW.""GenderRequirement"", NEW.""AgeRange"", NEW.""VoiceRequirements"", NEW.""PlayId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Roles_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Roles', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Roles_Arch"" (""Id"", ""Name"", ""GenderRequirement"", ""AgeRange"", ""VoiceRequirements"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Name"", NEW.""GenderRequirement"", NEW.""AgeRange"", NEW.""VoiceRequirements"", NEW.""PlayId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Roles_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Roles', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Roles_Arch"" (""Id"", ""Name"", ""GenderRequirement"", ""AgeRange"", ""VoiceRequirements"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Name"", OLD.""GenderRequirement"", OLD.""AgeRange"", OLD.""VoiceRequirements"", OLD.""PlayId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Roles_insert ON ""Roles"";
            CREATE TRIGGER trigger_Roles_insert AFTER INSERT ON ""Roles"" FOR EACH ROW EXECUTE FUNCTION tr_Roles_insert();
            DROP TRIGGER IF EXISTS trigger_Roles_update ON ""Roles"";
            CREATE TRIGGER trigger_Roles_update AFTER UPDATE ON ""Roles"" FOR EACH ROW EXECUTE FUNCTION tr_Roles_update();
            DROP TRIGGER IF EXISTS trigger_Roles_delete ON ""Roles"";
            CREATE TRIGGER trigger_Roles_delete BEFORE DELETE ON ""Roles"" FOR EACH ROW EXECUTE FUNCTION tr_Roles_delete();



            -- 8. Seats


            CREATE TABLE IF NOT EXISTS ""Seats_Arch"" (LIKE ""Seats"" INCLUDING ALL);
            ALTER TABLE ""Seats_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Seats_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Seats', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Seats_Arch"" (""Id"", ""RowNumber"", ""SeatNumber"", ""Category"", ""HallId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""RowNumber"", NEW.""SeatNumber"", NEW.""Category"", NEW.""HallId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Seats_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Seats', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Seats_Arch"" (""Id"", ""RowNumber"", ""SeatNumber"", ""Category"", ""HallId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""RowNumber"", NEW.""SeatNumber"", NEW.""Category"", NEW.""HallId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Seats_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Seats', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Seats_Arch"" (""Id"", ""RowNumber"", ""SeatNumber"", ""Category"", ""HallId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""RowNumber"", OLD.""SeatNumber"", OLD.""Category"", OLD.""HallId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Seats_insert ON ""Seats"";
            CREATE TRIGGER trigger_Seats_insert AFTER INSERT ON ""Seats"" FOR EACH ROW EXECUTE FUNCTION tr_Seats_insert();
            DROP TRIGGER IF EXISTS trigger_Seats_update ON ""Seats"";
            CREATE TRIGGER trigger_Seats_update AFTER UPDATE ON ""Seats"" FOR EACH ROW EXECUTE FUNCTION tr_Seats_update();
            DROP TRIGGER IF EXISTS trigger_Seats_delete ON ""Seats"";
            CREATE TRIGGER trigger_Seats_delete BEFORE DELETE ON ""Seats"" FOR EACH ROW EXECUTE FUNCTION tr_Seats_delete();



            -- 9. Employers


            CREATE TABLE IF NOT EXISTS ""Employers_Arch"" (LIKE ""Employers"" INCLUDING ALL);
            ALTER TABLE ""Employers_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Employers_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Employers_Arch"" (""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", ""Position"", ""Salary"", ""Status"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Fio"", NEW.""Gender"", NEW.""BirthDate"", NEW.""Contacts"", NEW.""Position"", NEW.""Salary"", NEW.""Status"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Employers_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Employers_Arch"" (""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", ""Position"", ""Salary"", ""Status"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Fio"", NEW.""Gender"", NEW.""BirthDate"", NEW.""Contacts"", NEW.""Position"", NEW.""Salary"", NEW.""Status"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Employers_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Employers_Arch"" (""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", ""Position"", ""Salary"", ""Status"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Fio"", OLD.""Gender"", OLD.""BirthDate"", OLD.""Contacts"", OLD.""Position"", OLD.""Salary"", OLD.""Status"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Employers_insert ON ""Employers"";
            CREATE TRIGGER trigger_Employers_insert AFTER INSERT ON ""Employers"" FOR EACH ROW EXECUTE FUNCTION tr_Employers_insert();
            DROP TRIGGER IF EXISTS trigger_Employers_update ON ""Employers"";
            CREATE TRIGGER trigger_Employers_update AFTER UPDATE ON ""Employers"" FOR EACH ROW EXECUTE FUNCTION tr_Employers_update();
            DROP TRIGGER IF EXISTS trigger_Employers_delete ON ""Employers"";
            CREATE TRIGGER trigger_Employers_delete BEFORE DELETE ON ""Employers"" FOR EACH ROW EXECUTE FUNCTION tr_Employers_delete();



            -- 10. Performances


            CREATE TABLE IF NOT EXISTS ""Performances_Arch"" (LIKE ""Performances"" INCLUDING ALL);
            ALTER TABLE ""Performances_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Performances_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Performances', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Performances_Arch"" (""Id"", ""Datetime"", ""BasePrice"", ""IsPremiere"", ""Status"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Datetime"", NEW.""BasePrice"", NEW.""IsPremiere"", NEW.""Status"", NEW.""PlayId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Performances_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Performances', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Performances_Arch"" (""Id"", ""Datetime"", ""BasePrice"", ""IsPremiere"", ""Status"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Datetime"", NEW.""BasePrice"", NEW.""IsPremiere"", NEW.""Status"", NEW.""PlayId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Performances_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Performances', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Performances_Arch"" (""Id"", ""Datetime"", ""BasePrice"", ""IsPremiere"", ""Status"", ""PlayId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Datetime"", OLD.""BasePrice"", OLD.""IsPremiere"", OLD.""Status"", OLD.""PlayId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Performances_insert ON ""Performances"";
            CREATE TRIGGER trigger_Performances_insert AFTER INSERT ON ""Performances"" FOR EACH ROW EXECUTE FUNCTION tr_Performances_insert();
            DROP TRIGGER IF EXISTS trigger_Performances_update ON ""Performances"";
            CREATE TRIGGER trigger_Performances_update AFTER UPDATE ON ""Performances"" FOR EACH ROW EXECUTE FUNCTION tr_Performances_update();
            DROP TRIGGER IF EXISTS trigger_Performances_delete ON ""Performances"";
            CREATE TRIGGER trigger_Performances_delete BEFORE DELETE ON ""Performances"" FOR EACH ROW EXECUTE FUNCTION tr_Performances_delete();



            -- 11. ProductionTeams


            CREATE TABLE IF NOT EXISTS ""ProductionTeams_Arch"" (LIKE ""ProductionTeams"" INCLUDING ALL);
            ALTER TABLE ""ProductionTeams_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_ProductionTeams_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('ProductionTeams', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""ProductionTeams_Arch"" (""Id"", ""ParticipationType"", ""ProductionPosition"", ""EmployerId"", ""RoleId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""ParticipationType"", NEW.""ProductionPosition"", NEW.""EmployerId"", NEW.""RoleId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_ProductionTeams_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('ProductionTeams', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""ProductionTeams_Arch"" (""Id"", ""ParticipationType"", ""ProductionPosition"", ""EmployerId"", ""RoleId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""ParticipationType"", NEW.""ProductionPosition"", NEW.""EmployerId"", NEW.""RoleId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_ProductionTeams_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('ProductionTeams', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""ProductionTeams_Arch"" (""Id"", ""ParticipationType"", ""ProductionPosition"", ""EmployerId"", ""RoleId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""ParticipationType"", OLD.""ProductionPosition"", OLD.""EmployerId"", OLD.""RoleId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_ProductionTeams_insert ON ""ProductionTeams"";
            CREATE TRIGGER trigger_ProductionTeams_insert AFTER INSERT ON ""ProductionTeams"" FOR EACH ROW EXECUTE FUNCTION tr_ProductionTeams_insert();
            DROP TRIGGER IF EXISTS trigger_ProductionTeams_update ON ""ProductionTeams"";
            CREATE TRIGGER trigger_ProductionTeams_update AFTER UPDATE ON ""ProductionTeams"" FOR EACH ROW EXECUTE FUNCTION tr_ProductionTeams_update();
            DROP TRIGGER IF EXISTS trigger_ProductionTeams_delete ON ""ProductionTeams"";
            CREATE TRIGGER trigger_ProductionTeams_delete BEFORE DELETE ON ""ProductionTeams"" FOR EACH ROW EXECUTE FUNCTION tr_ProductionTeams_delete();



            -- 12. Tickets


            CREATE TABLE IF NOT EXISTS ""Tickets_Arch"" (LIKE ""Tickets"" INCLUDING ALL);
            ALTER TABLE ""Tickets_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_Tickets_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Tickets', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Tickets_Arch"" (""Id"", ""UniqueNumber"", ""Price"", ""Status"", ""PerformanceId"", ""SeatId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""UniqueNumber"", NEW.""Price"", NEW.""Status"", NEW.""PerformanceId"", NEW.""SeatId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Tickets_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Tickets', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Tickets_Arch"" (""Id"", ""UniqueNumber"", ""Price"", ""Status"", ""PerformanceId"", ""SeatId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""UniqueNumber"", NEW.""Price"", NEW.""Status"", NEW.""PerformanceId"", NEW.""SeatId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_Tickets_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Tickets', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""Tickets_Arch"" (""Id"", ""UniqueNumber"", ""Price"", ""Status"", ""PerformanceId"", ""SeatId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""UniqueNumber"", OLD.""Price"", OLD.""Status"", OLD.""PerformanceId"", OLD.""SeatId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_Tickets_insert ON ""Tickets"";
            CREATE TRIGGER trigger_Tickets_insert AFTER INSERT ON ""Tickets"" FOR EACH ROW EXECUTE FUNCTION tr_Tickets_insert();
            DROP TRIGGER IF EXISTS trigger_Tickets_update ON ""Tickets"";
            CREATE TRIGGER trigger_Tickets_update AFTER UPDATE ON ""Tickets"" FOR EACH ROW EXECUTE FUNCTION tr_Tickets_update();
            DROP TRIGGER IF EXISTS trigger_Tickets_delete ON ""Tickets"";
            CREATE TRIGGER trigger_Tickets_delete BEFORE DELETE ON ""Tickets"" FOR EACH ROW EXECUTE FUNCTION tr_Tickets_delete();



            -- 13. PerformanceGroups


            CREATE TABLE IF NOT EXISTS ""PerformanceGroups_Arch"" (LIKE ""PerformanceGroups"" INCLUDING ALL);
            ALTER TABLE ""PerformanceGroups_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_PerformanceGroups_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('PerformanceGroups', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""PerformanceGroups_Arch"" (""Id"", ""PerformanceId"", ""ProductionTeamId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""PerformanceId"", NEW.""ProductionTeamId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_PerformanceGroups_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('PerformanceGroups', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""PerformanceGroups_Arch"" (""Id"", ""PerformanceId"", ""ProductionTeamId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""PerformanceId"", NEW.""ProductionTeamId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_PerformanceGroups_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('PerformanceGroups', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""PerformanceGroups_Arch"" (""Id"", ""PerformanceId"", ""ProductionTeamId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""PerformanceId"", OLD.""ProductionTeamId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_PerformanceGroups_insert ON ""PerformanceGroups"";
            CREATE TRIGGER trigger_PerformanceGroups_insert AFTER INSERT ON ""PerformanceGroups"" FOR EACH ROW EXECUTE FUNCTION tr_PerformanceGroups_insert();
            DROP TRIGGER IF EXISTS trigger_PerformanceGroups_update ON ""PerformanceGroups"";
            CREATE TRIGGER trigger_PerformanceGroups_update AFTER UPDATE ON ""PerformanceGroups"" FOR EACH ROW EXECUTE FUNCTION tr_PerformanceGroups_update();
            DROP TRIGGER IF EXISTS trigger_PerformanceGroups_delete ON ""PerformanceGroups"";
            CREATE TRIGGER trigger_PerformanceGroups_delete BEFORE DELETE ON ""PerformanceGroups"" FOR EACH ROW EXECUTE FUNCTION tr_PerformanceGroups_delete();



            -- 14. EmployerProperties


            CREATE TABLE IF NOT EXISTS ""EmployerProperties_Arch"" (LIKE ""EmployerProperties"" INCLUDING ALL);
            ALTER TABLE ""EmployerProperties_Arch"" ADD COLUMN ""JournalId"" INT, ADD COLUMN ""OperationType"" VARCHAR(10), ADD COLUMN ""OperationTime"" TIMESTAMP DEFAULT NOW();

            CREATE OR REPLACE FUNCTION tr_EmployerProperties_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('EmployerProperties', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""EmployerProperties_Arch"" (""Id"", ""Value"", ""EmployerId"", ""PropertyId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Value"", NEW.""EmployerId"", NEW.""PropertyId"", j_id, 'INSERT');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_EmployerProperties_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('EmployerProperties', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""EmployerProperties_Arch"" (""Id"", ""Value"", ""EmployerId"", ""PropertyId"", ""JournalId"", ""OperationType"")
                VALUES (NEW.""Id"", NEW.""Value"", NEW.""EmployerId"", NEW.""PropertyId"", j_id, 'UPDATE');
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION tr_EmployerProperties_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('EmployerProperties', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
                INSERT INTO ""EmployerProperties_Arch"" (""Id"", ""Value"", ""EmployerId"", ""PropertyId"", ""JournalId"", ""OperationType"")
                VALUES (OLD.""Id"", OLD.""Value"", OLD.""EmployerId"", OLD.""PropertyId"", j_id, 'DELETE');
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trigger_EmployerProperties_insert ON ""EmployerProperties"";
            CREATE TRIGGER trigger_EmployerProperties_insert AFTER INSERT ON ""EmployerProperties"" FOR EACH ROW EXECUTE FUNCTION tr_EmployerProperties_insert();
            DROP TRIGGER IF EXISTS trigger_EmployerProperties_update ON ""EmployerProperties"";
            CREATE TRIGGER trigger_EmployerProperties_update AFTER UPDATE ON ""EmployerProperties"" FOR EACH ROW EXECUTE FUNCTION tr_EmployerProperties_update();
            DROP TRIGGER IF EXISTS trigger_EmployerProperties_delete ON ""EmployerProperties"";
            CREATE TRIGGER trigger_EmployerProperties_delete BEFORE DELETE ON ""EmployerProperties"" FOR EACH ROW EXECUTE FUNCTION tr_EmployerProperties_delete();


            -- Архивная таблица
            CREATE TABLE IF NOT EXISTS ""Employers_Arch"" (LIKE ""Employers"" INCLUDING ALL);
            ALTER TABLE ""Employers_Arch"" ADD COLUMN IF NOT EXISTS ""JournalId"" INT;
            ALTER TABLE ""Employers_Arch"" ADD COLUMN IF NOT EXISTS ""OperationType"" VARCHAR(10);
            ALTER TABLE ""Employers_Arch"" ADD COLUMN IF NOT EXISTS ""OperationTime"" TIMESTAMP DEFAULT NOW();

            -- Функция для INSERT
            CREATE OR REPLACE FUNCTION tr_Employers_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Employers_Arch"" (
                    ""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", 
                    ""Email"", ""Password"", ""Position"", ""Salary"", ""Status"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    NEW.""Id"", NEW.""Fio"", NEW.""Gender"", NEW.""BirthDate"", NEW.""Contacts"", 
                    NEW.""Email"", NEW.""Password"", NEW.""Position"", NEW.""Salary"", NEW.""Status"", 
                    j_id, 'INSERT'
                );
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            -- Функция для UPDATE
            CREATE OR REPLACE FUNCTION tr_Employers_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Employers_Arch"" (
                    ""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", 
                    ""Email"", ""Password"", ""Position"", ""Salary"", ""Status"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    NEW.""Id"", NEW.""Fio"", NEW.""Gender"", NEW.""BirthDate"", NEW.""Contacts"", 
                    NEW.""Email"", NEW.""Password"", NEW.""Position"", NEW.""Salary"", NEW.""Status"", 
                    j_id, 'UPDATE'
                );
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            -- Функция для DELETE
            CREATE OR REPLACE FUNCTION tr_Employers_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Employers', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Employers_Arch"" (
                    ""Id"", ""Fio"", ""Gender"", ""BirthDate"", ""Contacts"", 
                    ""Email"", ""Password"", ""Position"", ""Salary"", ""Status"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    OLD.""Id"", OLD.""Fio"", OLD.""Gender"", OLD.""BirthDate"", OLD.""Contacts"", 
                    OLD.""Email"", OLD.""Password"", OLD.""Position"", OLD.""Salary"", OLD.""Status"", 
                    j_id, 'DELETE'
                );
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            -- Создание триггеров
            DROP TRIGGER IF EXISTS trigger_Employers_insert ON ""Employers"";
            CREATE TRIGGER trigger_Employers_insert 
                AFTER INSERT ON ""Employers"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Employers_insert();

            DROP TRIGGER IF EXISTS trigger_Employers_update ON ""Employers"";
            CREATE TRIGGER trigger_Employers_update 
                AFTER UPDATE ON ""Employers"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Employers_update();

            DROP TRIGGER IF EXISTS trigger_Employers_delete ON ""Employers"";
            CREATE TRIGGER trigger_Employers_delete 
                BEFORE DELETE ON ""Employers"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Employers_delete();


            -- Триггеры для таблицы Rehearsals
            -- Архивная таблица
            CREATE TABLE IF NOT EXISTS ""Rehearsals_Arch"" (LIKE ""Rehearsals"" INCLUDING ALL);
            ALTER TABLE ""Rehearsals_Arch"" ADD COLUMN IF NOT EXISTS ""JournalId"" INT;
            ALTER TABLE ""Rehearsals_Arch"" ADD COLUMN IF NOT EXISTS ""OperationType"" VARCHAR(10);
            ALTER TABLE ""Rehearsals_Arch"" ADD COLUMN IF NOT EXISTS ""OperationTime"" TIMESTAMP DEFAULT NOW();

            -- Функция для INSERT
            CREATE OR REPLACE FUNCTION tr_Rehearsals_insert()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Rehearsals', 'INSERT', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Rehearsals_Arch"" (
                    ""Id"", ""Datetime"", ""Location"", ""Description"", ""Status"", ""PlayId"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    NEW.""Id"", NEW.""Datetime"", NEW.""Location"", NEW.""Description"", NEW.""Status"", NEW.""PlayId"", 
                    j_id, 'INSERT'
                );
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            -- Функция для UPDATE
            CREATE OR REPLACE FUNCTION tr_Rehearsals_update()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Rehearsals', 'UPDATE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Rehearsals_Arch"" (
                    ""Id"", ""Datetime"", ""Location"", ""Description"", ""Status"", ""PlayId"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    NEW.""Id"", NEW.""Datetime"", NEW.""Location"", NEW.""Description"", NEW.""Status"", NEW.""PlayId"", 
                    j_id, 'UPDATE'
                );
                RETURN NEW;
            END; $$ LANGUAGE plpgsql;

            -- Функция для DELETE
            CREATE OR REPLACE FUNCTION tr_Rehearsals_delete()
            RETURNS TRIGGER AS $$
            DECLARE j_id INT;
            BEGIN
                INSERT INTO ""Journal"" (""TableName"", ""Operation"", ""OperationTime"", ""UserName"")
                VALUES ('Rehearsals', 'DELETE', NOW(), CURRENT_USER) RETURNING ""Id"" INTO j_id;
    
                INSERT INTO ""Rehearsals_Arch"" (
                    ""Id"", ""Datetime"", ""Location"", ""Description"", ""Status"", ""PlayId"", 
                    ""JournalId"", ""OperationType""
                ) VALUES (
                    OLD.""Id"", OLD.""Datetime"", OLD.""Location"", OLD.""Description"", OLD.""Status"", OLD.""PlayId"", 
                    j_id, 'DELETE'
                );
                RETURN OLD;
            END; $$ LANGUAGE plpgsql;

            -- Создание триггеров
            DROP TRIGGER IF EXISTS trigger_Rehearsals_insert ON ""Rehearsals"";
            CREATE TRIGGER trigger_Rehearsals_insert 
                AFTER INSERT ON ""Rehearsals"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Rehearsals_insert();

            DROP TRIGGER IF EXISTS trigger_Rehearsals_update ON ""Rehearsals"";
            CREATE TRIGGER trigger_Rehearsals_update 
                AFTER UPDATE ON ""Rehearsals"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Rehearsals_update();

            DROP TRIGGER IF EXISTS trigger_Rehearsals_delete ON ""Rehearsals"";
            CREATE TRIGGER trigger_Rehearsals_delete 
                BEFORE DELETE ON ""Rehearsals"" 
                FOR EACH ROW 
                EXECUTE FUNCTION tr_Rehearsals_delete();
        ";



        using var createTablesCmd = new NpgsqlCommand(createTablesSql, connection);
        createTablesCmd.ExecuteNonQuery();

        Console.WriteLine("Все таблицы созданы");
    }

    /// <summary>
    /// Полная перезагрузка: удаляет БД, создаёт заново и создаёт таблицы
    /// </summary>
    public void FullRecreate()
    {
        DropDatabaseIfExists();   // Удаляем БД если есть
        CreateDatabase();         // Создаём новую БД
        CreateAllTables();        // Создаём таблицы в новой БД
        Console.WriteLine("База данных полностью пересоздана!");
    }
}
// проверка таблиц через powershell
// docker exec -it irbistheatre_db psql -U admin -d IrbisTheatre -c "\dt"