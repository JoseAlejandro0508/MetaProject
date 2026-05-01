
namespace Meta_xi.Application;
public class UpdateServicePerHour : BackgroundService
{
    public readonly IServiceProvider serviceProvider;
    public UpdateServicePerHour(IServiceProvider _serviceProvider)
    {
        serviceProvider = _serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested){
            using (var scope = serviceProvider.CreateScope()){
                var updatePlansPerHour = scope.ServiceProvider.GetRequiredService<IUpdatePlansPerHour>();
                await updatePlansPerHour.UpdatePlansPerHour();
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}