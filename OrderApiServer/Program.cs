using Microsoft.EntityFrameworkCore;
using OrderApiServer.Data;
using OrderApiServer.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// SeriLog ���� �߰�
Log.Logger = new LoggerConfiguration()
    // �ֿܼ� �α� ���
    .WriteTo.Console()
    // ���Ͽ� �α� ���� (�Ϸ� ����)
    .WriteTo.File("logs/order_api_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

bool useMemoryDatabase = builder.Configuration.GetValue<bool>("UseMemoryDatabase");

if (useMemoryDatabase)
{
    // �޸� ���
    builder.Services.AddSingleton<IOrderRepository, MemoryOrderRepository>(); 
}
else
{
    // DB ���
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IOrderRepository, DbOrderRepository>(); 
}
 
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<OrderHub>("/orderHub");
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ���ø����̼� ���� �� �α� �����
Log.Information("�ֹ� API ���� ���۵�!");

app.Run();

// ���ø����̼� ���� �� �α� �÷���
Log.CloseAndFlush();