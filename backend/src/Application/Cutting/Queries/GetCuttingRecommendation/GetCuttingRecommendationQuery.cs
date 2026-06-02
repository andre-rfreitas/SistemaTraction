using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendation;

public record GetCuttingRecommendationQuery(Guid FabricRollId, int? DaysBack = null)
    : IRequest<CuttingRecommendationDto>;
