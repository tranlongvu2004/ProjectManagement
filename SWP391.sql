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
    RoleName NVARCHAR(50) NOT NULL  -- Mentor / InternLead / Intern
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

-- 3. UserProject (member of a project)
CREATE TABLE UserProject (
    UserProjectId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProjectId INT NOT NULL,
    IsLeader BIT DEFAULT 0,
    JoinedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


/* ================================
   B. PROJECT MANAGEMENT
=================================*/

-- 4. Projects
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

-- Update FK for UserProject → ProjectId
ALTER TABLE UserProject
ADD CONSTRAINT FK_UserProject_Project
FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId);


-- 5. Tasks
CREATE TABLE Tasks (
    TaskId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Priority NVARCHAR(20) DEFAULT 'Medium', -- Low / Medium / High
    Status NVARCHAR(20) DEFAULT 'ToDo',    -- ToDo / InProgress / Done
    ProgressPercent INT DEFAULT 0,
    Deadline DATETIME,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);


-- 6. TaskAssignment (task ↔ multiple users)
CREATE TABLE TaskAssignment (
    TaskAssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
/* ================================
   C. COMMUNICATION
=================================*/

-- 8. Comments (discussion inside task)
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

-- 10. Progress Reports (daily/weekly)
CREATE TABLE Reports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    LeaderId INT NOT NULL,
    ReportType NVARCHAR(20),    -- daily / weekly
    FilePath NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    FOREIGN KEY (LeaderId) REFERENCES Users(UserId)
);
/* ================================
   E. SEED ROLES (optional)
=================================*/

INSERT INTO Roles (RoleName) VALUES
('Mentor'),
('InternLead'),
('Intern');

INSERT INTO Users(FullName, Email, PasswordHash, RoleId)
VALUES
('Nguyen Thanh Mentor', 'mentor@example.com', '123', 1),
('Tran Van Leader', 'leader@example.com', '123', 2),
('Intern A', 'internA@example.com', '123', 3),
('Intern B', 'internB@example.com', '123', 3),
('Intern C', 'internC@example.com', '123', 3);

INSERT INTO Projects (ProjectName, Description, Deadline, CreatedBy)
VALUES
('Lab Management System', 'System for managing lab tasks', '2025-03-30', 1),
('Intern Evaluation Tool', 'Evaluation tool for interns', '2025-04-15', 2);

-- Project 1
INSERT INTO UserProject (UserId, ProjectId, IsLeader)
VALUES
(1, 1, 0),  -- Mentor
(2, 1, 1),  -- Leader
(3, 1, 0),  -- Intern A
(4, 1, 0);  -- Intern B

-- Project 2
INSERT INTO UserProject (UserId, ProjectId, IsLeader)
VALUES
(2, 2, 1),  -- Leader
(5, 2, 0),  -- Intern C
(3, 2, 0);  -- Intern A

INSERT INTO Tasks (ProjectId, Title, Priority, Status, Deadline, CreatedBy)
VALUES
(1, 'Design database schema', 'High', 'InProgress', '2025-02-10', 2),
(1, 'API development', 'Medium', 'ToDo', '2025-02-20', 3),
(1, 'Frontend UI', 'Low', 'ToDo', '2025-02-28', 4),

(2, 'Define evaluation criteria', 'High', 'InProgress', '2025-03-01', 2),
(2, 'Build report module', 'Medium', 'ToDo', '2025-03-10', 5);

INSERT INTO TaskAssignment (TaskId, UserId)
VALUES
(1, 2),  -- Leader làm task 1
(2, 3),  -- Intern A
(3, 4),  -- Intern B
(4, 2),  -- Leader
(5, 5);  -- Intern C

INSERT INTO Comments (TaskId, UserId, Content)
VALUES
(1, 3, 'Hoàn thành bản nháp ERD'),
(1, 2, 'OK, thêm bảng phân quyền'),
(2, 3, 'Đang viết API'),
(3, 4, 'UI cần chốt màu chủ đạo');

INSERT INTO Reports (ProjectId, LeaderId, ReportType, FilePath)
VALUES
(1, 2, 'weekly', '/reports/week1_project1.pdf'),
(2, 2, 'daily', '/reports/day1_project2.pdf');



