using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.SendCuttingOrder;

public record SendCuttingOrderCommand(Guid OrderId) : IRequest<SendCuttingOrderResult>;
