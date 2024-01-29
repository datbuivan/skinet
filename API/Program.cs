using API.Middleware;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
// builder.Services.AddDbContext<StoreContext>(options =>
// {
//     options.UseSqlServer(builder.Configuration.GetConnectionString("ECommerceConnectstring"));
// });
// builder.Services.AddScoped<IProductRepository, ProductRepository>();
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// builder.Services.Configure<ApiBehaviorOptions>(options=>{
//     options.InvalidModelStateResponseFactory = actionContext =>{
//         var errors = actionContext.ModelState
//         .Where(e=>e.Value.Errors.Count >0)
//         .SelectMany(x=>x.Value.Errors)
//         .Select(x=>x.ErrorMessage).ToArray();
//         var errorResponse = new ApiValidationErrorResponse
//         {
//             Errors = errors
//         };
//         return new BadRequestObjectResult(errorResponse);
//     };
// });
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("CorsPolicy");
app.MapControllers();
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<StoreContext>();
var logger = services.GetRequiredService<ILogger<Program>>();
try
{
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
}
app.Run();
