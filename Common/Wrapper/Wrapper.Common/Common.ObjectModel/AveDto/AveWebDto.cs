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

namespace AvePoint.Wrapper.Common
{
    public class AveWebDto : IAveObjectDto, IComparable<IAveObjectDto>, IComparable<AveWebDto>
    {
        public const string ROOT_NAME = ".";

        private Guid mId;
        private string mName;
        private string mTitle;
        private AveObjectType mType;

        public AveWebDto()
        {
            mType = AveObjectType.Web;
        }

        public string Title
        {
            get { return this.mTitle; }
            set { this.mTitle = value; }
        }

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

        #region IComparable<IAveObjectDto> Members

        public int CompareTo(IAveObjectDto other)
        {
            AveWebDto webDto = other as AveWebDto;
            if (webDto == null)
            {
                return mType - other.Type;
            }
            return CompareTo(webDto);
        }

        public int CompareTo(AveWebDto other)
        {
            if (ROOT_NAME.Equals(mName))
            {
                if (ROOT_NAME.Equals(other.mName))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            if (ROOT_NAME.Equals(other.mName))
            {
                return -1;
            }
            return string.Compare(mName, other.mName,StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
