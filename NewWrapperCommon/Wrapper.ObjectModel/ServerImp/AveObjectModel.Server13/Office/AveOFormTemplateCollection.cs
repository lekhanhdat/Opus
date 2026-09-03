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
using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOFormTemplateCollection : AvePersistedChildCollection<IAveOFormTemplate>, IAveOFormTemplateCollection
    {
        private FormTemplateCollection mFormTemplateCollection;

        public AveOFormTemplateCollection(FormTemplateCollection formTemplates)
            : base(formTemplates)
        {
            mFormTemplateCollection = formTemplates;
        }

        public AveOFormTemplateCollection()
        { }

        #region IAveFormTemplateCollection Members

        public void UpgradeFormTemplate(string solutionPath, AveUpgradeType upgradeType)
        {
            mFormTemplateCollection.UpgradeFormTemplate(solutionPath, (FormTemplateCollection.UpgradeType)upgradeType);
        }

        public IAveOConverterMessageCollection VerifyFormTemplate(string solutionPath)
        {
            return new AveOConverterMessageCollection(FormTemplateCollection.VerifyFormTemplate(solutionPath));
        }

        #endregion

        public IAveOFormTemplate ItemFromFile(string filePath)
        {
            FormTemplate formTemplate = mFormTemplateCollection.ItemFromFile(filePath);
            if (formTemplate == null)
            {
                return null;
            }
            return new AveOFormTemplate(formTemplate);
        }

        public void UploadFormTemplate(string solutionPath)
        {
            mFormTemplateCollection.UploadFormTemplate(solutionPath);
        }

        public IAveOFormTemplate Item(Guid templateId)
        {
            FormTemplate formTemplate = mFormTemplateCollection.Item(templateId);
            if (formTemplate == null)
            {
                return null;
            }
            return new AveOFormTemplate(formTemplate);
        }

        public void RemoveFormTemplate(IAveOFormTemplate formTemplate)
        {
            mFormTemplateCollection.RemoveFormTemplate((formTemplate as AveOFormTemplate).FormTemplate);
        }

        public override int Count
        {
            get
            {
                return mFormTemplateCollection.Count;
            }
        }
    }
}
