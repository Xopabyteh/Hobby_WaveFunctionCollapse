using System.Drawing;

namespace WaveFunctionCollapseCore;

public readonly record struct Particle(
    ParticleHashCode HashCode,
    Color Color,
    ParticleHashCode[] AllowedAbove,
    ParticleHashCode[] AllowedBelow,
    ParticleHashCode[] AllowedLeft,
    ParticleHashCode[] AllowedRight)
{
    public override int GetHashCode()
    {
        return HashCode.GetHashCode();
    }
}