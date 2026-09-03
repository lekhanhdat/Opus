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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Global.Object
{
    public class RMSharePointOnPremiseSetting 
    {
        public int Id { set; get; }
    
        public Guid ScopeId { set; get; }

        public string ColumnName { get; set; }

        public Guid FieldId { set; get; }

        public string FullPath { get; set; }

        public Guid SiteGroupId { set; get; }

        public Guid SiteId { set; get; }

        public Guid WebId { set; get; }

        public Guid ListId { set; get; }
      
        public Guid FolderId { set; get; }

      
        public Guid TermStoreId { set; get; }


        public Guid TermSetId { set; get; }

  
        public Guid TermId { set; get; }


        public Guid DefaultTermId { set; get; }

 
        public string TermSetName { get; set; }


        public string Description { get; set; }


        public string TermName { get; set; }


        public string DefaultTermName { get; set; }

        public string DescriptionOfContainer { get; set; }

        public string TermNameOfContainer { get; set; }
     
        public Guid TermIdOfContainer { set; get; }
   
        public bool isEnableClassification { get; set; }

        public bool isFailedConfigClassification { get; set; }
  
        public bool isFailedConfigMetaDataColumn { get; set; }

        public bool IsEnableHoldPhyical { get; set; }

        public string ExistColumnName { get; set; }

        public bool IsUsingExistColumnName { get; set; }

        public bool SetDocLevelTermForExistColumn { get; set; }

        #region use this for quick config custom setting.

        public bool HaveConfigSetting { get; set; }//to do lock this setting for get job node

        public long SettingTime { get; set; }//update the datetime
   
        public string NodeInfo { get; set; }
        #endregion
 
        public bool NeedCheckDefaultValue { get; set; }

  
        public bool EMailToRecordOwner { get; set; }


        public bool IsDisplyaTermPath { set; get; }
 
        public int ApplyExistType { get; set; }

 
        public bool IsRemoved { set; get; }

     
        public bool EnableRelatedRecords { set; get; }

      
        public int EnableRecordManagement { get; set; }


        public bool IncludeDeclaredRecords { get; set; }

   
        public bool? ColumnRequired { set; get; }

        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string CollectionJobId1 { get; set; }
        /// <summary>
        ///  ！！！该属性要使用GroupLevel中的
        /// </summary>

        public bool? IsShowUniqueId { set; get; }
        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string DisposalJobId1 { get; set; }
        //[Column(TypeName = "nvarchar(max)")]
        //public string IdPath { get; set; }
        //[Column(TypeName = "bit")]
        public bool IsRunning { get; set; }


        public bool IsNewEdited { get; set; }

        public string SharePointSettingJobId { get; set; }


        public string AutoClassificationRules { get; set; }


        public int DeployTermMethod { get; set; }


        public int AutoJobOption { get; set; }


        public bool RunAutoFullJob { get; set; }


        public bool IsSyncData { set; get; }
    }
}
