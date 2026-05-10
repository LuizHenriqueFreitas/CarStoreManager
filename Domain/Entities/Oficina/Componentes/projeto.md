Para o gerenciamento correto das peças a maneira mais adequada 
de se construir esse sistema é integrar entrada automatica por 
xml da nota fiscal com confirmação manual das entradas - 
depois de aprovado ele relaciona/ cria os lotes de cada produto
no sistema baseado nas informações da NF, alem disso ja faz toda
a movimentação de estoque adicionando os novos produtos adequadamente
de acordo com o OEM de cada um deles.

É interessante manter a opção de entrada manual de componentes no sistema.

Componentes devem ter um campo de origem, caso o cliente traga as peças
para serem instaladas em seu carro o mecanico deve marcar origem como
fornecido pelo cliente ou algo assim.

Aqui devemos comecar a implementar sistemas que tenham relação com nota
fiscal para que nosso software esteja de acordo com a legislação alem de
historicos e logs para ter um grande banco de rastreabilidade da informações 
relacionadas a oficina que utilize o sistema.

por hora, visto que a implementação correta disso tudo requer muito tempo
de estudo e codificação, vou manter para implementar mais tarde e focar em
corrigir outros problemas, implementando por enquanto apenas a entrada manual 
e componentes no sistema.