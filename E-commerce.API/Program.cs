using E_commerce.API.Extensions;
using E_commerce.Application;
using E_commerce.Application.Profiles;
using E_commerce.Application.Settings;
using E_commerce.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace E_commerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Console.WriteLine($"Program BaseUrl: [{builder.Configuration["EmailConfirmation:BaseUrl"]}]");

            // Add services to the container.

            builder.Services.AddControllers();

            #region Apply Services 
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection("UrlSettings"));
            #endregion


            #region Payment
            builder.Services.Configure<PaymentGatewaySettings>(builder.Configuration.GetSection("Stripe"));
            #endregion
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<AuthorizedOperationTransformer>();
            });

           

            var app = builder.Build();

            #region Data Seeding
            await app.SeedDataBaseAsync();
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });

            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }

    internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document,OpenApiDocumentTransformerContext context,CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            return Task.CompletedTask;
        }
    }

    internal sealed class AuthorizedOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation,OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var requiresAuthorization = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<IAuthorizeData>()
                .Any();

            if (requiresAuthorization)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = Array.Empty<string>()
                });
            }

            return Task.CompletedTask;
        }
    }
}
