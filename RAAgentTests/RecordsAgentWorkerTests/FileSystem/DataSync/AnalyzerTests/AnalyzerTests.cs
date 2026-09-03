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
using Xunit.Abstractions;

namespace RecordsAgentWorkerTests.FileSystem.DataSync.AnalyzerTests;

public class AnalyzerTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public AnalyzerTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }
    [Fact]
    public void FSAnalyzerMock_AssembleFolder()
    {
        string rootPath = @"\\172.29.20.43\c$\Users\derek.nguyen\Desktop";
        var discoveryMock = new FSDiscoverMock(rootPath, "Test_folder_30k_v7");
        var analyzerMock = new FSAnalyzerMock(rootPath);
        var dirs = discoveryMock.QuerySubFoldersFileLevel();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        analyzerMock.AssembleFolderBasicInfo(dirs[0]);
        stopwatch.Stop();
        testOutputHelper.WriteLine($"Elapsed time for GetAllFiles: {stopwatch.ElapsedMilliseconds} ms");
    }
}