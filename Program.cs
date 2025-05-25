using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static Program;

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

    public static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += Math.Pow(a[i] - b[i], 2);
        }
        return Math.Sqrt(sum);
    }

    public static double ManhattanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += Math.Abs(a[i] - b[i]);
        }
        return sum;
    }

    public static double ChebyshevDistance(double[] a, double[] b)
    {
        double max = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double diff = Math.Abs(a[i] - b[i]);
            if (diff > max)
                max = diff;
        }
        return max;
    }

    public static double MinkowskiDistance(double[] a, double[] b, int p = 3)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += Math.Pow(Math.Abs(a[i] - b[i]), p);
        }
        return Math.Pow(sum, 1.0 / p);
    }

    public static double LogDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double val1 = Math.Max(a[i], 0.00001);
            double val2 = Math.Max(b[i], 0.00001);
            sum += Math.Abs(Math.Log(val1) - Math.Log(val2));
        }
        return sum;
    }


    delegate double DistanceMetric(double[] a, double[] b);

    static int ClassifyKNN(Sample testSample, List<Sample> trainingSamples, int k, DistanceMetric metric)
    {
        var distances = new List<(Sample sample, double distance)>();

        foreach (var sample in trainingSamples)
        {
            double distance = metric(testSample.Attributes, sample.Attributes);
            distances.Add((sample, distance));
        }

        distances.Sort((a, b) => a.distance.CompareTo(b.distance));

        var kNearest = distances.Take(k);

        var classCounts = new Dictionary<int, int>();
        foreach (var entry in kNearest)
        {
            int cls = entry.sample.ClassLabel;
            if (!classCounts.ContainsKey(cls))
                classCounts[cls] = 0;
            classCounts[cls]++;
        }

        return classCounts.OrderByDescending(c => c.Value).First().Key;
    }

    static double OneVsRestValidation(List<Sample> samples, int k, DistanceMetric metric)
    {
        int correct = 0;

        for (int i = 0; i < samples.Count; i++)
        {
            var testSample = samples[i];

            var trainingSamples = new List<Sample>(samples);
            trainingSamples.RemoveAt(i);

            int predictedClass = ClassifyKNN(testSample, trainingSamples, k, metric);

            if (predictedClass == testSample.ClassLabel)
                correct++;
        }

        return (double)correct / samples.Count * 100.0;
    }



    static void Main()
    {
        Console.WriteLine("Podaj nazwę pliku z próbkami (np. iris.txt):");
        string filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Plik nie istnieje!");
            return;
        }

        List<Sample> samples = LoadSamples(filePath);

        Console.WriteLine($"Wczytano {samples.Count} próbek.");

        NormalizeSamples(samples);
        Console.WriteLine("Znormalizowano dane.\n");

        Console.WriteLine("Wybierz metrykę:");
        Console.WriteLine("1 - Euklidesowa");
        Console.WriteLine("2 - Manhattan");
        Console.WriteLine("3 - Chebyshev");
        Console.WriteLine("4 - Minkowski (p=3)");
        Console.WriteLine("5 - Logarytmiczna");
        Console.Write("Twój wybór: ");

        if (!int.TryParse(Console.ReadLine(), out int metricChoice) || metricChoice < 1 || metricChoice > 5)
        {
            Console.WriteLine("Nieprawidłowy wybór metryki.");
            return;
        }

        Console.Write("Podaj wartość k: ");
        if (!int.TryParse(Console.ReadLine(), out int k) || k < 1)
        {
            Console.WriteLine("Nieprawidłowa wartość k.");
            return;
        }

        DistanceMetric selectedMetric = metricChoice switch
        {
            1 => EuclideanDistance,
            2 => ManhattanDistance,
            3 => ChebyshevDistance,
            4 => (a, b) => MinkowskiDistance(a, b, 3),
            5 => LogDistance,
            _ => EuclideanDistance
        };

        double accuracy = OneVsRestValidation(samples, k, selectedMetric);
        Console.WriteLine($"\nDokładność klasyfikacji k-NN (k={k}): {accuracy:F2}%");
    }


}
