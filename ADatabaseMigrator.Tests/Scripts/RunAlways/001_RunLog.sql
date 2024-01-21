IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RunLog' and xtype='U')
    CREATE TABLE RunLog (
        Id INT IDENTITY NOT NULL PRIMARY KEY,
        Timestamp DATETIME NOT NULL
    )
GO