using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.UploadSeparationList;

public record UploadSeparationListCommand(Stream PdfStream, string FileName)
    : IRequest<SeparationListDetailDto>;
