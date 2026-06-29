using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Ticket> Tickets { get; }
        DbSet<Technician> Technicians { get; }
        DbSet<Skill> Skills { get; }
        DbSet<TechnicianSkill> TechnicianSkills { get; }
        DbSet<TechnicianAvailability> TechnicianAvailabilities { get; }
        DbSet<Asset> Assets { get; }
        DbSet<AssetType> AssetTypes { get; }
        DbSet<TicketCategory> TicketCategories { get; }
        DbSet<Domain.Entities.TicketStatus> TicketStatuses { get; }
        DbSet<Priority> Priorities { get; }
        DbSet<TicketHistory> TicketHistories { get; }
        DbSet<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
