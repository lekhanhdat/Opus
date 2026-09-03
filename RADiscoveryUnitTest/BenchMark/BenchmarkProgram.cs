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
using System;

namespace BenchMark
{
    internal static class BenchmarkProgram
    {
        private const string ExcelPreviewCsvBenchmarkName = "excel-preview-csv";
        private const string VerifyMode = "verify";
        private const string RunMode = "run";

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return 1;
            }

            var benchmarkName = args[0].Trim().ToLowerInvariant();
            var mode = args[1].Trim().ToLowerInvariant();

            if (!string.Equals(benchmarkName, ExcelPreviewCsvBenchmarkName, StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unsupported benchmark '{args[0]}'.");
                PrintUsage();
                return 1;
            }

            if (string.Equals(mode, VerifyMode, StringComparison.Ordinal))
            {
                RMExcelPreviewCsvBenchmark.RunVerification();
                return 0;
            }

            if (string.Equals(mode, RunMode, StringComparison.Ordinal))
            {
                RMExcelPreviewCsvBenchmark.Run();
                return 0;
            }

            Console.Error.WriteLine($"Unsupported mode '{args[1]}'.");
            PrintUsage();
            return 1;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project C:\\CodeSpace\\reco\\RADiscoveryUnitTest\\RADiscoveryUnitTest.csproj -- excel-preview-csv verify");
            Console.WriteLine("  dotnet run --project C:\\CodeSpace\\reco\\RADiscoveryUnitTest\\RADiscoveryUnitTest.csproj -- excel-preview-csv run");
        }
    }
}
