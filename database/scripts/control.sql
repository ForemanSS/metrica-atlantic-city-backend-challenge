CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE carga_archivo (
    id uuid NOT NULL,
    nombre_archivo character varying(200) NOT NULL,
    ruta_archivo character varying(500),
    periodo character varying(20),
    usuario_id character varying(100) NOT NULL,
    usuario_email character varying(150) NOT NULL,
    estado character varying(30) NOT NULL,
    resultado character varying(30) NOT NULL,
    fecha_registro timestamp with time zone NOT NULL,
    fecha_inicio_proceso timestamp with time zone,
    fecha_cargado timestamp with time zone,
    fecha_fin timestamp with time zone,
    fecha_notificacion timestamp with time zone,
    total_registros integer NOT NULL,
    registros_validos integer NOT NULL,
    registros_insertados integer NOT NULL,
    registros_existentes integer NOT NULL,
    registros_invalidos integer NOT NULL,
    correlation_id character varying(100) NOT NULL,
    mensaje_error character varying(2000),
    CONSTRAINT "PK_carga_archivo" PRIMARY KEY (id)
);

CREATE TABLE data_procesada (
    id uuid NOT NULL,
    carga_id uuid NOT NULL,
    numero_fila integer NOT NULL,
    periodo character varying(20) NOT NULL,
    codigo_producto character varying(100) NOT NULL,
    nombre_producto character varying(250) NOT NULL,
    descripcion character varying(500) NOT NULL,
    categoria character varying(150) NOT NULL,
    cantidad integer NOT NULL,
    precio numeric(18,2) NOT NULL,
    fecha_registro timestamp with time zone NOT NULL,
    CONSTRAINT "PK_data_procesada" PRIMARY KEY (id),
    CONSTRAINT "FK_data_procesada_carga_archivo_carga_id" FOREIGN KEY (carga_id) REFERENCES carga_archivo (id) ON DELETE CASCADE
);

CREATE TABLE detalle_carga_error (
    id uuid NOT NULL,
    carga_id uuid NOT NULL,
    numero_fila integer,
    codigo_error character varying(100) NOT NULL,
    campo character varying(100),
    mensaje character varying(1000) NOT NULL,
    datos_originales jsonb,
    fecha_registro timestamp with time zone NOT NULL,
    CONSTRAINT "PK_detalle_carga_error" PRIMARY KEY (id),
    CONSTRAINT "FK_detalle_carga_error_carga_archivo_carga_id" FOREIGN KEY (carga_id) REFERENCES carga_archivo (id) ON DELETE CASCADE
);

CREATE TABLE historial_estado_carga (
    id uuid NOT NULL,
    carga_id uuid NOT NULL,
    estado character varying(30) NOT NULL,
    resultado character varying(30) NOT NULL,
    mensaje character varying(1000),
    correlation_id character varying(100) NOT NULL,
    fecha_evento timestamp with time zone NOT NULL,
    CONSTRAINT "PK_historial_estado_carga" PRIMARY KEY (id),
    CONSTRAINT "FK_historial_estado_carga_carga_archivo_carga_id" FOREIGN KEY (carga_id) REFERENCES carga_archivo (id) ON DELETE CASCADE
);

CREATE INDEX ix_carga_archivo_fecha_registro ON carga_archivo (fecha_registro);

CREATE INDEX ix_carga_archivo_periodo ON carga_archivo (periodo);

CREATE INDEX ix_carga_archivo_usuario_id ON carga_archivo (usuario_id);

CREATE UNIQUE INDEX ux_data_procesada_carga_fila ON data_procesada (carga_id, numero_fila);

CREATE UNIQUE INDEX ux_data_procesada_codigo_producto ON data_procesada (codigo_producto);

CREATE INDEX ix_detalle_carga_error_carga_id ON detalle_carga_error (carga_id);

CREATE INDEX ix_historial_estado_carga_carga_fecha ON historial_estado_carga (carga_id, fecha_evento);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260909052539_InitialLoadSchema', '9.0.20');

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

ALTER TABLE carga_archivo
ADD CONSTRAINT ck_carga_archivo_totales_no_negativos
CHECK (
    total_registros >= 0
    AND registros_validos >= 0
    AND registros_insertados >= 0
    AND registros_existentes >= 0
    AND registros_invalidos >= 0
);

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260909061259_AddPeriodConcurrencyProtection', '9.0.20');

COMMIT;

