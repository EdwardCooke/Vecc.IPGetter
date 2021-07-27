using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vecc.IPGetter
{
    public static class GetFunction
    {
        [FunctionName("get")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var ip = string.Empty;

            try
            {
                var header = req.Headers["X-Forwarded-For"];
                if (header.Count >= 1)
                {
                    log.LogInformation("Found header: {0}", header);
                    var value = header[0];
                    var ips = value.Split(',');
                    if (ips.Length >= 1)
                    {
                        ip = ips.First();
                        log.LogInformation("Found IP and Port: {0}", ip);
                        ip = ip.Split(':')[0];
                        log.LogInformation("Found IP: {0}", ip);
                    }
                    else
                    {
                        log.LogWarning("X-Forwarded-For header found but no values.");
                    }
                }
                else
                {
                    log.LogInformation("No X-Forwaded-For header found");
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Error getting IP from X-Forwarded-For header");
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                log.LogInformation("IP not found, attempting to retrieve from connection.RemoteIpAddress");
                try
                {
                    ip = req.HttpContext.Connection.RemoteIpAddress.ToString();
                    log.LogInformation("Got ip address: {0}", ip);
                }
                catch (Exception exception)
                {
                    log.LogError(exception, "Unable to get client IP from RemoteIpAddress");
                }
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                log.LogInformation("IP not found, returning notfound result");
                return new NotFoundResult();
            }

            log.LogInformation("IP Found, returning: {0}", ip);
            return new OkObjectResult(ip);
        }
    }
}
