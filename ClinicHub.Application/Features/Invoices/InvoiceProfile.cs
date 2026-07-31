using AutoMapper;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Invoices;

public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<InvoiceItem, InvoiceLineItemDto>();
    }
}
