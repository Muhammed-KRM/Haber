using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public UploadController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Profil fotoÄŸrafÄ± veya ilan resmi yÃ¼kler.
    /// Max 5MB, sadece jpg/png/webp.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seÃ§ilmedi.");

        if (file.Length > MaxFileSize)
            return BadRequest("Dosya boyutu 5MB'dan bÃ¼yÃ¼k olamaz.");

        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest("Sadece JPG, PNG ve WebP formatlarÄ± desteklenir.");

        using var stream = file.OpenReadStream();
        var url = await _fileStorage.UploadAsync(stream, file.FileName, file.ContentType);

        return Ok(new { imageUrl = url });
    }

    /// <summary>
    /// YÃ¼klenmiÅŸ bir dosyayÄ± siler.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string fileUrl)
    {
        await _fileStorage.DeleteAsync(fileUrl);
        return NoContent();
    }
}
