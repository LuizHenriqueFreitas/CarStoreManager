### ERP Project

### Sobre o projeto

Esse é um projeto voltado para o desenvolvimento de um gerenciador com 2 modulos, 1 voltado para controle de uma concessionaria de medio porte e outro voltado para controle de oficinas, também de medio porte. 

### Ferramenta usadas

Tudo isso esta sendo feito para o objetivo principal de aprender mais sobre desenvolvimento web, arquiteturas de projeto e as funcionalidades de ASP.NET core - especificamente estamos usando blazor com asp.net 10. Durante o desenvolvimento e estruturação dos primeiros prototipos foi usado o SQlite com finalidade de focar no desenvolvimento das regras de negocio e integração com paginas da interface - posteriormente a ideia é migrar para um banco de dados mais robusto como My SQL ou Postgress.

### Para Desenvolvedores
Caso seja de seu interesse utilizar esse projeto como base para desenvolvimento, estudo ou mesmo deseje rodar ele localmente apartir do codigo fonte no seu PC, será necessario configurar alguns pacotes Nuget. Para testes, durante o desenvolvimento foi usado sqlite como banco de dados, assim como seu respectivo entity framework.

Certifique-se de ter:
-  Dot.Net SDK 9.0.0
***todos os codifos aqui apresentados devem ser rodados na pasta /CarStoreManager/Web***
-  Ative o recurso de Entity Framework
    > `dotnet tool install --global dotnet-ef`
-  Adicione os pacotes Nuget de Entity Framework para sqlite
    > `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
    > `dotnet add package Microsoft.EntityFrameworkCore.Design`
    > `dotnet add package Microsoft.EntityFrameworkCore.Tools`
-  Execute a migração das Entidades apartir do framework
    > `dotnet ef migrations add NomeDaMigration --project ../Infrastructure --startup-project .`
    > ***este comando garante que a pasta migrations seja criada no diretorio correto, dentro de /Infrastructure***
-  Por fim, crie o arquivo fisico do banco de dados
    >  `dotnet ef database update`
    > ***este arquivo vai aparecer dentro de CarStoreManager/Web***


### Arquitetura

Considerando o escopo do projeto como um projeto de estudo para ganho de experiencia e tempo de desenvolvimento foi escolhida a arquitetura em camadas, separada, aqui, em: 
 - Web -> basicamente o front end;
 - Application e Domain que contem as regras de negocios e objetos utilizados -> basicamnet o back end;
 - Infrastructure, que contem as conexões com banco de dados, implementação de Entity framework e tambem interfaces de comunicação do servidor com o banco de dados propriamente dito.

