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

namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Cloud.Common; 
    #endregion

    enum FailoverMode
    {
        Off = -1,
        ReadWrite = 0,
        Read = 1
    }

    class HCPOpenParameter : CloudOpenParameter
    {
        public string Namespace { get; set; }
        public string Library { get; set; }
        public FailoverMode FailOverMode { set; get; }
        public bool IsValidate { get; set; }
        public Dictionary<string, string> WriteHeaders { get; set; }
        public bool IsRetry { get; set; }

        public string FailedPrimaryHost { get; set; }

        public string PrimaryHost { get; set; }

        public string SecondaryHost { get; set; }

        public bool IsHaveSecondaryHost { get; set; }

        public bool IsUsedSecondaryHost { get; set; }

        public HCPOpenParameter()
        {
            FailOverMode = FailoverMode.ReadWrite;
        }

    }
}
