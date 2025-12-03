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
CREATE TABLE ProgressReports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    LeaderId INT NOT NULL,
    ReportType NVARCHAR(20),    -- daily / weekly
    Summary NVARCHAR(MAX),
    Blockers NVARCHAR(MAX),
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
