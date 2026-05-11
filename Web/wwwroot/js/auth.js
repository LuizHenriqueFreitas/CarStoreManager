// Funções utilitárias de autenticação chamadas pelo Blazor via JSInterop.
// Encapsulam fetch para os endpoints REST que emitem/limpam o cookie de sessão.

window.carstoreLogin = async function (email, senha) {
    try {
        const resp = await fetch('/api/session/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({ email: email, senha: senha })
        });

        if (!resp.ok) {
            let error = 'Credenciais inválidas';
            try { const body = await resp.json(); if (body.error) error = body.error; } catch { }
            return { ok: false, error: error };
        }

        const data = await resp.json();
        return { ok: true, token: data.token, nome: data.nome, role: data.role, expiracao: data.expiracao };
    } catch (e) {
        return { ok: false, error: 'Erro de conexão: ' + e.message };
    }
};

window.carstoreLogout = async function () {
    try {
        await fetch('/api/session/logout', { method: 'POST', credentials: 'same-origin' });
    } catch { /* ignora */ }
    try { localStorage.removeItem('authToken'); } catch { }
};
