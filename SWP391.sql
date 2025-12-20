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

INSERT INTO Projects(ProjectId, ProjectName, Description, Deadline, Status, CreatedBy, CreatedAt, UpdatedAt)
VALUES
(1, 'Lab Management System', 'Project about manage lab', '2026-01-20 00:00:00.000', 'InProgress', 1, '2025-12-20 21:43:40.700', '2025-12-20 21:43:40.700')

/* ================================
   TASKS
================================ */
-- Parent tasks
INSERT INTO Tasks(ParentId, IsParent, ProjectId, Title, Description, Priority, Status, ProgressPercent, Deadline, CreatedBy, CreatedAt, UpdatedAt)
VALUES
(NULL, 1, 1, 'Setup Database', 'Design database schema for the Lab Management System', 'High', 'ToDo', 100, '2025-12-25', 1, GETDATE(), GETDATE()),
(NULL, 1, 1, 'Develop Backend APIs', 'Create API endpoints for managing labs, projects, and tasks', 'Medium', 'Doing', 100, '2026-01-05', 1, GETDATE(), GETDATE()),
(NULL, 1, 1, 'Frontend UI', 'Design and implement user interface', 'Medium', 'ToDo', 100, '2026-01-10', 1, GETDATE(), GETDATE());

-- Subtasks for 'Setup Database'
INSERT INTO Tasks(ParentId, IsParent, ProjectId, Title, Description, Priority, Status, ProgressPercent, Deadline, CreatedBy, CreatedAt, UpdatedAt)
VALUES
(1, 0, 1, 'Create Users Table', 'Implement Users table with roles', 'Medium', 'Completed', 100, '2025-12-21', 2, GETDATE(), GETDATE()),
(1, 0, 1, 'Create Projects Table', 'Implement Projects table', 'Medium', 'Completed', 100, '2025-12-21', 2, GETDATE(), GETDATE());

-- Subtasks for 'Develop Backend APIs'
INSERT INTO Tasks(ParentId, IsParent, ProjectId, Title, Description, Priority, Status, ProgressPercent, Deadline, CreatedBy, CreatedAt, UpdatedAt)
VALUES
(2, 0, 1, 'Auth & Roles API', 'Create login, register and role management APIs', 'High', 'Doing', 100, '2025-12-30', 3, GETDATE(), GETDATE()),
(2, 0, 1, 'Task Management API', 'APIs for creating, updating, deleting tasks', 'Medium', 'ToDo', 100, '2026-01-03', 3, GETDATE(), GETDATE());

/* ================================
   TASK ASSIGNMENTS
================================ */
INSERT INTO TaskAssignment(TaskId, UserId, AssignedAt)
VALUES
(1, 2, GETDATE()),
(2, 3, GETDATE()),
(3, 4, GETDATE()),
(4, 2, GETDATE()),
(5, 3, GETDATE());

/* ================================
   TASK ATTACHMENTS
================================ */
INSERT INTO TaskAttachments(TaskId, FileName, FilePath, FileType, FileSize, UploadedBy)
VALUES
(1, 'DatabaseSchema.pdf', '/uploads/DatabaseSchema.pdf', 'application/pdf', 102400, 2),
(4, 'AuthAPI.docx', '/uploads/AuthAPI.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 51200, 3);

/* ================================
   COMMENTS
================================ */
INSERT INTO Comments(TaskId, UserId, Content)
VALUES
(1, 2, 'Database schema looks good. Ready for review.'),
(4, 3, 'Authentication API is partially implemented. Need testing.'),
(5, 3, 'Task management endpoints still need error handling.');

/* ================================
   ACTIVITY LOG
================================ */
INSERT INTO ActivityLog(UserId, TargetUserId, ProjectId, TaskId, ActionType, Message)
VALUES
(2, NULL, 1, 1, 'Update', 'Marked Setup Database as Completed'),
(3, NULL, 1, 4, 'Create', 'Created Auth API task');

