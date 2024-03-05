using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;


using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;

namespace ActionFilter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GzipCompressionAttribute : Attribute, IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // No action needed after the result has been executed
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var response = context.HttpContext.Response;
            var originalBodyStream = response.Body;

            using (var memoryStream = new MemoryStream())
            {
                response.Body = memoryStream;

                context.HttpContext.Response.Headers.Add("Content-Encoding", "gzip");

                using (var compressionStream = new GZipStream(originalBodyStream, CompressionMode.Compress))
                {
                    memoryStream.CopyTo(compressionStream);
                }
            }
        }
    }
}