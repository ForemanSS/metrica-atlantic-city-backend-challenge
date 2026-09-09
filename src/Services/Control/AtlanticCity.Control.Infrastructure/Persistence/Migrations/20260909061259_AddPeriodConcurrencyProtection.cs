using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlanticCity.Control.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodConcurrencyProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                ADD CONSTRAINT ck_carga_archivo_estado
                CHECK (
                    estado IN (
                        'Pending',
                        'Processing',
                        'Loaded',
                        'Completed',
                        'Notified'
                    )
                );
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                ADD CONSTRAINT ck_carga_archivo_resultado
                CHECK (
                    resultado IN (
                        'Pending',
                        'Success',
                        'Partial',
                        'Rejected',
                        'Failed'
                    )
                );
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                ADD CONSTRAINT ck_carga_archivo_totales_no_negativos
                CHECK (
                    total_registros >= 0
                    AND registros_validos >= 0
                    AND registros_insertados >= 0
                    AND registros_existentes >= 0
                    AND registros_invalidos >= 0
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX ux_carga_archivo_periodo_bloqueante
                ON carga_archivo(periodo)
                WHERE periodo IS NOT NULL
                  AND estado IN (
                      'Pending',
                      'Processing',
                      'Loaded',
                      'Completed',
                      'Notified'
                  );
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE PROCEDURE sp_try_reserve_load_period(
                    IN p_load_id uuid,
                    IN p_period varchar,
                    OUT "ResultCode" varchar,
                    OUT "ConflictingLoadId" uuid,
                    OUT "ConflictingStatus" varchar
                )
                LANGUAGE plpgsql
                AS $procedure$
                DECLARE
                    v_current_status varchar(30);
                    v_current_period varchar(20);

                    v_conflicting_load_id uuid;
                    v_conflicting_status varchar(30);
                BEGIN
                    "ResultCode" := NULL;
                    "ConflictingLoadId" := NULL;
                    "ConflictingStatus" := NULL;

                    IF p_period IS NULL OR btrim(p_period) = '' THEN
                        RAISE EXCEPTION 'Period cannot be null or empty.';
                    END IF;

                    /*
                     * Advisory transaction lock.
                     *
                     * All attempts for the same period use the same lock key.
                     * Different periods can continue concurrently.
                     */
                    PERFORM pg_advisory_xact_lock(
                        hashtextextended(btrim(p_period), 0)
                    );

                    /*
                     * Lock the current load record while evaluating its state.
                     */
                    SELECT
                        estado,
                        periodo
                    INTO
                        v_current_status,
                        v_current_period
                    FROM carga_archivo
                    WHERE id = p_load_id
                    FOR UPDATE;

                    IF NOT FOUND THEN
                        "ResultCode" := 'LOAD_NOT_FOUND';
                        RETURN;
                    END IF;

                    /*
                     * Idempotent reservation.
                     *
                     * If the same load already owns this period and is processing,
                     * a redelivered command does not create a second reservation.
                     */
                    IF v_current_status = 'Processing'
                       AND v_current_period = btrim(p_period) THEN

                        "ResultCode" := 'ALREADY_RESERVED';
                        RETURN;
                    END IF;

                    IF v_current_status <> 'Pending' THEN
                        "ResultCode" := 'INVALID_STATUS';
                        RETURN;
                    END IF;

                    /*
                     * Detect another load using the same period.
                     */
                    SELECT
                        id,
                        estado
                    INTO
                        v_conflicting_load_id,
                        v_conflicting_status
                    FROM carga_archivo
                    WHERE id <> p_load_id
                      AND periodo = btrim(p_period)
                      AND estado IN (
                          'Pending',
                          'Processing',
                          'Loaded',
                          'Completed',
                          'Notified'
                      )
                    ORDER BY fecha_registro DESC
                    LIMIT 1;

                    IF FOUND THEN
                        "ConflictingLoadId" := v_conflicting_load_id;
                        "ConflictingStatus" := v_conflicting_status;

                        IF v_conflicting_status IN (
                            'Pending',
                            'Processing'
                        ) THEN
                            "ResultCode" := 'BLOCKED';
                        ELSE
                            "ResultCode" := 'REJECTED';
                        END IF;

                        RETURN;
                    END IF;

                    /*
                     * Reservation and transition to Processing occur atomically.
                     */
                    UPDATE carga_archivo
                    SET
                        periodo = btrim(p_period),
                        estado = 'Processing',
                        fecha_inicio_proceso =
                            COALESCE(fecha_inicio_proceso, CURRENT_TIMESTAMP)
                    WHERE id = p_load_id;

                    "ResultCode" := 'RESERVED';
                END;
                $procedure$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP PROCEDURE IF EXISTS
                sp_try_reserve_load_period(
                    uuid,
                    character varying
                );
                """);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS
                ux_carga_archivo_periodo_bloqueante;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                DROP CONSTRAINT IF EXISTS
                ck_carga_archivo_totales_no_negativos;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                DROP CONSTRAINT IF EXISTS
                ck_carga_archivo_resultado;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE carga_archivo
                DROP CONSTRAINT IF EXISTS
                ck_carga_archivo_estado;
                """);
        }
    }
}
