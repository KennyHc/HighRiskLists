using Microsoft.EntityFrameworkCore;
using HighRiskLists.Infrastructure.Data;
using HighRiskLists.Core.Entities;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Successfully connected to the database.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while connecting to the database: {ex.Message}");
    }
}

app.MapGet("/test-connection", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    using var connection = new SqlConnection(connectionString);
    try
    {
        await connection.OpenAsync();
        return Results.Ok($"Successfully connected to: {connection.Database}");
    }
    catch (Exception ex)
    {
        return Results.Text($"Connection failed: {ex.Message}");
    }
});

app.MapGet("/list-tables", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
    using var reader = await command.ExecuteReaderAsync();
    var tables = new List<string>();
    while (await reader.ReadAsync())
    {
        tables.Add(reader.GetString(0));
    }
    return Results.Ok(string.Join(", ", tables));
});

app.MapGet("/create-suppliers-table", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Suppliers' and xtype='U')
        CREATE TABLE Suppliers (
            Id INT PRIMARY KEY IDENTITY(1,1),
            Name NVARCHAR(200) NOT NULL,
            Address NVARCHAR(500),
            TradeName NVARCHAR(200),
            TaxId NVARCHAR(50),
            PhoneNumber NVARCHAR(50),
            Email NVARCHAR(200),
            Website NVARCHAR(200),
            Country NVARCHAR(100),
            AnnualBillingUSD DECIMAL(18,2),
            LastEdited DATETIME2,
            CreatedAt DATETIME2 NOT NULL
        )";
    await command.ExecuteNonQueryAsync();
    return Results.Ok("Suppliers table created or already exists.");
});

app.MapGet("/list-suppliers", async (ApplicationDbContext db) =>
{
    var suppliers = await db.Suppliers.Select(s => new { s.Id, s.Name, s.CreatedAt }).ToListAsync();
    return Results.Ok(suppliers);
});

app.Run();