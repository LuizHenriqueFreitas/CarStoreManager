// Gera um "PDF" a partir de um texto de contrato/documento sem nenhuma
// biblioteca de PDF: abre uma janela formatada pra impressão e aciona
// window.print(). O usuário escolhe "Salvar como PDF" no diálogo nativo do
// navegador/SO, que já pergunta o diretório de destino — é exatamente esse o
// mecanismo que dá "salvar localmente em diretório de escolha do usuário"
// sem precisar de download forçado nem biblioteca extra no servidor.
window.carstorePdf = {
    exportar: function (titulo, conteudo) {
        const win = window.open('', '_blank');
        if (!win) {
            alert('Não foi possível abrir a janela de impressão — verifique se o navegador está bloqueando pop-ups para este site.');
            return;
        }

        const escapar = (s) => (s || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');

        win.document.write(
            '<!DOCTYPE html><html lang="pt-BR"><head><meta charset="utf-8" />' +
            '<title>' + escapar(titulo) + '</title>' +
            '<style>' +
            'body { font-family: Georgia, "Times New Roman", serif; font-size: 13px; line-height: 1.6; ' +
            'padding: 48px; white-space: pre-wrap; word-wrap: break-word; color: #1a1a1a; }' +
            'h1 { font-size: 15px; margin: 0 0 28px; padding-bottom: 12px; border-bottom: 1px solid #ccc; }' +
            '@media print { body { padding: 0 32px; } }' +
            '</style></head><body>' +
            '<h1>' + escapar(titulo) + '</h1>' +
            '<div>' + escapar(conteudo) + '</div>' +
            '</body></html>'
        );
        win.document.close();
        win.focus();

        // setTimeout dá tempo do documento renderizar antes de abrir o
        // diálogo de impressão — chamar print() logo após write() costuma
        // imprimir uma página em branco em alguns navegadores.
        setTimeout(function () {
            win.print();
        }, 300);
    }
};
