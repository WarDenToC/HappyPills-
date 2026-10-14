using MedSafety.Core.Audit;
using MedSafety.Core.Security;
using MedSafety.Data;
using MedSafety.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MedSafety.Web.Services.CurrentUser>();
builder.Services.AddSingleton<MedSafety.Web.Services.PrescriptionStore>();

// SQLite data layer: formulary, patients and audit log are stored in medsafety.db
builder.Services.AddDbContextFactory<MedSafetyDbContext>(o => o.UseSqlite("Data Source=medsafety.db"));
builder.Services.AddSingleton<SqliteReferenceData>();
builder.Services.AddSingleton<IAuditLog, SqliteAuditLog>();
builder.Services.AddSingleton<AuthorizationService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Create and seed medsafety.db on first start
using (var db = app.Services.GetRequiredService<IDbContextFactory<MedSafetyDbContext>>().CreateDbContext())
{
    DatabaseSeeder.EnsureSeeded(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();