using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Dtf.Commands.CreateDtfModel;
using SistemaTraction.Application.Dtf.Commands.DeleteDtfModel;
using SistemaTraction.Application.Dtf.Commands.UpdateDtfModel;
using SistemaTraction.Application.Dtf.Queries.GetDtfModelById;
using SistemaTraction.Application.Dtf.Queries.GetDtfModels;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Tests.Dtf;

public class DtfModelCrudTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly CreateDtfModelCommandHandler _createHandler;
    private readonly UpdateDtfModelCommandHandler _updateHandler;
    private readonly DeleteDtfModelCommandHandler _deleteHandler;
    private readonly GetDtfModelsQueryHandler _getAllHandler;
    private readonly GetDtfModelByIdQueryHandler _getByIdHandler;

    public DtfModelCrudTests()
    {
        _context = TestDbContextFactory.Create();
        _createHandler = new CreateDtfModelCommandHandler(_context);
        _updateHandler = new UpdateDtfModelCommandHandler(_context);
        _deleteHandler = new DeleteDtfModelCommandHandler(_context);
        _getAllHandler = new GetDtfModelsQueryHandler(_context);
        _getByIdHandler = new GetDtfModelByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Create_ValidModel_PersistsCorrectly()
    {
        var command = new CreateDtfModelCommand("Angel", "Folha 1", 9, 49.80m);

        var id = await _createHandler.Handle(command, CancellationToken.None);

        var saved = await _context.DtfModels.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Angel");
        saved.SheetLabel.Should().Be("Folha 1");
        saved.StampsPerSheet.Should().Be(9);
        saved.SheetCost.Should().Be(49.80m);
        saved.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsDomainException()
    {
        var command = new CreateDtfModelCommand("", "Folha 1", 9, 49.80m);

        var act = () => _createHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Create_ZeroStampsPerSheet_ThrowsDomainException()
    {
        var command = new CreateDtfModelCommand("Angel", "Folha 1", 0, 49.80m);

        var act = () => _createHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task Create_ZeroSheetCost_ThrowsDomainException()
    {
        var command = new CreateDtfModelCommand("Angel", "Folha 1", 9, 0m);

        var act = () => _createHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task Update_ExistingModel_UpdatesFieldsCorrectly()
    {
        var createId = await _createHandler.Handle(
            new CreateDtfModelCommand("Angel", "Folha 1", 9, 49.80m), CancellationToken.None);

        await _updateHandler.Handle(
            new UpdateDtfModelCommand(createId, "Angel Updated", "Folha 1A", 10, 55.00m),
            CancellationToken.None);

        var updated = await _context.DtfModels.FindAsync(createId);
        updated!.Name.Should().Be("Angel Updated");
        updated.SheetLabel.Should().Be("Folha 1A");
        updated.StampsPerSheet.Should().Be(10);
        updated.SheetCost.Should().Be(55.00m);
    }

    [Fact]
    public async Task Update_NonExistentModel_ThrowsDomainException()
    {
        var act = () => _updateHandler.Handle(
            new UpdateDtfModelCommand(Guid.NewGuid(), "X", "Folha X", 1, 49.80m),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task Delete_ExistingModel_SoftDeletes()
    {
        var id = await _createHandler.Handle(
            new CreateDtfModelCommand("Flaming Skull", "Folha 7", 8, 49.80m), CancellationToken.None);

        await _deleteHandler.Handle(new DeleteDtfModelCommand(id), CancellationToken.None);

        var model = await _context.DtfModels.FindAsync(id);
        model!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistentModel_ThrowsDomainException()
    {
        var act = () => _deleteHandler.Handle(
            new DeleteDtfModelCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyNonDeletedModels()
    {
        var id1 = await _createHandler.Handle(
            new CreateDtfModelCommand("Model A", "Folha 1", 5, 49.80m), CancellationToken.None);
        var id2 = await _createHandler.Handle(
            new CreateDtfModelCommand("Model B", "Folha 2", 6, 49.80m), CancellationToken.None);

        await _deleteHandler.Handle(new DeleteDtfModelCommand(id2), CancellationToken.None);

        var result = await _getAllHandler.Handle(new GetDtfModelsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(id1);
    }

    [Fact]
    public async Task GetById_ExistingModel_ReturnsDto()
    {
        var id = await _createHandler.Handle(
            new CreateDtfModelCommand("Red Rage", "Folha 5", 4, 49.80m), CancellationToken.None);

        var result = await _getByIdHandler.Handle(new GetDtfModelByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Red Rage");
        result.StampsPerSheet.Should().Be(4);
    }

    [Fact]
    public async Task GetById_DeletedModel_ReturnsNull()
    {
        var id = await _createHandler.Handle(
            new CreateDtfModelCommand("Flying Souls", "Folha 2", 6, 49.80m), CancellationToken.None);
        await _deleteHandler.Handle(new DeleteDtfModelCommand(id), CancellationToken.None);

        var result = await _getByIdHandler.Handle(new GetDtfModelByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
