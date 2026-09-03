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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveCommonRequest : IAveCommonRequest
    {
        private const string mRequestType = "Microsoft.SharePoint.Library.SPRequest";
        private const string mRequest_ValidateSubscriptionFilter_Method = "ValidateSubscriptionFilter";
        private const string mRequest_IrmClientPresent_Method = "IrmClientPresent";
        private const string mRequest_IrmClientReady_Method = "IrmClientReady";
        private const string mRequest_MapUrlToListAndView_Method = "MapUrlToListAndView";
        private object mRequest;

        public AveCommonRequest()
        {
            mRequest = AveAssemblyUtility.CreateInstance(mRequestType);
        }

        public AveCommonRequest(object request)
        {
            mRequest = request;
        }

        #region IAveCommonRequest Members

        internal object Request
        {
            set
            {
                mRequest = value;
            }
        }

        public bool ValidateSubscriptionFilter(string bstrFilter)
        {
            //ADO-157963 13 以上版本返回int类型 
            return (int)AveAssemblyUtility.InvokeMethod(mRequest, mRequest_ValidateSubscriptionFilter_Method, bstrFilter) == 1;
        }

        public bool IrmClientPresent()
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mRequest, mRequest_IrmClientPresent_Method);
        }

        public int IrmClientReady(bool bAD, string bstrUrl, out string pbstrMachineName)
        {
            string outPrama = String.Empty;
            int irmClientReady = (int)AveAssemblyUtility.InvokeMethod(mRequest, mRequest_IrmClientReady_Method, new Type[] { typeof(bool), typeof(string), typeof(string).MakeByRefType() }, new object[] { bAD, bstrUrl, outPrama });
            pbstrMachineName = outPrama;
            return irmClientReady;
        }

        public void SetWebAssociatedGroups(string bstrUrl, string bstrGroups)
        {
            AveAssemblyUtility.InvokeMethod(mRequest, "SetWebAssociatedGroups", new Type[] { typeof(string), typeof(string) }, new object[] { bstrUrl, bstrGroups });
        }

        public void Dispose()
        {
            AveAssemblyUtility.InvokeMethod(mRequest, "Dispose", new Type[] { }, new object[] { });
        }

        public void MapUrlToListAndView(string bstrUrl, string bstrUrlToMap, out Guid pgListId, out Guid pgViewId)
        {
            Guid _pgListId = Guid.Empty;
            Guid _pgViewId = Guid.Empty;
            object[] paramObjs = new object[] { bstrUrl, bstrUrlToMap, _pgListId, _pgViewId };
            AveAssemblyUtility.InvokeMethod(mRequest, mRequest_MapUrlToListAndView_Method, new Type[] { typeof(string), typeof(string), typeof(Guid).MakeByRefType(), typeof(Guid).MakeByRefType() }, paramObjs);
            pgListId = (Guid)paramObjs[2];
            pgViewId = (Guid)paramObjs[3];
        }

        public void SetGhostedFile(object[] args)
        {
            AveAssemblyUtility.InvokeMethod(mRequest, mRequest.GetType(), "SetGhostedFile", args);
        }

        public void SetGhostedFile(object[] args, Type[] paramTypes)
        {
            AveAssemblyUtility.InvokeMethod(mRequest, mRequest.GetType(), "SetGhostedFile", paramTypes, args);
        }

        #endregion
    }
}
