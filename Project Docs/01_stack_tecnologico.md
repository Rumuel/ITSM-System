# Definicao do Stack Tecnologico

## Projeto
Sistema de Gestao de Infraestruturas e Incidentes (ITSM)

## Stack escolhido

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- JWT para autenticacao e autorizacao
- Swagger/OpenAPI para documentacao da API

### Frontend
- ReactJS
- TypeScript
- React Router
- Axios ou Fetch API
- Biblioteca visual a definir: Bootstrap, Tailwind CSS ou Material UI

### Ferramentas de desenvolvimento
- Visual Studio 2026
- Git
- GitHub
- SQL Server Management Studio ou Azure Data Studio

## Justificacao
Foi escolhida uma solucao integrada em Visual Studio, composta por uma API em ASP.NET Core e uma aplicacao cliente em ReactJS com TypeScript. Esta abordagem permite separar a logica de negocio e persistencia de dados da interface de utilizador, mantendo uma estrutura full-stack organizada, escalavel e adequada ao desenvolvimento profissional.

O ASP.NET Core facilita a criacao de APIs REST, integracao com Entity Framework Core, autenticacao com JWT e documentacao com Swagger. O ReactJS com TypeScript permite criar uma interface moderna, com maior seguranca de tipos e menor probabilidade de erros no frontend.

## Decisao arquitetural inicial
A solucao sera organizada em camadas:

- API: controladores, endpoints, autenticacao e documentacao Swagger.
- Application: servicos, regras de negocio e casos de uso.
- Domain: entidades principais e regras centrais do dominio.
- Infrastructure: acesso a dados, Entity Framework Core, repositorios e configuracoes externas.
- Frontend: interface de utilizador em React com TypeScript.
