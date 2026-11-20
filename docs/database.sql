/*
    RaceDay Database
    PROG6212 Programming 2B
    Part 1 - SQL Database Script
*/

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/*
    Drop tables in dependency order.
    This allows the script to be rerun during development.
*/

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL
    DROP TABLE dbo.Results;

IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrolments;

IF OBJECT_ID('dbo.EventCategories', 'U') IS NOT NULL
    DROP TABLE dbo.EventCategories;

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;

IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO


/*
    USERS
*/

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserId),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


/*
    EVENTS
*/

CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL
    CONSTRAINT CK_Events_EventType
    CHECK (EventType IN ('Run', 'Walk', 'Cycle')),
    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Events_Status DEFAULT 'Planned',
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventId),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT CK_Events_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Events_Status
        CHECK (Status IN
        (
            'Planned',
            'Open',
            'Closed',
            'Completed'
        ))
);
GO


/*
    CATEGORIES
*/

CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryId),

    CONSTRAINT UQ_Categories_Name
        UNIQUE (CategoryName)
);
GO


/*
    EVENT CATEGORIES
*/

CREATE TABLE dbo.EventCategories
(
    EventCategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaximumParticipants INT NOT NULL,

    CONSTRAINT PK_EventCategories
        PRIMARY KEY (EventCategoryId),

    CONSTRAINT FK_EventCategories_Event
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT FK_EventCategories_Category
        FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(CategoryId),

    CONSTRAINT UQ_EventCategories_Event_Category
        UNIQUE (EventId, CategoryId),

    CONSTRAINT CK_EventCategories_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_EventCategories_MaxParticipants
        CHECK (MaximumParticipants > 0)
);
GO


/*
    ENROLMENTS
*/

CREATE TABLE dbo.Enrolments
(
    EnrolmentId INT IDENTITY(1,1) NOT NULL,
    ParticipantId INT NOT NULL,
    EventCategoryId INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolments_Status DEFAULT 'Registered',

    CONSTRAINT PK_Enrolments
        PRIMARY KEY (EnrolmentId),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT FK_Enrolments_EventCategory
        FOREIGN KEY (EventCategoryId)
        REFERENCES dbo.EventCategories(EventCategoryId),

    CONSTRAINT UQ_Enrolments_Participant_EventCategory
        UNIQUE (ParticipantId, EventCategoryId),

    CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN
        (
            'Registered',
            'Cancelled',
            'Completed'
        ))
);
GO


/*
    RESULTS
*/

CREATE TABLE dbo.Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrolmentId INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,
    RecordedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Results_RecordedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultId),

    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments(EnrolmentId),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentId),

    CONSTRAINT CK_Results_Position
        CHECK (Position > 0)
);
GO

/*
    SEED USERS
*/

INSERT INTO dbo.Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role,
    PhoneNumber
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.organiser@raceday.co.za',
    'HASHED_PASSWORD_PLACEHOLDER_1',
    'Organiser',
    '0821111111'
),
(
    'Aisha',
    'Naidoo',
    'aisha.organiser@raceday.co.za',
    'HASHED_PASSWORD_PLACEHOLDER_2',
    'Organiser',
    '0822222222'
),
(
    'Yusuf',
    'Ahmed',
    'yusuf.participant@raceday.co.za',
    'HASHED_PASSWORD_PLACEHOLDER_3',
    'Participant',
    '0823333333'
),
(
    'Lerato',
    'Dlamini',
    'lerato.participant@raceday.co.za',
    'HASHED_PASSWORD_PLACEHOLDER_4',
    'Participant',
    '0824444444'
);
GO



/*
    SEED EVENTS
*/

INSERT INTO dbo.Events
(
    OrganiserId,
    EventName,
    Description,
    EventDate,
    StartTime,
    Location,
    DistanceKm,
    EventType,
    Status
)
VALUES
(
    1,
    'Johannesburg Spring Run',
    'Community road running event in Johannesburg.',
    '2027-09-12',
    '07:00',
    'Johannesburg',
    10.00,
    'Run',
    'Open'
),
(
    1,
    'Pretoria Charity Walk',
    'Charity walking event supporting local community organisations.',
    '2027-10-03',
    '08:00',
    'Pretoria',
    8.00,
    'Walk',
    'Open'
),
(
    2,
    'Cape Town Coastal Cycle',
    'Road cycling event along the Cape Town coastline.',
    '2027-11-21',
    '06:30',
    'Cape Town',
    42.00,
    'Cycle',
    'Planned'
);
GO



/*
    ASSIGN CATEGORIES TO EVENTS
*/

INSERT INTO dbo.EventCategories
(
    EventId,
    CategoryId,
    EntryFee,
    MaximumParticipants
)
VALUES
(1, 1, 150.00, 1000),
(1, 2, 100.00, 300),
(1, 3, 120.00, 300),

(2, 4, 80.00, 500),
(2, 1, 80.00, 500),

(3, 5, 350.00, 750),
(3, 1, 350.00, 500);
GO


/*
    SAMPLE ENROLMENTS
*/

INSERT INTO dbo.Enrolments
(
    ParticipantId,
    EventCategoryId,
    Status
)
VALUES
(
    3,
    1,
    'Completed'
),
(
    4,
    2,
    'Registered'
),
(
    3,
    4,
    'Registered'
),
(
    4,
    6,
    'Registered'
);
GO


/*
    SAMPLE RESULT
*/

INSERT INTO dbo.Results
(
    EnrolmentId,
    FinishTime,
    Position
)
VALUES
(
    1,
    '00:52:31',
    42
);
GO

/*
    VALIDATION QUERIES
*/

SELECT *
FROM dbo.Users;

SELECT *
FROM dbo.Events;

SELECT *
FROM dbo.Categories;

SELECT *
FROM dbo.EventCategories;

SELECT *
FROM dbo.Enrolments;

SELECT *
FROM dbo.Results;


/*
    COUNT VALIDATION
*/

SELECT
    'Users' AS Entity,
    COUNT(*) AS RecordCount
FROM dbo.Users

UNION ALL

SELECT
    'Events',
    COUNT(*)
FROM dbo.Events

UNION ALL

SELECT
    'Categories',
    COUNT(*)
FROM dbo.Categories

UNION ALL

SELECT
    'EventCategories',
    COUNT(*)
FROM dbo.EventCategories

UNION ALL

SELECT
    'Enrolments',
    COUNT(*)
FROM dbo.Enrolments

UNION ALL

SELECT
    'Results',
    COUNT(*)
FROM dbo.Results;
GO