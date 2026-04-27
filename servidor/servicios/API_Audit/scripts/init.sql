

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AuditDB')
BEGIN
    CREATE DATABASE AuditDB;
END
GO

USE AuditDB;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AuditLogs' AND xtype = 'U')
BEGIN
    CREATE TABLE AuditLogs (
        Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EntityType   NVARCHAR(100)    NOT NULL,
        EntityId     NVARCHAR(100)    NOT NULL,
        Action       NVARCHAR(20)     NOT NULL,
        PerformedBy  NVARCHAR(200)    NOT NULL,
        Timestamp    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        Payload      NVARCHAR(MAX)    NULL,
        IpAddress    NVARCHAR(45)     NULL,
        Status       NVARCHAR(20)     NOT NULL DEFAULT 'Success',
        ErrorMessage NVARCHAR(500)    NULL
    );

    CREATE INDEX IX_AuditLogs_Entity    ON AuditLogs (EntityType, EntityId);
    CREATE INDEX IX_AuditLogs_User      ON AuditLogs (PerformedBy);
    CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs (Timestamp);

    PRINT 'Tabla AuditLogs creada correctamente.';
END
GO
