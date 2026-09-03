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
using System.Text;

namespace AvePoint.GCommon.Contract.AveModuleContract
{

    public static class AveModuleContract
    {
        public static DOCAVEPLATFORM DOCAVEPLATFORM = new DOCAVEPLATFORM();

    }



    public class DOCAVEPLATFORM : AveModuleContainer
    {
        private const int MODULE_TYPE_DOCAVE_PLATFORM_ID = 0;
        private const string MODULE_TYPE_DOCAVE_PLATFORM_NAME = "DocAvePlatform";

        private readonly DATAPROTECTION dataprotection = new DATAPROTECTION();

        public DATAPROTECTION DATAPROTECTION
        {
            get { return dataprotection; }
        }

        private readonly MIGRATION migration = new MIGRATION();

        public MIGRATION MIGRATION
        {
            get { return migration; }
        }

        private readonly COMPLIANCE compliance = new COMPLIANCE();

        public COMPLIANCE COMPLIANCE
        {
            get { return compliance; }
        }

        private readonly REPORTCENTER reportcenter = new REPORTCENTER();

        public REPORTCENTER REPORTCENTER
        {
            get { return reportcenter; }
        }

        private readonly STORAGEOPTIMIZATION storageoptimization = new STORAGEOPTIMIZATION();

        public STORAGEOPTIMIZATION STORAGEOPTIMIZATION
        {
            get { return storageoptimization; }
        }

        private readonly ADMINISTRATION administration = new ADMINISTRATION();

        public ADMINISTRATION ADMINISTRATION
        {
            get { return administration; }
        }

        private readonly CONTROLPANNEL controlpannel = new CONTROLPANNEL();

        public CONTROLPANNEL CONTROLPANNEL
        {
            get { return controlpannel; }
        } 


        public override int ID
        {
            get { return MODULE_TYPE_DOCAVE_PLATFORM_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_PLATFORM_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(this.DATAPROTECTION);
            result.Add(this.MIGRATION);
            result.Add(this.COMPLIANCE);
            result.Add(this.REPORTCENTER);
            result.Add(this.STORAGEOPTIMIZATION);
            result.Add(this.ADMINISTRATION);
            result.Add(this.CONTROLPANNEL);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }
}
