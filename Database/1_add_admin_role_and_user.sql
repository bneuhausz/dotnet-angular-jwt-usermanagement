DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @Now DATETIME2 = SYSDATETIME();

INSERT INTO Roles (Id, Name, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @AdminRoleId,
    'Admin',
    0,
    @Now,
    @AdminUserId
);

INSERT INTO Users (Id, UserName, Email, PasswordHash, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @AdminUserId,
    'admin',
    'admin@example.com',
    'AQAAAAIAAYagAAAAEDANxtfMKLqXjS1h16o9b3bhnUOCJiOFgZlPxxUZA3IAWDHtRC6gGWwFRq2hUsEDVA==', --admin
    0,
    @Now,
    @AdminUserId
);

INSERT INTO UserRoles (Id, UserId, RoleId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminUserId,
    @AdminRoleId,
    0,
    @Now,
    @AdminUserId
);