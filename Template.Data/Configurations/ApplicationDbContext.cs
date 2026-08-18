using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Template.Data.Entities;

namespace Template.Data.Configurations
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.LawFirm)
                .WithMany(l => l.Users)
                .HasForeignKey(u => u.LawFirmId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lawyer>()
                .HasOne(l => l.LawFirm)
                .WithMany(f => f.Lawyers)
                .HasForeignKey(l => l.LawFirmId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Case>()
                .HasOne(c => c.Type)
                .WithMany(t => t.Cases)
                .HasForeignKey(c => c.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Case>()
                .HasOne(c => c.Status)
                .WithMany(s => s.Cases)
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Case>()
                .HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CreatedCases)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasOne(r => r.Case)
                .WithMany(c => c.Reports)
                .HasForeignKey(r => r.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Report>()
                .HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Document>()
                .HasOne(d => d.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Document>()
                .HasOne(d => d.UploadedByUser)
                .WithMany()
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Case)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.PaymentMilestone)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.PaymentMilestoneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CaseAssignment>()
                .HasOne(a => a.Case)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaseAssignment>()
                .HasOne(a => a.AssignedUser)
                .WithMany(u => u.CaseAssignments)
                .HasForeignKey(a => a.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CaseAssignment>()
                .HasOne(a => a.AssignedLawFirm)
                .WithMany(f => f.CaseAssignments)
                .HasForeignKey(a => a.AssignedLawFirmId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CaseAssignment>()
                .HasOne(a => a.AssignedLawyer)
                .WithMany(l => l.CaseAssignments)
                .HasForeignKey(a => a.AssignedLawyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Hearing>()
                .HasOne(h => h.Case)
                .WithMany(c => c.Hearings)
                .HasForeignKey(h => h.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaseUpdate>()
                .HasOne(u => u.Case)
                .WithMany(c => c.Updates)
                .HasForeignKey(u => u.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaseUpdate>()
                .HasOne(u => u.UpdatedByUser)
                .WithMany()
                .HasForeignKey(u => u.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FinancialProvision>()
                .HasOne(f => f.Case)
                .WithMany(c => c.FinancialProvisions)
                .HasForeignKey(f => f.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.Case)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CaseId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OtherInstruction>()
                .HasOne(i => i.AssignedLawFirm)
                .WithMany(f => f.OtherInstructions)
                .HasForeignKey(i => i.AssignedLawFirmId)
                .OnDelete(DeleteBehavior.Restrict);

            
        }
        


        public DbSet<Role> Roles { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<LawFirm> LawFirms { get; set; }

        public DbSet<Lawyer> Lawyers { get; set; }

        public DbSet<Case> Cases { get; set; }

        public DbSet<CaseStatus> CaseStatuses { get; set; }

        public DbSet<CaseType> CaseTypes { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PaymentMilestone> PaymentMilestones { get; set; }

        public DbSet<CaseAssignment> CaseAssignments { get; set; }

        public DbSet<Hearing> Hearings { get; set; }

        public DbSet<CaseUpdate> CaseUpdates { get; set; }

        public DbSet<FinancialProvision> FinancialProvisions { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<OtherInstruction> OtherInstructions { get; set; }
    }
}