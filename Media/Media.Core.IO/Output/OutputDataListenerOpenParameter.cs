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




namespace AvePoint.Media.Core.IO.Output
{
    #region using directives
    using System;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public class OutputDataListenerOpenParameter<T> where T : IndexBase
    {
        public String JobId { get; set; }
        public Int32 MaxFileSize { get; set; }
        public String DataVolume { get; set; }
        public IXSystem CacheSystem { get; set; }
        public BackupJobBase BackupJob { get; set; }
        public IXSystem DataLogicalDevice { get; set; }
        public IOutputDataHandler<T> OutputDataHandler { get; set; }
        public Boolean storeMD5 { get; set; }
        public AccessTierType AccessTier { get; set; }
    }
}
