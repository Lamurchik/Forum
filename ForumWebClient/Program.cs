using ForumWebClient.Models.DI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddTransient<ApiService>(sp =>
{
    var httpClient= sp.GetRequiredService<HttpClient>();
    var hostName = "localhost:7173";
    return new ApiService(httpClient, hostName);
});

builder.Services.AddSession(options =>
{
    
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.AddHttpClient();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.Run();
