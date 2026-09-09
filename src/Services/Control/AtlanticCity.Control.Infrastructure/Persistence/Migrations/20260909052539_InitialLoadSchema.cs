using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlanticCity.Control.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLoadSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carga_archivo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ruta_archivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resultado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_inicio_proceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_cargado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_notificacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    total_registros = table.Column<int>(type: "integer", nullable: false),
                    registros_validos = table.Column<int>(type: "integer", nullable: false),
                    registros_insertados = table.Column<int>(type: "integer", nullable: false),
                    registros_existentes = table.Column<int>(type: "integer", nullable: false),
                    registros_invalidos = table.Column<int>(type: "integer", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mensaje_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carga_archivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_procesada",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_fila = table.Column<int>(type: "integer", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    categoria = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_procesada", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_procesada_carga_archivo_carga_id",
                        column: x => x.carga_id,
                        principalTable: "carga_archivo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_carga_error",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_fila = table.Column<int>(type: "integer", nullable: true),
                    codigo_error = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    datos_originales = table.Column<string>(type: "jsonb", nullable: true),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_carga_error", x => x.id);
                    table.ForeignKey(
                        name: "FK_detalle_carga_error_carga_archivo_carga_id",
                        column: x => x.carga_id,
                        principalTable: "carga_archivo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historial_estado_carga",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resultado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_evento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_estado_carga", x => x.id);
                    table.ForeignKey(
                        name: "FK_historial_estado_carga_carga_archivo_carga_id",
                        column: x => x.carga_id,
                        principalTable: "carga_archivo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_carga_archivo_fecha_registro",
                table: "carga_archivo",
                column: "fecha_registro");

            migrationBuilder.CreateIndex(
                name: "ix_carga_archivo_periodo",
                table: "carga_archivo",
                column: "periodo");

            migrationBuilder.CreateIndex(
                name: "ix_carga_archivo_usuario_id",
                table: "carga_archivo",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_data_procesada_carga_fila",
                table: "data_procesada",
                columns: new[] { "carga_id", "numero_fila" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_data_procesada_codigo_producto",
                table: "data_procesada",
                column: "codigo_producto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_detalle_carga_error_carga_id",
                table: "detalle_carga_error",
                column: "carga_id");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_carga_carga_fecha",
                table: "historial_estado_carga",
                columns: new[] { "carga_id", "fecha_evento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_procesada");

            migrationBuilder.DropTable(
                name: "detalle_carga_error");

            migrationBuilder.DropTable(
                name: "historial_estado_carga");

            migrationBuilder.DropTable(
                name: "carga_archivo");
        }
    }
}
