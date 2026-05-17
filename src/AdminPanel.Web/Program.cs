using AdminPanel.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace AdminPanel.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // The base address of the API
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5154") });

        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Add custom services
        builder.Services.AddScoped<ApiService>();

        await builder.Build().RunAsync();
    }
}
