/* =====================================================
   RESET DATABASE
===================================================== */
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LabProjectManagement')
BEGIN
    ALTER DATABASE LabProjectManagement SET MULTI_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LabProjectManagement;
END
GO

CREATE DATABASE LabProjectManagement;
GO
USE LabProjectManagement;
GO


/* =====================================================
   A. ROLES & USERS
===================================================== */

CREATE TABLE Roles (
    RoleId INT IDENTITY PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

CREATE TABLE Users (
    UserId INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleId INT NOT NULL,
    AvatarUrl NVARCHAR(MAX),
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_User_Role
        FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);


/* =====================================================
   B. PROJECT MANAGEMENT
===================================================== */

CREATE TABLE Projects (
    ProjectId INT IDENTITY PRIMARY KEY,
    ProjectName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Deadline DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT 'In Progress',
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Project_Creator
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE TABLE UserProject (
    UserProjectId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    ProjectId INT NOT NULL,
    IsLeader BIT DEFAULT 0,
    JoinedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_UserProject_User
        FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT FK_UserProject_Project
        FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
);


/* =====================================================
   C. TASKS
===================================================== */

CREATE TABLE Tasks (
    TaskId INT IDENTITY PRIMARY KEY,
    ParentId INT NULL,
    IsParent BIT DEFAULT 0,

    ProjectId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),

    Priority NVARCHAR(20) DEFAULT 'Medium',
    Status NVARCHAR(20) DEFAULT 'ToDo',
    ProgressPercent INT DEFAULT 0,

    Deadline DATETIME,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Task_Project
        FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),

    CONSTRAINT FK_Task_Creator
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),

    CONSTRAINT FK_Task_Parent
        FOREIGN KEY (ParentId) REFERENCES Tasks(TaskId)
);

CREATE TABLE TaskAssignment (
    TaskAssignmentId INT IDENTITY PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    DeadlineMailSent BIT DEFAULT 0,

    CONSTRAINT FK_TaskAssignment_Task
        FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),

    CONSTRAINT FK_TaskAssignment_User
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE TaskAttachments (
    AttachmentId INT IDENTITY PRIMARY KEY,
    TaskId INT NOT NULL,
    FileName NVARCHAR(255),
    FilePath NVARCHAR(MAX),
    FileType NVARCHAR(255),
    FileSize BIGINT,
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Attachment_Task
        FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),

    CONSTRAINT FK_Attachment_User
        FOREIGN KEY (UploadedBy) REFERENCES Users(UserId)
);


/* =====================================================
   D. COMMUNICATION & LOGGING
===================================================== */

CREATE TABLE Comments (
    CommentId INT IDENTITY PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Comment_Task
        FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),

    CONSTRAINT FK_Comment_User
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE ActivityLog (
    ActivityLogId INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,
    TargetUserId INT NULL,
    ProjectId INT NOT NULL,
    TaskId INT NULL,

    ActionType NVARCHAR(50) NOT NULL,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    Message NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (TargetUserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId)
);

CREATE TABLE TaskHistory (
    TaskHistoryId INT IDENTITY PRIMARY KEY,
    TaskId INT NULL,
    UserId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE RecycleBin (
    RecycleId INT IDENTITY PRIMARY KEY,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,
    DataSnapshot NVARCHAR(MAX) NOT NULL,
    DeletedBy INT NULL,
    DeletedAt DATETIME DEFAULT GETDATE()
);


/* =====================================================
   E. REPORTS
===================================================== */

CREATE TABLE Reports (
    ReportId INT IDENTITY PRIMARY KEY,
    ProjectId INT NOT NULL,
    LeaderId INT NOT NULL,
    ReportType NVARCHAR(20),
    FilePath NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (LeaderId) REFERENCES Users(UserId)
);


/* =====================================================
   F. SEED DATA
===================================================== */

INSERT INTO Roles (RoleName)
VALUES ('Mentor'), ('Intern');

INSERT INTO Users (FullName, Email, PasswordHash, RoleId)
VALUES
(N'Trần Bình Dương', 'DuongTB@fe.edu.vn', '123', 1),
(N'Trần Long Vũ', 'tlv04102004@gmail.com', '123', 2),
(N'Nghiêm Minh Đức', 'nghiemducls123@gmail.com', '123', 2),
(N'Nguyễn Huy Nghĩa', 'intern3@example.com', '123', 2);