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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.DocumentManagement.Internal;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveODocIdUiSettings : IAveODocIdUiSettings
    {
        private DocIdUiSettings mDocIdUiSettings;
        private const string mDocIdUiSettings_Type = "Microsoft.Office.DocumentManagement.Internal.DocIdUiSettings";

        public AveODocIdUiSettings(DocIdUiSettings docIdUiSettings)
        {
            mDocIdUiSettings = docIdUiSettings;
        }

        public AveODocIdUiSettings()
        { }

        public AveODocIdUiSettings(bool assignmentEnabled, string prefix)
        {
            mDocIdUiSettings = new DocIdUiSettings(assignmentEnabled, prefix);
        }

        internal DocIdUiSettings docIdUiSettings
        {
            get
            {
                return mDocIdUiSettings;
            }
        }

        public bool AssignmentEnabled
        {
            get
            {
                return mDocIdUiSettings.AssignmentEnabled;
            }
            set
            {
                mDocIdUiSettings.AssignmentEnabled = value;
            }
        }

        public string Prefix
        {
            get
            {
                return mDocIdUiSettings.Prefix;
            }
            set
            {
                mDocIdUiSettings.Prefix = value;
            }
        }

        public IAveODocIdUiSettings Load(IAveSite site)
        {
            object obj = AveAssemblyUtility.InvokeStaticMethod(mDocIdUiSettings_Type, "Load", new object[] { (site as AveSite).Site });
            if (obj == null)
            {
                return null;
            }
            return new AveODocIdUiSettings((DocIdUiSettings)obj);
        }

        public override bool Equals(object obj)
        {
            return (this.CompareTo(obj as IAveODocIdUiSettings) == 0);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is IAveODocIdUiSettings))
            {
                return 1;
            }
            return mDocIdUiSettings.CompareTo((obj as AveODocIdUiSettings).docIdUiSettings);
        }
    }
}
