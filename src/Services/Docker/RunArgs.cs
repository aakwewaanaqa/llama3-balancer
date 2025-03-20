using System.Text;

namespace Llama3.Balancer.Services.Docker;

public struct RunArgs() {
    public readonly bool   IsDetached = true;
    public          bool   IsRemoveOnStop;
    public          bool   IsInteractive;
    public          int    GpuCount;
    public          string Name;
    public          string PortExposing;
    public          string PortMapping;
    public          string ImageTag;

    public override string ToString() {
        var builder = new StringBuilder();
        if (IsDetached) builder.Append(" -d");
        if (IsInteractive) builder.Append(" -i");
        if (IsRemoveOnStop) builder.Append(" --rm");
        if (GpuCount != 0) {
            if (GpuCount < 0) builder.Append(" --gpus=all");
            else builder.Append($" --gpus={GpuCount}");
        }
        if (Name         != null) builder.Append($" --name=\"{Name}\"");
        if (PortMapping  != null) builder.Append($" -p {PortMapping}");
        if (PortExposing != null) builder.Append($" --expose={PortExposing}");
        builder.Append($" \"{ImageTag}\"");
        return builder.ToString();
    }
}
