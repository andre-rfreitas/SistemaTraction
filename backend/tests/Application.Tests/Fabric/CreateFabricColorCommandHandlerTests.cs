using FluentAssertions;
using SistemaTraction.Application.Fabric.Commands.CreateFabricColor;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Fabric;

public class CreateFabricColorCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly CreateFabricColorCommandHandler _handler;

    public CreateFabricColorCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateFabricColorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsColorToFabricType()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // simula novo escopo (handler recebe contexto fresh)

        var command = new CreateFabricColorCommand(fabricType.Id, "Preto", "#000000");
        var colorId = await _handler.Handle(command, CancellationToken.None);

        var saved = await _context.FabricColors.FindAsync(colorId);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Preto");
        saved.FabricTypeId.Should().Be(fabricType.Id);
    }

    [Fact]
    public async Task Handle_DuplicateColor_ThrowsDomainException()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var command = new CreateFabricColorCommand(fabricType.Id, "Preto", null);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Preto*");
    }

    [Fact]
    public async Task Handle_FabricTypeNotFound_ThrowsDomainException()
    {
        var command = new CreateFabricColorCommand(Guid.NewGuid(), "Preto", null);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    public void Dispose() => _context.Dispose();
}
