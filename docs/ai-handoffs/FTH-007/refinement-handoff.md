# Refinement Handoff — FTH-007

## Objetivo

Refinar o card `[FTH-007] Frontend` antes da especificação e implementação, mantendo as decisões compatíveis com `REQUIREMENTS.md` e os cards backend já definidos.

## Fontes consultadas

- `REQUIREMENTS.md`.
- `DEVELOPMENT_LOG.md`.
- Handoff de refinamento do FTH-002.
- Cards FTH-001 a FTH-006 no GitHub Project 4.
- Card `[FTH-007] Frontend` no GitHub Project 4.

## Decisões confirmadas

- A experiência será uma SPA em uma única página, sem navegação entre páginas e sem piscar a tela.
- O layout terá aparência de site completo: cabeçalho, logo, formulário centralizado e footer.
- A marca textual exibida no cabeçalho e footer será exatamente `Task Home Fundo`, com ícone simples e paleta fintech consistente.
- Incluir responsividade e acessibilidade básica somente com soluções simples: adaptação para telas menores, labels visíveis, foco/teclado e contraste adequado; requisitos avançados ficam fora do escopo.
- O formulário deverá ser visualmente bonito, organizado e com UI/UX adequada.
- O frontend fará validações preventivas antes do backend, incluindo `required` e `maxlength`.
- O SSN terá formatação/máscara visual.
- O campo de SSN terá botão para mostrar/ocultar o valor; a máscara visual será `000-00-0000` e o backend receberá somente os nove dígitos normalizados.
- `requestedAmount` será exibido como dólar, por exemplo `$ 1,234.56`.
- `requestedAmount` aceitará até duas casas decimais e será enviado como número normalizado, sem símbolo ou separador de milhar.
- `requestedAmount` deverá ser maior que `0` e menor que `1.000.000`.
- O envio exibirá um spinner de loading enquanto aguarda o backend.
- O resultado será exibido abaixo do formulário, na mesma tela.
- Sucesso será apresentado visualmente em verde.
- Erros serão apresentados visualmente em vermelho, com texto amigável e sem linguagem técnica.
- Toda a interface, incluindo labels, instruções, estados e mensagens amigáveis, será escrita em inglês.
- Haverá um botão para limpar o formulário e iniciar uma nova submissão.
- Durante o loading, os botões de enviar e limpar ficam desabilitados para impedir duplo envio; ao terminar, o envio é liberado novamente.
- Em sucesso ou erro, os dados permanecem preenchidos e visíveis.
- O botão de limpar remove os campos, mensagens e estados visuais, retornando a tela ao estado inicial.
- A decisão de aprovação/negação continuará exclusivamente no backend.
- O frontend não chamará diretamente o mock externo.
- A biblioteca visual aprovada por recomendação é Tailwind CSS + shadcn/ui, com Lucide React, React Hook Form e Zod.
- A mensagem de sucesso será única e amigável em inglês, como `Your application was approved successfully.`, exibindo o `applicationId` como referência e nunca o SSN.
- Layout aprovado: First Name + Last Name; Company Name + SSN; Address ocupando aproximadamente 80% + State no espaço restante; Requested Amount em linha própria; botões `Apply` e `Clear` em linha própria.
- `State` será um dropdown restrito a códigos USPS de exatamente duas letras.
- Erros `400` e `422` serão traduzidos para inglês amigável; erros de campo aparecerão junto aos campos e respostas com erro poderão exibir um alerta geral abaixo do formulário.
- O botão principal será rotulado `Apply` e o botão secundário `Clear`.
- Não haverá testes automatizados de frontend neste card, pois `REQUIREMENTS.md` exige testes de rule engine, cliente retornante e endpoint, mas não de frontend; os cenários visuais serão validados manualmente e documentados.
- A URL da API será configurada por `NEXT_PUBLIC_API_URL`, sem hardcode, com valor local padrão quando aplicável.
- `Apply` inicia habilitado; a validação ocorre no clique e o botão fica desabilitado somente durante o loading.
- Status HTTP inesperado usa mensagem genérica amigável, preserva os dados e libera o botão ao final do loading.
- Mensagens aprovadas: NY → `Applications from New York are not eligible.`; SSN blacklistado → `The provided SSN is not eligible.`; validação → mensagens simples por campo; erro desconhecido → `Please review the highlighted fields and try again.`; rede/`500` → `We couldn’t submit your application. Please try again.`. Códigos técnicos ficam ocultos.

## Regras herdadas dos requisitos/cards

- Campos: first name, last name, address com state, company name, requested amount e SSN.
- Todos os campos são obrigatórios.
- Limites: first name 100, last name 100, company name 200 e address 300 caracteres.
- `state` deve aceitar códigos USPS dos 50 estados e `DC`.
- SSN deve aceitar `123456789` ou `123-45-6789` e ser normalizado para dígitos.
- Resposta aprovada: `201` para novo cliente e `200` para cliente retornante.
- Negação: `422`, incluindo as razões de NY e SSN blacklistado, podendo haver múltiplos erros.
- Request inválida: `400`; falha inesperada: `500` com envelope genérico.
- SSN não deve aparecer em respostas ou mensagens expostas.
- O processamento externo é assíncrono e não deve ser tratado como uma chamada do frontend.

## Questões ainda abertas

- Detalhes visuais de implementação, como valores exatos de cores, espaçamentos e escolha do ícone da marca.
- Confirmação do contrato final do backend durante a especificação, sem alterar as decisões de UX já aprovadas.

## Sincronização GitHub

- 2026-09-09: Refinamento aprovado pelo usuário.
- O draft foi traduzido para inglês, atualizado com o escopo, regras e critérios de aceite consolidados.
- O draft foi convertido em issue real: [FTH-007 — Frontend](https://github.com/thivalente/TakeHomeFundo/issues/8).
