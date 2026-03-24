## 2026-24-03

### Atualizações
Refatoração de Application
- Nova estruturação interna de pastas separando as coisas de acordo com os modulos
- DTOs separados para cada finalidade
- Mappings, interfaces e Services revisados e atualizados - devem ser refinados

### Problemas pendentes
- especializar uma entidade para VeiculoCliente e uma para VeiculoEstoque apartir de Veiculo
- revisar relações entre entidades, por exemplo, um cliente por der 1 ou mais carros na oficina, enquanto cada carro da oficina pertence a apenas 1 cliente
- Implementar Testes Unitarios e integradosde integração
- Implementar Geters corretamente (principalmente nos mappings)
- Ajustar corretamente os services de acordo com suas interfaces

## 2026-22-03

### Atualizações
Refatoração do Domain completo.
- Extruturação das entidades, ValueObjects(VO), enums e Repositorys;
- entidades e VOs com regras e verificações basicas aplicadas;

Refatoração e organização dos namespaces do projeto.

### Melhorias para o futuro
- Adicionar metodos complementares nas entidades - como sistema de parcelamento com juros compostos, entre outros
- modularizar os VOs adicionando um arquivo generico e aplicando herança nos demais

### Problemas pendentes
- Necessario trabalhar na refatoração de Aplication e infrastructure;
- Nenhum avanço no front-end

## 2026-19-03

Estamos a 2 semanas estudando, prototipando e pensando em como desenvolver esse projeto,
temos até o momento:

 - BackEnd completo funcionando (aparentemente) - com certo nivel de modularidade ja implementada
     - Entidades
     - DTOs
     - APIs para crud
     - Interfaces
     - Mappings


- Conexão com sqlite para desenvolvimento rapido - no futuro vamos migrar de banco

- Front End em andamento
    - pagina principal da concessionaria 80% completa
    > faltam todas as outras paginas ainda

### Problemas pendentes
- Refetorar namespaces de todos os arquivos do back-end
- css esta todo depositado em wwwroot/css/site.css - deve ser modularizado no futuro

