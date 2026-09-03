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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region directives

    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;

    #endregion directives

    public class ArchiverFullTextIndexIndexService
        : ArchiverIndexServiceBase
        , IArchiverFullTextIndexIndexService
    {
        public Int64 GetIndexTotalCount(String jobId, String isSystemFile)
        {
            return this.HeadAndBodyService.GetIndexTotalCount(jobId, isSystemFile);
        }

        public List<ArchiverBasicIndex> GetNeedFiles(String jobId, String siteUrl, Int32 offset, Int32 length, String isSystemFile)
        {
            return this.HeadAndBodyService.GetNeedFiles(jobId, siteUrl, offset, length, isSystemFile);
        }

        public ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex childIndex)
        {
            return this.HeadAndBodyService.GetParentFolder(childIndex, string.Empty);
        }
    }
}