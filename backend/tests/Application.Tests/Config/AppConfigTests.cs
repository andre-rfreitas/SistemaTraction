using FluentAssertions;
using SistemaTraction.Application.Config.Commands.UpsertAppConfig;
using SistemaTraction.Application.Config.Queries.GetAppConfigByKey;
using SistemaTraction.Application.Config.Queries.GetAppConfigs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Config;

namespace SistemaTraction.Application.Tests.Config;

public class AppConfigTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly UpsertAppConfigCommandHandler _upsertHandler;
    private readonly GetAppConfigsQueryHandler _getAllHandler;
    private readonly GetAppConfigByKeyQueryHandler _getByKeyHandler;

    public AppConfigTests()
    {
        _context = TestDbContextFactory.Create();
        _upsertHandler = new UpsertAppConfigCommandHandler(_context);
        _getAllHandler = new GetAppConfigsQueryHandler(_context);
        _getByKeyHandler = new GetAppConfigByKeyQueryHandler(_context);
    }

    [Fact]
    public async Task Upsert_NovaChave_CriaConfig()
    {
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("sistema.moeda", "BRL"),
            CancellationToken.None);

        var config = _context.AppConfigs.First(c => c.Key == "sistema.moeda");
        config.Value.Should().Be("BRL");
    }

    [Fact]
    public async Task Upsert_ChaveExistente_AtualizaValor()
    {
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("sistema.moeda", "BRL"),
            CancellationToken.None);

        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("sistema.moeda", "USD"),
            CancellationToken.None);

        var configs = _context.AppConfigs.Where(c => c.Key == "sistema.moeda").ToList();
        configs.Should().HaveCount(1, "upsert nunca duplica a chave");
        configs[0].Value.Should().Be("USD");
    }

    [Fact]
    public async Task Upsert_ChaveVazia_ThrowsDomainException()
    {
        var act = () => _upsertHandler.Handle(
            new UpsertAppConfigCommand("", "valor"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetAll_RetornaConfigsOrdenadas()
    {
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("z.ultima", "1"), CancellationToken.None);
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("a.primeira", "2"), CancellationToken.None);

        var result = await _getAllHandler.Handle(
            new GetAppConfigsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Key.Should().Be("a.primeira");
        result[1].Key.Should().Be("z.ultima");
    }

    [Fact]
    public async Task GetByKey_ChaveExistente_RetornaConfig()
    {
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("dtf.alerta", "3"),
            CancellationToken.None);

        var result = await _getByKeyHandler.Handle(
            new GetAppConfigByKeyQuery("dtf.alerta"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Key.Should().Be("dtf.alerta");
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task GetByKey_ChaveInexistente_RetornaNull()
    {
        var result = await _getByKeyHandler.Handle(
            new GetAppConfigByKeyQuery("inexistente"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Upsert_ValorVazio_Permitido()
    {
        await _upsertHandler.Handle(
            new UpsertAppConfigCommand("feature.flag", ""),
            CancellationToken.None);

        var config = _context.AppConfigs.First(c => c.Key == "feature.flag");
        config.Value.Should().Be("");
    }

    public void Dispose() => _context.Dispose();
}
