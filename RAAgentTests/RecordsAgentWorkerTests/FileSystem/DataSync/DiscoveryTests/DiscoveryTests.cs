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
using AvePoint.RA.FileSystem.Stubs;
using Xunit.Abstractions;

namespace RecordsAgentWorkerTests.FileSystem.DataSync;

public class DiscoveryTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public DiscoveryTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void FSDiscoverMock_GetAll()
    {
        string rootPath = @"\\172.29.20.43\c$\Users\derek.nguyen\Desktop\Test_folder_30k_v7";
        string folderName = "sample";
        var mock = new FSDiscoverMock(rootPath, folderName);
        List<Stub> result = [];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long memoryBeforeBytes= GC.GetTotalMemory(false);
        foreach (var files in mock.GetAllFiles())
        {
            result.Add(files);
        }
        long memoryAfterBytes = GC.GetTotalMemory(false);
        stopwatch.Stop();
        testOutputHelper.WriteLine($"Elapsed time for GetAllFiles: {stopwatch.ElapsedMilliseconds} ms");
        testOutputHelper.WriteLine($"Memory used by GetFilesInBatch: {(memoryAfterBytes - memoryBeforeBytes) / (1024.0 * 1024.0):F2} MB");
        
    }
    
    [Fact]
    public void FSDiscoverMock_GetFilesInBatch()
    {
        string rootPath = @"\\172.29.20.43\c$\Users\derek.nguyen\Desktop\Test_folder_30k_v7";
        string folderName = "sample";
        var mock = new FSDiscoverMock(rootPath, folderName);
        List<Stub> result = [];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long memoryBeforeBytes = GC.GetTotalMemory(false);
        foreach (var files in mock.GetFilesInBatch())
        {
            result.AddRange(files);
        }
        long memoryAfterBytes = GC.GetTotalMemory(false);
        stopwatch.Stop();
        testOutputHelper.WriteLine($"Elapsed time for GetFilesInBatch: {stopwatch.ElapsedMilliseconds} ms");
        testOutputHelper.WriteLine($"Memory used by GetFilesInBatch: {(memoryAfterBytes - memoryBeforeBytes) / (1024.0 * 1024.0):F2} MB");
    }
    
    
}