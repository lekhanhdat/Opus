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

namespace AvePoint.ObjectModel.Server19
{
    public class AveFieldLink : IAveFieldLink
    {
        private SPFieldLink mFieldLink;

        public AveFieldLink(SPFieldLink fieldLink)
        {
            mFieldLink = fieldLink;
        }

        public AveFieldLink(IAveField field)
        {
            mFieldLink = new SPFieldLink((field as AveField).Field);
        }


        internal SPFieldLink FieldLink
        {
            get
            {
                return mFieldLink;
            }
        }

        #region IAveFieldLink Members

        public string DisplayName
        {
            get
            {
                return mFieldLink.DisplayName;
            }
            set
            {
                mFieldLink.DisplayName = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return mFieldLink.Hidden;
            }
            set
            {
                mFieldLink.Hidden = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mFieldLink.Id;
            }
        }

        public string Name
        {
            get
            {
                return mFieldLink.Name;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return mFieldLink.ReadOnly;
            }
            set
            {
                mFieldLink.ReadOnly = value;
            }
        }

        public bool Required
        {
            get
            {
                return mFieldLink.Required;
            }
            set
            {
                mFieldLink.Required = value;
            }
        }

        public void Delete()
        {

        }

        public string XPath
        {
            get
            {
                return mFieldLink.XPath;
            }
            set
            {
                mFieldLink.XPath = value;
            }
        }

        public string AggregationFunction
        {
            get
            {
                return mFieldLink.AggregationFunction;
            }
            set
            {
                mFieldLink.AggregationFunction = value;
            }
        }

        public string SchemaXml
        {
            get { return mFieldLink.SchemaXml; }
        }

        public string Customization
        {
            get
            {
                return mFieldLink.Customization;
            }
            set
            {
                mFieldLink.Customization = value;
            }
        }

        public string PIAttribute
        {
            get
            {
                return mFieldLink.PIAttribute;
            }
            set
            {
                mFieldLink.PIAttribute = value;
            }
        }

        public string PITarget
        {
            get
            {
                return mFieldLink.PITarget;
            }
            set
            {
                mFieldLink.PITarget = value;
            }
        }

        public string PrimaryPIAttribute
        {
            get
            {
                return mFieldLink.PrimaryPIAttribute;
            }
            set
            {
                mFieldLink.PrimaryPIAttribute = value;
            }
        }

        public string PrimaryPITarget
        {
            get
            {
                return mFieldLink.PrimaryPITarget;
            }
            set
            {
                mFieldLink.PrimaryPITarget = value;
            }
        }

        public bool ShowInDisplayForm
        {
            get
            {
                return mFieldLink.ShowInDisplayForm;
            }
            set
            {
                mFieldLink.ShowInDisplayForm = value;
            }
        }

        #endregion
    }
}
