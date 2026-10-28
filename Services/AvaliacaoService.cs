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
        avaliacao.Id = 0;
        avaliacao.NomeAvaliador = avaliacao.NomeAvaliador.Trim();
        avaliacao.Data = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(avaliacao.Comentario))
        {
            avaliacao.Comentario = null;
        }
        else
        {
            avaliacao.Comentario = avaliacao.Comentario.Trim();
        }

        Validator.ValidateObject(avaliacao, new ValidationContext(avaliacao), true);

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
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException erro)
        {
            _context.ChangeTracker.Clear();
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

    public async Task<List<Avaliacao>> Listar()
    {
        return await _context.Avaliacoes.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<List<Avaliacao>> BuscarPorCard(int codigoCard)
    {
        if (codigoCard <= 0)
        {
            throw new ArgumentException(MensagensValidacao.CodigoCardInvalido);
        }

        return await _context.Avaliacoes.AsNoTracking()
            .Where(a => a.CodigoCard == codigoCard).OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<double?> CalcularMedia(int codigoCard)
    {
        var avaliacoes = await BuscarPorCard(codigoCard);
        if (avaliacoes.Count == 0)
        {
            return null;
        }

        return avaliacoes.Average(a => a.Nota);
    }
}
