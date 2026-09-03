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



/********************************************************************
*                   Jersey City, NJ 07311
*                   United States of America
*                   Telephone: +1-800-661-6588
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
*  Copyright © 2001-2011 AvePoint® Inc. All Rights Reserved. 
*
*  Unpublished - All rights reserved under the copyright laws of the United States.
*  $Revision: 253196 $
*  $Author: ccnetreport $        
*  $Date: 2013-12-27 14:27:02 +0800 (Fri, 27 Dec 2013) $
*/
namespace AvePoint.GCommon.Contract.CommonFilter
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StubLastAccessTimeRule : PolicyRuleBase
    {
    }
}