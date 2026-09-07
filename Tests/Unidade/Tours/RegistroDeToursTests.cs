using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using CarStoreManager.Web.Tours;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Tours;

/// <summary>
/// Garante que TODA tela roteável do sistema tem um manual (tour guiado)
/// cadastrado, e que nenhum tour aponta para uma rota que não existe mais.
/// Ver docs/redesign — requisito "toda tela deve ter um manual de uso".
/// </summary>
public class RegistroDeToursTests
{
    // Rotas que legitimamente não têm tour (páginas de erro do framework).
    private static readonly HashSet<string> SemTourEsperado = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Error"
    };

    private static IEnumerable<string> TodasAsRotasDeComponente()
    {
        var assembly = typeof(RegistroDeTours).Assembly;
        foreach (var tipo in assembly.GetTypes())
        {
            if (!typeof(IComponent).IsAssignableFrom(tipo)) continue;
            foreach (var attr in tipo.GetCustomAttributes<RouteAttribute>())
                yield return attr.Template;
        }
    }

    private static string[] Segmentos(string rota) =>
        rota.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static bool CasaEstrutura(string template, string rota)
    {
        var ts = Segmentos(template);
        var rs = Segmentos(rota);
        if (ts.Length != rs.Length) return false;
        for (var i = 0; i < ts.Length; i++)
        {
            var t = ts[i];
            if (t.StartsWith('{') && t.EndsWith('}')) continue;
            if (!string.Equals(t, rs[i], StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    [Fact]
    public void TodaRotaDeComponente_TemUmTourCorrespondente()
    {
        var rotas = TodasAsRotasDeComponente()
            .Where(r => !SemTourEsperado.Contains(r))
            .Distinct()
            .ToList();

        rotas.Should().NotBeEmpty("o assembly Web deve conter componentes roteáveis");

        var semTour = rotas
            .Where(rota => RegistroDeTours.Todos.All(t => !CasaEstrutura(t.RotaTemplate, rota)))
            .ToList();

        semTour.Should().BeEmpty(
            "toda tela precisa de um manual — rotas sem tour: " + string.Join(", ", semTour));
    }

    [Fact]
    public void NenhumTour_ApontaParaRotaInexistente()
    {
        var rotas = TodasAsRotasDeComponente().Distinct().ToList();

        var orfaos = RegistroDeTours.Todos
            .Where(t => !rotas.Any(r => CasaEstrutura(t.RotaTemplate, r)))
            .Select(t => t.RotaTemplate)
            .ToList();

        orfaos.Should().BeEmpty(
            "tours apontando para rotas que não existem mais: " + string.Join(", ", orfaos));
    }

    [Fact]
    public void TodoTour_TemPeloMenosUmPassoDeIntroducao()
    {
        foreach (var tour in RegistroDeTours.Todos)
        {
            tour.Passos.Should().NotBeEmpty($"o tour de {tour.RotaTemplate} precisa de passos");
            tour.Passos.Should().Contain(p => p.Seletor == null,
                $"o tour de {tour.RotaTemplate} precisa de ao menos um passo de introdução (sem seletor)");
        }
    }

    [Fact]
    public void TodoTour_TemTituloEDescricaoNaoVazios()
    {
        foreach (var tour in RegistroDeTours.Todos)
        {
            tour.Titulo.Should().NotBeNullOrWhiteSpace();
            tour.Descricao.Should().NotBeNullOrWhiteSpace();
            foreach (var passo in tour.Passos)
            {
                passo.Titulo.Should().NotBeNullOrWhiteSpace($"passo do tour {tour.RotaTemplate}");
                passo.Texto.Should().NotBeNullOrWhiteSpace($"passo '{passo.Titulo}' do tour {tour.RotaTemplate}");
            }
        }
    }

    [Fact]
    public void ObterPorRota_ResolveRotaConcreta_ParaTemplateComParametro()
    {
        var tour = RegistroDeTours.ObterPorRota("/oficina/os/" + Guid.NewGuid());
        tour.Should().NotBeNull();
        tour!.RotaTemplate.Should().Be("/oficina/os/{id:guid}");
    }

    [Fact]
    public void ObterPorRota_TelasNovasDoRedesign_TemTour()
    {
        string[] novas =
        {
            "/", "/financeiro", "/financeiro/despesas", "/financeiro/a-receber",
            "/financeiro/relatorios", "/oficina", "/oficina/ordens", "/oficina/recepcao",
            "/concessionaria", "/concessionaria/salao", "/concessionaria/consignacoes",
            "/concessionaria/test-drives", "/configuracoes", "/clientes",
            "/configuracoes/sistema", "/clientes/" + Guid.NewGuid(), "/equipe"
        };

        foreach (var rota in novas)
            RegistroDeTours.ObterPorRota(rota).Should().NotBeNull($"{rota} deve ter tour");
    }
}
