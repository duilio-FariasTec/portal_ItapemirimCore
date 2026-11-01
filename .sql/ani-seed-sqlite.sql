-- ============================================================================
-- ANI Core — Script SQLite de Criação de Tabelas e Inserts Padrões (Offline)
-- Executa diretamente no arquivo ani.db sem precisar subir a API (.NET)
-- Senha padrão dos usuários criados: 12345678
-- ============================================================================

PRAGMA foreign_keys = ON;
BEGIN TRANSACTION;

-- ----------------------------------------------------------------------------
-- 1. Criação das Tabelas (Compatível com EF Core / SQLite)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Usuario" (
    "UsuarioId" INTEGER NOT NULL CONSTRAINT "PK_Usuario" PRIMARY KEY AUTOINCREMENT,
    "UsuarioGuid" TEXT NOT NULL,
    "Nome" TEXT NOT NULL,
    "Login" TEXT NOT NULL,
    "Email" TEXT NULL,
    "SenhaHash" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Ativo',
    "ExigeTrocaSenhaNoProximoLogin" INTEGER NOT NULL DEFAULT 0,
    "CriadoEmUtc" TEXT NOT NULL,
    "AtualizadoEmUtc" TEXT NULL,
    CONSTRAINT "CK_Usuario_Status" CHECK ("Status" IN ('Ativo', 'Inativo', 'Bloqueado'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Usuario_UsuarioGuid" ON "Usuario" ("UsuarioGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Usuario_Login" ON "Usuario" ("Login");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Usuario_Email" ON "Usuario" ("Email") WHERE "Email" IS NOT NULL;

CREATE TABLE IF NOT EXISTS "Sistema" (
    "SistemaId" INTEGER NOT NULL CONSTRAINT "PK_Sistema" PRIMARY KEY AUTOINCREMENT,
    "SistemaGuid" TEXT NOT NULL,
    "Codigo" TEXT NOT NULL,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NULL,
    "Ativo" INTEGER NOT NULL DEFAULT 1,
    "CriadoEmUtc" TEXT NOT NULL,
    "AtualizadoEmUtc" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Sistema_SistemaGuid" ON "Sistema" ("SistemaGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Sistema_Codigo" ON "Sistema" ("Codigo");

CREATE TABLE IF NOT EXISTS "Perfil" (
    "PerfilId" INTEGER NOT NULL CONSTRAINT "PK_Perfil" PRIMARY KEY AUTOINCREMENT,
    "PerfilGuid" TEXT NOT NULL,
    "SistemaId" INTEGER NOT NULL,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NULL,
    "Ativo" INTEGER NOT NULL DEFAULT 1,
    "CriadoEmUtc" TEXT NOT NULL,
    "AtualizadoEmUtc" TEXT NULL,
    CONSTRAINT "FK_Perfil_Sistema" FOREIGN KEY ("SistemaId") REFERENCES "Sistema" ("SistemaId") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Perfil_PerfilGuid" ON "Perfil" ("PerfilGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Perfil_Sistema_Nome" ON "Perfil" ("SistemaId", "Nome");
CREATE INDEX IF NOT EXISTS "IX_Perfil_SistemaId" ON "Perfil" ("SistemaId");

CREATE TABLE IF NOT EXISTS "Permissao" (
    "PermissaoId" INTEGER NOT NULL CONSTRAINT "PK_Permissao" PRIMARY KEY AUTOINCREMENT,
    "PermissaoGuid" TEXT NOT NULL,
    "Codigo" TEXT NOT NULL,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NULL,
    "Recurso" TEXT NOT NULL,
    "Acao" TEXT NOT NULL,
    "DscUrl" TEXT NULL,
    "Ativo" INTEGER NOT NULL DEFAULT 1,
    "CriadoEmUtc" TEXT NOT NULL,
    "AtualizadoEmUtc" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Permissao_PermissaoGuid" ON "Permissao" ("PermissaoGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Permissao_Codigo" ON "Permissao" ("Codigo");

CREATE TABLE IF NOT EXISTS "UsuarioPerfil" (
    "UsuarioPerfilId" INTEGER NOT NULL CONSTRAINT "PK_UsuarioPerfil" PRIMARY KEY AUTOINCREMENT,
    "UsuarioPerfilGuid" TEXT NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "PerfilId" INTEGER NOT NULL,
    "SistemaId" INTEGER NOT NULL,
    "ConcedidoPorUsuarioId" INTEGER NULL,
    "ConcedidoEmUtc" TEXT NOT NULL,
    "RevogadoPorUsuarioId" INTEGER NULL,
    "RevogadoEmUtc" TEXT NULL,
    CONSTRAINT "FK_UsuarioPerfil_Usuario" FOREIGN KEY ("UsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UsuarioPerfil_Perfil" FOREIGN KEY ("PerfilId") REFERENCES "Perfil" ("PerfilId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UsuarioPerfil_Sistema" FOREIGN KEY ("SistemaId") REFERENCES "Sistema" ("SistemaId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UsuarioPerfil_ConcedidoPor" FOREIGN KEY ("ConcedidoPorUsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UsuarioPerfil_RevogadoPor" FOREIGN KEY ("RevogadoPorUsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_UsuarioPerfil_UsuarioPerfilGuid" ON "UsuarioPerfil" ("UsuarioPerfilGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_UsuarioPerfil_UsuarioSistema_Ativo" ON "UsuarioPerfil" ("UsuarioId", "SistemaId") WHERE "RevogadoEmUtc" IS NULL;
CREATE INDEX IF NOT EXISTS "IX_UsuarioPerfil_PerfilId" ON "UsuarioPerfil" ("PerfilId", "UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_UsuarioPerfil_SistemaId" ON "UsuarioPerfil" ("SistemaId");

CREATE TABLE IF NOT EXISTS "PerfilPermissao" (
    "PerfilPermissaoId" INTEGER NOT NULL CONSTRAINT "PK_PerfilPermissao" PRIMARY KEY AUTOINCREMENT,
    "PerfilPermissaoGuid" TEXT NOT NULL,
    "PerfilId" INTEGER NOT NULL,
    "PermissaoId" INTEGER NOT NULL,
    "ConcedidoPorUsuarioId" INTEGER NULL,
    "ConcedidoEmUtc" TEXT NOT NULL,
    "RevogadoPorUsuarioId" INTEGER NULL,
    "RevogadoEmUtc" TEXT NULL,
    CONSTRAINT "FK_PerfilPermissao_Perfil" FOREIGN KEY ("PerfilId") REFERENCES "Perfil" ("PerfilId") ON DELETE RESTRICT,
    CONSTRAINT "FK_PerfilPermissao_Permissao" FOREIGN KEY ("PermissaoId") REFERENCES "Permissao" ("PermissaoId") ON DELETE RESTRICT,
    CONSTRAINT "FK_PerfilPermissao_ConcedidoPor" FOREIGN KEY ("ConcedidoPorUsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT,
    CONSTRAINT "FK_PerfilPermissao_RevogadoPor" FOREIGN KEY ("RevogadoPorUsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_PerfilPermissao_PerfilPermissaoGuid" ON "PerfilPermissao" ("PerfilPermissaoGuid");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PerfilPermissao_Ativa" ON "PerfilPermissao" ("PerfilId", "PermissaoId") WHERE "RevogadoEmUtc" IS NULL;
CREATE INDEX IF NOT EXISTS "IX_PerfilPermissao_PermissaoId" ON "PerfilPermissao" ("PermissaoId", "PerfilId");

CREATE TABLE IF NOT EXISTS "Auditoria" (
    "AuditoriaId" INTEGER NOT NULL CONSTRAINT "PK_Auditoria" PRIMARY KEY AUTOINCREMENT,
    "AuditoriaGuid" TEXT NOT NULL,
    "AtorUsuarioId" INTEGER NULL,
    "Acao" TEXT NOT NULL,
    "Entidade" TEXT NOT NULL,
    "EntidadeId" TEXT NOT NULL,
    "Resultado" TEXT NOT NULL,
    "Origem" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "OcorridoEmUtc" TEXT NOT NULL,
    CONSTRAINT "FK_Auditoria_Ator" FOREIGN KEY ("AtorUsuarioId") REFERENCES "Usuario" ("UsuarioId") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Auditoria_AuditoriaGuid" ON "Auditoria" ("AuditoriaGuid");
CREATE INDEX IF NOT EXISTS "IX_Auditoria_OcorridoEmUtc" ON "Auditoria" ("OcorridoEmUtc" DESC);
CREATE INDEX IF NOT EXISTS "IX_Auditoria_Ator_OcorridoEmUtc" ON "Auditoria" ("AtorUsuarioId", "OcorridoEmUtc");

-- ----------------------------------------------------------------------------
-- 2. Inserts Padrões — Sistemas
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "Sistema" ("SistemaGuid", "Codigo", "Nome", "Descricao", "Ativo", "CriadoEmUtc")
VALUES
    ('bd723b78-139c-49c6-a64c-f8693be4ab35', 'PORTAL_ADMIN', 'Portal Corporativo ANI', 'Governança, gestão de acessos corporativos, usuários e auditoria centralizada', 1, datetime('now')),
    ('eebdb74a-a457-4d7d-aef9-5bf0861846e9', 'PORTAL_FINANCEIRO', 'Portal Financeiro', 'Gestão financeira, faturamento, conciliação e relatórios fiscais', 1, datetime('now')),
    ('a6d6ec10-1e9b-4191-be7a-9a5037391bbc', 'PORTAL_PASSAGENS', 'Portal de Passagens e Reservas', 'Emissão de bilhetes, controle de linhas, rotas e tarifas', 1, datetime('now'));

-- ----------------------------------------------------------------------------
-- 3. Inserts Padrões — Permissões
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "Permissao" ("PermissaoGuid", "Codigo", "Nome", "Descricao", "Recurso", "Acao", "DscUrl", "Ativo", "CriadoEmUtc")
VALUES
    ('8c85e5af-4f2d-417a-a2ab-034bdb3a58b2', 'home.view', 'Acesso à tela Home', 'Acesso à tela Home', 'Home', 'Visualizar', '/home', 1, datetime('now')),
    ('21620730-66c2-4d8b-8785-e94723ff221d', 'precos.view', 'Acesso à consulta/gestão de Preços', 'Acesso à consulta e gestão da tabela de preços', 'Precos', 'Visualizar', '/precos', 1, datetime('now')),
    ('794069c7-bc35-4c3b-b24d-7575b2e5ef90', 'passagens.emitir', 'Emissão e Reserva de Passagens', 'Permite emitir e reservar passagens rodoviárias', 'Passagens', 'Emitir', '/passagens', 1, datetime('now')),
    ('a35db673-d079-40d5-bb8c-50384de70fb3', 'usuarios.manage', 'Acesso à gestão de Usuários', 'Acesso completo à gestão de usuários, perfis e permissões', 'Usuarios', 'Gerenciar', '/usuarios', 1, datetime('now')),
    ('e2d9f50d-488d-4cec-927d-fd4c01cc42b1', 'relatorios.view', 'Visualização de Relatórios', 'Acesso a relatórios analíticos e dashboards executivos', 'Relatorios', 'Visualizar', '/relatorios', 1, datetime('now'));

-- ----------------------------------------------------------------------------
-- 4. Inserts Padrões — Perfis por Sistema
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "Perfil" ("PerfilGuid", "SistemaId", "Nome", "Descricao", "Ativo", "CriadoEmUtc")
VALUES
    ('a2360b4c-787b-472d-9498-7d6b7e4422e2', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_ADMIN'), 'Administrador', 'Super Administrador com governança do ANI Core', 1, datetime('now')),
    ('36397b4d-de7c-44c2-a24b-eb8b0b3ecfaa', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_ADMIN'), 'Auditor', 'Auditor de segurança e conformidade', 1, datetime('now')),
    ('9bc57726-a798-4659-9575-48423528fd67', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_FINANCEIRO'), 'Administrador', 'Administrador Geral do Portal Financeiro', 1, datetime('now')),
    ('0bb820a2-3552-416b-ac09-153b2df3c487', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_FINANCEIRO'), 'Analista Financeiro', 'Analista operacional de finanças e precificação', 1, datetime('now')),
    ('6a123b31-428f-4ce3-99d3-a511fede57ee', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_PASSAGENS'), 'Administrador', 'Administrador de Bilhetagem e Operação de Passagens', 1, datetime('now')),
    ('1904b8a8-5d6d-451b-9952-d77cc65426bd', (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_PASSAGENS'), 'Operador de Vendas', 'Emissor de bilhetes e reservas em guichês e web', 1, datetime('now'));

-- ----------------------------------------------------------------------------
-- 5. Inserts Padrões — Vínculos Perfil x Permissão
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "PerfilPermissao" ("PerfilPermissaoGuid", "PerfilId", "PermissaoId", "ConcedidoEmUtc")
VALUES
    -- PORTAL_ADMIN:Administrador
    ('11111111-0001-4000-8000-000000000001', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'usuarios.manage'), datetime('now')),
    ('11111111-0002-4000-8000-000000000002', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'relatorios.view'), datetime('now')),
    ('11111111-0003-4000-8000-000000000003', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now')),
    -- PORTAL_ADMIN:Auditor
    ('11111111-0004-4000-8000-000000000004', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Auditor'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'relatorios.view'), datetime('now')),
    ('11111111-0005-4000-8000-000000000005', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Auditor'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now')),
    -- PORTAL_FINANCEIRO:Administrador
    ('11111111-0006-4000-8000-000000000006', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'precos.view'), datetime('now')),
    ('11111111-0007-4000-8000-000000000007', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'relatorios.view'), datetime('now')),
    ('11111111-0008-4000-8000-000000000008', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now')),
    -- PORTAL_FINANCEIRO:Analista Financeiro
    ('11111111-0009-4000-8000-000000000009', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Analista Financeiro'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'precos.view'), datetime('now')),
    ('11111111-0010-4000-8000-000000000010', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Analista Financeiro'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now')),
    -- PORTAL_PASSAGENS:Administrador
    ('11111111-0011-4000-8000-000000000011', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'passagens.emitir'), datetime('now')),
    ('11111111-0012-4000-8000-000000000012', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'precos.view'), datetime('now')),
    ('11111111-0013-4000-8000-000000000013', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'relatorios.view'), datetime('now')),
    ('11111111-0014-4000-8000-000000000014', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Administrador'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now')),
    -- PORTAL_PASSAGENS:Operador de Vendas
    ('11111111-0015-4000-8000-000000000015', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Operador de Vendas'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'passagens.emitir'), datetime('now')),
    ('11111111-0016-4000-8000-000000000016', (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Operador de Vendas'), (SELECT "PermissaoId" FROM "Permissao" WHERE "Codigo" = 'home.view'), datetime('now'));

-- ----------------------------------------------------------------------------
-- 6. Inserts Padrões — Usuários (Senha '12345678' criptografada com ReversibleAes / SecretKey)
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "Usuario" ("UsuarioGuid", "Nome", "Login", "Email", "SenhaHash", "Status", "ExigeTrocaSenhaNoProximoLogin", "CriadoEmUtc")
VALUES
    ('be01292d-fee4-478a-8a87-2917adece1aa', 'Administrador do Sistema', 'admin', 'admin@itapemirim.com.br', 'UpDXvN++HDeyZaNe7obFsg==', 'Ativo', 0, datetime('now')),
    ('8d1728a1-5fdb-4f77-b6d2-aefc08e7b14b', 'Analista Financeiro', 'financeiro', 'financeiro@itapemirim.com.br', 'UpDXvN++HDeyZaNe7obFsg==', 'Ativo', 0, datetime('now')),
    ('9b348be9-34ef-4504-9059-0600bdd3efe1', 'Operador de Bilhetagem', 'operador', 'operador@itapemirim.com.br', 'UpDXvN++HDeyZaNe7obFsg==', 'Ativo', 0, datetime('now')),
    ('1ff7a4f3-feeb-4351-958a-4180493a6a8e', 'Duilio Farias', 'duilio.farias', 'duilio@fariastecnologia.com.br', 'UpDXvN++HDeyZaNe7obFsg==', 'Ativo', 0, datetime('now'));

UPDATE "Usuario"
SET "SenhaHash" = 'UpDXvN++HDeyZaNe7obFsg==',
    "ExigeTrocaSenhaNoProximoLogin" = 0,
    "AtualizadoEmUtc" = datetime('now');

-- ----------------------------------------------------------------------------
-- 7. Inserts Padrões — Vínculos Usuário x Perfil (Multi-sistema)
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO "UsuarioPerfil" ("UsuarioPerfilGuid", "UsuarioId", "PerfilId", "SistemaId", "ConcedidoPorUsuarioId", "ConcedidoEmUtc")
VALUES
    -- admin -> PORTAL_ADMIN:Administrador
    ('22222222-0001-4000-8000-000000000001',
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Administrador'),
        (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_ADMIN'),
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        datetime('now')),
    -- admin -> PORTAL_FINANCEIRO:Administrador
    ('22222222-0002-4000-8000-000000000002',
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_FINANCEIRO' AND p."Nome" = 'Administrador'),
        (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_FINANCEIRO'),
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        datetime('now')),
    -- duilio.farias -> PORTAL_ADMIN:Administrador
    ('22222222-0003-4000-8000-000000000003',
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'duilio.farias'),
        (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_ADMIN' AND p."Nome" = 'Administrador'),
        (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_ADMIN'),
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        datetime('now')),
    -- duilio.farias -> PORTAL_PASSAGENS:Administrador
    ('22222222-0004-4000-8000-000000000004',
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'duilio.farias'),
        (SELECT p."PerfilId" FROM "Perfil" p JOIN "Sistema" s ON s."SistemaId" = p."SistemaId" WHERE s."Codigo" = 'PORTAL_PASSAGENS' AND p."Nome" = 'Administrador'),
        (SELECT "SistemaId" FROM "Sistema" WHERE "Codigo" = 'PORTAL_PASSAGENS'),
        (SELECT "UsuarioId" FROM "Usuario" WHERE "Login" = 'admin'),
        datetime('now'));

COMMIT;

