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


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

#region moved to wrapper contract
//namespace AvePoint.Wrapper.Restore
//{
//    public enum WFAssociationConflictType
//    {
//        None = 0,
//        Template,
//        #region For future use
//        Configuration,
//        #endregion
//        Same,
//    }

//    public enum WFAssociationConflictResolutionOption
//    {
//         <summary>
//         association不会被restore
//         </summary>
//        NotOverwrite,
//         <summary>
//         重新命名association，直到不冲突。命名规则是: [backed up association name]_[number]
//         </summary>
//        Append,
//         <summary>
//         如果目的端association上没有workflow instance，则将目的端association删除后再restore；如果有instance，则skip
//         </summary>
//        Overwrite,
//         <summary>
//         无论目的端的association是否有instance，都先将其删除，然后重新restore；
//         </summary>
//        ForceOverwrite,
//         <summary>
//         不会删除目的端association，但会更新目的端association的一些配置属性；
//         </summary>
//        UpdateOverwrite,
//         <summary>
//         这个option是为instance所用。当还原instance时，如果其parent association没有被还原，
//         将在这个过程中重新还原parent association。为了使instance能够还原回去，
//         要保证其parent association不会被其本身的冲突规则再次skip掉，因此加了这个Option。
//         </summary>
//        ForceUse
//    }

//    public enum WFInstanceConflictResolutionOption
//    {
//        NotOverwrite,
//        Overwrite,
//        OverwriteByModifiedTime
//    }

//    public enum WorkflowTypeFilter
//    {
//        SPBuiltIn,
//        SPD,
//        VS
//    }

//}
#endregion