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




namespace AvePoint.GCommon.Contract.GranularRestore
{
    #region == using directives ==
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
    #endregion ==

     [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMGranularRestoreService
    {
        /// <summary> According to the jobId，update the job performance value。 </summary>
        /// <param name="jobId"></param>
        /// <param name="jobPerformance"></param>
        void UpdateJobPerformance(string jobId, string jobPerformance);

         /// <summary>  This method mainly provide external calls, according to the backup jobId generated restore job and do in place type restore. </summary>
         /// <param name="backupJobId"></param>
        /// <param name="type">Value:ConflictResolutionType.Overwrite or ConflictResolutionType.Replace</param>
         /// <param name="relatedJobID"></param>
        void GenerateRestoreJob(string backupJobId, ConflictResolutionType type, string relatedJobID);
    }
}
