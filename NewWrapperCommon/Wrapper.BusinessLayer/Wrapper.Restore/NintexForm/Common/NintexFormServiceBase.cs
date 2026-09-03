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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    public abstract class NintexFormServiceBase : INintexFormService
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected IAveList mAveList;
        protected IAveSPWeb mAveSPWeb;
        protected bool isPost;
        internal INintexFormContentProcessor contentProcessor;

        protected NintexFormServiceBase(IAveList aveList, IAveSPWeb aveSPWeb, bool isPost)
        {
            mAveList = aveList;
            mAveSPWeb = aveSPWeb;
            this.isPost = isPost;
            contentProcessor = null;
        }

        public static INintexFormService CreateNintexForm(IAveList aveList, IAveSPWeb aveSPWeb, bool isPost)
        {
            if (aveSPWeb.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (aveSPWeb.ParentSite.SPSite.UserAccountInfo.ConnectionType == BposConnectionType.AppToken)
                {
                    throw new Exception("Can not suppport restore nintex form with apptoken connection type");
                }
                if (aveSPWeb.ParentSite.SPSite.IsOnlineSite)
                {
                    return new NintexFormServiceOnline(aveList, aveSPWeb, isPost);
                }
                else
                {
                    throw new NotSupportedException("Do not support fake online.");
                }
            }
            else if (aveSPWeb.ParentSite.SPContextKind >= AveContextKind.ServerObjectModel)//避免以后SP再出新版本
            {
                return new NintexFormServiceServer(aveList, aveSPWeb, isPost);
            }

            throw new NotSupportedException(
                       string.Format("Do not support this object model. Model: {0}", aveSPWeb.ParentSite.SPContextKind));
        }

        public abstract void RestoreForm(AveNintexFormInfo nintexFormInfo, string contentTypeId);

        protected abstract void PublishNintexForm(string newNintexFormXml, string contentTypeId);

        public abstract void DeleteForm(string listId, string contentTypeId);
    }
}
