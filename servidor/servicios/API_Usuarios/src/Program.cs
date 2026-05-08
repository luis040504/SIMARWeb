using API_Usuarios.src.Data;
using API_Usuarios.src.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Controladores (Esto permite usar la carpeta /Controllers)
builder.Services.AddControllers();

// 2. Configurar Swagger/OpenAPI (Para probar tu API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Configurar la Base de Datos (PostgreSQL)
// La cadena de conexión "DefaultConnection" debe estar en appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Registrar HttpClient (Necesario para que UsuarioService llame al MS de Empleados)
builder.Services.AddHttpClient<IUsuarioService, UsuarioService>();

// 5. Registrar la Inyección de Dependencias de tus servicios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    // Habilitamos la interfaz visual de Swagger en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 6. Mapear los Controladores (Muy importante para que tus rutas funcionen)
app.MapControllers();

app.Run();