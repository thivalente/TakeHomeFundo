# Checklist de execução — frontend

Ticket: FTH-007 — Frontend

## Regras fixas obrigatórias

- Seguir o escopo do issue #8 e do `REQUIREMENTS.md`.
- Executar uma microentrega por vez e validar antes de avançar.
- Verificar o código existente antes de criar novos artefatos.
- Manter a decisão de aprovação/negação exclusivamente no backend.
- Não chamar o mock externo diretamente pelo frontend.
- Não alterar backend, persistência, rule engine, worker ou contrato sem necessidade comprovada.
- Não expor o SSN em mensagens, resultados ou logs do frontend.
- Não criar testes automatizados de frontend neste card; documentar a validação manual.
- Manter os textos visíveis da interface em inglês.
- Preservar dados preenchidos após sucesso ou erro; `Clear` deve restaurar o estado inicial.

## Execução

- [x] 1. Estrutura e layout SPA
  - [x] Substituir o placeholder em `src/frontend` por uma aplicação Next.js funcional.
  - [x] Criar uma única tela sem navegação entre páginas nem flicker.
  - [x] Implementar header, marca textual `Task Home Fundo`, formulário centralizado e footer.
  - [x] Garantir layout responsivo: duas colunas no desktop e uma coluna em telas pequenas.
  - [x] Usar labels visíveis, foco/teclado funcional e contraste adequado.
  - **Pronto quando:** a tela inicial renderizar como site completo, responsivo e navegável por teclado.

- [x] 2. Campos e validação preventiva
  - [x] Implementar First Name, Last Name, Company Name, Address, State, Requested Amount e SSN.
  - [x] Tornar todos os campos obrigatórios.
  - [x] Aplicar limites: First Name 100, Last Name 100, Company Name 200 e Address 300 caracteres.
  - [x] Restringir State aos códigos USPS dos 50 estados e `DC`, com placeholder `Select a state`.
  - [x] Aceitar SSN digitado/colado como nove dígitos ou `000-00-0000`.
  - [x] Exibir SSN como `000-00-0000`, com controle show/hide, e enviar somente os nove dígitos.
  - [x] Exibir Requested Amount em dólar, aceitar até duas casas, exigir valor `> 0` e `< 1,000,000`, e enviar número normalizado.
  - [x] Exibir erros junto aos campos, focar o primeiro campo inválido e usar `aria-live` para mensagens de resultado.
  - **Pronto quando:** uma entrada inválida não chamar a API e informar claramente o problema em inglês.

- [x] 3. Integração com o backend
  - [x] Enviar `POST /api/applications` usando `NEXT_PUBLIC_API_URL`, sem host hardcoded.
  - [x] Confirmar o payload com os nomes do contrato atual: `firstName`, `lastName`, `address`, `state`, `companyName`, `requestedAmount`, `ssn`.
  - [x] Manter `Apply` habilitado inicialmente e desabilitar `Apply` e `Clear` durante toda a requisição.
  - [x] Exibir spinner durante o loading e liberar os botões ao final.
  - **Pronto quando:** o envio aprovado chegar ao endpoint com SSN e valor normalizados, sem duplo envio.

- [x] 4. Resultados e erros
  - [x] Para HTTP 200/201, mostrar em verde `Your application was approved successfully.` e o `applicationId` como referência.
  - [x] Para HTTP 422, mostrar em vermelho todas as razões aplicáveis:
    - [x] `Applications from New York are not eligible.`
    - [x] `The provided SSN is not eligible.`
  - [x] Para HTTP 400, traduzir erros de validação para mensagens amigáveis e associá-los aos campos quando possível.
  - [x] Para HTTP 500, rede, timeout ou resposta inesperada, mostrar `We couldn’t submit your application. Please try again.` ou mensagem genérica aprovada equivalente.
  - [x] Preservar os valores preenchidos em qualquer resultado e ocultar códigos técnicos.
  - **Pronto quando:** sucesso, negação, validação, falha de sistema, rede e resposta desconhecida tiverem feedback visual correto.

- [x] 5. Limpeza e verificação manual
  - [x] Implementar `Clear` para apagar campos, mensagens, loading e resultado.
  - [x] Verificar fluxo aprovado.
  - [x] Verificar negação por NY.
  - [x] Verificar negação por SSN blacklistado.
  - [x] Verificar cliente retornante e atualização exibida como sucesso.
  - [x] Verificar erro de validação, erro 500/rede, retry e prevenção de duplo envio.
  - [x] Verificar responsividade, teclado, foco, contraste básico e ausência de SSN na saída.
  - [x] Registrar evidências e limitações da validação manual no handoff final.
  - **Pronto quando:** todos os cenários manuais do issue forem executados e documentados.

## Fora do escopo

- Backend, rule engine, persistência, worker, Outbox e mock externo.
- Autenticação/autorização.
- Testes automatizados de frontend, visual regression e acessibilidade avançada.
- Infraestrutura de UI que não seja necessária para manter a implementação simples.
