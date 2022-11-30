using Convos.API.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;

    public FileController(FileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet]
    [Route("{name}")]
    public async Task<IActionResult> GetFile(string name)
    {
        var bytes = await _fileService.GetFile(name);
        if (bytes == null) return NotFound();

        return File(bytes, "application/octet-stream");
    }
}