using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using z.FileUpload.Data;
using z.FileUpload.Interfaces;
using z.FileUpload.Repositories;
using z.FileUpload.Services;

namespace z.FileUpload
{
    public static class FileUploadExtensions
    {
        internal const string PolicyName = "FileUploadPolicy";
        public static IServiceCollection AddFileUpload(this IServiceCollection services, Action<FileUploadOptions> options = null)
        {

            services.AddCors(o => o.AddPolicy(PolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
             
            var fOption = new FileUploadOptions();
            options?.Invoke(fOption);

            services.AddSingleton(fOption);
            services.AddTransient<IFileRepository, LocalFileSystemRepository>();
            services.AddSingleton<IUploadService, UploadService>();

            return services;
        }

        public static IApplicationBuilder UseFileUpload(this IApplicationBuilder app)
        {
            app.UseCors(PolicyName); 
            return app;
        }
    }
}
