/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved.
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RA.Common.Util;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BenchMark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 10)]
    public class RMExcelPreviewCsvBenchmark
    {
        private const string BenchmarkFilePath = @"C:\Users\shaun.zhang\Desktop\AvePoint\WorkSpace\DEV\202607\0729-Excel解析Top50转csv then embedding\性能测试文件\500K.xlsx";
        private const int UnlimitedMaxChars = int.MaxValue;
        private const string UnlimitedCaseName = "ExcelUtil.ReadExcelPreviewAsCsv.unlimited";
        private const int SamplingIntervalMilliseconds = 10;
        private static readonly string PeakMemoryOutputPath = Path.Combine(Path.GetTempPath(), "RMExcelPreviewCsvBenchmark.peak-memory.tsv");
        private static readonly ConcurrentDictionary<string, PeakMemorySnapshot> PeakMemoryByCase = new ConcurrentDictionary<string, PeakMemorySnapshot>(StringComparer.Ordinal);
        private static readonly object PeakMemoryFileLock = new object();

        public static void Run()
        {
            PeakMemoryByCase.Clear();
            if (File.Exists(PeakMemoryOutputPath))
            {
                File.Delete(PeakMemoryOutputPath);
            }

            WritePeakMemoryHeader();
            Summary summary = BenchmarkRunner.Run<RMExcelPreviewCsvBenchmark>();
            PrintPeakMemorySummary(summary);
        }

        public static void RunVerification()
        {
            EnsureBenchmarkFileExists();

            using var stream = File.OpenRead(BenchmarkFilePath);
            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, Path.GetFileName(BenchmarkFilePath), UnlimitedMaxChars);

            Console.WriteLine($"Length: {csv.Length:N0}");
            Console.WriteLine($"Row count: {CountNonEmptyRows(csv):N0}");
            Console.WriteLine(csv[..Math.Min(csv.Length, 300)]);
        }

        [Benchmark(Description = UnlimitedCaseName)]
        public int ReadExcelPreviewAsCsvUnlimited()
        {
            return ExecuteCase(UnlimitedCaseName, UnlimitedMaxChars);
        }

        private static int ExecuteCase(string caseName, int maxChars)
        {
            EnsureBenchmarkFileExists();

            using var stream = File.OpenRead(BenchmarkFilePath);
            using var cancellation = new CancellationTokenSource();
            var samplingTask = StartPeakMemorySampling(caseName, cancellation.Token);

            var stopwatch = Stopwatch.StartNew();
            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, Path.GetFileName(BenchmarkFilePath), maxChars);
            stopwatch.Stop();

            cancellation.Cancel();
            samplingTask.GetAwaiter().GetResult();

            RecordPeakMemory(caseName, stopwatch.Elapsed, csv.Length, CountNonEmptyRows(csv));
            return csv.Length;
        }

        private static Task StartPeakMemorySampling(string caseName, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var process = Process.GetCurrentProcess();
                while (!cancellationToken.IsCancellationRequested)
                {
                    process.Refresh();
                    var workingSet = process.WorkingSet64;
                    var heapSize = GC.GetGCMemoryInfo().HeapSizeBytes;
                    PeakMemoryByCase.AddOrUpdate(
                        caseName,
                        _ => new PeakMemorySnapshot(workingSet, heapSize),
                        (_, current) => current.Update(workingSet, heapSize));

                    try
                    {
                        await Task.Delay(SamplingIntervalMilliseconds, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, cancellationToken);
        }

        private static void RecordPeakMemory(string caseName, TimeSpan elapsed, int csvLength, int rowCount)
        {
            PeakMemoryByCase.TryGetValue(caseName, out var peakMemory);
            var workingSet = peakMemory?.PeakWorkingSetBytes ?? 0;
            var heapSize = peakMemory?.PeakHeapBytes ?? 0;
            var line = string.Join(
                "\t",
                DateTime.UtcNow.ToString("O"),
                caseName,
                elapsed.TotalMilliseconds.ToString("F3"),
                csvLength,
                rowCount,
                workingSet,
                heapSize);

            lock (PeakMemoryFileLock)
            {
                File.AppendAllText(PeakMemoryOutputPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }

        private static void WritePeakMemoryHeader()
        {
            var header = "timestamp_utc\tcase_name\telapsed_ms\tcsv_length\trow_count\tpeak_working_set_bytes\tpeak_heap_bytes";
            File.WriteAllText(PeakMemoryOutputPath, header + Environment.NewLine, Encoding.UTF8);
        }

        private static void PrintPeakMemorySummary(Summary summary)
        {
            PrintCustomSummaryTable(summary);
            Console.WriteLine($"Benchmark summary generated: {summary.ResultsDirectoryPath}");
            Console.WriteLine($"Peak memory samples: {PeakMemoryOutputPath}");
        }

        private static void PrintCustomSummaryTable(Summary summary)
        {
            Console.WriteLine();
            Console.WriteLine("| Method | Mean | Error | StdDev | Gen0 | Gen1 | Gen2 | Allocated | PeakWorkingSetMb | PeakGcHeapMb |");
            Console.WriteLine("|--- |---:|---:|---:|---:|---:|---:|---:|---:|---:|");

            foreach (var report in summary.Reports)
            {
                var stats = report.ResultStatistics;
                if (stats == null)
                {
                    continue;
                }

                var methodName = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
                var peakMemory = ReadPeakMemoryFromTsv(UnlimitedCaseName);
                var gcStats = report.GcStats;
                var allocatedBytes = gcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase);

                Console.WriteLine(
                    $"| {methodName} | " +
                    $"{FormatDuration(stats.Mean)} | " +
                    $"{FormatDuration(stats.StandardError)} | " +
                    $"{FormatDuration(stats.StandardDeviation)} | " +
                    $"{gcStats.Gen0Collections:N4} | " +
                    $"{gcStats.Gen1Collections:N4} | " +
                    $"{gcStats.Gen2Collections:N4} | " +
                    $"{allocatedBytes / 1024d / 1024d:F2} MB | " +
                    $"{(peakMemory?.PeakWorkingSetBytes ?? 0) / 1024d / 1024d:F2} | " +
                    $"{(peakMemory?.PeakHeapBytes ?? 0) / 1024d / 1024d:F2} |");
            }

            Console.WriteLine();
        }

        private static int CountNonEmptyRows(string csv)
        {
            return csv
                .Split(new[] { "\r\n" }, StringSplitOptions.None)
                .Count(row => row.Length > 0);
        }

        private static PeakMemorySnapshot ReadPeakMemoryFromTsv(string caseName)
        {
            if (!File.Exists(PeakMemoryOutputPath))
            {
                return new PeakMemorySnapshot(0, 0);
            }

            long peakWorkingSetBytes = 0;
            long peakHeapBytes = 0;
            foreach (var line in File.ReadLines(PeakMemoryOutputPath).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split('\t');
                if (parts.Length < 7 || !string.Equals(parts[1], caseName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (long.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var workingSet))
                {
                    peakWorkingSetBytes = Math.Max(peakWorkingSetBytes, workingSet);
                }

                if (long.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out var heapBytes))
                {
                    peakHeapBytes = Math.Max(peakHeapBytes, heapBytes);
                }
            }

            return new PeakMemorySnapshot(peakWorkingSetBytes, peakHeapBytes);
        }

        private static void EnsureBenchmarkFileExists()
        {
            if (!File.Exists(BenchmarkFilePath))
            {
                throw new FileNotFoundException("The benchmark file was not found.", BenchmarkFilePath);
            }
        }

        private static string FormatDuration(double nanoseconds)
        {
            var seconds = nanoseconds / 1_000_000_000d;
            if (seconds >= 1d)
            {
                return $"{seconds:F3} s";
            }

            var milliseconds = nanoseconds / 1_000_000d;
            if (milliseconds >= 1d)
            {
                return $"{milliseconds:F3} ms";
            }

            var microseconds = nanoseconds / 1_000d;
            if (microseconds >= 1d)
            {
                return $"{microseconds:F3} us";
            }

            return $"{nanoseconds:F3} ns";
        }

        private sealed class PeakMemorySnapshot
        {
            public PeakMemorySnapshot(long peakWorkingSetBytes, long peakHeapBytes)
            {
                PeakWorkingSetBytes = peakWorkingSetBytes;
                PeakHeapBytes = peakHeapBytes;
            }

            public long PeakWorkingSetBytes { get; }

            public long PeakHeapBytes { get; }

            public PeakMemorySnapshot Update(long workingSetBytes, long heapBytes)
            {
                return new PeakMemorySnapshot(
                    Math.Max(PeakWorkingSetBytes, workingSetBytes),
                    Math.Max(PeakHeapBytes, heapBytes));
            }
        }
    }
}
