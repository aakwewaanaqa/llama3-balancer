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
public class DockerWrapper(DockerClient _client) : IDisposable {
    /// <summary>
    ///     Creates a container.
    /// </summary>
    /// <param name="args"><see cref="RunArgs"/></param>
    public Convert<Response<RunArgs>, Response<ContainerWrapper>> RunContainer =>
        async argsResponse => {
            if (argsResponse.IsNotOk) return argsResponse.As<ContainerWrapper>();
            var args = argsResponse.value;
            
            var startInfo = new ProcessStartInfo {
                FileName               = "docker",
                Arguments              = $"container run {args}",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };
            var process = Process.Start(startInfo);

            if (RunArgs.IS_DETACHED) await process.WaitForExitAsync();
            string error   = await process.StandardError.ReadToEndAsync();
            string message = await process.StandardOutput.ReadToEndAsync();

            if (!string.IsNullOrEmpty(error)) {
                return new Response<ContainerWrapper> {
                    status    = (int)HttpStatusCode.InternalServerError,
                    errorCode = ErrorCode.CREATE_CONTAINER_FAIL,
                    message   = error,
                };
            }

            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.OK,
                errorCode = ErrorCode.OK,
                message   = message,
                value =
                    new ContainerWrapper {
                        Id =
                            message.Trim(),
                        InnerIp =
                            (await _client
                                  .Containers
                                  .InspectContainerAsync(message.Trim())
                                  .Guard()
                            ).NetworkSettings.IPAddress,
                        HostUrl =
                            args.PortMap.HasHost
                                ? $"http://localhost:{args.PortMap.HostPort}"
                                : null,
                    }
            };
        };

    public void Dispose() {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
