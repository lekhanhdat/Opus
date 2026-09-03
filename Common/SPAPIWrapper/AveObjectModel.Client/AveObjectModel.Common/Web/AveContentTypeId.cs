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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    public struct AveContentTypeId : IAveContentTypeId
    {
        private const int c_cbGuid = 0x10;
        private static readonly AveContentTypeId m_emptyId;
        private static readonly byte[] m_nullByteArray;
        private static readonly AveContentTypeId m_rootId;
        private byte[] m_rgb;
        private int m_iHash;

        internal AveContentTypeId(byte[] rgb)
        {
            this.m_rgb = rgb;
            this.m_iHash = AveSPUtility.HexStringFromBytes(this.m_rgb).GetHashCode();
        }

        public AveContentTypeId(string id)
        {
            if (id == null)
            {
                throw new ArgumentException();
            }
            if ((id.Length % 2) != 0)
            {
                throw new ArgumentException();
            }
            char[] chArray = id.ToCharArray();
            if (((chArray.Length < 2) || (chArray[0] != '0')) || (chArray[1] != 'x'))
            {
                throw new ArgumentException();
            }
            int index = 2;
            int num2 = (chArray.Length - index) / 2;
            byte[] buffer = null;
            if (num2 > 0)
            {
                int num3 = 0;
                buffer = new byte[num2];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)((AveSPUtility.Hex(chArray[index]) << 4) | AveSPUtility.Hex(chArray[index + 1]));
                    index += 2;
                    if (num3 > 0)
                    {
                        num3--;
                    }
                    else if (buffer[i] == 0)
                    {
                        num3 = 0x10;
                    }
                }
                if (num3 > 0)
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                buffer = new byte[0];
            }
            this.m_rgb = buffer;
            this.m_iHash = AveSPUtility.HexStringFromBytes(this.m_rgb).GetHashCode();
        }

        public static AveContentTypeId Empty
        {
            get
            {
                return m_emptyId;
            }
        }
        internal static AveContentTypeId Root
        {
            get
            {
                return m_rootId;
            }
        }
        internal AveContentTypeId CreateChild(byte b)
        {
            if (b == 0)
            {
                return this.CreateChildFromGuid(Guid.NewGuid());
            }
            byte[] destinationArray = new byte[this.Length + 1];
            if (this.m_rgb != null)
            {
                Array.Copy(this.m_rgb, destinationArray, this.Length);
            }
            destinationArray[this.Length] = b;
            return new AveContentTypeId(destinationArray);
        }

        internal AveContentTypeId CreateChildFromGuid(Guid g)
        {
            byte[] destinationArray = new byte[(this.Length + 1) + 0x10];
            if (this.m_rgb != null)
            {
                Array.Copy(this.m_rgb, destinationArray, this.Length);
            }
            destinationArray[this.Length] = 0;
            Array.Copy(g.ToByteArray(), 0, destinationArray, this.Length + 1, 0x10);
            return new AveContentTypeId(destinationArray);
        }

        public IAveContentTypeId Parent
        {
            get
            {
                int length = 0;
                for (int i = 0; i < this.Length; i++)
                {
                    length = i;
                    if (this.m_rgb[i] == 0)
                    {
                        i += 0x10;
                    }
                }
                byte[] destinationArray = null;
                if (length > 0)
                {
                    destinationArray = new byte[length];
                    Array.Copy(this.m_rgb, destinationArray, length);
                }
                return new AveContentTypeId(destinationArray);
            }
        }
        public bool IsChildOf(IAveContentTypeId iid)
        {
            AveContentTypeId id = (AveContentTypeId)iid;
            if (this.Length < id.Length)
            {
                return false;
            }
            for (int i = 0; i < id.Length; i++)
            {
                if (id.m_rgb[i] != this.m_rgb[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsChildOf(string contentTypeId)
        {
            return IsChildOf(new AveContentTypeId(contentTypeId));
        }

        public bool IsParentOf(AveContentTypeId id)
        {
            return id.IsChildOf(this);
        }

        public static AveContentTypeId FindCommonParent(AveContentTypeId id1, AveContentTypeId id2)
        {
            int num = Math.Min(id1.Length, id2.Length);
            for (int i = 0; i < num; i++)
            {
                if (id1.m_rgb[i] != id2.m_rgb[i])
                {
                    num = i;
                    break;
                }
                if (id1.m_rgb[i] == 0)
                {
                    int num3 = Math.Min((i + 0x10) + 1, num);
                    for (int j = i + 1; j < num3; j++)
                    {
                        if (id1.m_rgb[j] != id2.m_rgb[j])
                        {
                            num = i;
                            break;
                        }
                    }
                    i += 0x10;
                }
            }
            byte[] destinationArray = null;
            if (num > 0)
            {
                destinationArray = new byte[num];
                Array.Copy(id1.m_rgb, destinationArray, num);
            }
            return new AveContentTypeId(destinationArray);
        }

        public override string ToString()
        {
            return AveSPUtility.HexStringFromBytes(this.m_rgb);
        }

        public override int GetHashCode()
        {
            return this.m_iHash;
        }

        public int CompareTo(AveContentTypeId id)
        {
            if (object.ReferenceEquals(null, id))
            {
                return 1;
            }
            int num = Math.Min(this.Length, id.Length);
            for (int i = 0; i < num; i++)
            {
                int num3 = this.m_rgb[i] - id.m_rgb[i];
                if (num3 != 0)
                {
                    return num3;
                }
            }
            return (this.Length - id.Length);
        }

        public override bool Equals(object o)
        {
            return (((o != null) && (o is AveContentTypeId)) && (this.CompareTo((AveContentTypeId)o) == 0));
        }

        int IComparable.CompareTo(object o)
        {
            if (!(o is AveContentTypeId))
            {
                throw new ArgumentException();
            }
            return this.CompareTo((AveContentTypeId)o);
        }

        public static bool operator ==(AveContentTypeId id1, AveContentTypeId id2)
        {
            if (object.ReferenceEquals(null, id1))
            {
                return object.ReferenceEquals(null, id2);
            }
            return (id1.CompareTo(id2) == 0);
        }

        public static bool operator <=(AveContentTypeId id1, AveContentTypeId id2)
        {
            return (object.ReferenceEquals(null, id1) || (id1.CompareTo(id2) <= 0));
        }

        public static bool operator >=(AveContentTypeId id1, AveContentTypeId id2)
        {
            if (object.ReferenceEquals(null, id1))
            {
                return object.ReferenceEquals(null, id2);
            }
            return (id1.CompareTo(id2) >= 0);
        }

        public static bool operator !=(AveContentTypeId id1, AveContentTypeId id2)
        {
            return !(id1 == id2);
        }

        public static bool operator <(AveContentTypeId id1, AveContentTypeId id2)
        {
            return (id1 < id2);
        }

        public static bool operator >(AveContentTypeId id1, AveContentTypeId id2)
        {
            return (id1 > id2);
        }

        internal int Length
        {
            get
            {
                if (this.m_rgb != null)
                {
                    return this.m_rgb.Length;
                }
                return 0;
            }
        }
        internal int Generations
        {
            get
            {
                if (this.m_rgb == null)
                {
                    return 0;
                }
                int index = 0;
                int num2 = 0;
                while (index < this.m_rgb.Length)
                {
                    if (this.m_rgb[index] != 0)
                    {
                        index++;
                    }
                    else
                    {
                        index += 0x11;
                    }
                    num2++;
                }
                return num2;
            }
        }
        internal byte[] ToByteArray()
        {
            return this.m_rgb;
        }

        internal int CountCommonBytes(AveContentTypeId id)
        {
            byte[] rgb = this.m_rgb;
            byte[] buffer2 = id.m_rgb;
            int index = 0;
            while ((index < this.Length) && (index < id.Length))
            {
                if (rgb[index] != buffer2[index])
                {
                    return index;
                }
                index++;
            }
            return index;
        }

        internal bool IsFile
        {
            get
            {
                return this.IsChildOf(AveBuiltInContentTypeId.Document);
            }
        }
        internal bool IsFolder
        {
            get
            {
                return this.IsChildOf(AveBuiltInContentTypeId.Folder);
            }
        }
        internal bool IsNonDiscussionFolder
        {
            get
            {
                return (this.IsFolder && !this.IsChildOf(AveBuiltInContentTypeId.Discussion));
            }
        }
        internal bool IsItem
        {
            get
            {
                return (!this.IsFile && !this.IsFolder);
            }
        }
        internal bool CanBeFileOrFolder
        {
            get
            {
                int length = ((AveContentTypeId)Parent).Length;
                return ((((length == 0) || this.IsChildOf(AveBuiltInContentTypeId.Document)) || this.IsChildOf(AveBuiltInContentTypeId.Folder)) || (((length % 0x11) == 0) && ((AveContentTypeId)Parent).CanBeFileOrFolder));
            }
        }
        public static AveContentTypeId BestMatch(AveContentTypeId contentTypeId, IEnumerable contentTypeIdCollection)
        {
            AveContentTypeId root = Root;
            int num = 0;
            foreach (AveContentType contentType in contentTypeIdCollection)
            {
                AveContentTypeId id2 = (AveContentTypeId)contentType.Id;
                int num2 = id2.CountCommonBytes(contentTypeId);
                if (num2 > num)
                {
                    root = id2;
                    num = num2;
                }
                else if ((num2 == num) && (id2.Length < root.Length))
                {
                    root = id2;
                }
            }
            return root;
        }

        static AveContentTypeId()
        {
            m_emptyId = new AveContentTypeId("0x00C2208B8CE6E1422CADC1C521EAB2A68B");
            m_nullByteArray = null;
            m_rootId = new AveContentTypeId(m_nullByteArray);
        }
    }
}
