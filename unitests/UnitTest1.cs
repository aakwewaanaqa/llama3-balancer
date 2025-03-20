using aimy_galaxy_proxy.Common;
using Docker.DotNet;
using Docker.DotNet.Models;
using Llama3.Balancer.Services.Docker;
using Newtonsoft.Json;

namespace Unitests;

[TestFixture]
public class Tests {
    private DockerWrapper _docker;

    [SetUp]
    public void Setup() {
        _docker = new DockerWrapper();
    }

    [Test]
    public async Task TestRunAndRemove() {
        var response = await
            (_docker.CreateContainer, new RunArgs {
                Name           = "TestRunAndRemove",
                ImageTag       = "ponito/built-llama3",
                IsInteractive  = true,
                IsRemoveOnStop = true,
                PortExposing   = "11434",
            })
           .Start()
           .Delay(TimeSpan.FromSeconds(3))
           .FunctionAsync(async it => await it.value.Stop())
           .ToTask();
        Console.WriteLine(response.message);
        That(response.errorCode, EqualTo(0));
        That(response.IsOk,      True);
        That(response.value.Id,  Not.Null);
    }

    [Test]
    public async Task TestRunAndExecute() {
        var response = await
            (_docker.CreateContainer, new RunArgs {
                Name           = "TestRunAndRemove",
                ImageTag       = "ponito/built-llama3",
                IsInteractive  = true,
                IsRemoveOnStop = true,
                PortExposing   = "11434",
            })
           .Start()
           .Delay(TimeSpan.FromSeconds(3))
           .FunctionAsync(async it => {
                if (it.IsNotOk) return it;
                var shResponse = await it.value.StartSh();
                if (shResponse.IsNotOk) return shResponse;
                return await it.value.SendCommand("ollama run llama3");
            })
           .ToTask();
        Console.WriteLine(response.message);
        That(response.errorCode, EqualTo(0));
        That(response.IsOk,      True);
        That(response.value.Id,  Not.Null);
    }

    [TearDown]
    public void TearDown() {
        _docker.Dispose();
    }
}
