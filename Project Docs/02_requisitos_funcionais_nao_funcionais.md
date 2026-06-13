# Documento de Requisitos Funcionais e Nao-Funcionais

## 1. Enquadramento
O sistema a desenvolver consiste numa plataforma ITSM para gestao de infraestruturas informaticas, registo de ativos de hardware e software, e tratamento de incidentes e pedidos de suporte.

O sistema deve permitir que os utilizadores registem tickets e que estes sejam atribuidos automaticamente ao tecnico mais adequado, considerando carga de trabalho atual, competencias e disponibilidade horaria.

## 2. Atores do sistema

### Administrador
Responsavel pela gestao global do sistema, utilizadores, tecnicos, categorias, ativos e configuracoes.

### Tecnico
Responsavel por acompanhar e resolver tickets atribuidos.

### Utilizador
Responsavel por registar incidentes ou pedidos de suporte e acompanhar o seu estado.

## 3. Requisitos funcionais

| Codigo | Requisito | Prioridade |
|---|---|---|
| RF01 | O sistema deve permitir autenticar utilizadores. | Alta |
| RF02 | O sistema deve permitir gerir perfis de acesso: administrador, tecnico e utilizador. | Alta |
| RF03 | O administrador deve poder criar, editar, consultar e remover utilizadores. | Alta |
| RF04 | O administrador deve poder registar tecnicos. | Alta |
| RF05 | O administrador deve poder associar competencias aos tecnicos. | Alta |
| RF06 | O administrador deve poder gerir disponibilidades dos tecnicos. | Alta |
| RF07 | O sistema deve permitir registar ativos informaticos. | Alta |
| RF08 | O sistema deve permitir consultar, editar e remover ativos. | Media |
| RF09 | O sistema deve permitir criar tickets de incidente ou pedido de suporte. | Alta |
| RF10 | O sistema deve atribuir automaticamente um ticket ao tecnico mais adequado. | Alta |
| RF11 | A atribuicao automatica deve considerar carga de trabalho, competencias e disponibilidade. | Alta |
| RF12 | O tecnico deve poder alterar o estado de um ticket. | Alta |
| RF13 | O utilizador deve poder consultar os seus tickets. | Media |
| RF14 | O administrador deve poder consultar todos os tickets. | Alta |
| RF15 | O sistema deve manter historico das alteracoes feitas nos tickets. | Alta |
| RF16 | O sistema deve permitir gerir categorias de tickets. | Media |
| RF17 | O sistema deve permitir gerir prioridades dos tickets. | Media |
| RF18 | O sistema deve registar logs ou auditoria das principais acoes. | Media |
| RF19 | A API deve disponibilizar endpoints documentados para as principais funcionalidades. | Alta |
| RF20 | O sistema deve permitir pesquisar e filtrar tickets por estado, prioridade, categoria e tecnico. | Media |

## 4. Requisitos nao funcionais

| Codigo | Requisito | Prioridade |
|---|---|---|
| RNF01 | O sistema deve ser desenvolvido em ASP.NET Core Web API e ReactJS com TypeScript. | Alta |
| RNF02 | O sistema deve utilizar uma base de dados relacional SQL Server. | Alta |
| RNF03 | A persistencia de dados deve ser feita com Entity Framework Core. | Alta |
| RNF04 | O sistema deve utilizar autenticacao baseada em JWT. | Alta |
| RNF05 | As passwords devem ser armazenadas com hash seguro. | Alta |
| RNF06 | A API deve ser documentada com Swagger/OpenAPI. | Alta |
| RNF07 | O codigo deve seguir principios de Clean Code e organizacao por camadas. | Alta |
| RNF08 | A base de dados deve estar normalizada. | Alta |
| RNF09 | O sistema deve validar dados no backend e no frontend. | Alta |
| RNF10 | O sistema deve tratar erros de forma controlada. | Alta |
| RNF11 | O sistema deve incluir testes unitarios para funcionalidades principais. | Media |
| RNF12 | O repositorio Git deve conter commits organizados e identificaveis por etapa. | Alta |
| RNF13 | O sistema deve compilar e executar sem erros no momento da defesa. | Alta |
| RNF14 | A solucao deve permitir futuras extensoes, como notificacoes ou dashboards. | Baixa |

## 5. Regra de negocio principal
Quando um ticket e criado, o sistema deve escolher automaticamente o melhor tecnico atraves de um algoritmo de atribuicao.

O algoritmo deve considerar:

1. Competencias do tecnico face a categoria do ticket.
2. Carga de trabalho atual do tecnico.
3. Disponibilidade horaria do tecnico.
4. Prioridade do ticket.

A estrutura de dados prevista para apoiar esta decisao e uma fila de prioridade ou heap, onde os tecnicos disponiveis sao ordenados de acordo com uma pontuacao calculada pelo sistema.
