### ERP Project

### Sobre o projeto

Esse é um projeto voltado para o desenvolvimento de um gerenciador com 2 modulos, 1 voltado para controle de uma concessionaria de medio porte e outro voltado para controle de oficinas, também de medio porte. 

### Ferramenta usadas

Tudo isso esta sendo feito para o objetivo principal de aprender mais sobre desenvolvimento web, arquiteturas de projeto e as funcionalidades de ASP.NET core - especificamente estamos usando blazor com asp.net 10. Durante o desenvolvimento e estruturação dos primeiros prototipos foi usado o SQlite com finalidade de focar no desenvolvimento das regras de negocio e integração com paginas da interface - posteriormente a ideia é migrar para um banco de dados mais robusto como My SQL ou Postgress.

### Arquitetura

Considerando o escopo do projeto como um projeto de estudo para ganho de experiencia e tempo de desenvolvimento foi escolhida a arquitetura em camadas, separada, aqui, em: 
 - Web -> basicamente o front end;
 - Application e Domain que contem as regras de negocios e objetos utilizados -> basicamnet o back end;
 - Infrastructure, que contem as conexões com banco de dados, implementação de Entity framework e tambem interfaces de comunicação do servidor com o banco de dados propriamente dito.

