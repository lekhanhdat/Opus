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
    public static class AgentTypeStructure
    {
        public static AgentTypeContainer root = new AgentTypeContainer();

        static AgentTypeStructure()
        {

            root.displayName = ModuleContract.DOCAVEPLATFORM.Name;


            AgentTypeContainer dataProtection = new AgentTypeContainer();
            dataProtection.displayName = ModuleContract.DOCAVEPLATFORM.DATAPROTECTION.Name;

            AgentTypeContainer administration = new AgentTypeContainer();
            administration.displayName = ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.Name;

            AgentTypeContainer storageOptimization = new AgentTypeContainer();
            storageOptimization.displayName = ModuleContract.DOCAVEPLATFORM.STORAGEOPTIMIZATION.Name;

            root.AgentTypeContainer.Add(dataProtection);
            root.AgentTypeContainer.Add(administration);
            root.AgentTypeContainer.Add(storageOptimization);


            buildDataProtection(dataProtection);

            buildAdministration(administration);

            buildStorageOptimization(storageOptimization);


        }

        
        


     #region DataProtection
        private static void buildDataProtection(AgentTypeContainer dataProtection)
        {
            AgentTypeUnit granularBackup = new AgentTypeUnit();
            granularBackup.DisplayName = ModuleContract.DOCAVEPLATFORM.DATAPROTECTION.GRANULARBACKUP.Name;
            granularBackup.Modules.Add(ModuleContract.DOCAVEPLATFORM.DATAPROTECTION.GRANULARBACKUP);
            dataProtection.AgentTypeUnits.Add(granularBackup);

        }

     #endregion



     #region Administration
        private static void buildAdministration(AgentTypeContainer Administration)
        {
            AgentTypeUnit administrator = new AgentTypeUnit();
            administrator.DisplayName = ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.CENTRALADMIN.Name;
            administrator.Modules.Add(ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.CENTRALADMIN);
            Administration.AgentTypeUnits.Add(administrator);


            AgentTypeUnit replicator = new AgentTypeUnit();
            replicator.DisplayName = ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.REPLICATOR.Name;
            replicator.Modules.Add(ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.REPLICATOR);
            Administration.AgentTypeUnits.Add(replicator);

            
            AgentTypeUnit contentManager = new AgentTypeUnit();
            contentManager.DisplayName = ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.CONTENTMANAGER.Name;
            contentManager.Modules.Add(ModuleContract.DOCAVEPLATFORM.ADMINISTRATION.CONTENTMANAGER);
            Administration.AgentTypeUnits.Add(contentManager);
   


        }
     #endregion

     #region Storage Optimization
        private static void buildStorageOptimization(AgentTypeContainer storageOptimization)
        {
            AgentTypeUnit extender = new AgentTypeUnit();
            extender.DisplayName = ModuleContract.DOCAVEPLATFORM.STORAGEOPTIMIZATION.EXTENDER.Name;
            extender.Modules.Add(ModuleContract.DOCAVEPLATFORM.STORAGEOPTIMIZATION.EXTENDER);
            storageOptimization.AgentTypeUnits.Add(extender);




        }
     #endregion


    }




    class AgentTypeContainer
    {
        private List<AgentTypeContainer> agentTypeContainer = new List<AgentTypeContainer>();
        private List<AgentTypeUnit> agentTypeUnits = new List<AgentTypeUnit>();
        
        public string displayName { get; set; }

        public List<AgentTypeContainer> AgentTypeContainer
        {
            get { return agentTypeContainer; }

        }
        public List<AgentTypeUnit> AgentTypeUnits
        {
            get { return agentTypeUnits; }

        }

    }


    class AgentTypeUnit {


        private List<AveModule> modules = new List<AveModule>();

        public string DisplayName { get; set; }

        public List<AveModule> Modules
        {
            get { return modules; }

        }

        public List<string> getContainedAgentTypes() {
            List<string> result = new List<string>();
            foreach (AveModule module in modules)
            {
                result.AddRange(module.getAllAgentTypes());
            }
            return result;
        }

    }
}
