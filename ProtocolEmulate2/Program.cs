// Program.cs
using ProtocolEmulate2.Controllers;
using ProtocolEmulate2.Services;
using PtlEmulator.App.Socket;

var builder = WebApplication.CreateBuilder(args);

// Adicione serviços ao contêiner.
builder.Services.AddControllersWithViews();

// Registre os BaseTcpListeners como serviços singletons
Console.Write("Digite as portas que deseja usar (separadas por vírgula): ");
var portsInput = Console.ReadLine();
var ports = portsInput.Split(',').Select(p => int.Parse(p.Trim())).ToArray();

foreach (var port in ports)
{
    builder.Services.AddSingleton<BaseTcpListener>(sp => new BaseTcpListener(port));
}

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

var listeners = app.Services.GetServices<BaseTcpListener>();
var displayService = app.Services.GetRequiredService<DisplayService>();

var clienteId = 1;
foreach (var listener in listeners)
{
    
    listener.ClientConnectedEvent += displayService.OnClientConnected;
    displayService.SendMessageEvent += (clientId, messageType, device, value) =>
    {
        var client = listener.GetClient(clientId) as AtopClient;
        client?.SendMessage(messageType, device, value);
    };

    listener.StartServer(clienteId);
    clienteId++;
}

app.Run();