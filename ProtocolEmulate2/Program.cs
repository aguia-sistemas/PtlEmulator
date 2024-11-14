// Program.cs
using ProtocolEmulate2.Controllers;
using ProtocolEmulate2.Services;
using PtlEmulator.App.Socket;

var builder = WebApplication.CreateBuilder(args);

// Adicione serviços ao contêiner.
builder.Services.AddControllersWithViews();

// Registre o BaseTcpListener como um serviço singleton
builder.Services.AddSingleton<BaseTcpListener>(sp => new BaseTcpListener(4660));

// Registre o HomeController como um serviço singleton
builder.Services.AddSingleton<HomeController>();

// Registre o DisplayService como um serviço singleton
builder.Services.AddSingleton<DisplayService>();

var app = builder.Build();

// Configure o pipeline de requisição HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var listener = app.Services.GetRequiredService<BaseTcpListener>();
var displayService = app.Services.GetRequiredService<DisplayService>();

listener.ClientConnectedEvent += displayService.OnClientConnected;
displayService.SendMessageEvent += (clientId, messageType, device, value) =>
{
    var client = listener.GetClient(clientId) as AtopClient;
    client?.SendMessage(messageType, device, value);
};

listener.StartServer();

app.Run();