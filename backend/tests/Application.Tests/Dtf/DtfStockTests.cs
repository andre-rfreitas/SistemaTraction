using FluentAssertions;
using SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;
using SistemaTraction.Application.Dtf.Queries.GetDtfStockItemByModel;
using SistemaTraction.Application.Dtf.Queries.GetDtfStockItems;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Tests.Dtf;

public class DtfStockTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly RegisterDtfMovementCommandHandler _registerHandler;
    private readonly GetDtfStockItemsQueryHandler _getAllHandler;
    private readonly GetDtfStockItemByModelQueryHandler _getByModelHandler;

    private readonly Guid _modelId;

    public DtfStockTests()
    {
        _context = TestDbContextFactory.Create();
        _registerHandler = new RegisterDtfMovementCommandHandler(_context);
        _getAllHandler = new GetDtfStockItemsQueryHandler(_context);
        _getByModelHandler = new GetDtfStockItemByModelQueryHandler(_context);

        // Seed de um DtfModel para uso nos testes
        var model = DtfModel.Create("Angel", "Folha 1", 9, 49.80m);
        _context.DtfModels.Add(model);
        _context.SaveChanges();
        _modelId = model.Id;
    }

    [Fact]
    public async Task Entrada_PrimeiraMovimentacao_CriaStockItemEAtualiza()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, "Compra inicial"),
            CancellationToken.None);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(90); // 10 folhas * 9 estampas
    }

    [Fact]
    public async Task Entrada_SegundaMovimentacao_ReutilizaStockItemExistente()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, null),
            CancellationToken.None);

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 3, null),
            CancellationToken.None);

        var count = _context.DtfStockItems.Count(i => i.DtfModelId == _modelId);
        count.Should().Be(1, "deve existir apenas um StockItem por modelo");

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(72); // (5 + 3) folhas * 9
    }

    [Fact]
    public async Task Saida_ComEstoqueSuficiente_ReducStock()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 20, null),
            CancellationToken.None); // 180 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 7, "Produção"),
            CancellationToken.None); // -7 estampas

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(173);
    }

    [Fact]
    public async Task Saida_EstoqueInsuficiente_ThrowsDomainException()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, null),
            CancellationToken.None); // 45 estampas

        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 100, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public async Task Saida_SemEstoque_ThrowsDomainException()
    {
        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 1, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AjustePositivo_IncrementaEstoque()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // 90 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, 5, "Recontagem"),
            CancellationToken.None); // +5 estampas

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(95);

        var ajuste = _context.DtfStockMovements.Single(m => m.Type == DtfMovementType.Ajuste);
        ajuste.SheetCount.Should().BeNull();
    }

    [Fact]
    public async Task AjusteNegativo_DecrementaEstoque()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // 90 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, -3, "Perda"),
            CancellationToken.None);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(87);
    }

    [Fact]
    public async Task AjusteZero_ThrowsDomainException()
    {
        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, 0, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*zero*");
    }

    [Fact]
    public async Task AjusteNegativo_EstoqueInsuficiente_ThrowsDomainException()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 1, null),
            CancellationToken.None); // 9 estampas

        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, -50, "Erro"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public async Task ModeloNaoEncontrado_ThrowsDomainException()
    {
        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(Guid.NewGuid(), DtfMovementType.Entrada, 5, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task GetAll_RetornaItensComNomeDoModelo()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None);

        var items = await _getAllHandler.Handle(new GetDtfStockItemsQuery(), CancellationToken.None);

        items.Should().HaveCount(1);
        items[0].ModelName.Should().Be("Angel");
        items[0].SheetLabel.Should().Be("Folha 1");
        items[0].CurrentQuantity.Should().Be(90);
        items[0].StampsPerSheet.Should().Be(9);
    }

    [Fact]
    public async Task GetByModel_RetornaDetalheComMovimentos()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 20, "Compra"),
            CancellationToken.None); // +180 estampas, SheetCount 20

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 5, "Produção"),
            CancellationToken.None); // -5 estampas

        var detail = await _getByModelHandler.Handle(
            new GetDtfStockItemByModelQuery(_modelId), CancellationToken.None);

        detail.Should().NotBeNull();
        detail!.Item.CurrentQuantity.Should().Be(175);
        detail.Movements.Should().HaveCount(2);
        detail.Movements[0].Delta.Should().Be(-5);  // mais recente primeiro
        detail.Movements[0].SheetCount.Should().BeNull();
        detail.Movements[1].Delta.Should().Be(180);
        detail.Movements[1].SheetCount.Should().Be(20);
    }

    [Fact]
    public async Task GetByModel_SemStockItem_RetornaNull()
    {
        var result = await _getByModelHandler.Handle(
            new GetDtfStockItemByModelQuery(_modelId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Movimentos_SaoAppendOnly_NuncaAlterados()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // +90

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 3, null),
            CancellationToken.None); // -3

        var movements = _context.DtfStockMovements.ToList();
        movements.Should().HaveCount(2, "cada operação gera exatamente um registro novo");
        movements.Select(m => m.Delta).Should().BeEquivalentTo([90, -3]);
    }

    [Fact]
    public async Task Entrada_ConverteFolhasParaEstampasERegistraSheetCount()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, "Compra"),
            CancellationToken.None);

        var movement = _context.DtfStockMovements.Single();
        movement.Delta.Should().Be(45);       // 5 folhas * 9 estampas
        movement.SheetCount.Should().Be(5);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(45);
    }

    public void Dispose() => _context.Dispose();
}
