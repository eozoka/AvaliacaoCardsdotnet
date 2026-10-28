using System.ComponentModel.DataAnnotations;

namespace AvaliacaoCards.Entities;

public class Avaliacao
{
    public int Id
    {
        get;
        set;
    }

    [Range(1, int.MaxValue, ErrorMessage = MensagensValidacao.CodigoCardInvalido)]
    public int CodigoCard
    {
        get;
        set;
    }

    [Required(ErrorMessage = MensagensValidacao.NomeObrigatorio)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = MensagensValidacao.NomeInvalido)]
    public string NomeAvaliador
    {
        get;
        set;
    } = string.Empty;

    [Range(1, 5, ErrorMessage = MensagensValidacao.NotaInvalida)]
    public int Nota
    {
        get;
        set;
    }

    [MaxLength(500, ErrorMessage = MensagensValidacao.ComentarioInvalido)]
    public string? Comentario
    {
        get;
        set;
    }

    public DateTime Data
    {
        get;
        set;
    }
}
