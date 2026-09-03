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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Global.JobMessage
{
    public class OnPremRealtimeJobMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public string JobId { set; get; }        
        [DataMember(EmitDefaultValue = false)]
        public RealTimeAction Action { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public ChangeTermOption ChangeTermOption { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> DeclareIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DeclaredBy { get; set; }
    }

  
    public enum RealTimeAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ChangeTerm = 1,
        [EnumMember]
        Declare = 2,
        [EnumMember]
        UnDeclare = 3,
        [EnumMember]
        PhysicalMove = 4,
        [EnumMember]
        GlobalSearchAction = 5,
    }

   
    public class ChangeTermOption
    {
        //[DataMember(EmitDefaultValue = false)]
        //public List<Guid> SourceRecordIds { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public List<Guid> SourceEXORecordIds { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public List<Guid> SourcePhyRecordIds { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public List<Guid> SourceFSRecordIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string LogonUser { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceSPOnPremRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TargetTermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TargetTermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TargetTermUniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool OverWriteSubFiles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }
    }
}
