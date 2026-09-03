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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveTerm
    {
        private AveTermSet aveTermSet;
        private AveTerm parentTerm;

        public Guid Term
        {
            get;
            set;
        }

        public AveTermSet TermSet
        {
            get { return aveTermSet; }
            set { this.aveTermSet = value; }
        }

        internal IAveMetadataServiceApplication ServiceApplication
        {
            get
            {
                if (this.aveTermSet != null)
                {
                    return this.aveTermSet.ServiceApplication;
                }
                else
                {
                    return this.parentTerm.ServiceApplication;
                }
            }
        }

        public AveTerm(AveTermSet aveTermSet)
        {
            this.aveTermSet = aveTermSet;
        }

        public AveTerm(AveTerm term)
        {
            this.parentTerm = term;
            this.aveTermSet = term.TermSet;
        }

        public void Export(IAveBackupStream output, Guid termId)
        {
            output.WriteMetadata(AveMetadataType.MetadataTerm, GetTermInfo(termId));
        }

        private AveTermInfo GetTermInfo(Guid termId)
        {
            this.Term = termId;
            return this.ServiceApplication.GetTerm(this.aveTermSet.TermSet, termId);
        }
    }
}