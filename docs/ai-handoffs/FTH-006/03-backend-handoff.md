# Handoff — FTH-006

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_proxima_microentrega
- **Microentregas autorizadas:** —
- **Última aprovação humana:** autorização microentrega 1.1

## Planejamento global

- **Use case:** External Service Mock.
- **Objetivo:** disponibilizar um mock HTTP local, stateless, para receber Customer e Application.
- **Entradas:** payload completo em `POST /customers` ou `PUT /customers/{customerId}`.
- **Saída:** HTTP 200 com `{ "status": "received" }`.
- **Regras:** POST registra `Created`; PUT registra `Updated`; SSN sempre mascarado no log.
- **Fora do escopo:** banco, cache, persistência, Outbox, worker, regras de negócio, autenticação, autorização e testes automatizados.
- **Local:** projeto separado em `src/mock/FundoTakeHome.Mock.csproj`, na solution folder `03- Mocks`.

## Evidência — checklist 1.1

- **Evidência:** projeto compilou sem erros; POST e PUT manuais retornaram HTTP 200 com o status esperado.
- **Arquivos:** `src/mock/FundoTakeHome.Mock.csproj`, `src/mock/Program.cs`, `src/mock/Dockerfile`, `FundoTakeHome.slnx`; placeholder `src/mock/index.html` removido.
- **Testes:** verificação manual local dos endpoints; nenhuma suíte automatizada executada.
- **Micro-gate:** PASS
- **O que o código prova agora:** os dois endpoints registram método, rota, status, operação, IDs e payload com SSN mascarado.
- **O que ainda não prova:** validação final de arquitetura e execução via Docker Compose.

## Divergência encontrada

- `.cursor/rules/backend-core.mdc` e `.cursor/rules/backend-architecture-gates.mdc` não existem neste repositório. Nenhuma regra foi inventada ou criada.
