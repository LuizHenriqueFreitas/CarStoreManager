namespace CarStoreManager.Web.Tours;

/// <summary>
/// Ponte entre o botão de ajuda (no NavBar, ou embutido nas páginas que não
/// usam NavBar — Login, Consulta, AssinarTermo) e o componente
/// <c>TourGuiado</c>, que fica em MainLayout.
///
/// Registrado como Scoped — uma instância por circuito Blazor (por aba/
/// usuário conectado), não uma por componente. Escolhido em vez de
/// CascadingValue porque um valor em cascata só alcança descendentes do
/// componente que o declara: MainLayout está fora da árvore de
/// Login/Consulta/AssinarTermo (que usam LoginLayout ou nenhum layout), então
/// um CascadingValue declarado em MainLayout nunca chegaria até elas — exigiria
/// duplicar a fonte da verdade. Com um serviço Scoped, qualquer botão de ajuda,
/// em qualquer galho da árvore de componentes, injeta a mesma instância e
/// dispara o mesmo tour sem precisar de referência direta a TourGuiado.
/// </summary>
public class TourEstadoService
{
    /// <summary>TourGuiado assina este evento em MainLayout (ou na própria página, nas 3 sem NavBar).</summary>
    public event Func<Task>? OnSolicitarTour;

    public async Task SolicitarTourAsync()
    {
        if (OnSolicitarTour is not null)
            await OnSolicitarTour.Invoke();
    }
}
