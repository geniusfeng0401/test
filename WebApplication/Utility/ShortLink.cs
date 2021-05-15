using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApplication.Utility
{
    public class ShortLink
    {
        public string GetUrlChunk()
        {
            // Transform the "Id" property on this object into a short piece of text
            return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));
        }

        public static int GetId(string urlChunk)
        {
            // Reverse our short url text back into an interger Id
            var id = 0;
            try
            {
                id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(urlChunk));
                return id;
            }
            catch(Exception ex)
            {
                return id;
            }
        }

        public int Id { get; set; }

        public string Url { get; set; }
    }

    public static class HandleURL
    {
        public static Response HandleShortenUrl(HttpContext context, string requestedUrl)
        {
            var response = new Response();
            // Perform basic validation
            if (String.IsNullOrEmpty(requestedUrl))
            {
                response.isError = true;
                response.errorMessage = "Url cannot be empty";
                return response;
            }

            // Test our URL
            if (!Uri.TryCreate(requestedUrl, UriKind.Absolute, out Uri result))
            {
                response.isError = true;
                response.errorMessage = "Could not understand URL.";
                return response;
            }

            var url = result.ToString();

            // Ask for LiteDB and persist a short link
            var db = context.RequestServices.GetService(typeof(ILiteDatabase)) as ILiteDatabase;
            var links = db.GetCollection<ShortLink>(BsonAutoId.Int32);
            // Temporary short link 
            var entry = new ShortLink
            {
                Url = url
            };

            // Insert our short-link
            links.Insert(entry);

            var urlChunk = entry.GetUrlChunk();
            var responseUri = $"{context.Request.Scheme}://{context.Request.Host}/test/{urlChunk}";
            response.isError = false;
            response.shortenUrl = responseUri;
            return response;
        }

        public static Task HandleRedirect(HttpContext context)
        {
            var db = context.RequestServices.GetService(typeof(ILiteDatabase)) as ILiteDatabase;

            var collection = db.GetCollection<ShortLink>(BsonAutoId.Int32);

            var path = context.Request.Path.ToUriComponent().Replace("/test", "").Trim('/');
            var id = ShortLink.GetId(path);
            var entry = collection.Find(p => p.Id == id).FirstOrDefault();

            if (entry != null)
                context.Response.Redirect(entry.Url);
            else
                context.Response.Redirect("/");

            return Task.CompletedTask;
        }
    }

    public class Response
    {
        public string shortenUrl { get; set; }
        public bool isError { get; set; }
        public string errorMessage { get; set; }
    }
}
