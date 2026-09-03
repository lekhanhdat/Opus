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

namespace LS.SPWorkflowProcessor.SerializableObjects
{
    [Flags]
    public enum SPContentTypeScope
    {
        List,
        Web,
    }

    [Serializable]
    public class SPContentTypeSerializableData
    {
        public SPContentTypeSerializableData mParentData = null;
        public string mDescription;
        public string mDisplayFormTemplateName;
        public string mDisplayFormUrl;
        /// <summary>
        /// if the form exist in the current web,we should backup the web url,so that we can replace the form url in restore
        /// </summary>
        public string mDisplayFormWebUrl;
        public string mDocumentTemplate;
        public string mEditFormTemplateName;
        public string mEditFormUrl;
        /// <summary>
        /// if the form exist in the current web,we should backup the web url,so that we can replace the form url in restore
        /// </summary>
        public string mEditFormWebUrl;
        public string mGroup;
        public string mName;
        public string mOriginalName;
        public string mNewDocumentControl;
        public string mNewFormTemplateName;
        public string mNewFormUrl;
        /// <summary>
        /// if the form exist in the current web,we should backup the web url,so that we can replace the form url in restore
        /// </summary>
        public string mNewFormWebUrl;

        public bool mHidden;
        public bool mReadOnly;
        public bool mRequireClientRenderingOnNew;
        public bool mSealed;

        //custom fields

        /// <summary>
        /// if the ct exists in the same parent web of backed up/restored ct's, the level is 0
        /// if the ct exists in the parent web of backed up/restored ct parent web, the level is 1
        /// and so on.
        /// </summary>
        public int mLevel;
        /// <summary>
        /// current ct index is 0, and parent ct index is 1, and parent parent ct index is 2, and so on
        /// </summary>
        public int mIndex;
        /// <summary>
        /// 
        /// </summary>
        public SPContentTypeScope mParentScope;

        //readonly fields
        public string mSchemaXml;
        public string mScope;
        public string mId;
        public string mNewId;

        //if workflow form is a xsn form, it will store xsn address in content type xmldocument collection
        //like 
        //<WorkflowForm xmlns="http://schemas.microsoft.com/sharepoint/v4/workflow/forms">~site/Workflows/SPDWorkflowDemo1/Task0.xsn</WorkflowForm>
        //<WorkflowFormData xmlns="http://schemas.microsoft.com/sharepoint/v4/workflow/formdata"><dfs:myFields xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:dms="http://schemas.microsoft.com/office/2009/documentManagement/types" xmlns:dfs="http://schemas.microsoft.com/office/InfoPath/2003/dataFormSolution" xmlns:q="http://schemas.microsoft.com/office/infopath/2009/WSSList/queryFields" xmlns:d="http://schemas.microsoft.com/office/infopath/2009/WSSList/dataFields" xmlns:ma="http://schemas.microsoft.com/office/2009/metadata/properties/metaAttributes" xmlns:pc="http://schemas.microsoft.com/office/infopath/2007/PartnerControls" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><dfs:queryFields /><dfs:dataFields><d:SharePointListItem_RW><d:FieldName_ToDoTitle>Task0</d:FieldName_ToDoTitle></d:SharePointListItem_RW></dfs:dataFields></dfs:myFields></WorkflowFormData>
        public List<string> mXmlDocuments = new List<string>();
        public int mInternalVersion;

        public List<SPWorkflowSubFileSerializableData> ResourceFolderFiles = new List<SPWorkflowSubFileSerializableData>();

        public void Dispose()
        { 
            mParentData = null;
            mDescription = null;
            mDisplayFormTemplateName = null;
            mDisplayFormUrl = null;
            mDisplayFormWebUrl = null;
            mDocumentTemplate = null;
            mEditFormTemplateName = null;
            mEditFormUrl = null;
            mEditFormWebUrl = null;
            mGroup = null;
            mName = null;
            mOriginalName = null;
            mNewDocumentControl = null;
            mNewFormTemplateName = null;
            mNewFormUrl = null;
            mNewFormWebUrl = null;


            mSchemaXml = null;
            mScope = null;
            mId = null;
            mNewId = null;


            mXmlDocuments.Clear();
        }
    }
}
