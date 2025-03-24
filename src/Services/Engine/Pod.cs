using Llama3.Balancer.Services.Docker;

namespace Llama3.Balancer.Services.Engine;

public struct Pod {
    public IPodKey          PodKey    { get; init; }
    public ContainerWrapper Container { get; init; }
}
