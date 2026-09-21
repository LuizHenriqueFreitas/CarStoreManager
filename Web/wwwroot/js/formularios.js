// Impede que a tecla Enter envie um <EditForm>/<form> sozinha.
//
// Por padrão, o HTML envia (submit) um formulário quando o usuário aperta
// Enter dentro de um <input> de texto — mesmo sem tocar em nenhum botão.
// Isso é surpreendente em telas com formulário grande e vários campos (ex.:
// Nova Ordem de Serviço, que embute buscas de cliente/veículo DENTRO do
// mesmo <EditForm>): apertar Enter num campo de busca podia enviar o
// formulário inteiro prematuramente, ou disparar o primeiro botão sem
// type="button" explícito (ver docs/redesign/25-integridade-formularios.md)
// — daí os relatos de "voltar pra página anterior"/"limpar tudo" sem
// nenhum clique. O pedido do dono é que envio e saída do formulário só
// aconteçam por clique do mouse.
//
// Esta função captura o Enter ANTES do navegador agir e cancela só a ação
// NATIVA de "enviar o formulário" (preventDefault) — não interfere em
// nenhum atalho de teclado que a própria aplicação já implementa de
// propósito (ex.: "Enter pra entrar" no login, Esc pra fechar modal), já
// que esses são tratados por código próprio (@onkeydown do Blazor), que
// continua recebendo o evento normalmente.
(function () {
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            var el = e.target;
            if (!el || el.tagName !== 'INPUT') return;

            var tipo = (el.getAttribute('type') || 'text').toLowerCase();
            // Esses tipos não disparam submit nativo por Enter de qualquer
            // forma, ou já SÃO a ação explícita — nada a bloquear.
            if (tipo === 'submit' || tipo === 'button' || tipo === 'checkbox' || tipo === 'radio' || tipo === 'file' || tipo === 'range') {
                return;
            }

            if (!el.closest('form')) return;

            e.preventDefault();
            return;
        }

        // Backspace fora de um campo editável era, em navegadores antigos,
        // atalho pra "voltar" no histórico (removido na maioria dos
        // navegadores atuais, mas mantido aqui como reforço — o pedido é
        // que só um clique do mouse tire o usuário do formulário).
        if (e.key === 'Backspace') {
            var alvo = e.target;
            var editavel = alvo && (
                alvo.tagName === 'INPUT' || alvo.tagName === 'TEXTAREA' ||
                alvo.isContentEditable
            );
            if (!editavel) e.preventDefault();
        }
    }, true);
})();
