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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common.Office;
using SPDisposeCheck;
using System.Collections;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOProfileValueCollectionBase : AveAbstractCommonCollection, IAveOProfileValueCollectionBase
    {
        private ProfileValueCollectionBase mProfileValueCollectionBase;
        private AveOUserProfile mUserProfile;
        private AveOProperty mProperty;

        public AveOProfileValueCollectionBase(AveOUserProfile userProfile,ProfileValueCollectionBase profileValueCollectionBase)
            : base(profileValueCollectionBase)
        {
            mProfileValueCollectionBase = profileValueCollectionBase;
            mUserProfile = userProfile;
        }

        /// <summary>
        /// 目前只发现一种特殊类型：SPTimeZone
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected object ConvertAveToSP(object o)
        {
            if (o is AveTimeZone)
            {
                return (o as AveTimeZone).ID;
            }
            else
            {
                return o;
            }
        }

        /// <summary>
        /// 目前只发现一种特殊类型：SPTimeZone
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected object ConvertSPToAve(object o)
        {
            if (o is SPTimeZone)
            {
                return new AveTimeZone(o as SPTimeZone);
            }
            else
            {
                return o;
            }
        }

        #region IAveOProfileValueCollectionBase Members

        public IAveOProperty Property
        {
            get
            {
                if (mProperty == null)
                {
                    mProperty = new AveOProperty(mProfileValueCollectionBase.Property);
                }
                return mProperty;
            }
        }

        public int Capacity
        {
            get
            {
                return mProfileValueCollectionBase.Capacity;
            }
            set
            {
                mProfileValueCollectionBase.Capacity = value;
            }
        }

        public AvePrivacy Privacy
        {
            get
            {
                return (AvePrivacy)mProfileValueCollectionBase.Privacy;
            }
            set
            {
                mProfileValueCollectionBase.Privacy = (Privacy)value;
            }
        }

        //目前该方法中返回值类型为string，故直接返回，如果将来需要返回其他非基本类型值，需要在下面方法中特殊判断
        public object this[int index]
        {
            get
            {
                object obj = mProfileValueCollectionBase[index];
                if (obj == null)
                {
                    return null;
                }
                return ConvertSPToAve(obj);
            }
        }

        public void Clear()
        {
            mProfileValueCollectionBase.Clear();
        }

        //目前该方法中参数类型为string，故直接使用，如果将来需要其他非基本类型的参数，需要在下面方法中特殊判断
        public void Add(object o)
        {
            object obj = ConvertAveToSP(o);
            mProfileValueCollectionBase.Add(obj);
        }

        public object Value
        {
            get
            {
                return mProfileValueCollectionBase.Value;
            }
            set
            {
                mProfileValueCollectionBase.Value = value;
            }
        }

        /// <summary>
        /// 目前该属性返回SP对象，只用于判断是否为null，不能做其他处理
        /// </summary>
        public object ProfileSubtypeProperty
        {
            get { return mProfileValueCollectionBase.ProfileSubtypeProperty; }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return mProfileValueCollectionBase.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return false;
            }
        }

        #endregion

        internal override object CreatElementInstance(object obj)
        {
            return obj;
        }
    }
}
