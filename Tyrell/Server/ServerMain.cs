using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Tyrell.Server
{
    public class ServerMain
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("OK");
            });
        }
    }
}