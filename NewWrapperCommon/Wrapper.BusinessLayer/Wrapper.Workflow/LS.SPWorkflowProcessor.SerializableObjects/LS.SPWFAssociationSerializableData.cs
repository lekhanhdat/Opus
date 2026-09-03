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
using System.Collections;
using System.Data;
using System.Text;
using AvePoint.GCommon;
using System.Reflection;

namespace LS.SPWorkflowProcessor.SerializableObjects
{
    [Serializable]
    public class SPWFAssociationSerializableData
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public Guid mId;
        public Guid mParentAssociationId;
        public Guid mSourceId;
        public Guid mBaseId;
        public string mName;
        public string mInternalVersion;
        public string mOriginalName;
        public string mDescription;
        public string mStatusFieldName;
        public string mStatusFieldSchema;
        public string mContentTypeId;
        public int mInstanceCount;
        public int mInstanceCountDirty;
        public Guid mParentListId;
        public Guid mTaskListId;
        public Guid mHistoryListId;
        public string mTaskListTitle;
        public string mHistoryListTitle;
        public int mConfiguration;
        public int mAutoCleanupDays;
        public int mAuthor;
        public DateTime mCreated;
        public DateTime mModified;
        public string mInstantiationParams;
        public int mPermissionsManual;
        public int mVersion;
        public string mCodeBesideAssm;
        public bool mIsDeclarative;
        public string mInternalName;
        public string mAuthorLoginName;
        public List<SPWorkflowSubFileSerializableData> mFormFileUnit = new List<SPWorkflowSubFileSerializableData>();

        public SPWorkflowSubListSerializableData mTaskListUnit;
        public SPWorkflowSubListSerializableData mHistListUnit;
        public SPWorkflowSubListSerializableData mTemplateLibUnit;

        public object mSerializableCustomData;
        public string mParentId;
        public string mOriginalParentId;
        public bool mParentIsWebContentType;
        public bool mIsDefaultContentApprovalWorkflow;
        public bool mIsNintexReusableWorkflow;
		public bool mIsNintexSiteCollectionReusableWorklfow;

        public bool isReusableWrokflow;

        public Dictionary<string, SPFieldSerializableData> mIssueTrackingRefFields;
        private List<SPWFAssociationSerializableData> mChildUnits;
        public List<SPWFAssociationSerializableData> ChildUnits
        {
            get
            {
                if (mChildUnits == null)
                    mChildUnits = new List<SPWFAssociationSerializableData>();
                return mChildUnits;
            }
        }
        private Hashtable mProperties;
        public Hashtable Properties
        {
            get
            {
                if (mProperties == null)
                    mProperties = new Hashtable(StringComparer.OrdinalIgnoreCase);
                return mProperties;
            }

        }
        public void SetPropsFromDataRow(DataRow dr, DataColumnCollection columns)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetPropsFromDataRow");
            try
            {
                int fieldCount = columns.Count;

                StringBuilder b1 = new StringBuilder();
                foreach (DataColumn column in columns)
                {
                    if (dr.IsNull(column))
                        continue;
                    b1.Remove(0, b1.Length);
                    b1.Append("#");
                    b1.Append(column.ColumnName);
                    this.Properties.AddEx(b1.ToString(), dr[column]);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetPropertiesFromReaderException, e.Message);
                logger.Warn("An exception occurred while get properties from reader. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataRow");
            }
        }

        public byte[] ExportFileUnit { get; set; }
    }
}
