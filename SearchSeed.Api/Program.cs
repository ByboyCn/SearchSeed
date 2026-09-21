using SearchSeed.Api.Services;
using SearchSeed.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.IncludeFields = true;
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddSingleton<SeedService>(sp =>
{
    var svc = new SeedService();
    var store = sp.GetRequiredService<GalaxyStore>();
    svc.TryLoad = store.Load;
    svc.OnComputed = (s, n, r, f, res) => { store.Save(s, n, r, f, res); return true; };
    return svc;
});
builder.Services.AddSingleton<GalaxyStore>();
builder.Services.AddHostedService<PrecomputeService>();
builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<BlueprintService>();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();