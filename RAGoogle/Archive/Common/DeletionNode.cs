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
using System.Xml;

namespace RAGoogle.Archive
{
    internal class DeletionNode
    {

        private readonly bool isValid;
        private readonly char objectType;
        private readonly Guid listId;
        private readonly XmlElement headerInfo;
        private readonly XmlDocument document;
        private readonly string fullPath;

        public bool IsValid { get { return isValid; } }
        public char ObjectType { get { return objectType; } }
        public Guid ListId { get { return listId; } }
        public XmlElement HeaderInfo { get { return headerInfo; } }
        public XmlDocument Document { get { return document; } }

        public string FullPath { get { return fullPath; } }

        public string SPId { get { return HeaderInfo.GetAttribute("spId"); } }   //SAAS-12437 添加这个属性未分组提供方便。

        public DeletionNode(string header)
        {
            if (string.IsNullOrEmpty(header)
                || header.Equals("notEnd", StringComparison.OrdinalIgnoreCase)
                || header.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                isValid = false;
            }
            else
            {

                isValid = true;
            }
        }
    }
}