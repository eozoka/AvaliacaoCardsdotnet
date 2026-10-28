using AvaliacaoCards.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvaliacaoCards.Data;

public class BancoContext : DbContext
{
    public BancoContext(DbContextOptions<BancoContext> options) : base(options)
    {
    }

    public DbSet<Avaliacao> Avaliacoes
    {
        get;
        set;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var avaliacao = modelBuilder.Entity<Avaliacao>();
        avaliacao.ToTable("AC_AVALIACOES", tabela =>
        {
            tabela.HasCheckConstraint("AC_CK_CARD", "CODIGO_CARD > 0");
            tabela.HasCheckConstraint("AC_CK_NOTA", "NOTA BETWEEN 1 AND 5");
            tabela.HasCheckConstraint("AC_CK_NOME", "LENGTH(TRIM(NOME_AVALIADOR)) BETWEEN 2 AND 100");
        });
        avaliacao.HasKey(a => a.Id).HasName("AC_PK_AVALIACOES");
        avaliacao.Property(a => a.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        avaliacao.Property(a => a.CodigoCard).HasColumnName("CODIGO_CARD");
        avaliacao.Property(a => a.NomeAvaliador).HasColumnName("NOME_AVALIADOR")
            .HasMaxLength(100).IsUnicode(false).IsRequired();
        avaliacao.Property(a => a.Nota).HasColumnName("NOTA");
        avaliacao.Property(a => a.Comentario).HasColumnName("COMENTARIO")
            .HasMaxLength(500).IsUnicode(false).IsRequired(false);
        avaliacao.Property(a => a.Data).HasColumnName("DATA_AVALIACAO");
        avaliacao.Property<string>("NomeNormalizado").HasColumnName("NOME_NORMALIZADO")
            .HasMaxLength(100).IsUnicode(false)
            .HasComputedColumnSql("UPPER(TRIM(NOME_AVALIADOR))", stored: false);
        avaliacao.HasIndex("NomeNormalizado", nameof(Avaliacao.CodigoCard))
            .IsUnique().HasDatabaseName("AC_UQ_AVALIADOR_CARD");
    }
}
