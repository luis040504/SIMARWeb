var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient("ClientesApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8005");
});

builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8001");
});

builder.Services.AddHttpClient("UsuariosApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8002");
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient("UserApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8002");
});

builder.Services.AddHttpClient("EmpleadoApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8008");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();

app.UseSession();

app.UseAuthorization();



app.MapGet("/", context =>
{
    context.Response.Redirect("/inicio");
    return Task.CompletedTask;
});

app.MapPost("/logout", context =>
{
    context.Session.Clear();

    context.Response.Redirect("/inicio");

    return Task.CompletedTask;
});


app.MapRazorPages()
   .WithStaticAssets();



/* Redirigir la raíz a tu página específica
app.MapGet("/", context => {
    context.Response.Redirect("/WasteTraceability/RegisterWasteCollection/IndexRegisterWasteCollection");
    return Task.CompletedTask;
});*/

app.Run();
