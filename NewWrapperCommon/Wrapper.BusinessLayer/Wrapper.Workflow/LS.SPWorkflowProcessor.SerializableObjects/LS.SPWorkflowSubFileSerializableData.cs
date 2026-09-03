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
    [Serializable]
    public class SPWorkflowSubFileSerializableData
    {
        public string mName;
        public string mDirName;
        public string mLeafName;
        public string mSetupPath;
        public string mCharSetName;
        /// <summary>
        /// example "/NintexWorkflows/test/test.xoml.wfconfig.xml","/Workflows/test/test.xoml.wfconfig.xml"
        /// </summary>
        public string mListRelativeUrl;
        /// <summary>
        /// example "/test/test.xoml.wfconfig.xml"
        /// </summary>
        public string mRootFolderRelativeUrl;
        /// <summary>
        /// example "/test/test.xoml.wfconfig.xml"
        /// </summary>
        public string mFirstParentFolderRelativeUrl;
        public string mParentFolderName;

        public Guid mUniqueId;

        public int mItemId;
        public int mDocFlags;
        public int mUIVersion;
        public int mVersion;
        public DateTime mModified;
        public DateTime mCreated;
        public int mAuthorId;
        public string  mAuthorLogin;
        public int mEditorId;
        public string mEditorLogin;

        public bool mIsCurrentVersion;
        public bool mHasStream;

        public string ipfs_streamhash;

        public byte[] mContent;

        public string mTemplateLibTitle;

        public Dictionary<string, string> mGUIDDictionary;

        public SPWorkflowSubFileSerializableData()
        {
            mGUIDDictionary = new Dictionary<string, string>();
        }

        public void Dispose()
        {
            mGUIDDictionary.Clear();
        }
        public string mCategorySchemalXml;
    }
}
