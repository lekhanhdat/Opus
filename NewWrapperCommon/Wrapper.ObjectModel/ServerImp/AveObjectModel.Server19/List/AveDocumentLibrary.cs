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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveDocumentLibrary : AveList, IAveDocumentLibrary
    {
        private SPDocumentLibrary mDocumentLibrary;
        private IList<IAveCheckedOutFile> mCheckedOutFiles;

        protected SPDocumentLibrary innerDocumentLibrary
        {
            get
            {
                if (mDocumentLibrary.Version != this.List.Version)
                {
                    mDocumentLibrary = (SPDocumentLibrary)this.List;
                }

                return mDocumentLibrary;
            }
        }

        public AveDocumentLibrary(AveListCollection lists,SPDocumentLibrary documentLibrary)
            : base(lists, documentLibrary)
        {
            mDocumentLibrary = documentLibrary;
        }

        public AveDocumentLibrary(AveListCollection lists, SPList list)
            : base(lists, list)
        {
            mDocumentLibrary = (SPDocumentLibrary)list;
        }

        #region IAveDocumentLibrary Members

        public bool ThumbnailsEnabled
        {
            get
            {
                return innerDocumentLibrary.ThumbnailsEnabled;
            }
            set
            {
                innerDocumentLibrary.ThumbnailsEnabled = value;
            }
        }

        public int ThumbnailSize
        {
            get
            {
                return innerDocumentLibrary.ThumbnailSize;
            }
            set
            {
                innerDocumentLibrary.ThumbnailSize = value;
            }
        }

        public string DocumentTemplateUrl 
        {
            get
            {
                return innerDocumentLibrary.DocumentTemplateUrl; 
            }
            set
            {
                innerDocumentLibrary.DocumentTemplateUrl = value;
            }
        }

        public IList<IAveCheckedOutFile> CheckedOutFiles
        {
            get
            {
                if (mCheckedOutFiles == null)
                {
                    mCheckedOutFiles = new List<IAveCheckedOutFile>();
                    foreach (SPCheckedOutFile checkedOutFile in innerDocumentLibrary.CheckedOutFiles)
                    {
                        mCheckedOutFiles.Add(new AveCheckedOutFile(base.ParentWeb as AveWeb, checkedOutFile));
                    }
                }
                return mCheckedOutFiles;
            }
        }

        public void UpdateSPDocumentLibrary()
        {
            innerDocumentLibrary.Update();
        }

        public void TakeOverCheckedOutFile(string serverRelativeUrl)
        {
            AveAssemblyUtility.InvokeMethod(mDocumentLibrary, "TakeOverCheckedOutFile", new Type[] {typeof (string)}, new object[] {serverRelativeUrl});
        }

        #endregion

        public override void Reload()
        {
            base.Reload();
            mDocumentLibrary = (SPDocumentLibrary)base.List;
        }

        public string ServerRelativeDocumentTemplateUrl
        {
            get { return mDocumentLibrary.ServerRelativeDocumentTemplateUrl; }
        }

        public int WebImageHeight
        {
            get
            {
                return mDocumentLibrary.WebImageHeight;
            }
            set
            {
                mDocumentLibrary.WebImageHeight = value;
            }
        }

        public int WebImageWidth
        {
            get
            {
                return mDocumentLibrary.WebImageWidth;
            }
            set
            {
                mDocumentLibrary.WebImageWidth = value;
            }
        }
    }
}
