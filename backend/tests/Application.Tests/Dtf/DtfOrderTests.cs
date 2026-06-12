using FluentAssertions;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;
using Xunit;

namespace SistemaTraction.Application.Tests.Dtf;

public class DtfOrderTests
{
    [Fact]
    public void Create_WithItems_CreatesInDraftStatus()
    {
        var items = new List<(Guid, int)> { (Guid.NewGuid(), 10) };
        var order = DtfOrder.Create(1, items, "Nota teste");
        order.Status.Should().Be(DtfOrderStatus.Draft);
        order.OrderNumber.Should().Be(1);
        order.Notes.Should().Be("Nota teste");
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyItems_ThrowsDomainException()
    {
        var act = () => DtfOrder.Create(1, new List<(Guid, int)>(), null);
        act.Should().Throw<DomainException>().WithMessage("*pelo menos um modelo*");
    }

    [Fact]
    public void Create_WithDuplicateModel_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var items = new List<(Guid, int)> { (modelId, 5), (modelId, 3) };
        var act = () => DtfOrder.Create(1, items, null);
        act.Should().Throw<DomainException>().WithMessage("*duplicado*");
    }

    [Fact]
    public void AddItem_ToDraftOrder_AddsItem()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        var newModelId = Guid.NewGuid();
        order.AddItem(newModelId, 3);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_DuplicateModel_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5) }, null);
        var act = () => order.AddItem(modelId, 3);
        act.Should().Throw<DomainException>().WithMessage("*duplicado*");
    }

    [Fact]
    public void RemoveItem_FromDraftOrder_RemovesItem()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5), (Guid.NewGuid(), 3) }, null);
        order.RemoveItem(modelId);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_FromSentOrder_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5) }, null);
        order.MarkSent();
        var act = () => order.RemoveItem(modelId);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkSent_FromDraft_TransitionsToSent()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.Status.Should().Be(DtfOrderStatus.Sent);
        order.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkSent_FromSent_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        var act = () => order.MarkSent();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkReceived_FromSent_TransitionsToReceived()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.MarkReceived();
        order.Status.Should().Be(DtfOrderStatus.Received);
        order.ReceivedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkReceived_FromDraft_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        var act = () => order.MarkReceived();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_InDraft_SetsIsDeleted()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.Cancel();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Cancel_InSent_SetsIsDeleted()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.Cancel();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Cancel_InReceived_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.MarkReceived();
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*recebido*");
    }

    [Fact]
    public void UpdateNotes_InDraft_UpdatesNotes()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.UpdateNotes("nova nota");
        order.Notes.Should().Be("nova nota");
    }

    [Fact]
    public void UpdateNotes_InSent_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        var act = () => order.UpdateNotes("nova nota");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DtfOrderItem_Create_WithZeroQuantity_ThrowsDomainException()
    {
        var act = () => DtfOrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 0);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DtfOrderItem_Create_WithNegativeQuantity_ThrowsDomainException()
    {
        var act = () => DtfOrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), -1);
        act.Should().Throw<DomainException>();
    }
}
