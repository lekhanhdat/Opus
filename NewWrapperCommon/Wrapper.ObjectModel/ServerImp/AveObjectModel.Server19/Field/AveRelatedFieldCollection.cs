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
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19
{
    internal class AveRelatedFieldCollection : AveAbstractCommonCollection<IAveRelatedField>, IAveRelatedFieldCollection
    {
        private AveList mList;
        private SPRelatedFieldCollection m_RelatedFieldCollection;

        public AveRelatedFieldCollection(AveList list, SPRelatedFieldCollection fields)
            : base(fields)
        {
            mList = list;
            m_RelatedFieldCollection = fields;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveRelatedField(t as SPRelatedField);
        }

        public override int Count
        {
            get { return this.m_RelatedFieldCollection.Count; }
        }
    }

    internal class AveRelatedField : IAveRelatedField
    {
        private SPRelatedField m_RelatedFieldField;

        public AveRelatedField(SPRelatedField field)
        {
            m_RelatedFieldField = field;
        }

        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior
        {
            get
            {
                return (AveRelationshipDeleteBehavior)this.m_RelatedFieldField.RelationshipDeleteBehavior;
            }
        }

        public Guid ListId
        {
            get
            {
                return this.m_RelatedFieldField.ListId;
            }
        }

        public Guid FieldId
        {
            get
            {
                return this.m_RelatedFieldField.FieldId;
            }
        }
    }
}
