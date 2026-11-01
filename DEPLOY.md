# Guia de Deploy e Conteinerização (Docker & AWS ECS Express Mode)

Este documento centraliza todas as configurações, comandos e passos operacionais para compilar, testar localmente em Docker e publicar a API **ANI Core — Portal Itapemirim** na nuvem da **Amazon Web Services (AWS)** utilizando o **Amazon ECR** e o **Amazon ECS Express Mode**.

---

## 📌 1. Visão Geral da Arquitetura de Deploy

```mermaid
flowchart LR
    Dev[Código Local / Git] -->|docker build| Docker[Imagem Docker .NET 10\ncom ani.db embutido]
    Docker -->|docker push| ECR[Amazon ECR\n107189286518.../portal-itapemirim-ani-api]
    ECR -->|Deploy Inicial ou Redeploy| ECS[Amazon ECS Express Mode\nFargate Serverless]
    ECS -->|Health Check 200 OK| Health[/health]
    ECS -->|HTTPS Automático| PublicURL[URL Pública Segura AWS\nSwagger / APIs / Telas]
```

### Detalhes Técnicos da Imagem:
* **Framework**: .NET 10 (C# 13 / ASP.NET Core)
* **Base Runtime**: `mcr.microsoft.com/dotnet/aspnet:10.0`
* **SDK Builder**: `mcr.microsoft.com/dotnet/sdk:10.0`
* **Usuário**: Não-root (`$APP_UID = 1654`) para segurança corporativa
* **Porta Interna do Contêiner**: `8080`
* **Health Check**: Endpoint nativo em `/health` respondendo HTTP 200 `Healthy`
* **Banco SQLite**: Arquivo `ani.db` (128 KB) já embutido em `/app/ani.db` com todas as permissões, perfis, usuários e rotas criadas no seed

---

## 💻 2. Execução Local com Docker

### 2.1 Subir via Docker Compose (Recomendado)
O arquivo [docker-compose.yml](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/docker-compose.yml) já está configurado na raiz:

```powershell
# Iniciar o contêiner em background com build automático
docker compose up -d --build

# Visualizar logs da aplicação em tempo real
docker compose logs -f

# Parar o contêiner
docker compose down
```

* **Swagger Local**: [http://localhost:5146](http://localhost:5146)
* **Health Check Local**: [http://localhost:5146/health](http://localhost:5146/health)

---

### 2.2 Build e Execução Manual via Docker CLI

```powershell
# 1. Compilar a imagem (o ponto final '.' indica o diretório atual)
docker build -t portal-itapemirim-ani-api:latest .

# 2. Executar o contêiner mapeando a porta 5146 para a porta interna 8080
docker run -d --name itapemirim-api -p 5146:8080 portal-itapemirim-ani-api:latest

# 3. Testar a resposta da API
curl http://localhost:5146/health
```

---

## ☁️ 3. Publicação no Amazon ECR (Elastic Container Registry)

As credenciais e o perfil da AWS já estão configurados no seu ambiente:
* **Perfil AWS CLI**: `duilio-FariasTec`
* **Região**: `us-east-1`
* **Account ID**: `107189286518`
* **Repositório ECR**: `portal-itapemirim-ani-api`
* **URI Oficial da Imagem**: `107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest`

### Comandos de Publicação no ECR:

```powershell
# 1. Autenticar o Docker no Amazon ECR
& 'C:\Users\Duilio Farias\AppData\Local\Programs\Amazon\AWSCLIV2\aws.exe' ecr get-login-password --region us-east-1 --profile duilio-FariasTec | docker login --username AWS --password-stdin 107189286518.dkr.ecr.us-east-1.amazonaws.com

# 2. Compilar a imagem com o banco ani.db incluído
docker build -t portal-itapemirim-ani-api:latest .

# 3. Aplicar a Tag para o ECR
docker tag portal-itapemirim-ani-api:latest 107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest

# 4. Enviar a imagem para a nuvem da AWS (Push)
docker push 107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest
```

---

## 🚀 4. Configuração no Amazon ECS Express Mode

O **Amazon ECS Express Mode** é o substituto moderno recomendado pela AWS para subir contêineres sem complexidade de infraestrutura.

### Passo a Passo no Console AWS:

1. Acesse o console do **[Amazon ECS Express Mode (us-east-1)](https://us-east-1.console.aws.amazon.com/ecs/v2/express-mode?region=us-east-1)**.
2. Clique em **Create service** (Criar serviço).
3. Preencha as configurações conforme a tabela abaixo:

| Campo no Console | Valor a Preencher | Observações |
| :--- | :--- | :--- |
| **Service name** | `itapemirim-core-api` | Nome do serviço |
| **Container image URI** | `107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest` | URI gerada no ECR |
| **Container port** | `8080` | Porta interna do contêiner .NET 10 |
| **Health check path** | `/health` | Endpoint de monitoramento de saúde |
| **Protocol** | `HTTP` | O ECS gerencia a terminação HTTPS na URL pública |

4. **Environment Variables (Variáveis de Ambiente)**:
   Adicione as seguintes chaves:
   * `ASPNETCORE_ENVIRONMENT`: `Development` *(permite abrir o Swagger UI na URL pública para testes)*
   * `DatabaseProvider`: `Sqlite`
   * `ConnectionStrings__DefaultConnection`: `Data Source=ani.db`
   * `Jwt__SecretKey`: `Itapemirim_PortalCore_SuperSecret_JwtKey_2026_Minimum32Bytes!`
   * `Jwt__Issuer`: `PortalItapemirimCore`
   * `Jwt__Audience`: `PortalItapemirimApp`

5. Clique em **Create & deploy**.
6. Em cerca de 3 minutos, a AWS gerará a **URL pública HTTPS** (ex: `https://xxxxxx.us-east-1.elb.amazonaws.com`).

---

## 🔄 5. Como Fazer Atualizações Futuras (Redeploy)

> [!IMPORTANT]
> **Você NÃO precisa criar um serviço novo no ECS a cada alteração de código!**
> O serviço no ECS é criado apenas uma vez. As próximas atualizações utilizam o fluxo abaixo:

### Passo a passo para atualizar a aplicação na AWS:

1. **Faça as alterações necessárias no código** (C#, Controllers, DTOs, etc.).
2. **Execute o build e o push da nova versão**:
   ```powershell
   # Compila a nova versão
   docker build -t portal-itapemirim-ani-api:latest .

   # Tag e Push para o ECR
   docker tag portal-itapemirim-ani-api:latest 107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest
   docker push 107189286518.dkr.ecr.us-east-1.amazonaws.com/portal-itapemirim-ani-api:latest
   ```

3. **Atualize o serviço no console da AWS**:
   * No console do **Amazon ECS**, abra o serviço `itapemirim-core-api`.
   * Clique em **Update service** (Atualizar serviço).
   * Marque a opção **Force new deployment** (Forçar nova implantação).
   * Clique em **Update**.

O ECS fará a substituição sem tempo de inatividade (*Zero Downtime*): o novo contêiner sobe, passa no `/health` e assume o tráfego da mesma URL existente!

---

## 🤖 6. Automação Contínua com GitHub Actions (Opcional - CI/CD)

Se você preferir que o deploy aconteça de forma 100% automática a cada `git push` no branch `develop`, crie o arquivo `.github/workflows/deploy.yml`:

```yaml
name: Deploy API to AWS ECS

on:
  push:
    branches: [ develop, main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Código
        uses: actions/checkout@v4

      - name: Configurar Credenciais AWS
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-region: us-east-1
          # Utilizar Secrets do repositório GitHub
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}

      - name: Login no Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build, Tag e Push no ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: portal-itapemirim-ani-api
          IMAGE_TAG: latest
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
```

---

## 🔑 7. Credenciais Padrão do Seed para Testes na AWS

Como o banco `ani.db` já vai embutido na imagem, assim que a API estiver no ar você pode testar diretamente na URL da AWS ou no Swagger:

| Usuário | Login | Senha | Perfil | Rotas / Permissões |
| :--- | :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `12345678` | Administrador | Todas (`home.view`, `precos.view`, `usuarios.manage`) |
| **Analista Financeiro** | `financeiro` | `12345678` | Financeiro | `home.view`, `precos.view` |
| **Admin Secundário** | `admin2` | `abcdefgh` | Administrador | Todas as telas administrativas |
| **Financeiro Secundário** | `financeiro2` | `abcdefgh` | Financeiro | Telas financeiras e visualização |

* **Endpoint de Login**: `POST /api/auth/login` (retorna o Token JWT com todas as claims de perfil e permissões).
* **Endpoint de Health**: `GET /health` (retorna `Healthy` com HTTP 200).
