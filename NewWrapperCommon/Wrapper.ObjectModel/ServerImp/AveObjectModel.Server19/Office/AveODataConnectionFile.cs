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
using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveODataConnectionFile : AvePersistedObject, IAveODataConnectionFile
    {
        private DataConnectionFile mDataConnectionFile;

        public AveODataConnectionFile()
            : this(new DataConnectionFile())
        { }

        public AveODataConnectionFile(DataConnectionFile dataConnectionFile)
            : base(dataConnectionFile)
        {
            mDataConnectionFile = dataConnectionFile;
        }

        public string Category
        {
            get
            {
                return mDataConnectionFile.Category;
            }
            set
            {
                mDataConnectionFile.Category = value;
            }
        }

        public string Description
        {
            get
            {
                return mDataConnectionFile.Description;
            }
            set
            {
                mDataConnectionFile.Description = value;
            }
        }

        public new string DisplayName
        {
            get
            {
                return mDataConnectionFile.DisplayName;
            }
            set
            {
                mDataConnectionFile.DisplayName = value;
            }
        }

        public bool HasDependants
        {
            get { return mDataConnectionFile.HasDependants; }
        }

        public Guid Id
        {
            get
            {
                return mDataConnectionFile.Id;
            }
            set
            {
                mDataConnectionFile.Id = value;
            }
        }

        public string Name
        {
            get
            {
                return mDataConnectionFile.Name;
            }
            set
            {
                mDataConnectionFile.Name = value;
            }
        }

        public bool WebAccessible
        {
            get
            {
                return mDataConnectionFile.WebAccessible;
            }
            set
            {
                mDataConnectionFile.WebAccessible = value;
            }
        }

        public string Xml
        {
            get { return mDataConnectionFile.Xml; }
        }

        public string[] EnumerateDependants()
        {
            return mDataConnectionFile.EnumerateDependants();
        }
    }
}
