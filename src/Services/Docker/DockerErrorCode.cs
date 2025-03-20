namespace Llama3.Balancer.Services.Docker;

public struct DockerErrorCode {
    public const int OK                     = 0;
    public const int CREATE_CONTAINER_FAIL  = 1;
    public const int STOP_CONTAINER_FAIL    = 2;
    
    public const int EXEC_CONTAINER_FAIL = 101;
}
