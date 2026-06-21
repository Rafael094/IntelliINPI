using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/documents")]
public sealed class NitDocumentsController(IMediator mediator, INitDocumentStorage storage) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await mediator.Send(new ListNitDocumentsQuery(), ct));

    [HttpPost, RequestSizeLimit(26_000_000)]
    public async Task<IActionResult> Upload([FromForm] NitDocumentUploadRequest request, CancellationToken ct)
    {
        NitDocumentUploadPolicy.Validate(request.File.FileName, request.File.ContentType, request.File.Length);
        await using var stream = request.File.OpenReadStream();
        var stored = await storage.SaveAsync(stream, request.File.FileName, request.Encrypt, ct);
        try
        {
            return Ok(await mediator.Send(new CreateNitDocumentCommand(request.Name, request.Type, request.InstitutionId, request.InventionId, request.ContractId, request.File.FileName, stored.StoredFileName, request.File.ContentType, request.File.Length, stored.StoragePath, stored.IsEncrypted, stored.EncryptionAlgorithm, stored.EncryptionIV), ct));
        }
        catch { await storage.DeleteAsync(stored.StoragePath, ct); throw; }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var document = await mediator.Send(new GetNitDocumentQuery(id), ct);
        var stream = await storage.OpenReadAsync(document.StoragePath, document.IsEncrypted, document.EncryptionIV, ct);
        await mediator.Send(new RecordNitDocumentDownloadCommand(id), ct);
        return File(stream, document.ContentType, document.OriginalFileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var path = await mediator.Send(new DeleteNitDocumentCommand(id), ct);
        await storage.DeleteAsync(path, ct);
        return NoContent();
    }
}

public sealed class NitDocumentUploadRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Outros";
    public Guid InstitutionId { get; set; }
    public Guid? InventionId { get; set; }
    public Guid? ContractId { get; set; }
    public IFormFile File { get; set; } = null!;
    public bool Encrypt { get; set; }
}
