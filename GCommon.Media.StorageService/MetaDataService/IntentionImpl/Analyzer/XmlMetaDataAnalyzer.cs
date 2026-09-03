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
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

    #endregion using directives

    internal class XmlMetaDataAnalyzer
        : MetaDataAnalyzerBase
    {
        public override MetaData Analyze(Byte[] metaData)
        {
            HoldServiceMetaData metaDataInfo = new HoldServiceMetaData() { MetadataInfo = new HashSet<MetaDataItemInfo>() };
            string xmlContent = Encoding.UTF8.GetString(metaData);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            XmlNode targetNode = GetTargetNode(doc);
            GetMetaDataInfo(metaDataInfo, targetNode);
            return metaDataInfo;
        }

        private void GetMetaDataInfo(HoldServiceMetaData metaDataInfo, XmlNode targetNode)
        {
            Dictionary<String, String> dic = new Dictionary<String, String>();
            foreach (XmlNode node in targetNode.ChildNodes)
            {
                if (node.Name.EqualsIgnoreCase("metaDataItem"))
                {
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes[i].Name.EqualsIgnoreCase("key"))
                        {
                            dic[node.Attributes[i].Value] = node.Attributes[i + 1].Value;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes[i].Name.EqualsIgnoreCase("key"))
                        {
                            MetaDataItemInfo metaDataItemInfo = new MetaDataItemInfo();
                            metaDataItemInfo.Name = node.Attributes[i].Value;
                            metaDataItemInfo.Value = node.Attributes[i + 1].Value;
                            metaDataItemInfo.ItemType = Type.GetType(node.Attributes[i + 2].Value);
                            metaDataInfo.MetadataInfo.Add(metaDataItemInfo);
                            break;
                        }
                    }
                }
            }
            IntegrateMetaData(metaDataInfo, dic);
        }

        private XmlNode GetTargetNode(XmlNode parentNode)
        {
            XmlNode targetNode = default(XmlNode);
            if (parentNode.ChildNodes.Count != 0)
            {
                foreach (XmlNode childNode in parentNode.ChildNodes)
                {
                    if (childNode.Name.Equals("metaData"))
                        targetNode = childNode;
                    else
                        targetNode = GetTargetNode(childNode);
                }
            }
            return targetNode;
        }

        private void IntegrateMetaData(HoldServiceMetaData metaDataInfo, Dictionary<String, String> dic)
        {
            long size = 0;
            long.TryParse(dic["ContentSize"], out size);
            metaDataInfo.ContentSize = size; 
            metaDataInfo.Title = dic["Title"];
            metaDataInfo.CreatedBy = dic["CreatedBy"];
            metaDataInfo.ModifiedBy = dic["ModifiedBy"];
            metaDataInfo.CreatedTime = dic["CreatedTime"];
            metaDataInfo.ModifiedTime = dic["ModifiedTime"];
            metaDataInfo.SPUrl = dic["SPUrl"];
            metaDataInfo.TimeZoneInfoId = dic["TimeZoneInfoId"];
        }
    }
}