using Microsoft.EntityFrameworkCore;
using WorkflowApi.Models;

namespace WorkflowApi.Data;

public class WorkflowContext : DbContext
{
    public WorkflowContext(DbContextOptions<WorkflowContext> options) : base(options) { }

    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<Execution> Executions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<WorkflowPermission> WorkflowPermissions { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowStepPermission> WorkflowStepPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Execution>()
            .HasOne(e => e.Workflow)
            .WithMany(w => w.Executions)
            .HasForeignKey(e => e.WorkflowId);

        // User configurations
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Role configurations
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // Permission configurations
        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        // Many-to-many User-Role
        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRole>(
                j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId),
                j => j.HasKey(ur => new { ur.UserId, ur.RoleId })
            );

        // Many-to-many Role-Permission
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<RolePermission>(
                j => j.HasOne(rp => rp.Permission).WithMany().HasForeignKey(rp => rp.PermissionId),
                j => j.HasOne(rp => rp.Role).WithMany().HasForeignKey(rp => rp.RoleId),
                j => j.HasKey(rp => new { rp.RoleId, rp.PermissionId })
            );

        // RefreshToken configurations
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

        // Workflow relationships
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .IsRequired(false);

        // Workflow permission relationships
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.ViewPermission)
            .WithMany()
            .HasForeignKey(w => w.ViewPermissionId)
            .IsRequired(false);
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.EditPermission)
            .WithMany()
            .HasForeignKey(w => w.EditPermissionId)
            .IsRequired(false);
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.ExecutePermission)
            .WithMany()
            .HasForeignKey(w => w.ExecutePermissionId)
            .IsRequired(false);
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.ManagePermission)
            .WithMany()
            .HasForeignKey(w => w.ManagePermissionId)
            .IsRequired(false);

        // WorkflowPermission configurations
        modelBuilder.Entity<WorkflowPermission>()
            .HasOne(wp => wp.Workflow)
            .WithMany()
            .HasForeignKey(wp => wp.WorkflowId);
        modelBuilder.Entity<WorkflowPermission>()
            .HasOne(wp => wp.Permission)
            .WithMany()
            .HasForeignKey(wp => wp.PermissionId);

        // WorkflowStep configurations
        modelBuilder.Entity<WorkflowStep>()
            .HasOne(ws => ws.Workflow)
            .WithMany()
            .HasForeignKey(ws => ws.WorkflowId);
        modelBuilder.Entity<WorkflowStep>()
            .HasOne(ws => ws.RequiredPermission)
            .WithMany()
            .HasForeignKey(ws => ws.RequiredPermissionId)
            .IsRequired(false);

        // WorkflowStepPermission configurations
        modelBuilder.Entity<WorkflowStepPermission>()
            .HasOne(wsp => wsp.WorkflowStep)
            .WithMany(ws => ws.WorkflowStepPermissions)
            .HasForeignKey(wsp => wsp.WorkflowStepId);
        modelBuilder.Entity<WorkflowStepPermission>()
            .HasOne(wsp => wsp.Permission)
            .WithMany()
            .HasForeignKey(wsp => wsp.PermissionId);
    }
}