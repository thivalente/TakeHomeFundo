# Fundo Take-Home — Requisitos do projeto

## Objetivo

Construir um fluxo simples de solicitação de empréstimo:

1. O usuário preenche um formulário no frontend Next.js.
2. O backend .NET aplica as regras de decisão.
3. Solicitações aprovadas salvam ou atualizam um cliente e uma aplicação.
4. Um evento é processado em background e envia os dados para um serviço externo mock via HTTP.

## Stack obrigatória

- Backend: .NET/C#.
- Frontend: Next.js.
- Serviço externo: mock local construído por nós.
- Banco: SQLite, PostgreSQL ou SQL Server; usar transações reais. EF Core InMemory não é permitido.

## Dados do formulário

- First name
- Last name
- Address, incluindo state
- Company name
- Requested amount
- SSN

## Regras de decisão

- State `NY` → negar.
- SSN na blacklist → negar.
- Sem regra de negação → aprovar.
- A decisão deve ficar em um rule engine no backend.
- Adicionar uma nova regra não deve exigir alteração nas regras existentes.
- Solicitações negadas devem levar a uma página de negação.

## Persistência

Em uma aprovação:

- Criar ou atualizar `Customer` com os dados pessoais.
- Criar ou atualizar `Application` com `id`, `requestedAmount` e `customerId`.
- O SSN identifica o cliente retornante.
- O mesmo SSN não pode criar um segundo cliente ou uma segunda aplicação.

## Transação e evento

- Customer, Application e evento devem fazer parte de uma unidade transacional.
- Se o banco ou a publicação do evento falhar, não pode haver dados incompletos.
- O evento deve ser processado em background, fora da requisição HTTP.
- Cliente novo → criar no serviço externo.
- Cliente retornante → atualizar no serviço externo.
- O serviço externo deve receber os dados por HTTP e retornar `200`.

## Testes prioritários

- Rule engine: aprovação, estado NY e SSN blacklist.
- Cliente retornante: atualização sem duplicação.
- Endpoint principal.
- Publicação/processamento do evento, quando aplicável.

## Documentação e entrega

- README com comandos copiáveis para executar backend, frontend, mock e banco.
- README com comandos para executar os testes.
- README com dados de teste para aprovação, negação e cliente retornante.
- Documentação da estrutura e responsabilidades das partes do sistema.
- Explicação de como adicionar uma nova regra.
- Explicação do evento em background, chamada HTTP e transação.
- Trade-offs e itens conscientemente deixados de fora.
- Link de vídeo público no topo do README.
- Um único repositório contendo backend, frontend e mock.

## Critérios de simplicidade

- Manter controllers/endpoints finos.
- Manter regras no domínio/aplicação.
- Isolar banco, HTTP e mensageria como infraestrutura substituível.
- Evitar camadas, padrões e abstrações sem necessidade real.
- Autenticação não é necessária.

