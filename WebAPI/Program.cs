using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

using WebAPI;

var builder = WebApplication.CreateBuilder(args);


var oDataBuilder = new ODataConventionModelBuilder();
oDataBuilder.EntitySet<WeatherForecast>("WeatherForecast");
IEdmModel model = oDataBuilder.GetEdmModel();

// Add services to the container.
builder.Services.AddControllers()
    .AddOData(options => options.AddRouteComponents("odata", model, new DefaultODataBatchHandler()).Filter().Select().Expand().OrderBy().SetMaxTop(25));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseODataBatching();
app.UseRouting();

app.MapControllers();

app.Run();


public partial class Program { }
