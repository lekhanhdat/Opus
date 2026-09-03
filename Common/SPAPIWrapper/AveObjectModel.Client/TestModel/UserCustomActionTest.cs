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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModel
{
    public class UserCustomActionTest
    {
        private IAveSite mSite;
        private IAveWeb mWeb;
        private IAveList mList;

        public UserCustomActionTest()
        {
            Init();
        }

        private void Init()
        {
            string user = "wbhu@m365x356450.onmicrosoft.com";
            string pwd = "demo12!@";
            WrapperConfiguration.BPOS_S.EnableMultiLanguage = true;
            string url = "https://m365x356450.sharepoint.com/sites/wbhu_usergroup_001";
            var factory = AveObjectModelFactory.CreateObjectModelFactory(url, new AveBPOSAccountInfo { UserName = user, Password = pwd });
            mSite = factory.CreateSite(url);
            mWeb = mSite.RootWeb;
            mList = mWeb.Lists["Documents"];
            var c1 = mSite.UserCustomActions;
            var c2 = mWeb.UserCustomActions;
            var c3 = mList.UserCustomActions;
        }
    }
}
