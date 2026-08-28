# CarStoreManager

Sistema de gerenciamento para concessionárias e oficinas de médio porte —
projeto de estudo focado em ASP.NET Core, Blazor Server e arquitetura em
camadas.

### Sobre o projeto

Um gerenciador com dois módulos: um voltado para controle de uma
concessionária de médio porte (estoque de veículos, propostas de venda,
consignação, integração com Mercado Livre) e outro voltado para controle de
oficina (ordens de serviço, estoque de componentes, checklists).

### Ferramentas usadas

Feito para aprender mais sobre desenvolvimento web, arquitetura em camadas e
as funcionalidades do ASP.NET Core — usando Blazor Server com ASP.NET 9 e
**SQLite** como banco, para manter o setup local em zero configuração.

### Stack técnico

- ASP.NET Core 9 · Blazor Server
- Entity Framework Core + SQLite
- Autenticação híbrida Cookie (Blazor) / JWT (API)
- xUnit + FluentAssertions + Moq
- Integração com a API do Mercado Livre (OAuth + webhook — ver limitação de
  ambiente local em `SETUP.md`)

### Para desenvolvedores

Instruções completas de setup local estão em **[SETUP.md](./SETUP.md)**.
Resumo rápido:

```bash
git clone <url-do-repo>
cd CarStoreManager
dotnet restore
dotnet run --project Web
```

O banco `carstore.db` é criado automaticamente na primeira execução (schema
+ admin semeado). Login inicial: `admin@teste.com` / `12345A`.

Para popular o banco com dados de teste realistas (clientes, veículos,
componentes, ordens de serviço etc.):

```bash
dotnet run --project Geradores
```

Ver `Geradores/README.md` para detalhes.

### Arquitetura

Arquitetura em camadas, separada em:

- **Web/** — front-end (Blazor Server) e API REST (Controllers)
- **Application/** e **Domain/** — regras de negócio e objetos de domínio
  (back-end)
- **Infrastructure/** — conexão com banco de dados, implementação de Entity
  Framework, repositories e clientes HTTP de integrações externas
- **Tests/** — testes unitários e de integração
- **Geradores/** — ferramenta de linha de comando para popular o banco com
  dados fake, reaproveitável para crescer a base a qualquer momento
