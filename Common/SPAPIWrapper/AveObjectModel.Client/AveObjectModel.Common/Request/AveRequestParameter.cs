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




namespace AvePoint.ObjectModel.Common
{
    using AvePoint.Wrapper.Common;
    using System;
    /// <summary>
    /// 此类用于多个对象共用一个IAverequest时使用，避免Dispose时，将IAvequest重复添加到缓存中
    /// </summary>
    public class AveRequestParameter : IDisposable
    {
        public IAveRequest AveRequest { get; private set; }
        public AveBPOSAccountInfo UserInfo { get; set; }
        public bool IsDisposed { get; private set; }

        public AveRequestParameter(IAveRequest aveRequest)
        {
            this.AveRequest = aveRequest;
            IsDisposed = false;
        }
        public AveRequestParameter(IAveRequest aveRequest,AveBPOSAccountInfo userAccountInfo)
        {
            this.AveRequest = aveRequest;
            this.UserInfo = userAccountInfo;
            IsDisposed = false;
        }
        public void Dispose()
        {
            AveRequest = null;
            UserInfo = null;
            IsDisposed = true;
        }
    }
}
