using API.Middleware;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Core.Entities.Identity;
using Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

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
app.UseSwaggerDocumentation();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<StoreContext>();
var identityContext = services.GetRequiredService<AppIdentityDbContext>();
var userManager = services.GetRequiredService<UserManager<AppUser>>();

var logger = services.GetRequiredService<ILogger<Program>>();
try
{
    await context.Database.MigrateAsync();
    await identityContext.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context);
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
}
app.Run();
