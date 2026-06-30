using FluentAssertions;
using SistemaTraction.Application.Separation.Commands.UpsertSkuCode;
using SistemaTraction.Application.Tests;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Tests.Separation;

public class UpsertSkuCodeCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly UpsertSkuCodeCommandHandler _handler;

    public UpsertSkuCodeCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpsertSkuCodeCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_EstampaCategory_WithoutDtfModelId_ThrowsDomainException()
    {
        var command = new UpsertSkuCodeCommand(null, "MADT", "Made in Traction", "Estampa", null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*modelo DTF*");
    }

    [Fact]
    public async Task Handle_EstampaCategory_WithDtfModelId_SavesDtfModelId()
    {
        var dtfModel = DtfModel.Create("Made in Traction", "Folha 1", 9, 49.80m);
        _context.DtfModels.Add(dtfModel);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpsertSkuCodeCommand(null, "MADT", "Made in Traction", "Estampa", dtfModel.Id);

        var dto = await _handler.Handle(command, CancellationToken.None);

        dto.DtfModelId.Should().Be(dtfModel.Id);
        dto.Category.Should().Be("Estampa");
        dto.Code.Should().Be("MADT");
    }

    [Fact]
    public async Task Handle_CorCategory_WithDtfModelId_ThrowsDomainException()
    {
        var command = new UpsertSkuCodeCommand(null, "RED", "Vermelho", "Cor", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Estampa*");
    }

    public void Dispose() => _context.Dispose();
}
