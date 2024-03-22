namespace WaveFunctionCollapseCore;

public readonly record struct ParticleHashCode(int Value)
{
    public override int GetHashCode()
    {
        return Value;
    }

    public static implicit operator ParticleHashCode(int value) => new(value);
}