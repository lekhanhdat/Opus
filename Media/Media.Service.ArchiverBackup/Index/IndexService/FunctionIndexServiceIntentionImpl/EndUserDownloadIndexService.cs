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
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class EndUserDownloadIndexService
        : ArchiverIndexServiceBase
        , IEndUserDownloadIndexService
    {
        public ArchiverBasicIndex GetCurrentIndex(String pathMd5)
        {
            return this.HeadAndBodyService.GetIndex(pathMd5);
        }

        public Dictionary<String, String> GetAllJobEncryptionInfos()
        {
            var encryptionInfos = new Dictionary<String, String>();
            var indexList = this.JobInfoIndexService.GetJobInfoIndexesByKey(ServiceConstants.EncryptionInfoKey);
            foreach (ArchiverJobInfoIndex jobInfoIndex in indexList)
            {
                encryptionInfos.Add(jobInfoIndex.JobId, jobInfoIndex.Value);
            }
            return encryptionInfos;
        }
    }
}