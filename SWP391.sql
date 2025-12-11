IF EXISTS (SELECT name FROM sys.databases WHERE name = 'LabProjectManagement')
BEGIN
    ALTER DATABASE LabProjectManagement SET MULTI_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LabProjectManagement;
END
GO
CREATE DATABASE LabProjectManagement;
GO
USE LabProjectManagement;
GO


/* ================================
   A. ROLES & USERS
=================================*/

-- 1. Roles
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL   -- Mentor / Intern
);

-- 2. Users
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleId INT NOT NULL,
    AvatarUrl NVARCHAR(MAX),
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);


/* ================================
   B. PROJECT MANAGEMENT
=================================*/

-- 3. Projects
CREATE TABLE Projects (
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Deadline DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT 'In Progress',
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

-- 4. UserProject
CREATE TABLE UserProject (
    UserProjectId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProjectId INT NOT NULL,
    IsLeader BIT DEFAULT 0,
    JoinedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
);


-- 5. Tasks (có IsParent + ParentId)
CREATE TABLE Tasks (
    TaskId INT IDENTITY(1,1) PRIMARY KEY,
    ParentId INT NULL,
    IsParent BIT DEFAULT 0,                        -- NEW
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
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    FOREIGN KEY (ParentId) REFERENCES Tasks(TaskId)
);


-- 6. TaskAssignment
CREATE TABLE TaskAssignment (
    TaskAssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


-- 7. TaskAttachments
CREATE TABLE TaskAttachments (
    AttachmentId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    FileName NVARCHAR(255),
    FilePath NVARCHAR(MAX),
    FileType NVARCHAR(50),
    FileSize BIGINT,
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UploadedBy) REFERENCES Users(UserId)
);


/* ================================
   C. COMMUNICATION
=================================*/

CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


/* ================================
   D. REPORTS
=================================*/

CREATE TABLE Reports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    LeaderId INT NOT NULL,
    ReportType NVARCHAR(20),
    FilePath NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (LeaderId) REFERENCES Users(UserId)
);


/* ================================
   E. SEED DATA
=================================*/

-- Roles
INSERT INTO Roles (RoleName) VALUES
('Mentor'),
('Intern');

-- Users
INSERT INTO Users(FullName, Email, PasswordHash, RoleId)
VALUES
('Nguyen Thanh Mentor', 'mentor@example.com', '123', 1),
('Tran Van Intern 1', 'intern1@example.com', '123', 2),
('Tran Van Intern 2', 'intern2@example.com', '123', 2),
('Tran Van Intern 3', 'intern3@example.com', '123', 2);


-- Projects
INSERT INTO Projects (ProjectName, Description, Deadline, CreatedBy)
VALUES
('Lab Management System', 'System for managing lab tasks', '2025-03-30', 1),
('Evaluation Tool', 'Evaluation tool for interns', '2025-04-10', 1);

-- Members
INSERT INTO UserProject (UserId, ProjectId, IsLeader)
VALUES
(1, 1, 1),
(2, 1, 0),
(3, 1, 0),

(1, 2, 1),
(4, 2, 0);


-- Tasks (Parents)
INSERT INTO Tasks (ProjectId, Title, IsParent, Priority, Status, Deadline, CreatedBy)
VALUES
(1, 'Design Database', 1, 'High', 'InProgress', '2025-02-10', 1),    -- TaskId = 1
(1, 'Develop API', 1, 'Medium', 'ToDo', '2025-02-20', 2),             -- TaskId = 2
(2, 'Define Evaluation Criteria', 1, 'High', 'InProgress', '2025-03-01', 1); -- TaskId = 3


-- SubTasks
INSERT INTO Tasks (ProjectId, ParentId, Title, IsParent, Status, CreatedBy)
VALUES
(1, 1, 'Create ERD', 0, 'InProgress', 2),          -- TaskId = 4
(1, 1, 'Review ERD with mentor', 0, 'ToDo', 1),    -- TaskId = 5
(1, 2, 'Build authentication API', 0, 'ToDo', 3);  -- TaskId = 6


-- TaskAssignments
INSERT INTO TaskAssignment (TaskId, UserId)
VALUES
(1, 1),
(2, 2),
(3, 1),
(4, 2),
(5, 1),
(6, 3);


-- Comments
INSERT INTO Comments (TaskId, UserId, Content)
VALUES
(1, 2, 'Database đang thiết kế'),
(4, 2, 'ERD bản nháp đã hoàn thành'),
(6, 3, 'Đang code Auth API');


-- Attachments
INSERT INTO TaskAttachments (TaskId, FileName, FilePath, FileType, FileSize, UploadedBy)
VALUES
(4, 'erd_draft.png', '/uploads/erd_draft.png', 'image', 205000, 2),
(1, 'db_schema.pdf', '/uploads/db_schema.pdf', 'pdf', 1034000, 1);


-- Reports
INSERT INTO Reports (ProjectId, LeaderId, ReportType, FilePath)
VALUES
(1, 1, 'weekly', '/reports/project1_week1.pdf'),
(2, 1, 'daily', '/reports/project2_day1.pdf');
