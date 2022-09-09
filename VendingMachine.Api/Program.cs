using VendingMachineApi.Authentication;
using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;
using VendingMachineApi.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Basic")
       .AddScheme<BasicAuthOptions, BasicAuthHandler>("Basic", _ => { });
       // .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
       // {
       //     options.RequireHttpsMetadata = false;
       //     options.SaveToken = true;
       //     options.TokenValidationParameters = new TokenValidationParameters
       //     {
       //         ValidAudience = configuration["JWT:ValidAudience"],
       //         ValidIssuer = configuration["JWT:ValidIssuer"],
       //         ValidateIssuerSigningKey = true,
       //         IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(configuration["JWT:Secret"]) ),
       //         ValidateIssuer = false,
       //         ValidateAudience = false,
       //         RequireSignedTokens = true
       //
       //     };
       // });

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IVendingService, VendingService>();
builder.Services.AddSingleton<IEntityStore<User>, DbUserStore>();
builder.Services.AddSingleton<IEntityStore<Product>, DbProductStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

// must be made public so that the test project can access it. cf https://stackoverflow.com/a/70490057
public partial class Program { }