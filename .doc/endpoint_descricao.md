Segue uma lista inicial de endpoints para o ANI Core. São propostas para detalhar e aprovar com o desenvolvedor; a matriz de permissões ainda será definida pelas áreas.
Usuários
Método e rota	Finalidade
GET /api/v1/usuarios	Listar usuários, com paginação e filtros aprovados
POST /api/v1/usuarios	Cadastrar usuário
GET /api/v1/usuarios/{id}	Consultar um usuário
PUT /api/v1/usuarios/{id}	Atualizar os dados permitidos
DELETE /api/v1/usuarios/{id}	Inativar/remover logicamente, preservando histórico
GET /api/v1/usuarios/{id}/perfis	Consultar perfis atribuídos
POST /api/v1/usuarios/{id}/perfis	Atribuir um perfil
DELETE /api/v1/usuarios/{id}/perfis/{perfilId}	Revogar a atribuição do perfil
GET /api/v1/usuarios/{id}/permissoes-efetivas	Consultar permissões resultantes dos perfis


Perfis e permissões
Método e rota	Finalidade
GET /api/v1/perfis	Listar perfis
POST /api/v1/perfis	Criar perfil
GET /api/v1/perfis/{id}	Consultar perfil
PUT /api/v1/perfis/{id}	Atualizar perfil
DELETE /api/v1/perfis/{id}	Desativar perfil, preservando histórico
GET /api/v1/permissoes	Listar permissões
POST /api/v1/permissoes	Cadastrar permissão
GET /api/v1/permissoes/{id}	Consultar permissão
PUT /api/v1/permissoes/{id}	Atualizar permissão
DELETE /api/v1/permissoes/{id}	Desativar permissão
GET /api/v1/perfis/{id}/permissoes	Consultar permissões do perfil
POST /api/v1/perfis/{id}/permissoes	Associar uma permissão ao perfil
DELETE /api/v1/perfis/{id}/permissoes/{permissaoId}	Revogar permissão do perfil


Login e usuário autenticado
Como o navegador usará Angular → BFF → ANI Core, as rotas de sessão expostas ao Angular devem ficar no BFF. Uma proposta para o BFF:
- POST /api/sessao/login
- POST /api/sessao/logout
- GET /api/sessao
- GET /api/sessao/me
O Core precisará oferecer ao BFF uma forma segura de validar as credenciais e obter a identidade/permissões do usuário. O contrato interno entre BFF e Core ainda precisa ser definido. Não exponha hash de senha nem confie no perfil enviado pelo navegador.
Auditoria
Método e rota	Finalidade
GET /api/v1/auditoria	Consultar eventos, com filtros e paginação aprovados
GET /api/v1/auditoria/{id}	Consultar um evento


O Core deve registrar inclusão, alteração, remoção e revogação conforme a política definida. O acesso à auditoria também precisa de autorização específica.
Ordem sugerida para a primeira entrega
1. Login/sessão BFF-Core, depois de definir os controles de senha e sessão.
2. Consulta e cadastro de usuários.
3. Consulta e manutenção de perfis.
4. Associação de perfis a usuários.
5. Catálogo e associação de permissões.
6. Auditoria e consulta de permissões efetivas.