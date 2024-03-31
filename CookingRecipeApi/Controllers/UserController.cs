using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AzureBlobHandler _azureBlobHandler;

        public UserController( AzureBlobHandler azureBlobHandler)
        {
            _azureBlobHandler = azureBlobHandler;
        }

        [HttpPost("test-upload")]
        public async Task<IActionResult> TestUpload( IFormFile file)
        {
            string? url = await _azureBlobHandler.UploadSingleBlob(file);
            if (url == null)
            {
                return BadRequest("url is null upload may fail");
            }
            return Ok(url);
        }
    }
}
