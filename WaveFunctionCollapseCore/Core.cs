using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace WaveFunctionCollapseCore;

public class Core
{
    private readonly int _sceneWidth;
    private readonly int _sceneHeight;

    private Dictionary<ParticleHashCode, Particle> availableParticles = null!;
    private Particle[] availableParticlesValuesCache;
    private ParticleHashCode[] availableParticleHashCodesCache;
    public Core(int sceneWidth, int sceneHeight)
    {
        _sceneWidth = sceneWidth;
        _sceneHeight = sceneHeight;

        InitializeParticles();
    }
    private void InitializeParticles()
    {
        Dictionary<ParticleHashCode, double> p(ParticleHashCode[] particleHashCodes, double[] chances)
            => particleHashCodes
                .Zip(chances, (hash, chance) => (hash, chance))
                .ToDictionary(x => x.hash, x => x.chance);

        var grass = new Particle(
            0,
            Color.Green,
            p([0, 1], [0.5, 0.5]),
            p([0], [1]),
            p([0], [1]),
            p([0], [1]));

        var sky = new Particle(
            1,
            Color.SkyBlue,
            p([1, 2], [0.9, 0.1]),
            p([1, 2], [0.9, 0.1]),
            p([1, 2], [0.9, 0.1]),
            p([1, 2], [0.9, 0.1]));

        var cloud = new Particle(
            2,
            Color.White,
            p([1, 2], [0.1, 0.9]),
            p([1, 2], [0.1, 0.9]),
            p([1, 2], [0.1, 0.9]),
            p([1, 2], [0.1, 0.9]));


        availableParticles = new()
        {
            [0] = grass,
            [1] = sky,
            [2] = cloud
        };
        availableParticlesValuesCache = availableParticles.Values.ToArray();
        availableParticleHashCodesCache = availableParticles.Keys.ToArray();
    }


    private Particle[,] scene = null!;
    private ParticleEntropy[,] sceneEntropy = null!;
    
    public void CollapseAndDrawIteratively(Action<DrawRequest> drawingMethod)
    {
        new Thread(() =>
        {
            scene = new Particle[_sceneWidth, _sceneHeight];
            sceneEntropy = new ParticleEntropy[_sceneWidth, _sceneHeight];

            for (int i = 0; i < _sceneWidth * _sceneHeight; i++)
            {
                // Calculate entropy
                CalculateEntropy();

                // Pick space with the lowest entropy (the least possible particles)
                (int X, int Y) lowestEntropySpace = GetLowestEntropySpace();

                // Collapse that space
                var chosenParticle = CollapseParticle(lowestEntropySpace.X, lowestEntropySpace.Y);

                //Draw
                var drawRequest = new DrawRequest(lowestEntropySpace.X, lowestEntropySpace.Y, chosenParticle.Color);
                drawingMethod(drawRequest);

            }
        }).Start();
    }

    /// <summary>
    /// Picks a particle from the particle entropy and writes it to the scene.
    /// <b>Does not recalculate the scene entropy!</b>
    /// </summary>
    /// <returns>Returns the particle that was written to the scene.</returns>
    private Particle CollapseParticle(int x, int y)
    {
        var particleEntropy = sceneEntropy[x, y];
        Particle chosenParticle;
        if (particleEntropy.PossibleParticles.Length == 1)
        {
            // Choose the only possible one
            chosenParticle = particleEntropy.PossibleParticles[0];
        }
        else
        {
            // Choose randomly from available, considering what other particles want to have as neighbors
            var neighbors = GetNeighbors(x, y);
            double ParticleScoreBasedOnNeighbors(Particle p) =>
                                                            neighbors[0] == default ? 1 : p.AllowedRight[neighbors[0].HashCode]
                                                          * (neighbors[1] == default ? 1 : p.AllowedBelow[neighbors[1].HashCode])
                                                          * (neighbors[2] == default ? 1 : p.AllowedLeft[neighbors[2].HashCode])
                                                          * (neighbors[3] == default ? 1 : p.AllowedAbove[neighbors[3].HashCode]);
            //neighbors[0] == default ? 1 : neighbors[0].AllowedLeft[p.HashCode]
            //                              * (neighbors[1] == default ? 1 : neighbors[1].AllowedAbove[p.HashCode])
            //                              * (neighbors[2] == default ? 1 : neighbors[2].AllowedRight[p.HashCode])
            //                              * (neighbors[3] == default ? 1 : neighbors[3].AllowedBelow[p.HashCode]);

            static Particle WeightedRandom((Particle Particle, double Score)[] items)
            {
                double totalWeight = items.Sum(item => item.Score);
                double randomValue = Random.Shared.NextDouble() * totalWeight;

                foreach (var item in items)
                {
                    randomValue -= item.Score;
                    if (randomValue <= 0)
                        return item.Particle;
                }

                // Shouldn't reach here, but return last item just in case
                return items.Last().Particle;
            }

            var scoredPossibleParticles = particleEntropy.PossibleParticles
                .Select(p => (p, ParticleScoreBasedOnNeighbors(p)))
                .ToArray();


            chosenParticle = WeightedRandom(scoredPossibleParticles);
        }

        scene[x, y] = chosenParticle;
        return chosenParticle;
    }

