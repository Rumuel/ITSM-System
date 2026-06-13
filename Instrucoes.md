Quero usar Entity Framework Core Code First com SQL Server e ASP.NET Core Identity.

Usa as tabelas padrão do Identity para utilizadores, roles, claims, logins e tokens.
Não cries uma tabela User personalizada.

Cria uma classe ApplicationUser : IdentityUser<int> com:
- Name
- IsActive
- relação opcional com Technician
- relação com tickets criados

Cria o ItsmDbContext herdando de:
IdentityDbContext<ApplicationUser, IdentityRole<int>, int>

Adiciona DbSets para as entidades do domínio ITSM:
Technician, Skill, TechnicianSkill, TechnicianAvailability, Asset, AssetType, Ticket, TicketCategory, TicketStatus, Priority, TicketHistory e AuditLog.

Configura as relações no OnModelCreating, mantendo as tabelas padrão do Identity.
Depois cria a migration inicial Code First para SQL Server.


1. Estrutura da solução
2. Entidades do domínio
3. Identity + DbContext
4. Migration inicial
5. AuthController
6. Roles: Administrador, Tecnico, Utilizador
7. Tickets
8. Atribuição automática de técnico
9. Frontend React
10. Testes