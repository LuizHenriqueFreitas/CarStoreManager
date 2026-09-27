// Gera um "PDF" a partir de um texto de contrato/documento sem nenhuma
// biblioteca de PDF: abre uma janela formatada pra impressão e aciona
// window.print(). O usuário escolhe "Salvar como PDF" no diálogo nativo do
// navegador/SO, que já pergunta o diretório de destino — é exatamente esse o
// mecanismo que dá "salvar localmente em diretório de escolha do usuário"
// sem precisar de download forçado nem biblioteca extra no servidor.
window.carstorePdf = {
    exportar: function (titulo, conteudo, fotoUrls) {
        const win = window.open('', '_blank');
        if (!win) {
            alert('Não foi possível abrir a janela de impressão — verifique se o navegador está bloqueando pop-ups para este site.');
            return;
        }

        const escapar = (s) => (s || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');

        const fotos = Array.isArray(fotoUrls) ? fotoUrls.filter(u => !!u) : [];
        const fotosHtml = fotos.length === 0 ? '' : (
            '<h2>Fotos anexadas</h2>' +
            '<div class="fotos-grid">' +
            fotos.map(u => '<img src="' + escapar(u) + '" alt="Foto anexada" />').join('') +
            '</div>'
        );

        win.document.write(
            '<!DOCTYPE html><html lang="pt-BR"><head><meta charset="utf-8" />' +
            '<title>' + escapar(titulo) + '</title>' +
            '<style>' +
            'body { font-family: Georgia, "Times New Roman", serif; font-size: 13px; line-height: 1.6; ' +
            'padding: 48px; color: #1a1a1a; }' +
            'h1 { font-size: 15px; margin: 0 0 28px; padding-bottom: 12px; border-bottom: 1px solid #ccc; }' +
            'h2 { font-size: 13px; margin: 32px 0 14px; padding-bottom: 8px; border-bottom: 1px solid #ccc; }' +
            '.conteudo { white-space: pre-wrap; word-wrap: break-word; }' +
            '.fotos-grid { display: flex; flex-wrap: wrap; gap: 12px; }' +
            '.fotos-grid img { width: 47%; max-height: 320px; object-fit: contain; border: 1px solid #ddd; page-break-inside: avoid; }' +
            '@media print { body { padding: 0 32px; } }' +
            '</style></head><body>' +
            '<h1>' + escapar(titulo) + '</h1>' +
            '<div class="conteudo">' + escapar(conteudo) + '</div>' +
            fotosHtml +
            '</body></html>'
        );
        win.document.close();
        win.focus();

        const imprimir = function () { win.print(); };

        if (fotos.length === 0) {
            // setTimeout dá tempo do documento renderizar antes de abrir o
            // diálogo de impressão — chamar print() logo após write() costuma
            // imprimir uma página em branco em alguns navegadores.
            setTimeout(imprimir, 300);
            return;
        }

        // Com fotos, espera cada <img> carregar (ou falhar) antes de
        // imprimir — senão a primeira renderização sai com espaços em
        // branco no lugar das imagens ainda não decodificadas. Timeout de
        // segurança caso alguma imagem trave/nunca resolva.
        const imgs = win.document.querySelectorAll('.fotos-grid img');
        let pendentes = imgs.length;
        let jaImprimiu = false;
        const tentarImprimir = function () {
            if (jaImprimiu) return;
            pendentes--;
            if (pendentes <= 0) {
                jaImprimiu = true;
                setTimeout(imprimir, 100);
            }
        };
        imgs.forEach(function (img) {
            if (img.complete) { tentarImprimir(); return; }
            img.addEventListener('load', tentarImprimir);
            img.addEventListener('error', tentarImprimir);
        });
        setTimeout(function () {
            if (!jaImprimiu) { jaImprimiu = true; imprimir(); }
        }, 4000);
    }
};
