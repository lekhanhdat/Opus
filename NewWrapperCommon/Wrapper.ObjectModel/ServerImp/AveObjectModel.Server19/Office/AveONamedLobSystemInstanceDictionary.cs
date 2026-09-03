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



namespace AvePoint.ObjectModel.Server19.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.ApplicationRegistry.MetadataModel;
    using System.Collections;
    #endregion

    class AveONamedLobSystemInstanceDictionary : IAveONamedLobSystemInstanceDictionary
    {
        private NamedLobSystemInstanceDictionary mNamedLobSystemInstanceDictionary;
        private Dictionary<string, IAveOLobSystemInstance> mLobSystemInstanceDictionary;

        public AveONamedLobSystemInstanceDictionary(NamedLobSystemInstanceDictionary namedLobSystemInstanceDictionary)
        {
            mNamedLobSystemInstanceDictionary = namedLobSystemInstanceDictionary;
            mLobSystemInstanceDictionary = new Dictionary<string, IAveOLobSystemInstance>();
            foreach (KeyValuePair<string, LobSystemInstance> lobSystemInstance in namedLobSystemInstanceDictionary)
            {
                mLobSystemInstanceDictionary.Add(lobSystemInstance.Key, new AveOLobSystemInstance((LobSystemInstance)lobSystemInstance.Value));
            }
        }

        #region IDictionary<string,IAveLobSystemInstance> Members

        public void Add(string key, IAveOLobSystemInstance value)
        {
            mLobSystemInstanceDictionary.Add(key, value);
            mNamedLobSystemInstanceDictionary.Add(key, (value as AveOLobSystemInstance).LobSystemInstance);
        }

        public bool ContainsKey(string key)
        {
            return mLobSystemInstanceDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get
            {
                return mLobSystemInstanceDictionary.Keys;
            }
        }

        public bool Remove(string key)
        {
            mNamedLobSystemInstanceDictionary.Remove(key);
            return mLobSystemInstanceDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out IAveOLobSystemInstance value)
        {
            return mLobSystemInstanceDictionary.TryGetValue(key, out value);
        }

        public ICollection<IAveOLobSystemInstance> Values
        {
            get
            {
                return mLobSystemInstanceDictionary.Values;
            }
        }

        public IAveOLobSystemInstance this[string key]
        {
            get
            {
                return mLobSystemInstanceDictionary[key];
            }
            set
            {
                mLobSystemInstanceDictionary[key] = value;
                mNamedLobSystemInstanceDictionary[key] = (value as AveOLobSystemInstance).LobSystemInstance;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,IAveLobSystemInstance>> Members

        public void Add(KeyValuePair<string, IAveOLobSystemInstance> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            mLobSystemInstanceDictionary.Clear();
            mNamedLobSystemInstanceDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, IAveOLobSystemInstance> item)
        {
            return mLobSystemInstanceDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IAveOLobSystemInstance>[] array, int arrayIndex)
        {
            if (array == null)
            { throw new ArgumentNullException("array"); }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            { throw new ArgumentOutOfRangeException("arrayIndex"); }

            if ((array.Length - arrayIndex) < mLobSystemInstanceDictionary.Count)
            { throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex."); }

            foreach (KeyValuePair<string, IAveOLobSystemInstance> item in mLobSystemInstanceDictionary)
            {
                array[arrayIndex++] = item;
            }
        }

        public int Count
        {
            get
            {
                return mLobSystemInstanceDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, IAveOLobSystemInstance> item)
        {
            if (!this.Contains(item))
            {
                return false;
            }
            return this.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,IAveLobSystemInstance>> Members

        public IEnumerator<KeyValuePair<string, IAveOLobSystemInstance>> GetEnumerator()
        {
            return mLobSystemInstanceDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
