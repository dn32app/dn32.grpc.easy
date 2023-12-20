using Microsoft.AspNetCore.Mvc;
using shared.example.contract;
using System.Diagnostics;

namespace client.example.Controllers;

[ApiController]
[Route("/")]
public class HomeController(IExampleGrpcController ExampleGrpcController) : ControllerBase
{
    [HttpGet]
    public async Task<object> GetAsync()
    {
        var result = await ExampleGrpcController.SimpleExampleAsync(new GrpcContractExampleV1
        {
            Metadata = new Dictionary<string, string> { { "value", "1" } },
            Name = "Marcelo"
        });

        var time = new Stopwatch(); time.Start();
        var count = 100m;

        for (var i = 0; i < count; i++)
        {
            await ExampleGrpcController.SimpleExampleAsync(new GrpcContractExampleV1
            {
                Metadata = new Dictionary<string, string> { { "value", i.ToString() } },
                Name = "Marcelo"
            });
        }

        time.Stop();

        var elapsedMilliseconds = time.ElapsedMilliseconds;
        
        // Run without debuggin for check the performance
        return new { result.Result, milliseconds = (elapsedMilliseconds / count) };
    }
}