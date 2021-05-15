using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Model;
using WebApplication.Utility;

namespace WebApplication.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("shortenurl")]
        public IActionResult ShortenUrl(UrlModel urlModel)
        {
            var response = HandleURL.HandleShortenUrl(_httpContextAccessor.HttpContext, urlModel.url);
            return Ok(response);
        }
    }
}
