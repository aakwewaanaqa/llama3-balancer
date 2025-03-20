using System.Diagnostics;
using System.Net;
using aimy_galaxy_proxy.Common;
using Docker.DotNet;
using Llama3.Balancer.Common;

namespace Llama3.Balancer.Services.Docker;

public class ContainerWrapper : IDisposable {
    public string Id  { get; init; }
    public string Ip  { get; init; }
    public string Url { get; init; }

    /// <summary>
    ///     Stops this container.
    /// </summary>
    /// <returns>Stopped one.</returns>
    public async Task<Response<ContainerWrapper>> Stop() {
        try {
            var startInfo = new ProcessStartInfo {
                FileName               = "docker",
                Arguments              = $"stop {Id}",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };

            var process = Process.Start(startInfo);
            await process.WaitForExitAsync();

            string error   = await process.StandardError.ReadToEndAsync();
            string message = await process.StandardOutput.ReadToEndAsync();

            if (!string.IsNullOrEmpty(error)) throw new Exception(error);

            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.OK,
                errorCode = DockerErrorCode.OK,
                message   = message,
                value     = this,
            };
        }
        catch (Exception e) {
            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.InternalServerError,
                errorCode = DockerErrorCode.STOP_CONTAINER_FAIL,
                message   = e.Message,
                value     = this,
            };
        }
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}
