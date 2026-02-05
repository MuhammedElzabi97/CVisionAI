using CVisionAI.Data;
using CVisionAI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC + API controllers
builder.Services.AddControllersWithViews();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for frontend clients (can be expanded as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                "http://localhost:5173", // React web
                "http://localhost:3000",
                "http://localhost:19006", // Expo / React Native (common)
                "http://localhost:19000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Application services (MVP implementations)
builder.Services.AddHttpClient<IAiWriterService, AiWriterService>(client =>
{
    var baseUrl = builder.Configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<IAtsScoringService, AtsScoringService>();
builder.Services.AddScoped<ICvRenderer, CvRenderer>();
builder.Services.AddScoped<IExportService, FileExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files for wwwroot
app.UseStaticFiles();

// Static files for generated exports under /storage
var storagePath = Path.Combine(app.Environment.ContentRootPath, "storage");
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage",
    ContentTypeProvider = new FileExtensionContentTypeProvider()
});

app.UseRouting();

app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// API controllers
app.MapControllers();

app.Run();
