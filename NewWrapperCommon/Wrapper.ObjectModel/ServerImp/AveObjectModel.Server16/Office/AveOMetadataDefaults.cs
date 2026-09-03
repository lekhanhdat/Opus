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



using Microsoft.Office.DocumentManagement;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMetadataDefaults : IAveOMetadataDefaults
    {
        private MetadataDefaults mMetadataDefaults;

        public AveOMetadataDefaults(MetadataDefaults metadataDefaults)
        {
            mMetadataDefaults = metadataDefaults;
        }

        public AveOMetadataDefaults(IAveList list)
        {
            mMetadataDefaults = new MetadataDefaults((list as AveList).List);
        }

        #region IAveMetadataDefaults Members

        public string GetFieldDefault(string folderPath, string fieldName)
        {
            return mMetadataDefaults.GetFieldDefault(folderPath, fieldName);
        }

        public string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFieldDefault(string folderPath, string fieldName)
        {
            return mMetadataDefaults.RemoveFieldDefault(folderPath, fieldName);
        }

        public bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath)
        {
            throw new NotImplementedException();
        }

        public bool SetFieldDefault(string folderPath, string fieldName, string value)
        {
            return mMetadataDefaults.SetFieldDefault(folderPath, fieldName, value);
        }

        public bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string value)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            mMetadataDefaults.Update();
        }

        #endregion
    }
}
