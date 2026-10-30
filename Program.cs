
using System.ComponentModel.DataAnnotations;
using System.Text;
using AvaliacaoCards.Data;
using AvaliacaoCards.Entities;
using AvaliacaoCards.Services;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

Console.OutputEncoding = Encoding.UTF8;

// mantém a aplicação em execução até o usuário sair ou ocorrer um encerramento
while (true)
{
    try
    {
        using var banco = Conectar();

        // prepara o banco antes de permitir o cadastro de avaliações
        InicializadorBanco.Preparar(banco);

        Console.WriteLine("Banco pronto para registrar avaliações.");

        var avaliacaoService = new AvaliacaoService(banco);

        await ExecutarMenu(avaliacaoService);
        break;
    }
    catch (EndOfStreamException)
    {
        break;
    }
    catch (Exception erro) when (erro is OracleException or DbUpdateException)
    {
        Console.WriteLine("Não foi possível preparar o Oracle. Confira a rede FIAP, as credenciais e a permissão para criar tabela.");
    }
    catch (Exception erro) when (erro is ArgumentException or InvalidOperationException)
    {
        Console.WriteLine(erro.Message);
    }
    catch (Exception)
    {
        Console.WriteLine("Não foi possível iniciar. Tente novamente ou encerre o console.");
    }
}

// solicita as credenciais e configura a conexão com o Oracle
static BancoContext Conectar()
{
    Console.WriteLine("CONEXÃO ORACLE - oracle.fiap.com.br:1521/ORCL");

    string usuario = Ler("Usuário do Oracle: ").Trim();
    string senha = LerSenha("Senha do Oracle: ");

    // impede a tentativa de conexão sem as credenciais obrigatórias
    if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(senha))
    {
        throw new ArgumentException("Informe o usuário e a senha do Oracle.");
    }

    var conexao = new OracleConnectionStringBuilder
    {
        DataSource = "oracle.fiap.com.br:1521/ORCL",
        UserID = usuario,
        Password = senha,
        PersistSecurityInfo = false,
        ConnectionTimeout = 15
    };

    // configura o Entity Framework para utilizar o Oracle com compatibilidade da versão 19
    var options = new DbContextOptionsBuilder<BancoContext>()
        .UseOracle(conexao.ConnectionString, oracle => oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19))
        .Options;

    return new BancoContext(options);
}

