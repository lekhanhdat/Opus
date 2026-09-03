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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Portal;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveRatingsSettingsPage : IAveRatingsSettingsPage,IDisposable
    {
        private const string mRatingsSettingsPage_ContainsFieldById_Method = "ContainsFieldById";
        private const string mRatingsSettingsPage_EnableRatings_Method = "EnableRatings";
        private const string mRatingsSettingsPage_DisableRatings_Method = "DisableRatings";
        private RatingsSettingsPage mRatingsSettingsPage;

        public AveRatingsSettingsPage(RatingsSettingsPage ratingsSettingsPage)
        {
            mRatingsSettingsPage = ratingsSettingsPage;
        }

        public AveRatingsSettingsPage()
        {
            mRatingsSettingsPage = new RatingsSettingsPage();
        }

        #region IAveRatingsSettingsPage Members

        public bool ContainsFieldById(Guid id, IAveFieldCollection fieldColl)
        {
            object[] paramObjs = new object[] { id, (fieldColl as AveFieldCollection).FieldCollection };
            return (bool)AveAssemblyUtility.InvokeStaticMethod(mRatingsSettingsPage.GetType(), mRatingsSettingsPage_ContainsFieldById_Method, new Type[] { typeof(Guid), typeof(SPFieldCollection) }, paramObjs);
        }

        public bool EnableRatings(IAveList list, bool repropagate)
        {
            object[] paramObjs = new object[] { (list as AveList).List, repropagate };
            return (bool)AveAssemblyUtility.InvokeStaticMethod(mRatingsSettingsPage.GetType(), mRatingsSettingsPage_EnableRatings_Method, new Type[] { typeof(SPList), typeof(bool) }, paramObjs);
        }

        public void DisableRatings(IAveList list)
        {
            object[] paramObjs = new object[] { (list as AveList).List };
            AveAssemblyUtility.InvokeStaticMethod(mRatingsSettingsPage.GetType(), mRatingsSettingsPage_DisableRatings_Method, new Type[] { typeof(SPList) }, paramObjs);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mRatingsSettingsPage != null)
            {
                mRatingsSettingsPage.Dispose();
                mRatingsSettingsPage = null;
            }
        }

        #endregion

        public bool IsAlowRated(IAveList list)
        {
            Type t = Type.GetType("Microsoft.SharePoint.Portal.RatingsFeatureConstants,Microsoft.SharePoint.Portal,Version=16.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c");
            bool flag1 = this.ContainsFieldById((Guid)Invoker.GetStaticRawProperty(t, "RatingsFieldGuid_AverageRating"), list.Fields);
            bool flag2 = this.ContainsFieldById((Guid)Invoker.GetStaticRawProperty(t, "RatingsFieldGuid_RatingCount"), list.Fields);
            return flag1 && flag2;
        }
    }
}
