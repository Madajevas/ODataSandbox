using idunno.Authentication.Basic;

using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

using System.Security.Claims;

using WebAPI;

var builder = WebApplication.CreateBuilder(args);


var oDataBuilder = new ODataConventionModelBuilder();
oDataBuilder.EntitySet<WeatherForecast>("WeatherForecast");
IEdmModel model = oDataBuilder.GetEdmModel();

// Add services to the container.
builder.Services.AddControllers()
    .AddOData(options => options.AddRouteComponents("odata", model, new DefaultODataBatchHandler()).Filter().Select().Expand().OrderBy().SetMaxTop(25));

builder.Services
    .AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = "oauthtokenexchange";
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                var username = context.Username;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    new Claim( ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                };
                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();
    
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// test middleware
app.Use((context, next) =>
{
    return next();
});

app.UseODataBatching();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }
