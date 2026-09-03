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
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public static class AveRestoreProtocolAnalyzer
    {
        public const string PROTOCOL_TAIL_BREAK = "Break";
        public const string PROTOCOL_TAIL_END = "End";
        public const char   PROTOCOL_HEADER_END = '1';
        public const string PROTOCOL_HEADER_TYPE = "type";
        public const string PROTOCOL_HEADER_PATH = "path";

        public static AveRestoreProtocolHeader AnalyzeHeader(string headerStr)
        {
            AveRestoreProtocolHeader protocolHeader = null;
            if (headerStr != null)            
            {
                protocolHeader = new AveRestoreProtocolHeader();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(headerStr);
                XmlNode headerNode = doc.FirstChild;
                protocolHeader.Type = Convert.ToChar(headerNode.Attributes[PROTOCOL_HEADER_TYPE].Value);
                protocolHeader.Path = headerNode.Attributes[PROTOCOL_HEADER_PATH].Value;
            }
            return protocolHeader;
        }

        public static bool IsRestoreEnd(AveRestoreProtocolHeader header)
        {
            return header.Type == PROTOCOL_HEADER_END;
        }

        public static bool IsRestoreEnd(string tailStr)
        {
            return PROTOCOL_TAIL_END.Equals(GetFileEndNode(tailStr).FirstChild.Name);
        }

        public static bool IsBreak(string tailStr)
        {
            return PROTOCOL_TAIL_BREAK.Equals(GetFileEndNode(tailStr).FirstChild.Name);
        }       

        internal static XmlNode GetFileEndNode(string tailStr)
        {
            //"<FileTail length=\"625\"><Break/></FileTail>"
            XmlDocument doc = new XmlDocument();            
            doc.LoadXml(tailStr);
            return doc.FirstChild;
        }
    }
}
