# Backend Handoff — FTH-004

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_proxima_microentrega
- **Microentregas autorizadas:** —
- **Última aprovação humana:** nenhuma ainda

## Planejamento global

- **Objetivo:** tratar exceções inesperadas na borda da API e retornar o envelope genérico sem expor detalhes técnicos.
- **Entrada:** qualquer requisição que gere uma exceção não tratada no pipeline.
- **Saída:** HTTP 500 com `data: null` e erro `system.unexpected_error`.
- **Regras principais:** registrar a exceção completa com `ILogger` no console; resposta pública fixa; preservar erros de validação e negócio.
- **Dados trafegados/transformados:** método e caminho da requisição entram apenas no log; nenhum detalhe da exceção vai para a resposta.
- **BC / jornada / passo:** infraestrutura transversal da API / tratamento de falhas / resposta HTTP.
- **Patterns obrigatórios:** middleware ASP.NET Core, `ILogger`, envelope comum.
- **Arquivos esperados:** middleware, modelos do envelope, registro no `Program.cs`, checklist e handoff.
- **Arquivos proibidos:** alterações em regras de negócio, persistência, logging customizado, streaming e testes específicos do middleware.
- **Escopo autenticado e ownership:** não aplicável.
- **BDD:** cenários do card FTH-004 serão consolidados conforme os critérios de aceite.

