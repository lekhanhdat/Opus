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

namespace AvePoint.Wrapper.Common
{
    public class AveBackupOption
    {
        #region Metadata backup options
        private bool mBackupRelatedTermsOnly;
        private bool mBackupRelatedTermSets;
        private bool mBackupInheritedTheme;
        private bool mBackupInheritedNavigation;
        private bool mBackupNintexForm = true;
        private bool mBackupContentTypeByAPI = true;
        private bool mBackupContentTypeDocumentTemplateFile = true;
        private bool mBackupWebpartPropertiesForOffice365 = true;
        private bool mBackupMetadataNavigation = true;
        private bool mBackupMultiLanguage = false;

        public bool BackupMultiLanguage
        {
            get { return mBackupMultiLanguage; }
            set { mBackupMultiLanguage = value; }
        }
        public bool BackupRelatedTermsOnly
        {
            get { return mBackupRelatedTermsOnly; }
            set { mBackupRelatedTermsOnly = value; }
        }
        public bool BackupRelatedTermSets
        {
            get { return mBackupRelatedTermSets; }
            set { mBackupRelatedTermSets = value; }
        }
        public bool BackupInheritedTheme
        {
            get { return mBackupInheritedTheme; }
            set { mBackupInheritedTheme = value; }
        }

        public bool BackupInheritedNavigation
        {
            get { return mBackupInheritedNavigation; }
            set { mBackupInheritedNavigation = value; }
        }

        public bool BackupNintexForm
        {
            get { return mBackupNintexForm; }
            set { mBackupNintexForm = value; }
        }

        public bool BackupMetadataNavigation
        {
            get { return mBackupMetadataNavigation; }
            set { mBackupMetadataNavigation = value; }
        }
        /// <summary>
        /// Backup Lookup value GUID
        /// </summary>
        public bool BackupItemTPGUIDofLookupValue { get; set; }

        /// <summary>
        /// Remove Available User
        /// </summary>
        public bool RemoveAvailableUser { get; set; }

        public bool BackupContentTypeByAPI
        {
            get { return mBackupContentTypeByAPI; }
            set { mBackupContentTypeByAPI = value; }
        }

        public bool BackupContentTypeDocumentTemplateFile
        {
            get { return mBackupContentTypeDocumentTemplateFile; }
            set { mBackupContentTypeDocumentTemplateFile = value; }
        }

        public bool BackupWebpartPropertiesForOffice365
        {
            get { return mBackupWebpartPropertiesForOffice365; }
            set { mBackupWebpartPropertiesForOffice365 = value; }
        }

        public Action<AveFieldCollectionInfo> BeforeExportFieldsAction { get; set; }
        public Action<AveContentTypeCollectionInfo> BeforeExportContentTypesAction { get; set; }

        #endregion
    }
}