    /// <summary>
    /// Populates the sceneEntropy array with the possible particles for each space
    /// </summary>
    private void CalculateEntropy()
    {
        for (int x = 0; x < _sceneWidth; x++)
        for (int y = 0; y < _sceneHeight; y++)
        {
            if (scene[x, y] != default)
                continue; //Space is already collapsed
            
            var neighbors = GetNeighbors(x, y);
            
            if (neighbors.All(n => n == default))
            {
                //No neighbor, anything is possible
                sceneEntropy[x, y] = new ParticleEntropy(availableParticlesValuesCache);
                continue;
            } 

            // -> We have a neighbor, calculate entropy
            // based on the intersection of allowed particles in each direction
            // Remember, neighbors returned are sorted in clockwise order starting from right
            // Also remember, that this particle is to the LEFT of our right neighbor

            var availableFromRight = neighbors[0] == default
                ? availableParticleHashCodesCache 
                : neighbors[0].AllowedLeftParticlesCache; 
            var availableFromBelow = neighbors[1] == default
                ? availableParticleHashCodesCache
                : neighbors[1].AllowedAboveParticlesCache;
            var availableFromLeft = neighbors[2] == default 
                ? availableParticleHashCodesCache 
                : neighbors[2].AllowedRightParticlesCache;
            var availableFromAbove = neighbors[3] == default 
                ? availableParticleHashCodesCache 
                : neighbors[3].AllowedBelowParticlesCache;

            // The common particles
            var possibleParticles = availableParticlesValuesCache
                .Where(p => availableFromRight.Contains(p.HashCode))
                .Where(p => availableFromBelow.Contains(p.HashCode))
                .Where(p => availableFromLeft.Contains(p.HashCode))
                .Where(p => availableFromAbove.Contains(p.HashCode))
                .ToArray();

            sceneEntropy[x, y] = new ParticleEntropy(possibleParticles);
        }
    }

    /// <returns>
    /// Returns a random space with the lowest entropy (the least possible particles)
    /// </returns>
    private (int X, int Y) GetLowestEntropySpace()
    {
        var lowestEntropy = int.MaxValue;
        var lowestEntropySpaces = new List<(int X, int Y)>(sceneEntropy.Length);

        for (int x = 0; x < _sceneWidth; x++)
        for (int y = 0; y < _sceneHeight; y++)
        {
            if (scene[x, y] != default)
                continue; //Space is already collapsed

            var entropy = sceneEntropy[x, y].PossibleParticles.Length;
            if (entropy < lowestEntropy)
            {
                lowestEntropy = entropy;
                lowestEntropySpaces.Clear();
                lowestEntropySpaces.Add((x, y));
            } else if (entropy == lowestEntropy)
            {
                lowestEntropySpaces.Add((x, y));
            }
        }

        return lowestEntropySpaces[Random.Shared.Next(lowestEntropySpaces.Count)];
    }

    /// <returns>
    /// Returns neighboring particles in a clockwise order starting from right, ending at above.
    /// If the neighboring space is out of bounds, return default. 
    /// </returns>
    private Particle[] GetNeighbors(int x, int y) 
        => [
            x < _sceneWidth - 1 ? scene[x + 1, y] : default,
            y > 0 ? scene[x, y - 1] : default,
            x > 0 ? scene[x - 1, y] : default,
            y < _sceneHeight - 1 ? scene[x, y + 1] : default
        ];

    internal readonly record struct ParticleEntropy(Particle[] PossibleParticles);
}