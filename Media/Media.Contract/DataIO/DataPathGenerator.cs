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

//namespace MediaContract;
//public class DataPathGenerator : IDataPathGenerator
//{
//    public string ModulePath { get; }
//    public string PlanId { get; }
//    public string CycleId { get; }
//    public string JobId { get; }
//    public string FileNamePrefix { get; }

//    public DataPathGenerator(string modulePath, string planId, string cycleId, string jobId, string fileNamePrefix = "")
//    {
//        ModulePath = modulePath;
//        PlanId = planId;
//        CycleId = cycleId;
//        JobId = jobId;
//        FileNamePrefix = fileNamePrefix;
//    }

//    public DataPathGenerator(DataModule module, string planId, string cycleId, string jobId, string fileNamePrefix = "")
//        : this(ConvertModule(module), planId, cycleId, jobId, fileNamePrefix)
//    {
//    }

//    private static String ConvertModule(DataModule module) => module switch
//    {
//        DataModule.GranularPlatform => DataPathConstants.GranularModulePath,
//        DataModule.PowerPlatform => DataPathConstants.PowerPlatformModulePath,
//        DataModule.ExchangePlatform => DataPathConstants.ExchangeModulePath,
//        _ => throw new NotSupportedException(module.ToString())
//    };

//    public string GenerateFileName(long prefixNumber, long fileNumber, FileType fileType)
//    {
//        return Path.Combine(ModulePath, PlanId, CycleId, JobId, $"{GetDataFileType(fileType)}{prefixNumber}_{fileNumber}{FileNamePrefix}.dat");
//    }

//    private static string GetDataFileType(FileType fileType)
//    {
//        return fileType switch
//        {
//            FileType.Content => DataPathConstants.ContentFileNamePrefix,
//            FileType.MetaData => DataPathConstants.MetaFileNamePrefix,
//            _ => throw new NotSupportedException(fileType.ToString())
//        };
//    }
//}