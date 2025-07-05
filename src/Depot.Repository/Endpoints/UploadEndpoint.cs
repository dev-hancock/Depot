namespace Depot.Repository.Endpoints;

using System.Reactive.Linq;
using Handlers;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

public static class UploadEndpoint
{
    public async static Task<IResult> Handle([FromRoute] string repository, IMediator mediator, HttpContext context)
    {
        var user = context.User?.Identity?.Name;

        if (user == null)
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
                    x.FileName,
                    repository,
                    x.OpenReadStream(),
                    x.ContentType,
                    user)))
            .ToObservable()
            .Merge();

        return Results.NoContent();
    }
}