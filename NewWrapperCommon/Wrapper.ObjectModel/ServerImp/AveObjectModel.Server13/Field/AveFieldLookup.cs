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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    class AveFieldLookup : AveField, IAveFieldLookup
    {
        private SPFieldLookup mFieldLookup;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveFieldLookup(AveFieldCollection fieldColl, SPFieldLookup field)
            : base(fieldColl, field)
        {
            mFieldLookup = field;
        }

        #region IAveFieldLookup Members

        public bool AllowMultipleValues
        {
            get
            {
                return mFieldLookup.AllowMultipleValues;
            }
            set
            {
                mFieldLookup.AllowMultipleValues = value;
            }
        }

        public bool IsRelationship
        {
            get
            {
                return mFieldLookup.IsRelationship;
            }
            set
            {
                mFieldLookup.IsRelationship = value;
            }
        }

        public string LookupField
        {
            get
            {
                return mFieldLookup.LookupField;
            }
            set
            {
                mFieldLookup.LookupField = value;
            }
        }

        public string LookupList
        {
            get
            {
                return mFieldLookup.LookupList;
            }
            set
            {
                AveAssemblyUtility.InvokeMethod(mFieldLookup, typeof(SPField), "SetFieldAttributeValue", new object[] { "List", value });
                AveAssemblyUtility.SetFieldValue(mFieldLookup, "lookupList", value);
                AveAssemblyUtility.SetFieldValue(mFieldLookup, "lookupListSet", true);
                //mFieldLookup.LookupList = value;
            }
        }

        public Guid LookupWebId
        {
            get
            {
                return mFieldLookup.LookupWebId;
            }
            set
            {
                mFieldLookup.LookupWebId = value;
            }
        }

        public string PrimaryFieldId
        {
            get
            {
                return mFieldLookup.PrimaryFieldId;
            }
            set
            {
                mFieldLookup.PrimaryFieldId = value;
            }
        }

        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior
        {
            get
            {
                return (AveRelationshipDeleteBehavior)mFieldLookup.RelationshipDeleteBehavior;
            }
            set
            {
                mFieldLookup.RelationshipDeleteBehavior = (SPRelationshipDeleteBehavior)value;
            }
        }

        public int Version
        {
            get { return mFieldLookup.Version; }
        }

        public bool PrependId
        {
            get
            {
                return mFieldLookup.PrependId;
            }
            set
            {
                mFieldLookup.PrependId = value;
            }
        }

        public bool UnlimitedLengthInDocumentLibrary
        {
            get
            {
                return mFieldLookup.UnlimitedLengthInDocumentLibrary;
            }
            set
            {
                mFieldLookup.UnlimitedLengthInDocumentLibrary = value;
            }
        }

        public bool CountRelated
        {
            get
            {
                return mFieldLookup.CountRelated;
            }
            set
            {
                mFieldLookup.CountRelated = value;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                if (!this.AllowMultipleValues)
                {
                    return typeof(IAveFieldLookupValue);
                }
                return typeof(IAveFieldLookupValueCollection);
            }
        }

        public bool IsDependentLookup
        {
            get { return mFieldLookup.IsDependentLookup; }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (this.AllowMultipleValues)
            {
                return new AveFieldLookupValueCollection(new SPFieldLookupValueCollection(value));
            }
            return new AveFieldLookupValue(new SPFieldLookupValue(value));
        }

        public override string GetFieldValueAsText(object value)
        {
            return mFieldLookup.GetFieldValueAsText(value);
        }

        #endregion        
    

        public string LookupListTitle
        {
            get
            {
                try
                {
                    return this.ParentList.ParentWeb.Lists.GetById(new Guid(LookupList)).Title;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetLookupListTitleError, e.ToString());
                    return string.Empty;
                }
            }
        }
    }
}
