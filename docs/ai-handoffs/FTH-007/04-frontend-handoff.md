# Handoff final — FTH-007 Frontend

## Status

Concluído.

## Entrega

- Tela única em Next.js com layout responsivo.
- Formulário com validação preventiva, máscara de SSN e formatação de valor.
- Estados restritos aos códigos USPS.
- Integração com `POST /api/applications` usando `NEXT_PUBLIC_API_URL`.
- Estados visuais para carregamento, aprovação, negação, validação, falha de rede e erro inesperado.
- `Clear` restaura o formulário, limpa mensagens, referência, resultado e visibilidade do SSN.
- Mensagens de aprovação e negação não expõem o SSN.

## Validação manual

Os cenários abaixo foram executados manualmente pelo usuário:

- aplicação aprovada;
- negação por estado `NY`;
- negação por SSN blacklistado;
- cliente retornante com atualização da aplicação;
- validação de campos obrigatórios e limites;
- erro HTTP 500/falha de rede e retry;
- prevenção de duplo envio durante o carregamento;
- limpeza completa pelo botão `Clear`;
- responsividade, teclado, foco e contraste básico;
- ausência de SSN na saída visual.

## Evidências técnicas

- `dotnet build src/backend/FundoTakeHome.Api/FundoTakeHome.Api.csproj --no-restore`: concluído sem erros.
- O endpoint retornante foi validado após a correção das consultas filtradas por value objects.
- O ambiente Docker completo foi validado com frontend, API e mock saudáveis.

## Limitações

- Não foram criados testes automatizados de frontend, conforme o escopo definido no checklist.
- A validação de interface foi manual; não foi feita auditoria avançada de acessibilidade nem visual regression.
