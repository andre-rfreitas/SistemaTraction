using FluentAssertions;
using SistemaTraction.Application.Fabric.Commands.CreateFabricType;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Tests.Fabric;

public class CreateFabricTypeCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly CreateFabricTypeCommandHandler _handler;

    public CreateFabricTypeCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateFabricTypeCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFabricType()
    {
        var command = new CreateFabricTypeCommand("Malha", "Regular", 20m, 10m, 80);

        var id = await _handler.Handle(command, CancellationToken.None);

        var saved = await _context.FabricTypes.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Malha");
        saved.Variation.Should().Be("Regular");
        saved.PricePerKg.Should().Be(20m);
    }

    [Fact]
    public async Task Handle_InvalidPrice_ThrowsDomainException()
    {
        var command = new CreateFabricTypeCommand("Malha", "Regular", 0m, 10m, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsDomainException()
    {
        var command = new CreateFabricTypeCommand("", "Regular", 20m, 10m, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    public void Dispose() => _context.Dispose();
}
