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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.ObjectModel.Common
{
    class AveSolutionLanguagePack : IAveSolutionLanguagePack
    {
        public bool ApplicationServerDeployment
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsCasPolicy
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsGlobalAssembly
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsWebApplicationResource
        {
            get { throw new NotImplementedException(); }
        }

        public bool Deployed
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.Collection<IAveWebApplication> DeployedWebApplications
        {
            get { throw new NotImplementedException(); }
        }

        public AveServerRole DeploymentServerType
        {
            get { throw new NotImplementedException(); }
        }

        public IAveDeploymentConfigCollection DeploymentConfigs
        {
            get { throw new NotImplementedException(); }
        }

        public AveSolutionDeploymentState DeploymentState
        {
            get { throw new NotImplementedException(); }
        }

        public bool GlobalDeployment
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWebPartPackage
        {
            get { throw new NotImplementedException(); }
        }

        public string LastOperationDetails
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastOperationEndTime
        {
            get { throw new NotImplementedException(); }
        }

        public AveSolutionOperationResult LastOperationResult
        {
            get { throw new NotImplementedException(); }
        }

        public uint LocaleId
        {
            get { throw new NotImplementedException(); }
        }

        public bool ResetWebServer
        {
            get { throw new NotImplementedException(); }
        }

        public IAvePersistedFile SolutionFile
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SolutionId
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsPreviousVersion
        {
            get { throw new NotImplementedException(); }
        }

        public bool WfeGlobalDeployment
        {
            get { throw new NotImplementedException(); }
        }

        public void DeleteTempDir(string dir)
        {
            throw new NotImplementedException();
        }

        public List<IAveWebApplication> DeployedWebApplicationsInLocalServer()
        {
            throw new NotImplementedException();
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, bool force)
        {
            throw new NotImplementedException();
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, System.Collections.ObjectModel.Collection<IAveWebApplication> webApplications, bool force)
        {
            throw new NotImplementedException();
        }

        public void DeployLocal(bool globalInstallWPPackDlls, bool force)
        {
            throw new NotImplementedException();
        }

        public void DeployLocal(bool globalInstallWPPackDlls, System.Collections.ObjectModel.Collection<IAveWebApplication> webApplications, bool force)
        {
            throw new NotImplementedException();
        }

        public bool IsDeployedToWebApplication(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Retract(DateTime dt)
        {
            throw new NotImplementedException();
        }

        public void Retract(DateTime dt, System.Collections.ObjectModel.Collection<IAveWebApplication> webApplications)
        {
            throw new NotImplementedException();
        }

        public IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path)
        {
            return CreateSolutionPackageFromFile(path, Path.GetFileName(path));
        }

        public IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path, string name)
        {
            return new AveFarmSolutionPackage(path, name);
        }

        public void Upgrade(string path)
        {
            throw new NotImplementedException();
        }

        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get { throw new NotImplementedException(); }
        }

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public IAveFarm Farm
        {
            get { throw new NotImplementedException(); }
        }

        public Guid ID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAvePersistedObject Parent
        {
            get { throw new NotImplementedException(); }
        }

        public AveObjectStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TypeName
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Hashtable Properties
        {
            get { throw new NotImplementedException(); }
        }

        public bool WasCreated
        {
            get { throw new NotImplementedException(); }
        }

        public long Version
        {
            get { throw new NotImplementedException(); }
        }

        public IAveLastUpdateInfo LastUpdateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Provision()
        {
            throw new NotImplementedException();
        }

        public void Unprovision()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Uncache()
        {
            throw new NotImplementedException();
        }

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }
    }
}
