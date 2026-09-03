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



namespace AvePoint.ObjectModel.Server19
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Administration;
    using System.Security;
    #endregion

    class AvePeoplePickerSearchActiveDirectoryDomain : AveAutoSerializingObject, IAvePeoplePickerSearchActiveDirectoryDomain
    {
        private SPPeoplePickerSearchActiveDirectoryDomain mPeoplePickerSearchActiveDirectoryDomain;

        public AvePeoplePickerSearchActiveDirectoryDomain(SPPeoplePickerSearchActiveDirectoryDomain peoplePickerSearchActiveDirectoryDomain)
        {
            mPeoplePickerSearchActiveDirectoryDomain = peoplePickerSearchActiveDirectoryDomain;
        }

        #region IAvePeoplePickerSearchActiveDirectoryDomain Members

        public string CustomFilter
        {
            get
            {
                return mPeoplePickerSearchActiveDirectoryDomain.CustomFilter;
            }
            set
            {
                mPeoplePickerSearchActiveDirectoryDomain.CustomFilter = value;
            }
        }

        public string DomainName
        {
            get
            {
                return mPeoplePickerSearchActiveDirectoryDomain.DomainName;
            }
            set
            {
                mPeoplePickerSearchActiveDirectoryDomain.DomainName = value;
            }
        }

        public byte[] EncryptedPassword
        {
            get
            {
                return (byte[])AveAssemblyUtility.GetFieldValue(mPeoplePickerSearchActiveDirectoryDomain, "EncryptedPassword");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mPeoplePickerSearchActiveDirectoryDomain, "EncryptedPassword", value);
            }
        }

        public bool IsForest
        {
            get
            {
                return mPeoplePickerSearchActiveDirectoryDomain.IsForest;
            }
            set
            {
                mPeoplePickerSearchActiveDirectoryDomain.IsForest = value;
            }
        }

        public string LoginName
        {
            get
            {
                return mPeoplePickerSearchActiveDirectoryDomain.LoginName;
            }
            set
            {
                mPeoplePickerSearchActiveDirectoryDomain.LoginName = value;
            }
        }

        public SecureString Password
        {
            get
            {
                return (SecureString)AveAssemblyUtility.GetPropertyValue(mPeoplePickerSearchActiveDirectoryDomain, "Password");
            }
        }

        #endregion
    }
}
