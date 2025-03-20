namespace Llama3.Balancer;

public static class Program {

    private static WebApplicationBuilder CreateBuilder(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        return builder;
    }

    private static int RunApplication(WebApplicationBuilder builder) {
        var app = builder.Build();

        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
        return 0;
    }

    public static int Main(string[] args) {
        return RunApplication(CreateBuilder(args));
    }
}