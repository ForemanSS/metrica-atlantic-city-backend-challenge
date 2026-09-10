CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE auth_user (
    id uuid NOT NULL,
    email character varying(200) NOT NULL,
    display_name character varying(200) NOT NULL,
    password_hash character varying(1000) NOT NULL,
    role character varying(50) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    CONSTRAINT "PK_auth_user" PRIMARY KEY (id)
);

CREATE TABLE auth_refresh_token (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token_hash character varying(128) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    revoked_at timestamp with time zone,
    created_by_ip character varying(100),
    revoked_by_ip character varying(100),
    replaced_by_token_id uuid,
    CONSTRAINT "PK_auth_refresh_token" PRIMARY KEY (id),
    CONSTRAINT "FK_auth_refresh_token_auth_refresh_token_replaced_by_token_id" FOREIGN KEY (replaced_by_token_id) REFERENCES auth_refresh_token (id) ON DELETE SET NULL,
    CONSTRAINT "FK_auth_refresh_token_auth_user_user_id" FOREIGN KEY (user_id) REFERENCES auth_user (id) ON DELETE CASCADE
);

CREATE INDEX ix_auth_refresh_token_expires_at ON auth_refresh_token (expires_at);

CREATE INDEX "IX_auth_refresh_token_replaced_by_token_id" ON auth_refresh_token (replaced_by_token_id);

CREATE INDEX ix_auth_refresh_token_user_id ON auth_refresh_token (user_id);

CREATE UNIQUE INDEX ux_auth_refresh_token_hash ON auth_refresh_token (token_hash);

CREATE UNIQUE INDEX ux_auth_user_email ON auth_user (email);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260909182245_InitialAuthenticationSchema', '9.0.20');

ALTER TABLE auth_refresh_token ADD version bigint NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260910013740_AddRefreshTokenConcurrencyVersion', '9.0.20');

COMMIT;

