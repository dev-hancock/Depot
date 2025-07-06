namespace Depot.Repository.Endpoints;

using System.Reactive.Linq;
using Extensions;
using Handlers;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

public static class UploadEndpoint
{
    public async static Task<IResult> Handle([FromRoute] string repository, [FromRoute] string path, IMediator mediator,
        HttpContext context)
    {
        var id = context.GetUserId();

        if (id == null)
        {
            return Results.Unauthorized();
        }

        var form = await context.Request.ReadFormAsync();

        if (form.Files.Count == 0)
        {
            return Results.BadRequest();
        }

        var errors = await form.Files
            .Select(x => mediator
                .Send(new UploadHandler.Request(
                    context.GetTenantId()!.Value,
                    context.GetUserId()!.Value,
                    x.FileName,
                    path,
                    repository,
                    x.OpenReadStream(),
                    x.ContentType,
                    context.GetUser()!)))
            .ToObservable()
            .Merge();

        return Results.NoContent();
    }
}