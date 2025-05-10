// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.Linq;
using DTC.Core.Extensions;
using Newtonsoft.Json;

namespace DTC.Core.AI;

/// <summary>
/// A simple feedforward neural network with optional backpropagation.
/// </summary>
public class NeuralNetwork
{
    [JsonProperty] private int[] m_layerSizes;
    [JsonProperty] private double[][] m_neurons;
    [JsonProperty] private double[][][] m_weights;
    [JsonProperty] private double m_learningRate;

    /// <summary>
    /// Initializes a new neural network with the given layer sizes and learning rate.
    /// </summary>
    public NeuralNetwork(int inputSize, int[] hiddenLayers, int outputSize, double learningRate = 0.05f)
    {
        m_learningRate = learningRate;
        m_layerSizes = new[] { inputSize }.Concat(hiddenLayers).Append(outputSize).ToArray();
        m_neurons = m_layerSizes.Select(size => new double[size]).ToArray();
        m_weights = new double[m_layerSizes.Length - 1][][];

        for (var l = 0; l < m_weights.Length; l++)
        {
            var inSize = m_layerSizes[l];
            var outSize = m_layerSizes[l + 1];
            m_weights[l] = new double[outSize][];
            for (var o = 0; o < outSize; o++)
                m_weights[l][o] = new double[inSize];
        }
        
        Clear();
    }

    /// <summary>
    /// Performs a forward pass through the network and returns the predicted output.
    /// </summary>
    public double[] Predict(double[] input)
    {
#if DEBUG
        if (input.Any(o => o < -1.0 || o > 1.0))
            throw new ArgumentException("Input values must be in the range [-1, 1].");
#endif
        
        Array.Copy(input, m_neurons[0], input.Length);

        for (var l = 1; l < m_layerSizes.Length; l++)
        {
            var prev = m_neurons[l - 1];
            var current = m_neurons[l];
            var weights = m_weights[l - 1];
            for (var j = 0; j < m_layerSizes[l]; j++)
            {
                var w = weights[j];
                var sum = 0.0;
                for (var i = 0; i < m_layerSizes[l - 1]; i++)
                    sum += prev[i] * w[i];

                // Use ReLU activation on hidden layers, identity on output
                current[j] = l == m_layerSizes.Length - 1 ? sum : ReLu(sum);
            }
        }

        return m_neurons[^1]; // output layer
    }

    /// <summary>
    /// Trains the network on a single (input, target) pair using backpropagation.
    /// </summary>
    public void Train(double[] input, double[] target)
    {
        Predict(input); // forward pass

        var deltas = new double[m_neurons.Length][];
        for (var l = 0; l < m_neurons.Length; l++)
            deltas[l] = new double[m_neurons[l].Length];

        // Compute error at output layer
        for (var i = 0; i < target.Length; i++)
            deltas[^1][i] = m_neurons[^1][i] - target[i];

        // Back-propagate errors
        for (var l = m_layerSizes.Length - 2; l > 0; l--)
        {
            for (var i = 0; i < m_layerSizes[l]; i++)
            {
                var error = 0.0;
                for (var j = 0; j < m_layerSizes[l + 1]; j++)
                    error += m_weights[l][j][i] * deltas[l + 1][j];

                // Apply ReLU derivative
                deltas[l][i] = m_neurons[l][i] > 0 ? error : 0;
            }
        }

        // Update weights
        for (var l = 0; l < m_weights.Length; l++)
        {
            for (var j = 0; j < m_weights[l].Length; j++)
            {
                for (var i = 0; i < m_weights[l][j].Length; i++)
                {
                    var delta = deltas[l + 1][j] * m_neurons[l][i];
                    m_weights[l][j][i] -= m_learningRate * delta;
                }
            }
        }
    }

    /// <summary>
    /// Rectified Linear Unit activation function.
    /// </summary>
    private static double ReLu(double x) => Math.Max(0, x);

    /// <summary>
    /// Clears neuron states and reinitializes weights with random values.
    /// </summary>
    private void Clear()
    {
        foreach (var layer in m_neurons)
            Array.Clear(layer, 0, layer.Length);
        Randomize();
    }

    public void Randomize()
    {
        for (var l = 0; l < m_weights.Length; l++)
        {
            for (var j = 0; j < m_weights[l].Length; j++)
            {
                for (var i = 0; i < m_weights[l][j].Length; i++)
                    m_weights[l][j][i] = Random.Shared.NextDouble() * 2 - 1;
            }
        }
    }

    public void CrossWith(NeuralNetwork other, double crossoverRate)
    {
        for (var l = 0; l < m_weights.Length; l++)
        {
            for (var j = 0; j < m_weights[l].Length; j++)
            {
                for (var i = 0; i < m_weights[l][j].Length; i++)
                {
                    if (Random.Shared.NextDouble() < crossoverRate)
                        m_weights[l][j][i] = other.m_weights[l][j][i];
                }
            }
        }
    }
    
    public void Mutate(double mutationRate)
    {
        for (var l = 0; l < m_weights.Length; l++)
        {
            for (var j = 0; j < m_weights[l].Length; j++)
            {
                for (var i = 0; i < m_weights[l][j].Length; i++)
                {
                    var rand = Random.Shared;
                    if (rand.NextDouble() < mutationRate)
                        m_weights[l][j][i] = Math.Tanh(m_weights[l][j][i] + rand.GaussianSample(0.2));
                }
            }
        }
    }

    public NeuralNetwork Clone()
    {
        var clone = new NeuralNetwork(
            m_layerSizes[0],
            m_layerSizes.Skip(1).Take(m_layerSizes.Length - 2).ToArray(),
            m_layerSizes[^1],
            m_learningRate
        );
        for (var l = 0; l < m_weights.Length; l++)
        {
            for (var j = 0; j < m_weights[l].Length; j++)
            {
                for (var i = 0; i < m_weights[l][j].Length; i++)
                    clone.m_weights[l][j][i] = m_weights[l][j][i];
            }
        }

        return clone;
    }
}