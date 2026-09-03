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



namespace AvePoint.Wrapper.Common
{
    public enum AveWrapperErrorCode
    {
        UnKnown = 0x0,
        #region Reserved 0x1~0xFFF

        #endregion
        #region Common 0x1000~0x1FFF
        FileContentLengthDismatch = 0x1001,
        #endregion
        #region Workflow 0x2000~0x2FFF
        WorkflowRestoreError = 0x2000,
        //Definition
        WorkflowDefinitionConflict = 0x2001,
        DefinitionConflictResolutionOptionInvalid = 0x2002,
        //Instance
        ParentDefinitionCannotFind = 0x2801,
        #endregion

        #region Field 0x3000~0x3FFF
        FieldRestoreError = 0x3000,
        TextFieldPropertyCannotFind = 0x3001,
        TextFieldCannotFind = 0x3002,
        FieldHandleConflictError = 0x3003,
        CreateFieldError = 0x3004,
        CreateFieldByCustomMappingError = 0x3005,
        #endregion

        #region Content Type 0x4000~0x4FFF
        ContentTypeHandleConflictError = 0x4001,
        #endregion

        #region feature 0x5000~0x5FFF
        FeatureRestoreError = 0x5000,
        FeatureNotInstall=0x5001,
        #endregion
    }
}
