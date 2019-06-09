using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace eventphone.grafanalogo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression()
                .AddMvcCore()
                .AddFormatterMappings()
                .AddCors()
                .AddJsonFormatters();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
                .UseResponseCompression()
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{action=Index}",
                        defaults: new {controller = "Home"});
                });
        }
    }
}
