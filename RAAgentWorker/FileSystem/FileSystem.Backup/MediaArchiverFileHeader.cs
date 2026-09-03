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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.RA.CommonUtil;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MediaArchiverFileHeader
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MediaArchiverFileHeader));

        [DataMember]
        public AveSharePointType Type { get; set; }

        [DataMember]
        public String Path { get; set; }

        [DataMember]
        public String Url { get; set; }

        [DataMember]
        public Int32 IsMyProfileList { get; set; }

        [DataMember]
        public String ListBaseType { get; set; }

        //[DataMember]
        //public MediaArchiverFileHeaderType FileHeaderType { get; set; }

        [DataMember]
        public String HeaderExtraAttribute { get; set; }

        [DataMember]
        public Boolean IsAppData { get; set; }
        [DataMember]
        public String AppDataName { get; set; }
        [DataMember]
        public long CreateTime { get; set; }
        [DataMember]
        public long ModifyTime { get; set; }

        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string Editor { get; set; }

        [DataMember]
        public string StubInfo { get; set; }
        [DataMember]
        public string Extra { get; set; }

        [DataMember]
        public string NodeGuid { get; set; }

        [DataMember]
        public String BackupFileType { get; set; }

        public MediaArchiverFileHeader(String fileHeaderXml)
        {
            var docment = new XmlDocument();
            docment.LoadXml(fileHeaderXml);
            var rootElement = docment.DocumentElement;
            if (rootElement.HasAttribute("isMyProfileList"))
            {
                this.IsMyProfileList = Convert.ToInt32(rootElement.GetAttribute("isMyProfileList"));
            }
            if (rootElement.HasAttribute("type"))
            {
                this.Type = (AveSharePointType)(char.Parse(rootElement.GetAttribute("type").ToUpper()));
            }
            if (rootElement.HasAttribute("path"))
            {
                this.Path = rootElement.GetAttribute("path");
            }
            if (rootElement.HasAttribute("fullPath"))
            {
                this.Url = rootElement.GetAttribute("fullPath");
            }
            if (rootElement.HasAttribute("listBaseType"))
            {
                this.ListBaseType = rootElement.GetAttribute("listBaseType");
            }
            //if (rootElement.HasAttribute("fileHeaderType"))
            //{
            //    this.FileHeaderType = (MediaArchiverFileHeaderType)int.Parse(rootElement.GetAttribute("fileHeaderType"));
            //}

            if (rootElement.HasAttribute("isAppData"))
            {
                this.IsAppData = Boolean.Parse(rootElement.GetAttribute("isAppData"));
            }
            if (rootElement.HasAttribute("appDataName"))
            {
                this.AppDataName = rootElement.GetAttribute("appDataName");
            }
            if (rootElement.HasAttribute("Created"))
            {
                try
                {
                    this.CreateTime = Convert.ToInt64(rootElement.GetAttribute("Created"));
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse Created, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("Modified"))
            {
                try
                {
                    this.ModifyTime = Convert.ToInt64(rootElement.GetAttribute("Modified"));
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse Modified, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("Author"))
            {
                try
                {
                    this.Author = rootElement.GetAttribute("Author");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse Author, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("Editor"))
            {
                try
                {
                    this.Editor = rootElement.GetAttribute("Editor");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse Editor, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("stubInfo"))
            {
                try
                {
                    this.StubInfo = rootElement.GetAttribute("stubInfo");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse stubInfo, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("nodeGuid"))
            {
                try
                {
                    this.NodeGuid = rootElement.GetAttribute("nodeGuid");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse nodeGuid, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("BackupFileType"))
            {
                try
                {
                    this.BackupFileType = rootElement.GetAttribute("BackupFileType");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse BackupFileType, ex:{e}");
                }
            }
            if (rootElement.HasAttribute("extra"))
            {
                try
                {
                    this.Extra = rootElement.GetAttribute("extra");
                }
                catch (Exception e)
                {
                    logger.Warn($@"fail parse extra, ex:{e}");
                }
            }
            XmlNodeList nodeList = rootElement.GetElementsByTagName("HeaderExtraAttribute");
            if (nodeList != null && nodeList.Count > 0)
            {
                this.HeaderExtraAttribute = nodeList[0].OuterXml;
            }
            else
            {
                this.HeaderExtraAttribute = string.Empty;
            }
            docment.RemoveAll();
        }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Media Archiver File Header: ");
            stringBuilder.AppendFormat("Type: {0}, ", this.Type);
            stringBuilder.AppendFormat("Path: {0}, ", this.Path);
            stringBuilder.AppendFormat("Url: {0}, ", this.Url);
            //stringBuilder.AppendFormat("File Header Type: {0}, ", this.FileHeaderType);
            stringBuilder.AppendFormat("Header Extra Attribute: {0}", this.HeaderExtraAttribute);
            return stringBuilder.ToString();
        }
    }
}
