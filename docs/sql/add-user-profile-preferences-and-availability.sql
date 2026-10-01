-- Schema update: user profile preferences + weekly availability
-- Applies the changes from AppUserConfiguration / UserAvailabilityConfiguration to the hosted database.
-- Idempotent: safe to run multiple times.

IF COL_LENGTH('dbo.AspNetUsers', 'Title') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD Title NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.AspNetUsers', 'Bio') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD Bio NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.AspNetUsers', 'City') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD City NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.AspNetUsers', 'Country') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD Country NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.AspNetUsers', 'TimeZone') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD TimeZone NVARCHAR(64) NULL;
GO
IF COL_LENGTH('dbo.AspNetUsers', 'OpenForInstantSwaps') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD OpenForInstantSwaps BIT NOT NULL CONSTRAINT DF_AspNetUsers_OpenForInstantSwaps DEFAULT(1);
GO
IF COL_LENGTH('dbo.AspNetUsers', 'OnlineOnly') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD OnlineOnly BIT NOT NULL CONSTRAINT DF_AspNetUsers_OnlineOnly DEFAULT(0);
GO
IF COL_LENGTH('dbo.AspNetUsers', 'AutoMatchBarterRequests') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD AutoMatchBarterRequests BIT NOT NULL CONSTRAINT DF_AspNetUsers_AutoMatchBarterRequests DEFAULT(0);
GO

IF OBJECT_ID(N'dbo.UserAvailability', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserAvailability
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserAvailability PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        DayOfWeek INT NOT NULL,
        TimeBlock INT NOT NULL,
        CONSTRAINT FK_UserAvailability_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_UserAvailability_UserId_DayOfWeek_TimeBlock
        ON dbo.UserAvailability (UserId, DayOfWeek, TimeBlock);
END
GO
