using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjectForm.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

builder.Services.AddDbContext<InternDBcontext>(ConfigureDb);

static void ConfigureDb(IServiceProvider sp, DbContextOptionsBuilder options)
{
    var config = sp.GetRequiredService<IConfiguration>();
    options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intern System API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName); 
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intern System API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();            // Session burada
app.UseAuthentication();     // Eğer login varsa
app.UseAuthorization();



app.MapControllerRoute(
    name: "login-short",
    pattern: "login",
    defaults: new { controller = "Home", action = "Login" }
);

app.MapControllerRoute(
    name: "todo-short",
    pattern: "todo",
    defaults: new { controller = "Home", action = "Todo" }
);

app.MapControllerRoute(
    name: "loginpage-alias",
    pattern: "LoginPage",
    defaults: new { controller = "Home", action = "Login" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run(); 