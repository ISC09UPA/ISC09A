using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Comentarios de solo texto de una publicación (plano, sin respuestas anidadas).</summary>
[ApiController]
[Route("api/posts/{postId:guid}/comments")]
[Authorize]
[Produces("application/json")]
public class CommentsController(ReportUDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Lista los comentarios de una publicación, del más antiguo al más reciente.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Lista de comentarios.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentResponse>>> GetAll([FromRoute] Guid postId)
    {
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return PostNotFound();

        var comments = await db.Comments.AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Body = c.Body,
            Author = AuthorDisplay.Resolve(c.Author, IdentityMode.Username),
            CreatedAt = c.CreatedAt,
        }).ToList());
    }

    /// <summary>Comenta una publicación. El autor del comentario siempre se muestra con su username (el anonimato aplica a publicaciones).</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <param name="request">Texto del comentario (máx. 1000 caracteres).</param>
    /// <response code="201">Comentario creado.</response>
    /// <response code="400">Texto inválido.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponse>> Create([FromRoute] Guid postId, [FromBody] CreateCommentRequest request)
    {
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (post is null) return PostNotFound();

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            AuthorId = currentUser.UserId!.Value,
            Body = request.Body.Trim(),
        };
        post.CommentCount++;
        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        var author = await db.Users.AsNoTracking().SingleAsync(u => u.Id == comment.AuthorId);
        return CreatedAtAction(nameof(GetAll), new { postId }, new CommentResponse
        {
            Id = comment.Id,
            Body = comment.Body,
            Author = AuthorDisplay.Resolve(author, IdentityMode.Username),
            CreatedAt = comment.CreatedAt,
        });
    }

    private ActionResult PostNotFound() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "Publicación no encontrada",
        Detail = "No existe una publicación con ese id.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
    });
}
