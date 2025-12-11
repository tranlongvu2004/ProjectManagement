using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PorjectManagement.Models;

public partial class LabProjectManagementContext : DbContext
{
    public LabProjectManagementContext()
    {
    }

    public LabProjectManagementContext(DbContextOptions<LabProjectManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAssignment> TaskAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProject> UserProjects { get; set; }

    public virtual DbSet<TaskAttachment> TaskAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA8A7396FB");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Task).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__TaskId__6B24EA82");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__UserId__6C190EBB");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__761ABEF083672835");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.ProjectName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v == ProjectStatus.InProgress ? "InProgress" 
                        : v == ProjectStatus.Completed ? "Completed" 
                        : v == ProjectStatus.Dropped ? "Dropped" 
                        : null,
                    v => v == "InProgress" ? ProjectStatus.InProgress 
                        : v == "Completed" ? ProjectStatus.Completed 
                        : v == "Dropped" ? ProjectStatus.Dropped 
                        : (ProjectStatus?)null
                );
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Projects__Create__59063A47");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48056DBBFF5C");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReportType).HasMaxLength(20);

            entity.HasOne(d => d.Leader).WithMany(p => p.Reports)
                .HasForeignKey(d => d.LeaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reports__LeaderI__70DDC3D8");

            entity.HasOne(d => d.Project).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reports__Project__6FE99F9F");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A6C2E32FD");

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949B1CBF918B7");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Priority)
                .HasConversion(
                    v => v == TaskPriority.Low ? "Low" 
                        : v == TaskPriority.Medium ? "Medium" 
                        : v == TaskPriority.High ? "High" 
                        : v == TaskPriority.Necessary ? "Necessary" 
                        : null,
                    v => v == "Low" ? TaskPriority.Low 
                        : v == "Medium" ? TaskPriority.Medium 
                        : v == "High" ? TaskPriority.High 
                        : v == "Necessary" ? TaskPriority.Necessary 
                        : (TaskPriority?)null
                );
            entity.Property(e => e.ProgressPercent).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v == TaskStatus.ToDo ? "ToDo" 
                        : v == TaskStatus.Doing ? "Doing" 
                        : v == TaskStatus.Completed ? "Completed" 
                        : null,
                    v => v == "ToDo" ? TaskStatus.ToDo 
                        : v == "Doing" ? TaskStatus.Doing 
                        : v == "Completed" ? TaskStatus.Completed 
                        : (TaskStatus?)null
                );
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsParent).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__CreatedBy__628FA481");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__ProjectId__619B8048");

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.SubTasks)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.TaskAssignmentId).HasName("PK__TaskAssi__75E8D23FFD436608");

            entity.ToTable("TaskAssignment");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskAssignments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskAssig__TaskI__66603565");

            entity.HasOne(d => d.User).WithMany(p => p.TaskAssignments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskAssig__UserI__6754599E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CCC2A4186");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349A3C8F9C").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v == UserStatus.Active ? "active" 
                        : v == UserStatus.Inactive ? "inactive" 
                        : v == UserStatus.Dropped ? "dropped" 
                        : null,
                    v => v == "active" ? UserStatus.Active 
                        : v == "inactive" ? UserStatus.Inactive 
                        : v == "dropped" ? UserStatus.Dropped 
                        : (UserStatus?)null
                );

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4E88ABD4");
        });

        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.HasKey(e => e.UserProjectId).HasName("PK__UserProj__5F7DD497B2F9C08F");

            entity.ToTable("UserProject");

            entity.Property(e => e.IsLeader).HasDefaultValue(false);
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Project).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProject_Project");

            entity.HasOne(d => d.User).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserProje__UserI__534D60F1");
        });

        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId);

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Task)
                .WithMany(p => p.TaskAttachments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UploadedByNavigation)
                .WithMany(p => p.TaskAttachments)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
