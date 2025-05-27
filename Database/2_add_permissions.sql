DECLARE @Now DATETIME2 = SYSDATETIME();

-- Get existing Admin user and role IDs
DECLARE @AdminUserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE UserName = 'admin');
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin');

-- Step 1: Maintenance (Menu)
DECLARE @MaintenancePermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @MaintenancePermissionId,
    'Maintenance',
    'Menu',
    NULL,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @MaintenancePermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 2: Users (Menu)
DECLARE @UsersMenuPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @UsersMenuPermissionId,
    'Users',
    'Menu',
    @MaintenancePermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @UsersMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 3: Roles (Menu)
DECLARE @RolesMenuPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @RolesMenuPermissionId,
    'Roles',
    'Menu',
    @MaintenancePermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @RolesMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 4: ReadUsers (Action)
DECLARE @ReadUsersPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @ReadUsersPermissionId,
    'ReadUsers',
    'Action',
    @UsersMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @ReadUsersPermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 5: MaintainUsers (Action)
DECLARE @MaintainUsersPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @MaintainUsersPermissionId,
    'MaintainUsers',
    'Action',
    @UsersMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @MaintainUsersPermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 6: ReadRoles (Action)
DECLARE @ReadRolesPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @ReadRolesPermissionId,
    'ReadRoles',
    'Action',
    @RolesMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @ReadRolesPermissionId,
    0,
    @Now,
    @AdminUserId
);

-- Step 7: MaintainRoles (Action)
DECLARE @MaintainRolesPermissionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Permissions (Id, Name, Type, ParentPermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    @MaintainRolesPermissionId,
    'MaintainRoles',
    'Action',
    @RolesMenuPermissionId,
    0,
    @Now,
    @AdminUserId
);

INSERT INTO RolePermissions (Id, RoleId, PermissionId, IsDeleted, CreatedAt, CreatedBy)
VALUES (
    NEWID(),
    @AdminRoleId,
    @MaintainRolesPermissionId,
    0,
    @Now,
    @AdminUserId
);
