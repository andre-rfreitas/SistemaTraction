using FluentAssertions;
using SistemaTraction.Application.Fabric.Commands.UpdateFabricType;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Fabric;

public class UpdateFabricTypeCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly UpdateFabricTypeCommandHandler _handler;

    public UpdateFabricTypeCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateFabricTypeCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesFabricType()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var command = new UpdateFabricTypeCommand(fabricType.Id, "Moletom", "Premium", 35m, 8m, null);
        await _handler.Handle(command, CancellationToken.None);

        var updated = await _context.FabricTypes.FindAsync(fabricType.Id);
        updated!.Name.Should().Be("Moletom");
        updated.PricePerKg.Should().Be(35m);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsDomainException()
    {
        var command = new UpdateFabricTypeCommand(Guid.NewGuid(), "X", "Y", 10m, 5m, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    public void Dispose() => _context.Dispose();
}
