# Development Log

## Propósito do arquivo

Este arquivo é a memória permanente das decisões globais do sistema. Ele deve explicar escolhas de arquitetura, produto, persistência, tratamento de erros, testes e trade-offs que ajudam a entender a solução como um todo.

Não registrar aqui:

- histórico ou progresso de cards;
- perguntas e respostas de refinamento;
- checklists de implementação;
- decisões temporárias específicas de uma única feature;
- nomes de cards como contexto necessário para entender uma decisão.

Cards e handoffs descrevem o trabalho de uma feature. Este arquivo descreve as decisões que continuam válidas para o sistema inteiro.

Antes de registrar qualquer decisão, consulte o [REQUIREMENTS.md](REQUIREMENTS.md).
O requisito do desafio é a fonte de verdade; este arquivo explica como decidi atendê-lo.

Cada entrada deve:

- ser escrita em primeira pessoa do singular;
- explicar a decisão e sua motivação;
- relacionar a decisão ao requisito ou trade-off que ela atende;
- registrar o princípio permanente, e não apenas o detalhe de implementação;
- evitar duplicar decisões já registradas.

Antes de adicionar uma entrada, verifique se ela responde: “Uma pessoa que
nunca viu os cards entenderá melhor a arquitetura ou o produto ao ler esta
decisão?”

## Registro

### Decisões

- Organizei a solução em hosts e código compartilhado. A API e o Worker são projetos executáveis separados, enquanto Domain, Application e Infrastructure ficam no projeto `FundoTakeHome.Backend`.
- Usei Vertical Slice para manter cada fluxo organizado por feature, sem criar camadas genéricas sem necessidade.
- Mantive os boundaries da Clean Architecture dentro do projeto compartilhado, com dependências apontando para dentro.
- Usei Strategy para representar cada regra de decisão de forma independente. Assim, posso adicionar uma nova regra sem alterar as regras existentes, conforme solicitado pelo desafio.
- Usei `ErrorOr` para representar erros esperados de negócio como resultado do fluxo, sem usar exceptions como controle normal da aplicação. Falhas inesperadas de infraestrutura seguem para o tratamento global de exceções.
- Usei `FluentValidation` para concentrar a validação das propriedades e `Shouldly` para manter os testes legíveis.
- Usei Minimal API porque o desafio tem poucos endpoints e não precisa da estrutura adicional de controllers.
- Mantive o fluxo do frontend como uma SPA de uma única tela, com formulário e resultado no mesmo contexto, para reduzir navegação e manter a experiência simples e direta, conforme a preocupação do desafio com simplicidade.
- Usei SQLite porque é simples para executar localmente e suporta transações reais, como exigido pelo desafio. Dados de negócio que precisam ser consultados de forma reproduzível, como a blacklist de SSNs, também ficam persistidos no banco em vez de serem mantidos em memória.
- Usei xUnit, Shouldly e Moq para testar regras de negócio e casos de uso da Application.
- Mantive os testes restritos a Domain e Application, onde está o comportamento de negócio relevante.
- Usei o Transactional Outbox para salvar os dados aprovados e o evento na mesma transação. O worker processa o evento depois, fora da requisição HTTP.
- Usei Value Objects para representar IDs, SSN, `requestedAmount` e `state`, evitando valores primitivos espalhados e concentrando suas validações e normalizações.
- Usei GUID version 7 para os identificadores, gerando-os na aplicação antes da persistência. Assim, os IDs já estão disponíveis para relacionamentos, eventos e responses.
- Usei uma única chamada `SaveChangesAsync()` para persistir os dados e a mensagem de outbox de forma atômica, aproveitando a transação automática do EF Core com banco relacional.
- Modelei a relação entre Customer e Application como um-para-um, conforme a simplificação definida pelo desafio de manter uma única aplicação por SSN.
- Mantive o Outbox com apenas identificadores e metadados do evento. O worker busca os dados atuais no banco antes de enviá-los ao serviço externo, evitando duplicar dados pessoais na mensagem.
- Usei entrega `at-least-once`, pois o banco e o serviço externo não participam da mesma transação. O serviço externo deve aceitar a repetição do mesmo evento sem criar duplicidades.
- Usei `LockId` e `LockedUntil` para reservar temporariamente uma mensagem e permitir que outro worker a assuma quando o lease expirar.
- Mantive a integração externa atrás de uma interface, para que o restante da aplicação não dependa diretamente de HTTP.
- Mantive a aprovação persistida mesmo quando o serviço externo falha. Nesse caso, o worker controla novas tentativas usando o Outbox.
- Escolhi HTTP `422 Unprocessable Entity` para requests estruturalmente válidos, mas rejeitados por regras de elegibilidade. Reservei HTTP `400` para requests inválidos ou incompletos.
- Tratei falhas de infraestrutura, como indisponibilidade do banco durante a consulta da blacklist, como erros inesperados. Elas são encaminhadas ao handler global e resultam em HTTP `500`, em vez de serem convertidas em uma negação de negócio.
- Populei a blacklist com números repetidos de nove dígitos somente como dados sintéticos para testes e demonstrações. Esses valores não representam uma blacklist real.
- Não persisto solicitações negadas, pois somente aplicações aprovadas devem gerar Customer, Application e evento; uma negação não produz efeitos colaterais de persistência.

### Simplificações

- Não implementei autenticação ou autorização, pois o desafio informa que autenticação não é necessária.
- Não implementei logging estruturado, métricas, tracing, monitoramento operacional, dashboards ou alertas, pois o desafio tem escopo local e não exige observabilidade de produção. Caso logging seja adicionado futuramente, o SSN não deverá ser exposto.
- O SSN será armazenado aberto no banco; não haverá criptografia, tokenização ou mascaramento em repouso.
  - A validação do SSN será simples: aceitar dígitos ou formato com hífens, normalizar para 9 dígitos e não validar regras reais da SSA ou checksum.
- Decidi aceitar apenas os 50 estados dos Estados Unidos e `DC`, rejeitando territórios, para manter o campo limitado a estados válidos para o escopo do desafio.
- Decidi limitar o `requestedAmount` a valores maiores que `0` e menores que `1.000.000`, embora o requisito não defina um valor máximo, para manter a entrada controlada e dentro do escopo da solução.
- Decidi limitar o tamanho de `firstName`, `lastName`, `companyName` e `address` para evitar entradas excessivas e possíveis problemas de persistência no banco de dados.
- Não implementei concorrência distribuída avançada. Usei apenas aquisição atômica e lease no Outbox para proteger o processamento básico entre workers.

### Fora do escopo

- Não implementei Polly ou outra biblioteca de retry.
- Não implementei backoff variável ou exponencial, como tentativas após `2s`, `5s` e `10s`. O worker usa intervalo fixo de 5 segundos e no máximo três tentativas.
- Não implementei message broker, circuit breaker, dead-letter queue ou ferramenta de reprocessamento manual.
