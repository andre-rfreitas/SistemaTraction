using FluentAssertions;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypes;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Fabric;

public class GetFabricTypesQueryHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly GetFabricTypesQueryHandler _handler;

    public GetFabricTypesQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetFabricTypesQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyNonDeletedFabricTypes()
    {
        var active = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        var deleted = FabricType.Create("Moletom", "Premium", 35m, 8m, null);
        deleted.MarkAsDeleted();
        _context.FabricTypes.AddRange(active, deleted);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetFabricTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Malha");
    }

    [Fact]
    public async Task Handle_IncludesActiveColorsOnly()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto", "#000000");
        fabricType.AddColor("Branco", "#FFFFFF");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetFabricTypesQuery(), CancellationToken.None);

        result[0].Colors.Should().HaveCount(2);
        result[0].Colors.Select(c => c.Name).Should().Contain(["Preto", "Branco"]);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoFabricTypes()
    {
        var result = await _handler.Handle(new GetFabricTypesQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
