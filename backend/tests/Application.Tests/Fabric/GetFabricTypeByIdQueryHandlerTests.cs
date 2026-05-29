using FluentAssertions;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Fabric;

public class GetFabricTypeByIdQueryHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly GetFabricTypeByIdQueryHandler _handler;

    public GetFabricTypeByIdQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetFabricTypeByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ExistingId_ReturnsFabricTypeDto()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto", "#000000");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetFabricTypeByIdQuery(fabricType.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(fabricType.Id);
        result.Colors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NonExistingId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetFabricTypeByIdQuery(Guid.NewGuid()), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedFabricType_ReturnsNull()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.MarkAsDeleted();
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetFabricTypeByIdQuery(fabricType.Id), CancellationToken.None);
        result.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
