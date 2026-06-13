# Diagrama de Classes da Logica de Dominio

O seguinte diagrama representa as classes principais da logica de dominio do sistema.

```mermaid
classDiagram
    class User {
        +int Id
        +string Name
        +string Email
        +string PasswordHash
        +bool IsActive
    }

    class Role {
        +int Id
        +string Name
    }

    class Technician {
        +int Id
        +int MaxActiveTickets
        +bool IsAvailable
        +GetCurrentWorkload()
    }

    class Skill {
        +int Id
        +string Name
        +string Description
    }

    class TechnicianSkill {
        +int TechnicianId
        +int SkillId
        +int Level
    }

    class TechnicianAvailability {
        +int Id
        +DayOfWeek WeekDay
        +TimeSpan StartTime
        +TimeSpan EndTime
        +IsAvailableAt(dateTime)
    }

    class Asset {
        +int Id
        +string Name
        +string SerialNumber
        +string Location
        +string Status
    }

    class AssetType {
        +int Id
        +string Name
    }

    class Ticket {
        +int Id
        +string Title
        +string Description
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +AssignTo(technician)
        +ChangeStatus(status)
    }

    class TicketCategory {
        +int Id
        +string Name
        +string RequiredSkill
    }

    class TicketStatus {
        +int Id
        +string Name
    }

    class Priority {
        +int Id
        +string Name
        +int Weight
    }

    class TicketHistory {
        +int Id
        +string OldStatus
        +string NewStatus
        +string Comment
        +DateTime ChangedAt
    }

    class AuditLog {
        +int Id
        +string Action
        +string EntityName
        +int EntityId
        +DateTime CreatedAt
    }

    class TicketAssignmentService {
        +AssignBestTechnician(ticket)
        -CalculateScore(technician, ticket)
    }

    User "many" --> "1" Role
    Technician "1" --> "1" User
    Technician "1" --> "many" TechnicianSkill
    Skill "1" --> "many" TechnicianSkill
    Technician "1" --> "many" TechnicianAvailability
    User "1" --> "many" Ticket
    Technician "1" --> "many" Ticket
    Ticket "many" --> "1" TicketCategory
    Ticket "many" --> "1" TicketStatus
    Ticket "many" --> "1" Priority
    Ticket "many" --> "0..1" Asset
    Asset "many" --> "1" AssetType
    Ticket "1" --> "many" TicketHistory
    User "1" --> "many" AuditLog
    TicketAssignmentService ..> Ticket
    TicketAssignmentService ..> Technician
```

## Justificacao do dominio
As classes representam os principais conceitos do sistema: utilizadores, tecnicos, competencias, ativos e tickets. A classe TicketAssignmentService representa o servico responsavel pela regra de negocio principal: escolher o tecnico mais adequado para um ticket.
