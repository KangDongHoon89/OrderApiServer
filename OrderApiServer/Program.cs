using Microsoft.EntityFrameworkCore;
using OrderApiServer.Data;
using OrderApiServer.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// SeriLog 설정 추가
Log.Logger = new LoggerConfiguration()
    // 콘솔에 로그 출력
    .WriteTo.Console()
    // 파일에 로그 저장 (하루 단위)
    .WriteTo.File("logs/order_api_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

bool useMemoryDatabase = builder.Configuration.GetValue<bool>("UseMemoryDatabase");

if (useMemoryDatabase)
{
    // 메모리 모드
    builder.Services.AddSingleton<IOrderRepository, MemoryOrderRepository>(); 
}
else
{
    // DB 모드
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

// 애플리케이션 실행 시 로그 남기기
Log.Information("주문 API 서버 시작됨!");

app.Run();

// 애플리케이션 종료 시 로그 플러시
Log.CloseAndFlush();