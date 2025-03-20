using aimy_galaxy_proxy.Common;
using Docker.DotNet;
using Docker.DotNet.Models;
using Llama3.Balancer.Services.Docker;
using Newtonsoft.Json;

namespace Unitests;

[TestFixture]
public class Tests {
    private DockerWrapper _docker;
    private HttpClient    _http;

    [SetUp]
    public void Setup() {
        _docker = new DockerWrapper();
        _http   = new HttpClient();
    }

    [Test]
    public async Task TestRunAndRemove() {
        var response = await
            (_docker.CreateContainer, new RunArgs {
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
        var random = new Random().NextInt64(1000, 2000);
        var response = await
            (_docker.CreateContainer, new RunArgs {
                ImageTag       = "ponito/built-llama3",
                IsInteractive  = true,
                IsRemoveOnStop = true,
                GpuCount       = -1,
                PortMapping    = $"{random}:11434",
            })
           .Start()
           .Delay(TimeSpan.FromSeconds(3))
           .FunctionAsync(async it => {
                var    get = await _http.GetAsync(it.value.Url);
                string str = await get.Content.ReadAsStringAsync();
                Console.WriteLine(str);
                That(str, Not.Null);

                string endpoint = $"{it.value.Url}/api/generate";
                string json = JsonConvert.SerializeObject(new {
                    model  = "llama3",
                    prompt = "Read me a story~",
                    stream = false,
                });
                var post = await _http.PostAsync(endpoint, new StringContent(json));
                str = await post.Content.ReadAsStringAsync();
                Console.WriteLine(str);
                That(str, Not.Null);

                return it;
            })
           .FunctionAsync(async it => await it.value.Stop())
           .ToTask();
        Console.WriteLine(response.message);
        That(response.errorCode, EqualTo(0));
        That(response.IsOk,      True);
        That(response.value.Id,  Not.Null);
    }

    [Test]
    public async Task RunTwo() {
        await Task.WhenAll(
        TestRunAndExecute(),
        TestRunAndExecute());
    }

    [Test]
    public async Task RunThree() {
        await Task.WhenAll(
        TestRunAndExecute(),
        TestRunAndExecute(),
        TestRunAndExecute());
    }
    
    [Test]
    public async Task RunFour() {
        await Task.WhenAll(
        TestRunAndExecute(),
        TestRunAndExecute(),
        TestRunAndExecute(),
        TestRunAndExecute());
    }

    [TearDown]
    public void TearDown() {
        _docker.Dispose();
        _http.Dispose();
    }
}
