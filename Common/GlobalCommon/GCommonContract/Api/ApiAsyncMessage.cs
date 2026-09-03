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
namespace AvePoint.GCommon.Contract.Api
{
    public class ApiAsyncMessage
    {
        public string MessageId { get; set; }
        public ApiModule ApiModule { get; set; }
        public ApiAction ApiAction { get; set; }
        public string TenantGroupId { get; set; }
        public string TenantName { get; set; }
        public string UserId { get; set; }
        public string Extension { get; set; }
    }

    public enum ApiModule
    {
        None = 0,
        Archiver = 1,
        CentralAdmin = 2,
        Common = 3,
        DeploymentManager = 4,
    }

    public enum ApiAction
    {
        None = 0,
        Apply = 1,
        Run = 2,
        CloneUserPermission = 3,
        GrantTemporaryPermission = 4,
        SecuritySearch = 5,
        Reconnect = 6,
        Create = 9,
    }
}
