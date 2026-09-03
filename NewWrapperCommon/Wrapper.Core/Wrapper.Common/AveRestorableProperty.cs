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
using System.Text;
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    [DataContract]
    public class AveRestorableProperty<T>
    {
        [DataMember]
        private T mValue;
        [DataMember]
        private bool mIsAvailable;

        public AveRestorableProperty()
        {

        }

        public AveRestorableProperty(T value)
        {
            this.mValue = value;
            this.mIsAvailable = true;
        }

        public bool IsAvailable
        {
            get
            {
                return this.mIsAvailable;
            }
        }

        public T Value
        {
            get
            {
                if (!this.IsAvailable)
                {
                    return default(T);
                }
                return this.mValue;
            }
        }
        public T GetValueOrDefault()
        {
            return this.mValue;
        }

        public T GetValueOrDefault(T defaultValue)
        {
            if (!this.IsAvailable)
            {
                return defaultValue;
            }
            return this.mValue;
        }

        public override bool Equals(object obj)
        {
            AveRestorableProperty<T> other = obj as AveRestorableProperty<T>;
            if (other == null)
            {
                return false;
            }
            if (this.IsAvailable && other.IsAvailable)
            {
                if (this.Value == null)
                {
                    return other.Value == null;
                }
                return this.Value.Equals(other.Value);
            }
            return this.IsAvailable == other.IsAvailable;
        }

        public override int GetHashCode()
        {
            if (!this.IsAvailable)
            {
                return 0;
            }
            return this.mValue.GetHashCode();
        }

        public override string ToString()
        {
            if (!this.IsAvailable)
            {
                return string.Empty;
            }
            return this.mValue.ToString();
        }

        public static implicit operator AveRestorableProperty<T>(T value)
        {
            return new AveRestorableProperty<T>(value);
        }

        public static explicit operator T(AveRestorableProperty<T> value)
        {
            return value.Value;
        }

    }

    public static class AveRestorablePropertyExtension
    {
        public static void SafeSetValue<T>(this T self, AveRestorableProperty<T> value)
        {
            RunIfAvailable(self, value, null, () => self = value.Value);
        }

        public static void SafeSetValue<T>(this T self, AveRestorableProperty<T> value, Func<T, bool> predicate)
        {
            RunIfAvailable(self, value, predicate, () => self = value.Value);
        }

        private static void RunIfAvailable<T>(this T self, AveRestorableProperty<T> value, Func<T, bool> predicate, Action action)
        {
            if (value != null && value.IsAvailable &&
                (predicate == null || predicate(self)))
            {
                if (action != null)
                {
                    action();
                }
            }
        }
    }
}
