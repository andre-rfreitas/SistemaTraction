using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Fabric;

public class DeleteFabricColorCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly DeleteFabricColorCommandHandler _handler;

    public DeleteFabricColorCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new DeleteFabricColorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_SoftDeletesColor()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto", "#000000");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var colorId = fabricType.Colors.First().Id;
        await _handler.Handle(new DeleteFabricColorCommand(fabricType.Id, colorId), CancellationToken.None);

        var color = await _context.FabricColors.FindAsync(colorId);
        color!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ColorNotFound_ThrowsDomainException()
    {
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var act = () => _handler.Handle(new DeleteFabricColorCommand(fabricType.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrada*");
    }

    public void Dispose() => _context.Dispose();
}
