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
using System.Linq;
using System.Text;
using AvePoint.ObjectModel.ClientOM;
using System.IO;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveAttachmentRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAttachmentRestore));
        private string mWebRelativeUrl;
        private string mListTitle;
        private Guid mListId;
        private int mRowId;
        private string mAttachmentLeafName;
        private int mAttachmentSize;
        private bool mEnalbeVersioning;
        private AveClientOMRequest mRequest;
        private static object lockObj = new object();
        public AveAttachmentRestore(AveClientOMRequest request)
        {
            mRequest = request;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mWebRelativeUrl = data["WebUrl"] as string;
            mListTitle = data["ListTitle"] as string;
            mListId = new Guid(data["ListId"] as string);
            mRowId = Convert.ToInt32(data["DoclibRowId"]);
            mAttachmentLeafName = data["Name"] as string;
            mAttachmentSize = Convert.ToInt32(data["Size"]);
            mEnalbeVersioning = Convert.ToBoolean(data["EnableVersioning"]);
        }

        public Dictionary<string, object> RestoreAttachment(Dictionary<string, object> docData, Stream fileStream)
        {

            Dictionary<string, object> attachmentProperties;

            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                PrepareRestoreContext(docData);
                lock (lockObj)
                {
                    mRequest.DisableListVersion(mWebRelativeUrl, mListTitle, mListId, mEnalbeVersioning);
                    var tempContent = new byte[fileStream.Length];
                    fileStream.Read(tempContent, 0, tempContent.Length);
                    //byte[] tempContent = new Guid("1c834929-4f7d-4ac5-a3a7-a13fd8578ec7").ToByteArray();
                    attachmentProperties = mRequest.AddAttachmentNow(mWebRelativeUrl, mListTitle, mListId, mRowId, mAttachmentLeafName, tempContent);
                    mRequest.RevertListVersion(mWebRelativeUrl, mListTitle, mListId, mEnalbeVersioning); 
                }
            }

            return attachmentProperties;
        }

        public void Dispose()
        {
        }
    }
}
