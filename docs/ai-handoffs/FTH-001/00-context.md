# Contexto integral — FTH-001

## Context

O projeto precisa de uma base backend simples e consistente para o fluxo de solicitação de empréstimo e para os próximos use cases.

## Objective

Preparar a aplicação backend e o projeto de testes para que as próximas funcionalidades sejam implementadas sem reestruturar a solução.

## Scope

- Manter a solution existente `FundoTakeHome.slnx`.
- Criar o projeto da aplicação em `src/backend/FundoTakeHome.Api`.
- Criar o projeto de testes em `tests/FundoTakeHome.Tests`.
- Manter `src/frontend` disponível para a futura aplicação Next.js.
- Usar .NET 10, Minimal API e Swagger.
- Configurar SQLite com EF Core e suporte a transações reais.
- Configurar xUnit e `WebApplicationFactory`.
- Usar Shouldly para asserções.
- Usar FluentValidation para validação de propriedades.
- Usar ErrorOr para o Result Pattern e erros esperados da aplicação.
- Organizar o backend com uma estrutura Vertical Slice leve, mantendo endpoint, fluxo de aplicação, regras de domínio e infraestrutura da feature próximos.
- Seguir Clean Architecture dentro do projeto da aplicação, mantendo dependências apontando para dentro e responsabilidades separadas por pastas.
- Definir a base de um rule engine com Strategy, permitindo adicionar ou alterar regras sem modificar as regras existentes.
- Preparar a base necessária para o Outbox.
- Adicionar Dockerfiles e orquestração Docker Compose para backend, frontend e mock service.
- Configurar rede, portas, variáveis de ambiente e volume para o banco SQLite.
- Deixar o ambiente pronto para as implementações dos próximos cards.

## Out of scope

- Implementar o fluxo completo de submissão da aplicação.
- Implementar regras de aprovação e negação.
- Implementar o worker e o processamento completo do Outbox.
- Implementar comportamento do frontend.
- Implementar comportamento do mock service.
- Criar projetos de produção adicionais sem necessidade clara.

## Acceptance Criteria

- A solution compila com os projetos de aplicação e testes.
- A API inicia e o Swagger fica acessível.
- SQLite e EF Core ficam configurados para uso futuro.
- O projeto de testes possui xUnit, Shouldly e `WebApplicationFactory`.
- FluentValidation, ErrorOr e as abstrações iniciais de Strategy ficam disponíveis.
- A estrutura de pastas suporta o Vertical Slice `SubmitApplication`.
- A base do Outbox fica preparada sem antecipar a implementação do worker.
- O Docker Compose define backend, frontend e mock service.
- Dockerfiles e Compose são válidos e respeitam os limites esperados dos serviços.
- O SQLite usa volume para preservar dados entre reinícios do container.
- A comunicação entre serviços usa nomes do Compose e configuração, sem endereços fixos da máquina.
- O README contém o comando inicial para subir o ambiente com Docker Compose.

## Dependencies

- .NET 10 SDK.
- Pacotes NuGet estáveis compatíveis com .NET 10.
- Docker Engine.
- Docker Compose.

## User clarification

- Usar obrigatoriamente `D:\Projetos\Pessoal\Challenges\FundoTakeHome\FundoTakeHome.slnx`.
- O projeto API deve ficar dentro da solution folder `01- Api`.
- O projeto de testes deve ficar dentro da solution folder `02- Tests`.
- O objetivo mínimo demonstrável é subir o ambiente básico, acessar um endpoint de teste e visualizar o Swagger.
