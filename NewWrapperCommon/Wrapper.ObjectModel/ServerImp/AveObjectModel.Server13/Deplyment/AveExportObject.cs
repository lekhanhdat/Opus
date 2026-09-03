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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Deployment;

namespace AvePoint.ObjectModel.Server13
{
    class AveExportObject : IAveExportObject
    {
        private SPExportObject mExportObject;

        public AveExportObject(Guid objId, AveDeploymentObjectType objType, Guid parentObjId, bool excludeChildren)
        {
            mExportObject = new SPExportObject(objId, (SPDeploymentObjectType)objType, parentObjId, excludeChildren);
        }

        public AveExportObject()
        {
            mExportObject = new SPExportObject();
        }

        public string Url
        {
            get
            {
                return mExportObject.Url;
            }
            set
            {
                mExportObject.Url = value;
            }
        }

        internal SPExportObject ExportObject
        {
            get
            {
                return this.mExportObject;
            }
        }

        public bool ExcludeChildren
        {
            get;
            set;
        }

        public AveIncludeDescendants IncludeDescendants
        {
            get
            {
                return (AveIncludeDescendants)mExportObject.IncludeDescendants;
            }
            set
            {
                mExportObject.IncludeDescendants = (SPIncludeDescendants)value;
            }
        }

        public Guid Id
        {
            get
            {
                return mExportObject.Id;
            }
            set
            {
                mExportObject.Id = value;
            }
        }

        public AveDeploymentObjectType Type
        {
            get
            {
                return (AveDeploymentObjectType)mExportObject.Type;
            }
            set
            {
                mExportObject.Type = (SPDeploymentObjectType)value;
            }
        }
    }
}
