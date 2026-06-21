using IntelliINPI.Application.Abstractions;
using MediatR;

namespace IntelliINPI.Application.Imports;

public sealed record ImportRpiTrademarksRequest(int? RpiNumber);

public sealed record ImportRpiTrademarksCommand(int? RpiNumber) : IRequest<ImportTrademarksResult>;

public sealed class ImportRpiTrademarksCommandHandler(IInpiRpiTrademarkImporter importer)
    : IRequestHandler<ImportRpiTrademarksCommand, ImportTrademarksResult>
{
    public Task<ImportTrademarksResult> Handle(ImportRpiTrademarksCommand request, CancellationToken cancellationToken)
    {
        return importer.ImportAsync(request.RpiNumber, cancellationToken);
    }
}
