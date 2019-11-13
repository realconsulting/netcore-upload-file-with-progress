using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using z.FileUpload;

namespace UploadFileWithProgress
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = Int64.MaxValue;
            });

            services.AddFileUpload(options=> {
                options.RootPath = @"C:\Temp\UploadedFiles";
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseFileUpload();

            app.UseMvc(route =>
            {
                route.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=UploadChunked}");
            });
        }
    }
}
