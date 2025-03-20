using System.Diagnostics;
using System.Net;
using aimy_galaxy_proxy.Common;
using Docker.DotNet;
using Llama3.Balancer.Common;

namespace Llama3.Balancer.Services.Docker;

public class ContainerWrapper : IDisposable {
    public string       Id        { get; init; }
    public Process      ShProcess { get; private set; }
    public StreamWriter StdIn     { get; private set; }
    public StreamReader StdOut    { get; private set; }

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

    /// <summary>
    ///     Runs docker exec interactively.
    /// </summary>
    /// <returns>self</returns>
    public async Task<Response<ContainerWrapper>> StartSh() {
        try {
            var startInfo = new ProcessStartInfo {
                FileName               = "docker",
                Arguments              = $"exec -i {Id} sh",
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };
            ShProcess = Process.Start(startInfo);

            string error = await ShProcess.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error)) throw new Exception(error);

            this.StdIn  = ShProcess.StandardInput;
            this.StdOut = ShProcess.StandardOutput;

            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.OK,
                errorCode = DockerErrorCode.OK,
                value     = this,
            };
        }
        catch (Exception e) {
            return new Response<ContainerWrapper> {
                status    = (int)HttpStatusCode.InternalServerError,
                errorCode = DockerErrorCode.START_SH_FAIL,
                message   = e.Message,
                value     = this,
            };
        }
    }

    public async Task<Response<ContainerWrapper>> SendCommand(string command) {
        try {
            if (StdIn is null) throw new NullReferenceException("StdIn is null");
            await StdIn.WriteLineAsync(command);

            string error = await StdOut.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error)) throw new Exception(error);

            string message = await StdOut.ReadToEndAsync();
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
                errorCode = DockerErrorCode.SEND_SH_FAIL,
                message   = e.Message,
                value     = this,
            };
        }
    }

    public void Dispose() {
        StdIn?.Dispose();
        StdOut?.Dispose();
        ShProcess.Dispose();
        GC.SuppressFinalize(this);
    }
}
