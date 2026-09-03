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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract
{

    public class STreeBrowser : RemoteMessage<TreeBrowserArgs>
    {

        public override TreeBrowserArgs MethodArgs { get; set; }
        public override string MethodName { get { return MethodMapping.MT[typeof(STreeBrowser)]; } }

    }

    public enum BrowserResultEnum
    {
        Succeed,
        Failed
    }

    public class BrowserResult
    {
        public BrowserResultEnum Result { set; get; }
        public string Message { set; get; }

    }

    public class STreeBrowserExecute : RemoteInvoke<TreeBrowserArgs, BrowserResult>
    {
        public override TreeBrowserArgs MethodArgs { get; set; }
        public override BrowserResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(STreeBrowserExecute)];
    }

    public class TreeBrowserArgs
    {
        public string TenantId { set; get; }

        public int Type { set; get; }

        public string BatchId { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public string RootDir { set; get; }
    }


    public enum TreeBrowserType
    {
        Validation = 1,
        Browser = 2
    }




}