// controla as opções disponíveis e direciona cada operação para seu respectivo método
static async Task ExecutarMenu(AvaliacaoService avaliacaoService)
{
    while (true)
    {
        try
        {
            ExibirMenu();

            switch (Ler("Opção: "))
            {
                case "1":
                    await Avaliar(avaliacaoService);
                    break;

                case "2":
                    await ListarTodas(avaliacaoService);
                    break;

                case "3":
                    await BuscarPorCard(avaliacaoService);
                    break;

                case "4":
                    await MostrarMedia(avaliacaoService);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
        catch (EndOfStreamException)
        {
            return;
        }
        catch (Exception erro) when (erro is OracleException or DbUpdateException)
        {
            Console.WriteLine("Não foi possível acessar o Oracle. Confira a conexão e tente novamente.");
        }
        catch (Exception erro) when (erro is ArgumentException or ValidationException or FormatException)
        {
            Console.WriteLine($"Não foi possível concluir: {erro.Message}");
        }
        catch (Exception erro) when (erro is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine("Não foi possível concluir a operação. Tente novamente.");
        }
        catch (Exception)
        {
            Console.WriteLine("Ocorreu um erro inesperado. Tente novamente pelo menu.");
        }
    }
}

// apresenta as operações disponíveis para o usuário
static void ExibirMenu()
{
    Console.WriteLine("\nNEXO NUCLEAR - AVALIAÇÃO DE CARTÕES");
    Console.WriteLine("1 - Avaliar cartão");
    Console.WriteLine("2 - Listar todas as avaliações");
    Console.WriteLine("3 - Buscar por código do cartão");
    Console.WriteLine("4 - Média de um cartão");
    Console.WriteLine("0 - Sair");
}

// coleta os dados e solicita ao serviço o cadastro de uma nova avaliação
static async Task Avaliar(AvaliacaoService avaliacaoService)
{
    int codigo = LerNumero("Código do cartão: ");
    string nome = Ler("Nome do avaliador: ");
    int nota = LerNumero("Nota (1 a 5): ");
    string comentario = Ler("Comentário (opcional): ");

    var avaliacao = new Avaliacao
    {
        CodigoCard = codigo,
        NomeAvaliador = nome,
        Nota = nota,
        Comentario = comentario
    };

    await avaliacaoService.Cadastrar(avaliacao);

    Console.WriteLine("Avaliação cadastrada.");
}

// recupera todas as avaliações cadastradas e envia para exibição
static async Task ListarTodas(AvaliacaoService avaliacaoService)
{
    var avaliacoes = await avaliacaoService.Listar();
    Mostrar(avaliacoes);
}

// busca as avaliações vinculadas ao código informado
static async Task BuscarPorCard(AvaliacaoService avaliacaoService)
{
    int codigo = LerNumero("Código do cartão: ");

    var avaliacoes = await avaliacaoService.BuscarPorCard(codigo);

    Mostrar(avaliacoes);
}

// calcula a média das notas de um cartão e trata a ausência de avaliações
static async Task MostrarMedia(AvaliacaoService avaliacaoService)
{
    int codigo = LerNumero("Código do cartão: ");

    double? media = await avaliacaoService.CalcularMedia(codigo);

    if (media == null)
    {
        Console.WriteLine("Este cartão ainda não tem avaliações.");
        return;
    }

    Console.WriteLine($"Média do cartão: {media.Value:F2}");
}

// exibe as avaliações e trata campos opcionais e conversão de horário
static void Mostrar(List<Avaliacao> avaliacoes)
{
    if (avaliacoes.Count == 0)
    {
        Console.WriteLine("Nenhuma avaliação encontrada.");
        return;
    }

    foreach (var avaliacao in avaliacoes)
    {
        // substitui comentários nulos por um texto padrão
        string comentario = avaliacao.Comentario ?? "(sem comentário)";

        // interpreta a data armazenada como UTC e converte para o horário local
        DateTime dataLocal = DateTime.SpecifyKind(avaliacao.Data, DateTimeKind.Utc).ToLocalTime();

        Console.WriteLine($"Avaliação {avaliacao.Id} | cartão {avaliacao.CodigoCard} | {avaliacao.NomeAvaliador} | nota {avaliacao.Nota} | {dataLocal:dd/MM/yyyy HH:mm} | {comentario}");
    }
}

// lê uma informação digitada e identifica quando a entrada é encerrada
static string Ler(string mensagem)
{
    Console.Write(mensagem);

    return Console.ReadLine() ?? throw new EndOfStreamException();
}

// converte a entrada para inteiro e informa quando o valor não é válido
static int LerNumero(string mensagem)
{
    if (!int.TryParse(Ler(mensagem), out int numero))
    {
        throw new ArgumentException(MensagensValidacao.NumeroInvalido);
    }

    return numero;
}

// captura a senha sem exibir os caracteres digitados no console
static string LerSenha(string mensagem)
{
    Console.Write(mensagem);

    // permite a leitura normal quando a entrada vem de outro processo
    if (Console.IsInputRedirected)
    {
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    var senha = new StringBuilder();

    while (true)
    {
        var tecla = Console.ReadKey(true);

        if (tecla.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return senha.ToString();
        }

        // permite apagar o último caractere digitado
        if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0)
        {
            senha.Length--;
            Console.Write("\b \b");
        }
        // ignora teclas de controle e exibe um asterisco para cada caractere válido
        else if (!char.IsControl(tecla.KeyChar))
        {
            senha.Append(tecla.KeyChar);
            Console.Write('*');
        }
    }
}
