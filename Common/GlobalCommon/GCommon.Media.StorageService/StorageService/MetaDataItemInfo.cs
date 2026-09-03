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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives
    using System;

    #endregion

    public class MetaDataItemInfo
    {
        public String Name { get; set; }
        public String Value { get; set; }
        public Type ItemType { get; set; }
        public Boolean IsShowable { get; set; }

        public MetaDataItemInfo()
        { }

        public MetaDataItemInfo(
            String name,
            String value,
            Type itemType)
        {
            this.Name = name;
            this.Value = ((char)(9)).ToString() + value;
            this.ItemType = itemType;
        }

        public MetaDataItemInfo(
           String name,
           String value,
           Type itemType,
           Boolean isShowable)
            : this(name, value, itemType)
        {
            this.IsShowable = isShowable;
        }

        // override object.Equals
        public override Boolean Equals(Object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            var realObj = obj as MetaDataItemInfo;
            return this.Name.EqualsIgnoreCase(realObj.Name)
                && this.ItemType == realObj.ItemType;
        }

        // override object.GetHashCode
        public override Int32 GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return (this.Name + this.ItemType.FullName).GetHashCode();
        }
    }
}