
using System.ComponentModel.DataAnnotations;

namespace AvaliacaoCards.Entities;

public class Avaliacao
{
    // identificador único gerado pelo banco de dados
    public int Id
    {
        get;
        set;
    }

    // aceita apenas códigos de cartão maiores que zero
    [Range(1, int.MaxValue, ErrorMessage = MensagensValidacao.CodigoCardInvalido)]
    public int CodigoCard
    {
        get;
        set;
    }

    // exige um nome com tamanho entre 2 e 100 caracteres
    [Required(ErrorMessage = MensagensValidacao.NomeObrigatorio)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = MensagensValidacao.NomeInvalido)]
    public string NomeAvaliador
    {
        get;
        set;
    } = string.Empty;

    // permite apenas notas entre 1 e 5
    [Range(1, 5, ErrorMessage = MensagensValidacao.NotaInvalida)]
    public int Nota
    {
        get;
        set;
    }

    // permite comentários opcionais com até 500 caracteres
    [MaxLength(500, ErrorMessage = MensagensValidacao.ComentarioInvalido)]
    public string? Comentario
    {
        get;
        set;
    }

    // armazena a data e o horário em que a avaliação foi registrada
    public DateTime Data
    {
        get;
        set;
    }
}
