using System.Drawing;

namespace WaveFunctionCollapseCore;

/// <summary>
/// The dictionaries are used to store the probability of a particle being above, below, left or right of this particle
/// </summary>
/// <param name="HashCode">A Unique id for this particle</param>
/// <param name="Color">A Color so that we can draw it</param>
/// <param name="AllowedAbove">What particles can be above this particle? Key: allowed particle hashcode; Value: what chance does the particle have to be chosen as the one above</param>
/// <param name="AllowedBelow"></param>
/// <param name="AllowedLeft"></param>
/// <param name="AllowedRight"></param>
public readonly record struct Particle
{
    public ParticleHashCode HashCode { get; init; }
    public Color Color { get; init; }
    public Dictionary<ParticleHashCode, double> AllowedAbove { get; init; }
    public Dictionary<ParticleHashCode, double> AllowedBelow { get; init; }
    public Dictionary<ParticleHashCode, double> AllowedLeft { get; init; }
    public Dictionary<ParticleHashCode, double> AllowedRight { get; init; }

    public ParticleHashCode[] AllowedAboveParticlesCache { get; init; }
    public ParticleHashCode[] AllowedBelowParticlesCache { get; init; }
    public ParticleHashCode[] AllowedLeftParticlesCache { get; init; }
    public ParticleHashCode[] AllowedRightParticlesCache { get; init; }

    public Particle(
        ParticleHashCode hashCode,
        Color color,
        Dictionary<ParticleHashCode, double> allowedAbove, 
        Dictionary<ParticleHashCode, double> allowedBelow, 
        Dictionary<ParticleHashCode, double> allowedLeft,
        Dictionary<ParticleHashCode, double> allowedRight)
    {
        HashCode = hashCode;
        Color = color;
        AllowedAbove = allowedAbove;
        AllowedBelow = allowedBelow;
        AllowedLeft = allowedLeft;
        AllowedRight = allowedRight;

        AllowedAboveParticlesCache = AllowedAbove.Keys.ToArray();
        AllowedBelowParticlesCache = AllowedBelow.Keys.ToArray();
        AllowedLeftParticlesCache = AllowedLeft.Keys.ToArray();
        AllowedRightParticlesCache = AllowedRight.Keys.ToArray();
    }

    public override int GetHashCode()
    {
        return HashCode.GetHashCode();
    }
}