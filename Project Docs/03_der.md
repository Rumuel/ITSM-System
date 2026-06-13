# Diagrama Entidade-Relacionamento

O seguinte diagrama representa a proposta inicial de modelo de dados para o sistema ITSM.

```mermaid
erDiagram
    ROLE ||--o{ USER : has
    USER ||--o| TECHNICIAN : may_be
    USER ||--o{ TICKET : creates
    TECHNICIAN ||--o{ TICKET : assigned_to
    TECHNICIAN ||--o{ TECHNICIAN_SKILL : has
    SKILL ||--o{ TECHNICIAN_SKILL : linked_to
    TECHNICIAN ||--o{ TECHNICIAN_AVAILABILITY : has
    ASSET_TYPE ||--o{ ASSET : categorizes
    USER ||--o{ ASSET : responsible_for
    TICKET_CATEGORY ||--o{ TICKET : classifies
    TICKET_STATUS ||--o{ TICKET : defines_state
    PRIORITY ||--o{ TICKET : defines_priority
    ASSET ||--o{ TICKET : related_to
    TICKET ||--o{ TICKET_HISTORY : has
    USER ||--o{ TICKET_HISTORY : performs
    USER ||--o{ AUDIT_LOG : generates

    ROLE {
        int RoleId PK
        string Name
    }

    USER {
        int UserId PK
        int RoleId FK
        string Name
        string Email
        string PasswordHash
        bool IsActive
        datetime CreatedAt
    }

    TECHNICIAN {
        int TechnicianId PK
        int UserId FK
        int MaxActiveTickets
        bool IsAvailable
    }

    SKILL {
        int SkillId PK
        string Name
        string Description
    }

    TECHNICIAN_SKILL {
        int TechnicianId FK
        int SkillId FK
        int Level
    }

    TECHNICIAN_AVAILABILITY {
        int AvailabilityId PK
        int TechnicianId FK
        string WeekDay
        time StartTime
        time EndTime
    }

    ASSET_TYPE {
        int AssetTypeId PK
        string Name
    }

    ASSET {
        int AssetId PK
        int AssetTypeId FK
        int ResponsibleUserId FK
        string Name
        string SerialNumber
        string Location
        string Status
    }

    TICKET_CATEGORY {
        int CategoryId PK
        string Name
        string RequiredSkill
    }

    TICKET_STATUS {
        int StatusId PK
        string Name
    }

    PRIORITY {
        int PriorityId PK
        string Name
        int Weight
    }

    TICKET {
        int TicketId PK
        int CreatedByUserId FK
        int AssignedTechnicianId FK
        int CategoryId FK
        int StatusId FK
        int PriorityId FK
        int AssetId FK
        string Title
        string Description
        datetime CreatedAt
        datetime UpdatedAt
    }

    TICKET_HISTORY {
        int HistoryId PK
        int TicketId FK
        int ChangedByUserId FK
        string OldStatus
        string NewStatus
        string Comment
        datetime ChangedAt
    }

    AUDIT_LOG {
        int AuditLogId PK
        int UserId FK
        string Action
        string EntityName
        int EntityId
        datetime CreatedAt
    }
```

## Justificacao do modelo
O modelo separa utilizadores, perfis, tecnicos, competencias, ativos e tickets. A tabela TechnicianSkill resolve a relacao muitos-para-muitos entre tecnicos e competencias. A tabela TechnicianAvailability permite considerar disponibilidade horaria no algoritmo de atribuicao. O historico de tickets e a auditoria permitem rastrear alteracoes importantes no sistema.
