using AtlanticCity.Control.Domain.Loads;
using Microsoft.EntityFrameworkCore;

namespace AtlanticCity.Control.Infrastructure.Persistence;

public sealed class ControlDbContext(
    DbContextOptions<ControlDbContext> options)
    : DbContext(options)
{
    public DbSet<LoadFile> LoadFiles => Set<LoadFile>();

    public DbSet<ProcessedData> ProcessedData => Set<ProcessedData>();

    public DbSet<LoadError> LoadErrors => Set<LoadError>();

    public DbSet<LoadStatusHistory> LoadStatusHistory =>
        Set<LoadStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureLoadFile(modelBuilder);
        ConfigureProcessedData(modelBuilder);
        ConfigureLoadError(modelBuilder);
        ConfigureLoadStatusHistory(modelBuilder);
    }

    private static void ConfigureLoadFile(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LoadFile>();

        entity.ToTable("carga_archivo");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.FileName)
            .HasColumnName("nombre_archivo")
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.StoragePath)
            .HasColumnName("ruta_archivo")
            .HasMaxLength(500);

        entity.Property(x => x.Period)
            .HasColumnName("periodo")
            .HasMaxLength(20);

        entity.Property(x => x.UserId)
            .HasColumnName("usuario_id")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.UserEmail)
            .HasColumnName("usuario_email")
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.Result)
            .HasColumnName("resultado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasColumnName("fecha_registro")
            .IsRequired();

        entity.Property(x => x.ProcessingStartedAt)
            .HasColumnName("fecha_inicio_proceso");

        entity.Property(x => x.LoadedAt)
            .HasColumnName("fecha_cargado");

        entity.Property(x => x.CompletedAt)
            .HasColumnName("fecha_fin");

        entity.Property(x => x.NotifiedAt)
            .HasColumnName("fecha_notificacion");

        entity.Property(x => x.TotalRows)
            .HasColumnName("total_registros");

        entity.Property(x => x.ValidRows)
            .HasColumnName("registros_validos");

        entity.Property(x => x.InsertedRows)
            .HasColumnName("registros_insertados");

        entity.Property(x => x.ExistingRows)
            .HasColumnName("registros_existentes");

        entity.Property(x => x.InvalidRows)
            .HasColumnName("registros_invalidos");

        entity.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ErrorMessage)
            .HasColumnName("mensaje_error")
            .HasMaxLength(2000);

        entity.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_carga_archivo_fecha_registro");

        entity.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_carga_archivo_usuario_id");

        entity.HasIndex(x => x.Period)
            .HasDatabaseName("ix_carga_archivo_periodo");
    }

    private static void ConfigureProcessedData(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ProcessedData>();

        entity.ToTable("data_procesada");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.LoadId)
            .HasColumnName("carga_id");

        entity.Property(x => x.SourceRowNumber)
            .HasColumnName("numero_fila");

        entity.Property(x => x.Period)
            .HasColumnName("periodo")
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(x => x.ProductCode)
            .HasColumnName("codigo_producto")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ProductName)
            .HasColumnName("nombre_producto")
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.Category)
            .HasColumnName("categoria")
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(x => x.Quantity)
            .HasColumnName("cantidad");

        entity.Property(x => x.Price)
            .HasColumnName("precio")
            .HasPrecision(18, 2);

        entity.Property(x => x.CreatedAt)
            .HasColumnName("fecha_registro");

        entity.HasIndex(x => x.ProductCode)
            .IsUnique()
            .HasDatabaseName("ux_data_procesada_codigo_producto");

        entity.HasIndex(x => new
        {
            x.LoadId,
            x.SourceRowNumber
        })
            .IsUnique()
            .HasDatabaseName("ux_data_procesada_carga_fila");

        entity.HasOne<LoadFile>()
            .WithMany()
            .HasForeignKey(x => x.LoadId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLoadError(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LoadError>();

        entity.ToTable("detalle_carga_error");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.LoadId)
            .HasColumnName("carga_id");

        entity.Property(x => x.RowNumber)
            .HasColumnName("numero_fila");

        entity.Property(x => x.ErrorCode)
            .HasColumnName("codigo_error")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Field)
            .HasColumnName("campo")
            .HasMaxLength(100);

        entity.Property(x => x.Message)
            .HasColumnName("mensaje")
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(x => x.RawData)
            .HasColumnName("datos_originales")
            .HasColumnType("jsonb");

        entity.Property(x => x.CreatedAt)
            .HasColumnName("fecha_registro");

        entity.HasIndex(x => x.LoadId)
            .HasDatabaseName("ix_detalle_carga_error_carga_id");

        entity.HasOne<LoadFile>()
            .WithMany()
            .HasForeignKey(x => x.LoadId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLoadStatusHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LoadStatusHistory>();

        entity.ToTable("historial_estado_carga");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.LoadId)
            .HasColumnName("carga_id");

        entity.Property(x => x.Status)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.Property(x => x.Result)
            .HasColumnName("resultado")
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.Property(x => x.Message)
            .HasColumnName("mensaje")
            .HasMaxLength(1000);

        entity.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100);

        entity.Property(x => x.OccurredAt)
            .HasColumnName("fecha_evento");

        entity.HasIndex(x => new
        {
            x.LoadId,
            x.OccurredAt
        })
            .HasDatabaseName("ix_historial_estado_carga_carga_fecha");

        entity.HasOne<LoadFile>()
            .WithMany()
            .HasForeignKey(x => x.LoadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}