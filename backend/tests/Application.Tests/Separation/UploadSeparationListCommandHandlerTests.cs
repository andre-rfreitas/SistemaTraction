using FluentAssertions;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.Commands.UploadSeparationList;
using SistemaTraction.Application.Tests;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Tests.Separation;

public class UploadSeparationListCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;

    public UploadSeparationListCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
    }

    private UploadSeparationListCommandHandler CreateHandler(List<ParsedItem> items)
    {
        var parser = new FakePdfParser(new ParseResult(true, items, null));
        return new UploadSeparationListCommandHandler(_context, parser);
    }

    [Fact]
    public async Task Handle_FourSegmentSku_ResolvesEstampaAndLinksCorrectDtfModel()
    {
        var dtfModel = DtfModel.Create("Made in Traction", "Folha 1", 9, 49.80m);
        _context.DtfModels.Add(dtfModel);
        var skuCode = SkuCode.Create("MADT", "Made in Traction", SkuCodeCategory.Estampa, dtfModel.Id);
        _context.SkuCodes.Add(skuCode);
        _context.SkuCodes.Add(SkuCode.Create("RED", "Vermelho", SkuCodeCategory.Cor));
        _context.SkuCodes.Add(SkuCode.Create("G", "G", SkuCodeCategory.Tamanho));
        await _context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler([new ParsedItem("REG-MADT-RED-G", "", "", 2, 1)]);
        var result = await handler.Handle(
            new UploadSeparationListCommand(Stream.Null, "lista.pdf"), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        var item = result.Items[0];
        item.Sku.Should().Be("REG-MADT-RED-G");
        item.Estampa.Should().Be("Made in Traction");
        item.Color.Should().Be("Vermelho");
        item.Size.Should().Be("G");
        item.DtfModelId.Should().Be(dtfModel.Id);
    }

    [Fact]
    public async Task Handle_ThreeSegmentSku_LegacyFormat_NoEstampa()
    {
        _context.SkuCodes.Add(SkuCode.Create("BLK", "Preto", SkuCodeCategory.Cor));
        _context.SkuCodes.Add(SkuCode.Create("M", "M", SkuCodeCategory.Tamanho));
        await _context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler([new ParsedItem("BBL-BLK-M", "", "", 3, 1)]);
        var result = await handler.Handle(
            new UploadSeparationListCommand(Stream.Null, "lista.pdf"), CancellationToken.None);

        var item = result.Items[0];
        item.Estampa.Should().BeEmpty();
        item.Color.Should().Be("Preto");
        item.Size.Should().Be("M");
        item.DtfModelId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_FourSegmentSku_UnknownEstampaCode_UsesFallbackAndNullDtfModelId()
    {
        await _context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler([new ParsedItem("REG-UNKN-BLK-P", "", "", 1, 1)]);
        var result = await handler.Handle(
            new UploadSeparationListCommand(Stream.Null, "lista.pdf"), CancellationToken.None);

        var item = result.Items[0];
        item.Estampa.Should().Be("UNKN");
        item.DtfModelId.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}

file sealed class FakePdfParser(ParseResult result) : IPdfParser
{
    public ParseResult Parse(Stream pdfStream, string fileName) => result;
}
