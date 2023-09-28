using idunno.Authentication.Basic;

using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
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
    .AddOData(options => options.AddRouteComponents("odata", model, new DefaultODataBatchHandler())
    // .AddOData(options => options.AddRouteComponents("odata", model, new ParallelODataBatchHandler()) // to process in parallel
        .Filter().Select().Expand().OrderBy().SetMaxTop(25));

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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("test", policy => policy.RequireAssertion(context =>
    {
        return true;
    }));
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

internal class ParallelODataBatchHandler : DefaultODataBatchHandler
{
    public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
    {
        var tasks = requests.Select(r => r.SendRequestAsync(handler));
        await Task.WhenAll(tasks);
        return tasks.Select(t => t.Result).ToList();
    }
}
