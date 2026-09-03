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
using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOFormTemplate : AvePersistedObject, IAveOFormTemplate
    {
        private const string mFormTemplate_SolutionVersions_Member = "SolutionVersions";
        private const string mFormTemplate_OriginalFileName_Member = "OriginalFileName";
        private const string mFormTemplate_PhysicalFileName_Member = "PhysicalFileName";
        private Collection<string> mDataConnectionFileReferences;
        private FormTemplate mFormTemplate;
        private Dictionary<string, string> mProperties;

        public AveOFormTemplate(FormTemplate formTemplate)
            : base(formTemplate)
        {
            mFormTemplate = formTemplate;
        }

        internal FormTemplate FormTemplate
        {
            get
            {
                return mFormTemplate;
            }
        }

        #region IAveFormTemplate Members

        public AveQuiesceMode QuiesceStatus
        {
            get
            {
                return (AveQuiesceMode)mFormTemplate.QuiesceStatus;
            }
        }

        public string Category
        {
            get
            {
                return mFormTemplate.Category;
            }
            set
            {
                mFormTemplate.Category = value;
            }
        }

        public DateTime CreatedTimeUtc
        {
            get
            {
                return mFormTemplate.CreatedTimeUtc;
            }
        }

        public Collection<string> DataConnectionFileReferences
        {
            get
            {
                if (mDataConnectionFileReferences == null)
                {
                    mDataConnectionFileReferences = mFormTemplate.DataConnectionFileReferences;
                }
                return mDataConnectionFileReferences;
            }
        }

        public string Description
        {
            get
            {
                return mFormTemplate.Description;
            }
        }

        public override string DisplayName
        {
            get
            {
                return mFormTemplate.DisplayName;
            }
        }

        public Guid FeatureId
        {
            get
            {
                return mFormTemplate.FeatureId;
            }
        }

        public string FormId
        {
            get
            {
                return mFormTemplate.FormId;
            }
        }

        public AveFormTemplateState FormTemplateStatus
        {
            get
            {
                return (AveFormTemplateState)mFormTemplate.FormTemplateStatus;
            }
        }

        public bool FullTrust
        {
            get
            {
                return mFormTemplate.FullTrust;
            }
        }

        public DateTime ModifiedTimeUtc
        {
            get
            {
                return mFormTemplate.ModifiedTimeUtc;
            }
        }

        public bool Signed
        {
            get
            {
                return mFormTemplate.Signed;
            }
        }

        public Guid SolutionId
        {
            get
            {
                return mFormTemplate.SolutionId;
            }
        }

        public bool WorkflowEnabled
        {
            get
            {
                return mFormTemplate.WorkflowEnabled;
            }
        }

        public string PhysicalFileName
        {
            get
            {
                return Convert.ToString(AveAssemblyUtility.GetPropertyValue(mFormTemplate, mFormTemplate_PhysicalFileName_Member));
            }
        }

        public string OriginalFileName
        {
            get
            {
                return Convert.ToString(AveAssemblyUtility.GetPropertyValue(mFormTemplate, mFormTemplate_OriginalFileName_Member));
            }
        }

        public string SolutionVersions
        {
            get
            {
                return Properties[mFormTemplate_SolutionVersions_Member];
            }
        }

        public DateTime QuiesceEndTimeUtc
        {
            get
            {
                return (DateTime)AveAssemblyUtility.GetPropertyValue(mFormTemplate, "QuiesceEndTimeUtc");
            }
        }

        #endregion

        public void Activate(IAveSite site)
        {
            mFormTemplate.Activate((site as AveSite).Site);
        }

        public void Deactivate(IAveSite site)
        {
            mFormTemplate.Deactivate((site as AveSite).Site);
        }

        public void Quiesce(TimeSpan maxDuration)
        {
            mFormTemplate.Quiesce(maxDuration);
        }

        public void Unquiesce()
        {
            mFormTemplate.Unquiesce();
        }

        public new Dictionary<string, string> Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = AveAssemblyUtility.GetFieldValue(mFormTemplate, "_properties") as Dictionary<String, String>;
                }
                return mProperties;
            }
        }
    }
}
