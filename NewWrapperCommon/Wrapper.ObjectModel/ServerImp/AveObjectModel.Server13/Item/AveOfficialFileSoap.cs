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
using System.Net;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveOfficialFileSoap : IAveOfficialFileSoap
    {
        private const string mOfficialFileSoap_Type = "Microsoft.SharePoint.OfficialFileSoap";
        private const string mOfficialFileSoap_Credentials_Member = "Credentials";
        private const string mOfficialFileSoap_PreAuthenticate_Member = "PreAuthenticate";
        private const string mOfficialFileSoap_GetServerInfo_Method = "GetServerInfo";
        private object mOfficialFileSoap;

        public AveOfficialFileSoap(string absoluteUri)
        {
            mOfficialFileSoap = AveAssemblyUtility.CreateInstance(mOfficialFileSoap_Type, new Type[] { typeof(string) }, new Object[] { absoluteUri });
        }

        public AveOfficialFileSoap(Uri uri)
        {
            mOfficialFileSoap = AveAssemblyUtility.CreateInstance(mOfficialFileSoap_Type, new Type[] { typeof(Uri) }, new Object[] { uri });
        }

        #region IAveOfficialFileSoap Members

        //Methods

        public string GetServerInfo()
        {
            return (string)AveAssemblyUtility.InvokeMethod(mOfficialFileSoap, mOfficialFileSoap_GetServerInfo_Method, null);
        }

        //Properties

        public ICredentials Credentials
        {
            get
            {
                return (ICredentials)AveAssemblyUtility.GetPropertyValue(mOfficialFileSoap, mOfficialFileSoap_Credentials_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mOfficialFileSoap, mOfficialFileSoap_Credentials_Member, value);
            }
        }

        public bool PreAuthenticate
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mOfficialFileSoap, mOfficialFileSoap_PreAuthenticate_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mOfficialFileSoap, mOfficialFileSoap_PreAuthenticate_Member, value);
            }
        }

        #endregion
    }
}
