# 🚀 Setup — CarStoreManager

Clonar, rodar. Sem instalar banco, sem Docker, sem configurar nada.

## Pré-requisito

- [.NET 9 SDK](https://dotnet.microsoft.com/download) — só isso.

## Rodar o projeto

```bash
git clone https://github.com/seu-usuario/CarStoreManager.git
cd CarStoreManager
dotnet restore
dotnet run --project Web
```

Este checkout não tem `Web/Properties/launchSettings.json` versionado, então
o Kestrel sobe na porta padrão: abre em `http://localhost:5000`. O console
imprime a linha `Now listening on: ...` com a porta real ao subir — confira
ali se preferir não confiar neste número.

Na primeira execução, o `Program.cs` roda `Database.MigrateAsync()`
automaticamente: o arquivo `Web/carstore.db` é criado do zero, o schema é
aplicado a partir de `Infrastructure/Migrations/`, o admin é semeado e os
presets de checklist iniciais são criados. Nenhum passo manual de banco é
necessário — nas próximas execuções o `MigrateAsync()` só confirma que o
schema já está em dia e segue direto.

Login inicial (criado automaticamente pelo seed): **admin@teste.com** /
**12345A**.

### Popular com dados de teste (opcional)

```bash
dotnet run --project Geradores
```

Gera ~680 registros fake (usuários, clientes, veículos, componentes, OS
etc.) passando pelas mesmas regras de negócio da aplicação real. Pode
rodar de novo quantas vezes quiser para ir aumentando a base — ver
`Geradores/README.md`.

## Resetar o banco do zero

O banco é só um arquivo local, então resetar é apagar e rodar de novo:

```bash
# 1. pare o app (Ctrl+C)
rm Web/carstore.db
dotnet run --project Web
# opcional: dotnet run --project Geradores, pra repopular com dados fake
```

Alternativa via EF Core CLI (equivalente, não precisa achar o arquivo à mão):

```bash
dotnet ef database drop --project Infrastructure --startup-project Web
dotnet run --project Web
```

## Onde ficam os arquivos enviados

Fotos de veículos e de vistoria enviadas pela aplicação vão para
`Web/wwwroot/uploads/` — é conteúdo de runtime, **não é versionado**
(`.gitignore`). Se você resetar o banco, essa pasta não é limpa junto; os
registros que apontavam para esses arquivos somem do banco, mas os arquivos
em si ficam órfãos no disco — pode apagar `Web/wwwroot/uploads/` manualmente
se quiser limpar de vez.

## Mercado Livre — limitação conhecida do ambiente local

O código da integração (OAuth, webhook, publicação de anúncio) está
completo e não muda entre local/produção. **Mas** o Mercado Livre não aceita
`http://` nem `localhost` como URI de redirect do fluxo OAuth — exige uma
URL pública em HTTPS. Isso significa que **conectar a conta ML de verdade
não funciona rodando só localhost** — só a UI e as telas abrem normalmente,
sem uma conta conectada.

Para testar o fluxo OAuth real localmente, seria necessário expor a máquina
local via um túnel HTTPS (ex.: `ngrok http`, seguido da porta impressa no
`Now listening on: ...` ao subir; ou `localhost.run`) e
atualizar `MercadoLivre:RedirectUri` / `MercadoLivre:UrlBasePublica` em
`Web/appsettings.json` para a URL pública gerada pelo túnel, além de
cadastrar essa mesma URL no painel do app Mercado Livre. Isso fica **fora do
escopo desta tarefa** — não foi implementado nem configurado, só documentado
aqui para não ser esquecido na hora de testar de verdade.

## Estrutura do projeto

- **Domain/** — entidades e value objects (regras de negócio puras, sem
  dependência de banco/framework)
- **Application/** — services e DTOs (casos de uso)
- **Infrastructure/** — `AppDbContext`, repositories, migrations (SQLite)
- **Web/** — Blazor Server (UI) + Controllers (API REST)
- **Tests/** — testes unitários e de integração
- **Geradores/** — ferramenta de linha de comando para popular o banco com
  dados fake realistas (ver `Geradores/README.md`)

## Tecnologias

- ASP.NET Core 9 · Blazor Server
- Entity Framework Core + SQLite
- Autenticação híbrida Cookie (Blazor) / JWT (API) via scheme "Smart"
- xUnit + FluentAssertions + Moq (testes)

## Contribuindo

1. Branch: `git checkout -b feature/sua-feature`
2. Commit: `git commit -m "..."`
3. Antes de commitar uma mudança de entidade, gere e **commite** a migration:
   ```bash
   dotnet ef migrations add NomeDescritivoDaMudanca --project Infrastructure --startup-project Web
   ```
   Os arquivos gerados em `Infrastructure/Migrations/` **precisam ser
   commitados** — sem eles, quem clonar o repo depois de você não consegue
   recriar o schema.
4. Push + Pull Request
