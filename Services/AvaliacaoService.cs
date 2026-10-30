
using System.ComponentModel.DataAnnotations;
using AvaliacaoCards.Data;
using AvaliacaoCards.Entities;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace AvaliacaoCards.Services;

public class AvaliacaoService
{
    private readonly BancoContext _context;

    public AvaliacaoService(BancoContext context)
    {
        _context = context;
    }

    public async Task Cadastrar(Avaliacao avaliacao)
    {
        // garante que o identificador seja gerado pelo banco
        avaliacao.Id = 0;

        // remove espaços desnecessários e registra a data em UTC
        avaliacao.NomeAvaliador = avaliacao.NomeAvaliador.Trim();
        avaliacao.Data = DateTime.UtcNow;

        // converte comentários vazios em null para evitar armazenar conteúdo desnecessário
        if (string.IsNullOrWhiteSpace(avaliacao.Comentario))
        {
            avaliacao.Comentario = null;
        }
        else
        {
            avaliacao.Comentario = avaliacao.Comentario.Trim();
        }

        // executa as validações definidas na entidade antes de acessar o banco
        Validator.ValidateObject(avaliacao, new ValidationContext(avaliacao), true);

        // normaliza o nome para identificar avaliações duplicadas sem diferenciar maiúsculas e minúsculas
        string nomeNormalizado = avaliacao.NomeAvaliador.ToUpperInvariant();

        bool jaAvaliou = await _context.Avaliacoes.AnyAsync(a =>
            a.CodigoCard == avaliacao.CodigoCard && a.NomeAvaliador.ToUpper() == nomeNormalizado);

        if (jaAvaliou)
        {
            throw new ArgumentException(MensagensValidacao.AvaliacaoDuplicada);
        }

        _context.Avaliacoes.Add(avaliacao);

        try
        {
            // salva a avaliação no banco de forma assíncrona
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException erro)
        {
            // limpa o rastreamento para evitar manter alterações pendentes após uma falha
            _context.ChangeTracker.Clear();

            // trata a violação de chave única do Oracle como uma avaliação duplicada
            if (erro.InnerException is OracleException oracle && oracle.Number == 1)
            {
                throw new ArgumentException(MensagensValidacao.AvaliacaoDuplicada);
            }

            throw;
        }
        catch (Exception)
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    // lista todas as avaliações sem rastrear alterações, pois os dados são apenas consultados
    public async Task<List<Avaliacao>> Listar()
    {
        return await _context.Avaliacoes
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<List<Avaliacao>> BuscarPorCard(int codigoCard)
    {
        // impede consultas com códigos de cartão inválidos
        if (codigoCard <= 0)
        {
            throw new ArgumentException(MensagensValidacao.CodigoCardInvalido);
        }

        // retorna somente as avaliações do cartão informado, sem rastrear alterações
        return await _context.Avaliacoes
            .AsNoTracking()
            .Where(a => a.CodigoCard == codigoCard)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<double?> CalcularMedia(int codigoCard)
    {
        // reutiliza a busca para validar o código e recuperar as avaliações do cartão
        var avaliacoes = await BuscarPorCard(codigoCard);

        // retorna null quando não existem notas para calcular a média
        if (avaliacoes.Count == 0)
        {
            return null;
        }

        return avaliacoes.Average(a => a.Nota);
    }
}
