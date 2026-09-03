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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.IO;
    using AvePoint.Media.Common;

    #endregion

    public class ArchiverFullTextVolumeGenerator : ArchiverVolumeGeneratorBase
    {
        public override string GenerateDataVolume(VolumeParameter param)
        {
            throw new NotImplementedException();
        }

        public override string GenerateIndexVolume(VolumeParameter param)
        {
            if (!param.IndexCrawlId.IsNullOrEmpty())
            {
                string crawlId = param.IndexCrawlId + "-crawl-";
                return Path.Combine(ServiceConstants.DefaultIndexPath, Path.Combine(ServiceConstants.ArchiverFullTextIndexPath, crawlId));
            }
            String webAppName, siteName;
            ParseSitePath(param.SiteCollectionUrl, out webAppName, out siteName);
            return Path.Combine(ServiceConstants.DefaultIndexPath, Path.Combine(Path.Combine(Path.Combine(Path.Combine(ServiceConstants.ArchiverFullTextIndexPath, ConvertFarmNameToUpper(ProcessFarmName(param.FarmName))), webAppName), siteName), param.JobId));
        }

        public override string GenerateTempDataVolume(VolumeParameter param)
        {
            throw new NotImplementedException();
        }
    }
}