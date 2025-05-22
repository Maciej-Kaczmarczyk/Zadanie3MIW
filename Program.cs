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

    static void Main()
    {
        string filePath = "iris.txt";
        List<Sample> samples = LoadSamples(filePath);

        foreach (var sample in samples)
        {
            Console.WriteLine($"Atrybuty: {string.Join(", ", sample.Attributes)} | Klasa: {sample.ClassLabel}");
        }

        Console.WriteLine($"Wczytano {samples.Count} próbek.");
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
