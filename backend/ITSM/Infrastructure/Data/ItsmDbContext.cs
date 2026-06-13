using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ItsmDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ItsmDbContext(DbContextOptions<ItsmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<Technician> Technicians { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<TechnicianSkill> TechnicianSkills { get; set; } = null!;
        public DbSet<TechnicianAvailability> TechnicianAvailabilities { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<AssetType> AssetTypes { get; set; } = null!;
        public DbSet<TicketCategory> TicketCategories { get; set; } = null!;
        public DbSet<TicketStatus> TicketStatuses { get; set; } = null!;
        public DbSet<Priority> Priorities { get; set; } = null!;
        public DbSet<TicketHistory> TicketHistories { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.IsActive)
                      .HasDefaultValue(true);

                entity.HasOne(u => u.Technician)
                      .WithOne(t => t.User)
                      .HasForeignKey<Technician>(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.CreatedTickets)
                      .WithOne(t => t.CreatedByUser)
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ResponsibleAssets)
                      .WithOne(a => a.ResponsibleUser)
                      .HasForeignKey(a => a.ResponsibleUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Technician>(entity =>
            {
                entity.Property(t => t.MaxActiveTickets).HasDefaultValue(5);
                entity.Property(t => t.IsAvailable).HasDefaultValue(true);

                entity.HasIndex(t => t.UserId).IsUnique();
            });

            builder.Entity<Skill>(entity =>
            {
                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(s => s.Description)
                      .HasMaxLength(500);
            });

            builder.Entity<TechnicianSkill>(entity =>
            {
                entity.HasKey(ts => new { ts.TechnicianId, ts.SkillId });

                entity.Property(ts => ts.Level)
                      .IsRequired();

                entity.HasOne(ts => ts.Technician)
                      .WithMany(t => t.TechnicianSkills)
                      .HasForeignKey(ts => ts.TechnicianId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ts => ts.Skill)
                      .WithMany(s => s.TechnicianSkills)
                      .HasForeignKey(ts => ts.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TechnicianAvailability>(entity =>
            {
                entity.Property(a => a.WeekDay).IsRequired();
                entity.Property(a => a.StartTime).IsRequired();
                entity.Property(a => a.EndTime).IsRequired();

                entity.HasOne(a => a.Technician)
                      .WithMany(t => t.Availabilities)
                      .HasForeignKey(a => a.TechnicianId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssetType>(entity =>
            {
                entity.Property(a => a.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            builder.Entity<Asset>(entity =>
            {
                entity.Property(a => a.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(a => a.SerialNumber)
                      .HasMaxLength(100);

                entity.Property(a => a.Location)
                      .HasMaxLength(150);

                entity.Property(a => a.Status)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasOne(a => a.AssetType)
                      .WithMany(t => t.Assets)
                      .HasForeignKey(a => a.AssetTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TicketCategory>(entity =>
            {
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.Description)
                      .HasMaxLength(500);

                entity.Property(c => c.RequiredSkill)
                      .HasMaxLength(100);

                entity.HasMany(c => c.Tickets)
                      .WithOne(t => t.Category)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TicketStatus>(entity =>
            {
                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasData(
                    new TicketStatus { Id = 1, Name = "Novo" },
                    new TicketStatus { Id = 2, Name = "Em progresso" },
                    new TicketStatus { Id = 3, Name = "Resolvido" },
                    new TicketStatus { Id = 4, Name = "Fechado" },
                    new TicketStatus { Id = 5, Name = "Reaberto" });
            });

            builder.Entity<Priority>(entity =>
            {
                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasData(
                    new Priority { Id = 1, Name = "Baixa", Weight = 1 },
                    new Priority { Id = 2, Name = "Media", Weight = 2 },
                    new Priority { Id = 3, Name = "Alta", Weight = 3 },
                    new Priority { Id = 4, Name = "Critica", Weight = 4 });
            });

            builder.Entity<Ticket>(entity =>
            {
                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(t => t.Description)
                      .HasMaxLength(2000);

                entity.Property(t => t.CreatedAt)
                      .IsRequired();

                entity.HasOne(t => t.CreatedByUser)
                      .WithMany(u => u.CreatedTickets)
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedTechnician)
                      .WithMany(t => t.AssignedTickets)
                      .HasForeignKey(t => t.AssignedTechnicianId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Tickets)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Status)
                      .WithMany(s => s.Tickets)
                      .HasForeignKey(t => t.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Priority)
                      .WithMany(p => p.Tickets)
                      .HasForeignKey(t => t.PriorityId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Asset)
                      .WithMany(a => a.Tickets)
                      .HasForeignKey(t => t.AssetId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TicketHistory>(entity =>
            {
                entity.Property(h => h.OldStatus).HasMaxLength(50);
                entity.Property(h => h.NewStatus).HasMaxLength(50);
                entity.Property(h => h.Comment).HasMaxLength(1000);
                entity.Property(h => h.ChangedAt).IsRequired();

                entity.HasOne(h => h.Ticket)
                      .WithMany(t => t.Histories)
                      .HasForeignKey(h => h.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.ChangedByUser)
                      .WithMany(u => u.TicketHistories)
                      .HasForeignKey(h => h.ChangedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.Property(a => a.Action)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.EntityName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.CreatedAt)
                      .IsRequired();

                entity.HasOne(a => a.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
