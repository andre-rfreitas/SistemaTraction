using MediatR;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;

/// <summary>
/// Registra uma movimentação de estoque DTF.
/// <para><c>Quantity</c> representa o número de <b>folhas</b> na Entrada
/// (convertido para estampas via StampsPerSheet) e o número de <b>estampas</b>
/// na Saída e no Ajuste (Ajuste pode ser negativo).</para>
/// </summary>
public record RegisterDtfMovementCommand(
    Guid DtfModelId,
    DtfMovementType Type,
    int Quantity,
    string? Reason
) : IRequest<Guid>;
