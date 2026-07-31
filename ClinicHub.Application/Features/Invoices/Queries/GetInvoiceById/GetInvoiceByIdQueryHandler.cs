using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithItemsAsync(request.InvoiceId)
            ?? throw new NotFoundException(localizer[LocalizationKeys.InvoiceMessages.NotFound]);

        return mapper.Map<InvoiceDto>(invoice);
    }
}
