using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using aimy_galaxy_proxy.Common;
using Docker.DotNet;
using Docker.DotNet.Models;
using Llama3.Balancer.Common;

namespace Llama3.Balancer.Services.Docker;

/// <summary>
///     Docker services wrapped inside a wrapper.
/// </summary>
public class DockerWrapper : IDisposable {
    /// <summary>
    ///     Connects to local docker daemon.
    /// </summary>
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    /// <summary>
    ///     Creates a container.
    /// </summary>
    /// <param name="args"><see cref="RunArgs"/></param>
    public Convert<RunArgs, Response<ContainerWrapper>> CreateContainer =>
        async args => {
            var startInfo = new ProcessStartInfo {
                FileName               = "docker",
                Arguments              = $"container run {args}",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };
            var process = Process.Start(startInfo);

            if (args.IsDetached) await process.WaitForExitAsync();
            string error   = await process.StandardError.ReadToEndAsync();
            string message = await process.StandardOutput.ReadToEndAsync();

            if (!string.IsNullOrEmpty(error)) {
                return new Response<ContainerWrapper> {
                    status    = (int)HttpStatusCode.InternalServerError,
                    errorCode = DockerErrorCode.CREATE_CONTAINER_FAIL,
                    message   = error,
                };
            }

            string id  = message.Trim();
            string ip  = null;
            string url = null;
            if (!string.IsNullOrEmpty(args.PortMapping)) {
                var regex              = new Regex("(.+):.+");
                var match              = regex.Match(args.PortMapping);
                if (match.Success) url = $"http://localhost:{match.Groups[1].Value}";
            }

            var inspectResponse = await _client.Containers.InspectContainerAsync(id);
            ip = inspectResponse.NetworkSettings.IPAddress;

            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.OK,
                errorCode = DockerErrorCode.OK,
                message   = message,
                value = new ContainerWrapper {
                    Id  = id,
                    Ip  = ip,
                    Url = url,
                }
            };
        };

    /// <summary>
    ///     Stops the container passed in.
    /// </summary>
    /// <param name="containerResponse">Chained previous call giving the <see cref="ContainerWrapper"/></param>
    public Pipe<Response<ContainerWrapper>> Stop =>
        async containerResponse => {
            if (containerResponse.IsNotOk) return containerResponse;
            return await containerResponse.value.Stop();
        };

    public void Dispose() {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
