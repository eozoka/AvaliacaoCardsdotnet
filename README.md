# Nexo Nuclear - Avaliação de Cartões
## Grupo e integrantes
Grupo: **Nexo Nuclear**.
| Nome | RM | Turma |
|---|---|---|
| Murillo Kenzo | RM566948 | 2TDSPA |
| Kauan Prado | RM566990 | 2TDSPA |
| Cauã Rocha | RM567593 | 2TDSPR |
| Gustavo Souza | RM567134 | 2TDSPR |

## Solução
Cartões educativos apresentam aplicações da tecnologia nuclear em linguagem acessível. O console registra notas e comentários dos leitores para ajudar o grupo a melhorar os conteúdos.

## O que foi desenvolvido nesta Sprint
Cadastro de avaliações, listagem completa, busca por código do cartão e cálculo da média. Há validação de campos, comentário opcional e bloqueio de uma segunda avaliação do mesmo nome para o mesmo cartão, sem diferenciar maiúsculas/minúsculas. Erros são tratados para permitir novas tentativas no menu.

## Como o .NET se aplica à solução
O .NET implementa a coleta de feedback sobre os cartões, enquanto a API Java gerencia seus conteúdos e perguntas educativas. Notas e comentários permitem identificar explicações que precisam de revisão. A aplicação console usa EF Core com Oracle para manter as avaliações entre execuções; o código do cartão é informado manualmente, sem depender de uma chamada à API Java.

## Arquitetura
`Entities/`: entidade Avaliacao e constantes de MensagensValidacao; `Services/AvaliacaoService.cs`: cadastro, consultas e regras, com métodos assíncronos e acesso pelo campo `_context`; `Data/`: mapeamento EF Core e criação da tabela ausente; `Program.cs`: conexão, menu e entrada/saída.

## Como executar
Requer SDK .NET 10, acesso à rede da FIAP e conta Oracle com permissão e quota para criar tabela. Na pasta do projeto:
```powershell
dotnet build
dotnet run
```
Informe usuário e senha Oracle no console a cada execução; a senha é mascarada no terminal interativo. As credenciais não são salvas. Servidor: `oracle.fiap.com.br:1521/ORCL`, com compatibilidade SQL Oracle 19c.

## Tabela criada
`AC_AVALIACOES`, criada somente se estiver ausente no schema conectado, sem `EnsureCreated`.
| Coluna | Tipo e regra |
|---|---|
| ID | NUMBER(10), identity e PRIMARY KEY |
| CODIGO_CARD | NUMBER(10), obrigatório e maior que zero |
| NOME_AVALIADOR | VARCHAR2(100 CHAR), obrigatório, de 2 a 100 caracteres após trim |
| NOME_NORMALIZADO | Coluna virtual calculada por UPPER(TRIM(NOME_AVALIADOR)) |
| NOTA | NUMBER(10), obrigatório, de 1 a 5 |
| COMENTARIO | VARCHAR2(500 CHAR), opcional; vazio é salvo como NULL |
| DATA_AVALIACAO | TIMESTAMP(7), obrigatório; horário UTC |
A constraint `AC_UQ_AVALIADOR_CARD` garante UNIQUE em `(NOME_NORMALIZADO, CODIGO_CARD)`. Nome normalizado é uma coluna técnica, não um atributo adicional da entidade. Se houver uma tabela com estrutura antiga, use um schema destinado a esta versão ou solicite sua migração antes de executar.

## Roteiro de demonstração
1. Execute, informe as credenciais Oracle e confira a preparação da tabela.
2. Avalie o cartão 1 como Ana, com nota 5 e comentário vazio; liste e confira `(sem comentário)`.
3. Tente avaliar o cartão 1 como ANA: a duplicidade deve ser recusada; tente nota 6 e código 0.
4. Avalie o cartão 1 como Bruno, com nota 3; busque pelo código e consulte a média 4,00.
5. Digite texto em um campo numérico e uma opção inexistente; confira as mensagens e o retorno ao menu.
6. Saia com 0, execute novamente e liste para conferir a persistência; consulte um cartão sem avaliações.
