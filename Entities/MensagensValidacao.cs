
namespace AvaliacaoCards.Entities;

public static class MensagensValidacao
{
    // validações do cartão
    public const string CodigoCardInvalido =
        "O código do cartão deve ser maior que zero.";

    // validações do avaliador
    public const string NomeObrigatorio =
        "Informe o nome do avaliador.";

    public const string NomeInvalido =
        "O nome deve ter entre 2 e 100 caracteres.";

    public const string AvaliacaoDuplicada =
        "Este avaliador já avaliou este cartão.";

    // validações da avaliação
    public const string NotaInvalida =
        "A nota deve ser de 1 a 5.";

    public const string ComentarioInvalido =
        "O comentário pode ter até 500 caracteres.";

    // validações gerais
    public const string NumeroInvalido =
        "Digite um número inteiro válido.";
}
}
