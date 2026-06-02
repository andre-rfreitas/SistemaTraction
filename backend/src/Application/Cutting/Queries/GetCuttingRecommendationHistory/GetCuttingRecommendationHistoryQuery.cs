using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendationHistory;

public record GetCuttingRecommendationHistoryQuery(int Take = 10)
    : IRequest<List<CuttingRecommendationHistoryItemDto>>;
