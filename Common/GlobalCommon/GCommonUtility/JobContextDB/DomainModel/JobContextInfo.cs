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
namespace AvePoint.GCommon.Utility.JobContextDB
{
    #region using directives
    using System;
    using System.Collections.Generic;

    #endregion

    [Table(JobContextConstants.TB_JOB_CONTEXT)]
    public class JobContextInfo
        : IDomainBase
    {
        [Column("COL_ID")]
        public string Id { get; set; }

        [Column("COL_Content")]
        public string Content { get; set; }

        public Dictionary<string, Object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<string, Object>();
            result.Add("@COL_ID", Id);
            result.Add("@COL_Content", Content);
            return result;
        }

        public override string ToString()
        {
            return string.Format("ID : {0}.", Id);
        }
    }
}
