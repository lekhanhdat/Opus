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
    #region using diectives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOActionControlDto
    {
        [DataMember]
        public String Id { get; set; }                   //主键                     *
       
        [DataMember]
        public String FarmName { get; set; }             //farm的名称      *
      
        [DataMember]
        public String FarmID { get; set; }               //farm的GUID for content
       
        [DataMember]
        public String WebAppURL { get; set; }            //web application URL for content
        
        [DataMember]
        public String WebAppID { get; set; }             //web application的GUID   for content
       
        [DataMember]
        public String ContentDBName { get; set; }        //content database名称 for content
        
        [DataMember]
        public String ContentDBID { get; set; }          //content database的GUID for content
       
        [DataMember]
        public String SiteURL { get; set; }              //site collection URL        *
        
        [DataMember]
        public String SiteID { get; set; }               //site collection的GUID      *
       
        [DataMember]
        public String JobID { get; set; }                //如果具有子job，这里存储的应该是子jobId
       
        [DataMember]
        public MediaArchiverJobType Type { get; set; }   //类型 [0 backup * 1 restore * 2 full text index * 3 retention * 4 sync deletion * 5 export]    *
       
        [DataMember]
        public Int32 Category { get; set; }              //数据类型区分[Archive,Vault...]
       
        [DataMember]
        public Int32 Status { get; set; }                //状态 [0 running * 1 finished]                *
        
        [DataMember]
        public String AgentHost { get; set; }            //操作数据的agent  for content
       
        [DataMember]
        public String MediaHost { get; set; }            //操作数据的media
        
        [DataMember]
        public Int32 SpVersion { get; set; }             //SharePoint版本 [0 SharePoint2003 * 2 SharePoint2007 * 4 SharePoint2010]  for content
        
        [DataMember]
        public Int32 StorageType { get; set; }           //EBS或RBS状态[1 EBS * 2 RBS] for content
        
        [DataMember]
        public Int64 UpdateTime { get; set; }            //更新时间

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SO Action Control DTO: ");
            stringBuilder.AppendFormat("Id: {0}, ", this.Id);
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Job ID: {0}, ", this.JobID);
            stringBuilder.AppendFormat("Type: {0}, ", this.Type);
            stringBuilder.AppendFormat("Agent Host: {0}, ", this.AgentHost);
            stringBuilder.AppendFormat("Media Host: {0}", this.MediaHost);
            return stringBuilder.ToString();
        }
    }
}
