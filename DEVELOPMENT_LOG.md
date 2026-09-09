# Registro de desenvolvimento

Este arquivo registra, em ordem, as ações, decisões e próximos passos do projeto.

## Como usar

Registrar a sequência do trabalho e as decisões importantes do projeto.

## Registro

### 2026-09-09 — Preparação

- Criado o repositório `thivalente/TakeHomeFundo` e um Project View Kanban para organizar o planejamento e os cards.
- Adicionado o arquivo `REQUIREMENTS.md` com os requisitos do desafio.

### Decisões

- Usar Vertical Slice porque cada funcionalidade fica organizada em um único lugar, facilitando entender e alterar um fluxo sem espalhar arquivos pela solução.
- Usar apenas dois projetos para manter a solução simples: um projeto para a aplicação e outro para os testes.
- Seguir Clean Architecture dentro do projeto para separar responsabilidades sem criar projetos e camadas desnecessários.
- Usar Strategy porque cada regra de decisão será independente; assim, uma nova regra pode ser adicionada sem alterar as regras existentes.
- Usar `ErrorOr` para representar erros esperados como resultado do fluxo, sem usar exceptions como controle normal da aplicação.
- Usar `FluentValidation` para concentrar a validação das propriedades e `Shouldly` para deixar as asserções dos testes mais legíveis.
- Usar Minimal API porque o desafio tem poucos endpoints e não precisa da estrutura adicional de controllers.
- Usar SQLite porque é simples para executar localmente e suporta transações reais.
- Usar xUnit e `WebApplicationFactory` para testar regras e endpoints.
- Preparar a base do Outbox para garantir que o evento seja persistido junto com os dados, deixando o processamento em background para um card próprio.
