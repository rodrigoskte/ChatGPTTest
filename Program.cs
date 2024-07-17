using OpenIATest.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
ConfigChatGPTSettings(builder);
ConfigChatGPTSettingsCvp(builder);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void ConfigChatGPTSettings(WebApplicationBuilder builder)
{
    var secret = builder.Configuration.GetSection("ChatGPTSettings");
    builder.Services.Configure<ChatGPTSettings>(secret);
}

static void ConfigChatGPTSettingsCvp(WebApplicationBuilder builder)
{
    var secret = builder.Configuration.GetSection("ChatGPTSettingsCvp");
    builder.Services.Configure<ChatGPTSettingsCvp>(secret);
}