# Development Log

## Instruções para agentes

Use este arquivo para registrar, de forma curta, a sequência do trabalho e as principais decisões tomadas durante o desafio.

Antes de registrar qualquer decisão, consulte o [REQUIREMENTS.md](REQUIREMENTS.md). O requisito do desafio é a fonte de verdade; este arquivo apenas explica como decidi atendê-lo.

Escreva em primeira pessoa do singular, sem repetir detalhes de implementação ou registrar tarefas que não contribuam para explicar a solução. Para cada decisão, explique qual solicitação do desafio ela atende.

## Registro

### 2026-09-09 — Preparação

- Criei o repositório `thivalente/TakeHomeFundo` e um Project View Kanban para organizar o planejamento e os cards.
- Adicionei o arquivo `REQUIREMENTS.md` com os requisitos do desafio.

### Decisões

- Usarei apenas um projeto de aplicação para manter a solução simples e evitar over-engineering, uma preocupação explícita do desafio. Em um projeto real, eu avaliaria separar a aplicação em mais projetos.
- Usarei Vertical Slice para evitar que as responsabilidades fiquem confusas dentro desse único projeto, mantendo cada feature organizada em um mesmo espaço.
- Mesmo com um único projeto, preservarei os boundaries da Clean Architecture: a API ficará separada dos casos de uso, as regras de negócio ficarão isoladas e a infraestrutura será mantida substituível, com as dependências apontando para dentro.
- Usarei Strategy para representar cada regra de decisão de forma independente. Assim, posso adicionar uma nova regra sem alterar as regras existentes, conforme solicitado pelo desafio.
- Usarei `ErrorOr` para representar erros esperados como resultado do fluxo, sem usar exceptions como controle normal da aplicação.
- Usarei `FluentValidation` para concentrar a validação das propriedades e `Shouldly` para manter os testes legíveis.
- Usarei Minimal API porque o desafio tem poucos endpoints e não precisa da estrutura adicional de controllers.
- Usarei SQLite porque é simples para executar localmente e suporta transações reais, como exigido pelo desafio.
- Usarei xUnit e `WebApplicationFactory` para testar as regras e os endpoints relevantes.
- Prepararei a base do Outbox para manter os dados e o evento como uma unidade consistente, deixando o processamento em background para um card próprio.
