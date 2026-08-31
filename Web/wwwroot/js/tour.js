// Tour guiado embutido (manual do usuário) — funções mínimas de suporte ao
// componente TourGuiado.razor. Sem biblioteca de terceiros de propósito: o
// projeto inteiro evita dependência JS externa, e um tour guiado não precisa
// de mais que "medir um elemento" e "rolar até ele".
window.tourGuiado = {
    // Geometria do elemento (em coordenadas de PÁGINA, já somando o scroll)
    // e o tamanho do viewport — usados em C# pra posicionar o destaque e o
    // balão sem nunca deixar o balão sair da tela.
    medir: function (seletor) {
        return this._medirElemento(document.querySelector("[data-tour='" + seletor + "']"));
    },

    // Mede o próprio balão do tour já renderizado — usado em C# pra
    // reajustar a posição com a ALTURA REAL (o cálculo inicial só estima a
    // altura, já que ela varia com o tamanho do texto; textos maiores ou
    // telas mais baixas, tipo um monitor 1366x768/1280x720, podiam deixar o
    // balão vazando por baixo da viewport antes desse reajuste existir).
    medirBalao: function () {
        return this._medirElemento(document.querySelector('.tour-balao, .tour-balao-centralizado'));
    },

    _medirElemento: function (el) {
        var vw = window.innerWidth;
        var vh = window.innerHeight;
        var sy = window.scrollY;

        if (!el) {
            return { existe: false, top: 0, left: 0, width: 0, height: 0, viewportWidth: vw, viewportHeight: vh, scrollY: sy };
        }

        var r = el.getBoundingClientRect();
        return {
            existe: true,
            top: r.top + sy,
            left: r.left + window.scrollX,
            width: r.width,
            height: r.height,
            viewportWidth: vw,
            viewportHeight: vh,
            scrollY: sy
        };
    },

    // Rola suavemente até o elemento e só resolve a Promise depois — não tem
    // callback nativo pro fim de um scrollIntoView suave, então usamos um
    // tempo fixo, curto o bastante pra não atrasar o passo seguinte.
    rolarAte: function (seletor) {
        return new Promise(function (resolve) {
            var el = document.querySelector("[data-tour='" + seletor + "']");
            if (!el) { resolve(); return; }
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            setTimeout(resolve, 400);
        });
    },

    // Trava o scroll da página enquanto o tour está ativo — sem isso, o
    // usuário rolando a página descolaria o destaque do elemento (as
    // coordenadas são calculadas uma vez por passo, não seguidas ao vivo).
    travarScroll: function (travar) {
        document.body.style.overflow = travar ? 'hidden' : '';
    },

    // Listener de teclado global, só enquanto o tour está aberto — Esc/setas
    // funcionam independente de qual elemento interno do balão está focado
    // (um clique em "Próximo" move o foco pro próprio botão).
    _tecladoCallback: null,
    registrarTeclado: function (dotNetRef) {
        this._tecladoCallback = function (e) {
            if (e.key === 'Escape' || e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
                dotNetRef.invokeMethodAsync('TratarTeclaJs', e.key);
            }
        };
        document.addEventListener('keydown', this._tecladoCallback);
    },
    pararTeclado: function () {
        if (this._tecladoCallback) {
            document.removeEventListener('keydown', this._tecladoCallback);
            this._tecladoCallback = null;
        }
    },

    // Redimensionar a janela invalida as coordenadas medidas — remede o
    // passo atual em vez de deixar o destaque desalinhado.
    _resizeCallback: null,
    registrarResize: function (dotNetRef) {
        this._resizeCallback = function () {
            dotNetRef.invokeMethodAsync('RemedirPassoAtual');
        };
        window.addEventListener('resize', this._resizeCallback);
    },
    pararResize: function () {
        if (this._resizeCallback) {
            window.removeEventListener('resize', this._resizeCallback);
            this._resizeCallback = null;
        }
    },

    // Devolve o foco pro botão de ajuda ao encerrar o tour — todo botão de
    // ajuda do sistema (NavBar ou as 3 páginas sem NavBar) leva essa mesma
    // classe, então não precisa saber qual página está aberta.
    focarBotaoAjuda: function () {
        var btn = document.querySelector('.tour-btn-ajuda');
        if (btn) btn.focus();
    }
};
