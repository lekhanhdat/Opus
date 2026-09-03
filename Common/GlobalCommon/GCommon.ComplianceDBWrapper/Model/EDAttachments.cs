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
using System.Collections;
using System.Collections.Generic;
using AvePoint.GCommon.ComplianceDBWrapper.Model;

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDAttachments : IList<EDAttachment>
    {
        private List<EDAttachment> _attachments;

        #region - List 扩展功能 -

        public EDAttachments()
        {
            _attachments = new List<EDAttachment>();
        }


        public IEnumerator<EDAttachment> GetEnumerator()
        {
            return _attachments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _attachments.GetEnumerator();
        }

        public void Add(EDAttachment item)
        {
            _attachments.Add(item);
        }

        public void Clear()
        {
           _attachments.Clear();
        }

        public bool Contains(EDAttachment item)
        {
            return _attachments.Contains(item);
        }

        public void CopyTo(EDAttachment[] array, int arrayIndex)
        {
           _attachments.CopyTo(array,arrayIndex);
        }

        public bool Remove(EDAttachment item)
        {
           return _attachments.Remove(item);
        }

        public int Count
        {
            get { return _attachments.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(EDAttachment item)
        {
            return _attachments.IndexOf(item);
        }

        public void Insert(int index, EDAttachment item)
        {
            _attachments.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            _attachments.RemoveAt(index);
        }

        public EDAttachment this[int index]
        {
            get { return _attachments[index]; }
            set { _attachments[index] = value; }
        }

        #endregion
    }
}
