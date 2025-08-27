using Marten;

namespace HelpDesk.Api.Employee.Issues;

public class VipNotificationBackgroundWorker(
    ILogger<VipNotificationBackgroundWorker> logger,
    IServiceScopeFactory scopeFactory
   ) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Go check if there are any unprocessed stuff in the database 
        // And THEN start listening to the channel.
        logger.LogInformation("Starting VipNotification Background Worker");

        // when we start, get the list of vips.
        // add another timer that every 20 minutes refreshes that list.

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await CheckForUnhandledVips(stoppingToken);
        }
    }
    private async Task CheckForUnhandledVips(CancellationToken token)
    {
       
        using var scope = scopeFactory.CreateScope(); // "defer" in golang
        using var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var vipsApi = scope.ServiceProvider.GetRequiredService<CheckVipService>();
        var problems = await session.Query<EmployeeProblemEntity>()
            .Where(p => p.Status == SubmittedIssueStatus.AwaitingTechAssignment)
            .ToListAsync(token);

        // When this runs (every 20 seconds ) AND if there are any problems, then ask the API for a list of VIPs
        if (problems.Any())
        {
            List<string> vips = await vipsApi.GetCurrentVipsAsync(token);
            foreach(var problem in problems)
            {
                if(vips.Contains(problem.ReportedBy))
                {
                    // send the Notification
                    logger.LogWarning("Need to send a notification for a vip {issue} (vip {vip})", problem.Id, problem.ReportedBy);
                }
            }
        }

        // go through each of these and if they are a VIP then do a notification.
        //foreach(var problem in problems)
        //{
        //    // WTCYWYH 
        //    var isVip = await vipsApi.CheckIfProblemIsForVip(problem.ReportedBy);
        //    if( isVip)
        //    {
        //        // send a notification to the person that handles VIPS
        //    }
        //}
    
        logger.LogInformation("There are {count} unhandled problems", problems.Count());

    }

   

}
