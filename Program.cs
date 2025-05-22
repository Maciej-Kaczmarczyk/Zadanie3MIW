using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

class Program
{

    public class Sample
    {
        public double[] Attributes { get; set; }
        public int ClassLabel { get; set; }

        public Sample(double[] attributes, int classLabel)
        {
            Attributes = attributes;
            ClassLabel = classLabel;
        }
    }

    static void NormalizeSamples(List<Sample> samples)
    {
        int attributeCount = samples[0].Attributes.Length;
        double[] minValues = new double[attributeCount];
        double[] maxValues = new double[attributeCount];

        for (int i = 0; i < attributeCount; i++)
        {
            minValues[i] = double.MaxValue;
            maxValues[i] = double.MinValue;
        }

        foreach (var sample in samples)
        {
            for (int i = 0; i < attributeCount; i++)
            {
                if (sample.Attributes[i] < minValues[i])
                    minValues[i] = sample.Attributes[i];

                if (sample.Attributes[i] > maxValues[i])
                    maxValues[i] = sample.Attributes[i];
            }
        }

        foreach (var sample in samples)
        {
            for (int i = 0; i < attributeCount; i++)
            {
                double min = minValues[i];
                double max = maxValues[i];
                sample.Attributes[i] = (sample.Attributes[i] - min) / (max - min);
            }
        }
    }

    static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += Math.Pow(a[i] - b[i], 2);
        }
        return Math.Sqrt(sum);
    }

    static void Main()
    {
        string filePath = "iris.txt";
        List<Sample> samples = LoadSamples(filePath);

        foreach (var sample in samples)
        {
            Console.WriteLine($"Atrybuty: {string.Join(", ", sample.Attributes)} | Klasa: {sample.ClassLabel}");
        }

        Console.WriteLine($"Wczytano {samples.Count} próbek.");
        NormalizeSamples(samples);
        Console.WriteLine("Znormalizowano dane.");

    }

    static List<Sample> LoadSamples(string filePath)
    {
        List<Sample> samples = new List<Sample>();
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 5) continue;

            double[] attributes = new double[4];
            for (int i = 0; i < 4; i++)
            {
                attributes[i] = double.Parse(parts[i], CultureInfo.InvariantCulture);
            }

            int classLabel = int.Parse(parts[4]);
            samples.Add(new Sample(attributes, classLabel));
        }

        return samples;
    }

}
