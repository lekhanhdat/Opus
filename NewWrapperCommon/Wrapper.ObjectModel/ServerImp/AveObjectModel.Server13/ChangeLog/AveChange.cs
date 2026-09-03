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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server13
{
    public class AveChange : IAveChange
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveChange));
        private SPChange mChange = null;
        private long mChangeNumber = -1;
        private AveChangeCollection mChangeCollection = null;
        private Guid mInternalListId;
        private Guid mInternalUniqueId;
        private string mInternalUrl;
        private Guid mInternalWebId;

        public AveChange(SPChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException();
            }

            mChange = change;
        }

        internal SPChange Change
        {
            get { return mChange; }
        }

        #region Public Properties of SPChange

        public AveChangeType ChangeType
        {
            get { return (AveChangeType)mChange.ChangeType; }
        }

        public Guid SiteId
        {
            get { return mChange.SiteId; }
        }

        public DateTime Time
        {
            get { return mChange.Time; }
        }

        #endregion

        #region The Internal Properties of SPChange

        public long ChangeNumber
        {
            get
            {
                if (mChangeNumber < 0)
                {
                    var number = GetNonPublicPropertyOfSPChange(mChange, "ChangeNumber");
                    mChangeNumber = number == null ? -1 : (long)number;
                }
                return mChangeNumber;
            }
        }

        public IAveChangeCollection ChangeCollection
        {
            get 
            {
                if (mChangeCollection == null)
                {
                    var changeCollection = GetNonPublicPropertyOfSPChange(mChange, "ChangeCollection");
                    mChangeCollection = changeCollection == null ? null : new AveChangeCollection(changeCollection as SPChangeCollection);
                }
                return mChangeCollection;
            }
        }

        public Guid InternalListId
        {
            get 
            {
                if (mInternalListId == null || mInternalListId == Guid.Empty)
                {
                    var internalListId = GetNonPublicPropertyOfSPChange(mChange, "InternalListId");
                    mInternalListId = internalListId == null ? Guid.Empty : (Guid)internalListId;
                }
                return mInternalListId;
            }
        }

        public Guid InternalUniqueId
        {
            get
            {
                if (mInternalUniqueId == null || mInternalUniqueId == Guid.Empty)
                {
                    var uniqueId = GetNonPublicPropertyOfSPChange(mChange, "InternalUniqueId");
                    mInternalUniqueId = uniqueId == null ? Guid.Empty : (Guid)uniqueId;
                }
                return mInternalUniqueId;
            }
        }

        public string InternalUrl
        {
            get
            {
                if (string.IsNullOrEmpty(mInternalUrl))
                {
                    var internalUrl = GetNonPublicPropertyOfSPChange(mChange, "InternalUrl");
                    mInternalUrl = internalUrl == null ? string.Empty : internalUrl.ToString();
                }
                return mInternalUrl;
            }
        }

        public Guid InternalWebId
        {
            get
            {
                if (mInternalWebId == null || mInternalWebId == Guid.Empty)
                {
                    var internalWebid = GetNonPublicPropertyOfSPChange(mChange, "InternalWebId");
                    mInternalWebId = internalWebid == null ? Guid.Empty : (Guid)internalWebid;
                }
                return mInternalWebId;
            }
        }

        protected object GetNonPublicPropertyOfSPChange(SPChange instance, string propertyName)
        {
            try
            {
                Type t = instance.GetType();
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance;
                PropertyInfo pi = t.GetProperty(propertyName, flags);
                if (pi != null)
                {
                    return pi.GetValue(instance);
                }
            }
            catch (Exception ex)
            {
                log.Warn("An exception occurred while getting the {0} property: {1}", propertyName, ex.ToString());
            }

            return null;
        }

        /// <summary>
        /// 此属性会被频繁调用，每次都反射有效率问题。  由于这个属性不能被改变，在这里加Cache。
        /// </summary>
        object[] mRows;
        public object[] Rows
        {
            get
            {
                if (mRows == null)
                {
                    mRows = (object[])AveAssemblyUtility.GetFieldValue(mChange, "m_row");
                }
                return mRows;
            }
        }
        #endregion
    }
}
