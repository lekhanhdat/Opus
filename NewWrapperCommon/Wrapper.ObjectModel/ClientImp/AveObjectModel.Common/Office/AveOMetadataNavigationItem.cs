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

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOMetadataNavigationItem : AveClientObject, IAveOMetadataNavigationItem
    {
        public Guid FieldId
        {
            get { throw new NotImplementedException(); }
        }

        public string FieldDisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public string FieldTitle
        {
            get { throw new NotImplementedException(); }
        }

        public Wrapper.Common.AveFieldType FieldType
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSupportedType
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsContentTypeField
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsTaxonomyField
        {
            get { throw new NotImplementedException(); }
        }

        public void InitializeForContentType(string displayName)
        {
            throw new NotImplementedException();
        }

        public void InitializeForFolder()
        {
            throw new NotImplementedException();
        }

        public void InitializeFromSPField(Wrapper.Common.IAveField metaDataField)
        {
            throw new NotImplementedException();
        }

        public string FieldTypeAsString
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOSupportedFieldsLookup SupportedFields
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SpecialFieldIdContentType
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SpecialFieldIdFolder
        {
            get { throw new NotImplementedException(); }
        }

        public Wrapper.Common.IAveField TryGetFieldObject(Wrapper.Common.IAveFieldCollection sourceFieldCollection)
        {
            throw new NotImplementedException();
        }

        public Wrapper.Common.IAveField TryGetFieldObject(Wrapper.Common.IAveWeb web, Guid listId)
        {
            throw new NotImplementedException();
        }

        public void UpdateToMatchSPField(Wrapper.Common.IAveField matchingField)
        {
            throw new NotImplementedException();
        }
    }
}
