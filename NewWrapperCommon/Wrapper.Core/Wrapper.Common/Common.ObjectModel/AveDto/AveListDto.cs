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

namespace AvePoint.Wrapper.Common
{
    public class AveListDto : IAveObjectDto, IComparable<IAveObjectDto>, IComparable<AveListDto>
    {
        private Guid mId;
        private string mName;
        private AveObjectType mType;
        private AveListTemplateType mTemplateType;
        private List<AveItemDto> mItems;

        public string Name
        {
            get { return this.mName; }
            set { this.mName = value; }
        }

        public Guid ID
        {
            get { return this.mId; }
            set { this.mId = value; }
        }

        public AveObjectType Type
        {
            get { return this.mType; }
            set { this.mType = value; }
        }

        public AveListTemplateType TemplateType
        {
            get { return this.mTemplateType; }
            set { this.mTemplateType = value; }
        }

        public List<AveItemDto> Items
        {
            get { return this.mItems; }
            set { this.mItems = value; }
        }

        #region IComparable<IAveObjectDto> Members

        public int CompareTo(IAveObjectDto other)
        {
            AveListDto listDto = other as AveListDto;
            if (listDto == null)
            {
                return mType - other.Type;
            }
            return CompareTo(listDto);
        }

        public int CompareTo(AveListDto other)
        {
            return string.Compare(mName, other.mName,StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
