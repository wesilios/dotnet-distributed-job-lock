using System.Net;
using Microsoft.AspNetCore.Mvc;
using TRECs.Library.Application;
using TRECs.Library.Domain;

namespace Domain.Controllers.Abstractions;

public abstract class PublicApiBaseController<T> : Controller<T>
{
    protected IActionResult ProcessResponsePublicMessage<TU>(Response<TU> response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.OK => Ok(response),
            HttpStatusCode.Created => Created(string.Empty, response),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => StatusCode((int)HttpStatusCode.Forbidden, response),
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.UnprocessableEntity => UnprocessableEntity(response),
            _ => StatusCode(500, response)
        };
    }

    protected IActionResult ProcessResponsePublicMessage<TData>(PublicResponse<TData> response)
    {
        return response.StatusCode switch
        {
            (int)HttpStatusCode.OK => Ok(response),
            (int)HttpStatusCode.Created => Created(string.Empty, response),
            (int)HttpStatusCode.BadRequest => BadRequest(response),
            (int)HttpStatusCode.Unauthorized => Unauthorized(response),
            (int)HttpStatusCode.Forbidden => StatusCode((int)HttpStatusCode.Forbidden, response),
            (int)HttpStatusCode.NotFound => NotFound(response),
            (int)HttpStatusCode.UnprocessableEntity => UnprocessableEntity(response),
            _ => StatusCode(500, response)
        };
    }
}
