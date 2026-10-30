# Nexo Nuclear - Avaliação de Cartões

## Grupo e integrantes

Grupo: **Nexo Nuclear**

| Nome | RM | Turma |
|---|---|---|
| Murillo Kenzo | RM566948 | 2TDSPA |
| Kauan Prado | RM566990 | 2TDSPA |
| Cauã Rocha | RM567593 | 2TDSPR |
| Gustavo Souza | RM567134 | 2TDSPR |

## Sobre a solução

Os cartões educativos apresentam aplicações da tecnologia nuclear de forma acessível e didática. Para acompanhar a experiência dos leitores, desenvolvemos uma aplicação que permite registrar notas e comentários sobre os conteúdos, ajudando o grupo a identificar possíveis melhorias.

## Funcionalidades desenvolvidas nesta Sprint

Nesta Sprint, foram implementadas as seguintes funcionalidades:

- Cadastro de avaliações com nota e comentário opcional.
- Listagem de todas as avaliações registradas.
- Busca de avaliações pelo código do cartão.
- Cálculo da média das notas de cada cartão.
- Validação dos dados informados pelo usuário.
- Impedimento de avaliações duplicadas pelo mesmo nome no mesmo cartão, independentemente de letras maiúsculas ou minúsculas.
- Tratamento de erros para permitir que o usuário continue utilizando o menu após uma operação inválida.

## Aplicação do .NET na solução

O .NET é utilizado para implementar o sistema de feedback dos cartões educativos, enquanto a API Java é responsável pelo gerenciamento dos conteúdos e das perguntas educativas.

As avaliações registradas permitem identificar quais conteúdos possuem boa aceitação e quais explicações podem precisar de ajustes.

A aplicação utiliza C# com Entity Framework Core e Oracle para armazenar os dados de forma persistente. O código do cartão é informado diretamente pelo usuário, sem necessidade de realizar uma chamada à API Java.

## Organização do projeto

A estrutura foi dividida em camadas para separar as responsabilidades e facilitar a manutenção:

- `Entities/`: contém a entidade `Avaliacao` e as constantes de mensagens em `MensagensValidacao`.
- `Services/AvaliacaoService.cs`: concentra as regras de negócio, o cadastro e as consultas, utilizando métodos assíncronos e o contexto `_context`.
- `Data/`: responsável pelo mapeamento das entidades com o banco Oracle e pela preparação da tabela.
- `Program.cs`: controla a conexão com o banco, a navegação pelo menu e a entrada e saída de dados.

## Como executar

**Requisitos:**
- SDK .NET 10 instalado.
- Acesso à rede da FIAP.
- Conta Oracle com permissão e quota para criação de tabelas.

Na pasta do projeto, execute:

```powershell
dotnet build
dotnet run
```

Durante a inicialização, informe o usuário e a senha do Oracle. As credenciais são solicitadas a cada execução e não ficam armazenadas. No terminal interativo, a senha é exibida de forma mascarada.

**Configurações de conexão:**
- Servidor: `oracle.fiap.com.br:1521/ORCL`
- Compatibilidade SQL: Oracle Database 19c.

## Estrutura da tabela

A tabela `AC_AVALIACOES` é criada automaticamente apenas quando ainda não existe no schema do usuário conectado. A implementação utiliza SQL diretamente, sem depender do método `EnsureCreated`.

| Coluna | Tipo e regras |
|---|---|
| ID | NUMBER(10), identity e chave primária (PRIMARY KEY) |
| CODIGO_CARD | NUMBER(10), obrigatório e maior que zero |
| NOME_AVALIADOR | VARCHAR2(100 CHAR), obrigatório, com tamanho entre 2 e 100 caracteres após trim |
| NOME_NORMALIZADO | Coluna virtual calculada com UPPER(TRIM(NOME_AVALIADOR)) |
| NOTA | NUMBER(10), obrigatório, com valores entre 1 e 5 |
| COM