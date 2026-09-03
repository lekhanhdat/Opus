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
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeFileHeader
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public long ReceiveTime { get; set; }
        //记录folder的类型
        [DataMember]
        public Int32 NodeType { get; set; }

        /// <summary>
        /// 每个节点之间以(Char)0x12分割
        /// </summary>
        [DataMember]
        public String ParentFullPath { get; set; }

        [DataMember]
        public ExchangeDataType DataType { get; set; }

        /// <summary>
        /// 0 means normal data, 1 means deleted data
        /// </summary>
        [DataMember]
        public Int32 BackupType { get; set; }

        [DataMember]
        public string DisplayTo { get; set; }
        [DataMember]
        public string Sender { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public long SendDate { get; set; }
        [DataMember]
        public bool HasAttach { get; set; }
        [DataMember]
        public string Path { get; set; }
        public string ParentName { get; set; }
        public int ChildCount { get; set; }

        public string ItemName { get; set; }

        public ExchangeFileHeader() { }
        public ExchangeFileHeader(String fileHeaderXml)
        {
            var docment = new XmlDocument();
            docment.LoadXml(fileHeaderXml);
            var rootElement = docment.DocumentElement;
            if (rootElement.HasAttribute(EXOCommonWord.Name))
            {
                this.Name = rootElement.GetAttribute(EXOCommonWord.Name);
            }
            if (rootElement.HasAttribute(EXOCommonWord.ParentFullPath))
            {
                this.ParentFullPath = rootElement.GetAttribute(EXOCommonWord.ParentFullPath);
            }
            if (rootElement.HasAttribute(EXOCommonWord.NodeType))
            {
                this.NodeType = int.Parse(rootElement.GetAttribute(EXOCommonWord.NodeType));
            }
            if (rootElement.HasAttribute(EXOCommonWord.DataType))
            {
                this.DataType = (ExchangeDataType)int.Parse(rootElement.GetAttribute(EXOCommonWord.DataType));
            }
            if (rootElement.HasAttribute(EXOCommonWord.DisplayTo))
            {
                this.DisplayTo = rootElement.GetAttribute(EXOCommonWord.DisplayTo);
            }
            if (rootElement.HasAttribute(EXOCommonWord.Sender))
            {
                this.Sender = rootElement.GetAttribute(EXOCommonWord.Sender);
            }
            if (rootElement.HasAttribute(EXOCommonWord.Category))
            {
                this.Category = rootElement.GetAttribute(EXOCommonWord.Category);
            }
            if (rootElement.HasAttribute(EXOCommonWord.SendDate))
            {
                this.SendDate = long.Parse(rootElement.GetAttribute(EXOCommonWord.SendDate));
            }
            if (rootElement.HasAttribute(EXOCommonWord.HasAttach))
            {
                this.HasAttach = bool.Parse(rootElement.GetAttribute(EXOCommonWord.HasAttach));
            }
            if (rootElement.HasAttribute(EXOCommonWord.Path))
            {
                this.Path = rootElement.GetAttribute(EXOCommonWord.Path);
            }
            docment.RemoveAll();
        }
        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Exchange File Header: ");
            stringBuilder.AppendFormat("Name: {0}, ", this.Name);
            stringBuilder.AppendFormat("Path: {0}, ", this.ParentFullPath);
            stringBuilder.AppendFormat("DataType: {0}, ", this.DataType);
            stringBuilder.AppendFormat("BackupType: {0}, ", this.BackupType);
            stringBuilder.AppendFormat("NodeType: {0}, ",this.NodeType);
            return stringBuilder.ToString();
        }
    }
}
