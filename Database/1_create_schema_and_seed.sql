USE AuthDemo;
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy INT NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy INT NULL
);

ALTER TABLE Users ADD CONSTRAINT UQ_Users_UserName UNIQUE (UserName);
ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);

ALTER TABLE Users
ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy INT NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy INT NULL
);

ALTER TABLE Roles ADD CONSTRAINT UQ_Roles_Name UNIQUE (Name);

ALTER TABLE Roles
ADD CONSTRAINT FK_Roles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roles_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Permissions (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ParentPermissionId INT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('Menu', 'Action')),

    CONSTRAINT FK_Permissions_Parent FOREIGN KEY (ParentPermissionId) REFERENCES Permissions(Id)
);

ALTER TABLE Permissions ADD CONSTRAINT UQ_Permissions_Name UNIQUE (Name);
GO

CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);
GO

DECLARE @AdminUserId INT;
DECLARE @AdminRoleId INT;
DECLARE @Now DATETIME2 = SYSDATETIME();

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, UserName, Email, PasswordHash, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    1,
    'admin',
    'admin@example.com',
    'AQAAAAIAAYagAAAAEDANxtfMKLqXjS1h16o9b3bhnUOCJiOFgZlPxxUZA3IAWDHtRC6gGWwFRq2hUsEDVA==',
    0,
    @Now,
    1
);
SET IDENTITY_INSERT Users OFF;
SET @AdminUserId = 1;

INSERT INTO Roles (Name, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    'Admin',
    0,
    @Now,
    @AdminUserId
);
SET @AdminRoleId = SCOPE_IDENTITY();

INSERT INTO UserRoles (UserId, RoleId)
VALUES (
    @AdminUserId,
    @AdminRoleId
);
GO

DECLARE @Now DATETIME2 = SYSDATETIME();
DECLARE @AdminUserId INT = (SELECT TOP 1 Id FROM Users WHERE UserName = 'admin');
DECLARE @AdminRoleId INT = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin');

DECLARE @MaintenancePermissionId INT;
INSERT INTO Permissions (Name, Type, ParentPermissionId)
VALUES (
    'Maintenance',
    'Menu',
    NULL
);
SET @MaintenancePermissionId = SCOPE_IDENTITY();

INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (
    @AdminRoleId,
    @MaintenancePermissionId
);

DECLARE @UsersMenuPermissionId INT;
INSERT INTO Permissions (Name, Type, ParentPermissionId)
VALUES (
    'Users',
    'Menu',
    @MaintenancePermissionId
);
SET @UsersMenuPermissionId = SCOPE_IDENTITY();

INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (
    @AdminRoleId,
    @UsersMenuPermissionId
);

DECLARE @RolesMenuPermissionId INT;
INSERT INTO Permissions (Name, Type, ParentPermissionId)
VALUES (
    'Roles',
    'Menu',
    @MaintenancePermissionId
);
SET @RolesMenuPermissionId = SCOPE_IDENTITY();

INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (
    @AdminRoleId,
    @RolesMenuPermissionId
);

DECLARE @MaintainUsersPermissionId INT;
INSERT INTO Permissions (Name, Type, ParentPermissionId)
VALUES (
    'MaintainUsers',
    'Action',
    @UsersMenuPermissionId
);
SET @MaintainUsersPermissionId = SCOPE_IDENTITY();

INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (
    @AdminRoleId,
    @MaintainUsersPermissionId
);

DECLARE @MaintainRolesPermissionId INT;
INSERT INTO Permissions (Name, Type, ParentPermissionId)
VALUES (
    'MaintainRoles',
    'Action',
    @RolesMenuPermissionId
);
SET @MaintainRolesPermissionId = SCOPE_IDENTITY();

INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (
    @AdminRoleId,
    @MaintainRolesPermissionId
);
GO

CREATE TABLE UserRefreshTokens (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    ReplacedByToken NVARCHAR(MAX) NULL,

    CONSTRAINT FK_UserRefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_UserRefreshTokens_UserId ON UserRefreshTokens(UserId);
GO

CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    InsertedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EntityType NVARCHAR(255) NULL,
    [TableName] NVARCHAR(255) NULL,
    PrimaryKey NVARCHAR(MAX) NULL,
    Action NVARCHAR(50) NULL,
    UserId INT NULL,
    Changes NVARCHAR(MAX) NULL,
    TraceId NVARCHAR(100) NULL,
    TransactionId NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_AuditLogs_EntityType_PrimaryKey ON AuditLogs (EntityType, PrimaryKey);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs (UserId);
CREATE INDEX IX_AuditLogs_InsertedDate ON AuditLogs (InsertedDate);
GO