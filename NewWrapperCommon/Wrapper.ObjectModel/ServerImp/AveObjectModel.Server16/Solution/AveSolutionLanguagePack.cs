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
using Microsoft.SharePoint.Administration;
using AvePoint.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server16
{
    class AveSolutionLanguagePack : AvePersistedObject, IAveSolutionLanguagePack
    {
        private SPSolutionLanguagePack mSolutionLanguagePack;
        private Collection<IAveWebApplication> mDeployedWebApplications;
        private IAveDeploymentConfigCollection mDeploymentConfigs;
        private AvePersistedFile mPersistedFile;

        public AveSolutionLanguagePack(SPSolutionLanguagePack solutionLanguagePack)
            : base(solutionLanguagePack)
        {
            mSolutionLanguagePack = solutionLanguagePack;
        }

        public AveSolutionLanguagePack()
            : this(new SPSolutionLanguagePack())
        { }

        public bool ApplicationServerDeployment
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "ApplicationServerDeployment");
            }
        }

        public bool ContainsCasPolicy
        {
            get
            {
                return mSolutionLanguagePack.ContainsCasPolicy;
            }
        }

        public bool ContainsGlobalAssembly
        {
            get
            {
                return mSolutionLanguagePack.ContainsGlobalAssembly;
            }
        }

        public bool ContainsWebApplicationResource
        {
            get
            {
                return mSolutionLanguagePack.ContainsWebApplicationResource;
            }
        }

        public bool Deployed
        {
            get
            {
                return mSolutionLanguagePack.Deployed;
            }
        }

        public Collection<IAveWebApplication> DeployedWebApplications
        {
            get
            {
                if (mDeployedWebApplications == null)
                {
                    mDeployedWebApplications = new Collection<IAveWebApplication>();
                    foreach (SPWebApplication webApplication in mSolutionLanguagePack.DeployedWebApplications)
                    {
                        mDeployedWebApplications.Add(new AveWebApplication(webApplication));
                    }
                }
                return mDeployedWebApplications;
            }
        }

        public AveServerRole DeploymentServerType
        {
            get
            {
                return (AveServerRole)(int)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "DeploymentServerType");
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public IAveDeploymentConfigCollection DeploymentConfigs
        {
            get
            {
                if (mDeploymentConfigs == null)
                {
                    mDeploymentConfigs = new AveDeploymentConfigCollection(AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "DeploymentConfigs"));
                }
                return mDeploymentConfigs;
            }
        }

        public AveSolutionDeploymentState DeploymentState
        {
            get
            {
                return (AveSolutionDeploymentState)mSolutionLanguagePack.DeploymentState;
            }
        }

        public bool GlobalDeployment
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "GlobalDeployment");
            }
        }

        public bool IsWebPartPackage
        {
            get
            {
                return mSolutionLanguagePack.IsWebPartPackage;
            }
        }

        public string LastOperationDetails
        {
            get
            {
                return mSolutionLanguagePack.LastOperationDetails;
            }
        }

        public DateTime LastOperationEndTime
        {
            get
            {
                return mSolutionLanguagePack.LastOperationEndTime;
            }
        }

        public AveSolutionOperationResult LastOperationResult
        {
            get
            {
                return (AveSolutionOperationResult)mSolutionLanguagePack.LastOperationResult;
            }
        }

        public uint LocaleId
        {
            get
            {
                return mSolutionLanguagePack.LocaleId;
            }
        }

        public bool ResetWebServer
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "ResetWebServer");
            }
        }

        public Guid SolutionId
        {
            get { return mSolutionLanguagePack.SolutionId; }
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, bool force)
        {
            mSolutionLanguagePack.Deploy(dt, globalInstallWPPackDlls, force);
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force)
        {
            mSolutionLanguagePack.Deploy(dt, globalInstallWPPackDlls, GetWebApplications(webApplications), force);
        }

        public void DeployLocal(bool globalInstallWPPackDlls, bool force)
        {
            mSolutionLanguagePack.DeployLocal(globalInstallWPPackDlls, force);
        }

        public void DeployLocal(bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force)
        {
            mSolutionLanguagePack.DeployLocal(globalInstallWPPackDlls, GetWebApplications(webApplications), force);
        }

        public bool IsDeployedToWebApplication(Guid id)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mSolutionLanguagePack, "IsDeployedToWebApplication", new Type[] { typeof(Guid) }, new object[] { id });
        }

        public void Retract(DateTime dt)
        {
            mSolutionLanguagePack.Retract(dt);
        }

        public void Retract(DateTime dt, Collection<IAveWebApplication> webApplications)
        {
            mSolutionLanguagePack.Retract(dt, GetWebApplications(webApplications));
        }

        private Collection<SPWebApplication> GetWebApplications(Collection<IAveWebApplication> webApplications)
        {
            Collection<SPWebApplication> webApps = new Collection<SPWebApplication>();
            foreach (IAveWebApplication webApplication in webApplications)
            {
                webApps.Add((webApplication as AveWebApplication).WebApplication);
            }
            return webApps;
        }

        public IAvePersistedFile SolutionFile
        {
            get
            {
                if (mPersistedFile == null)
                {
                    SPPersistedFile persistedFile = mSolutionLanguagePack.SolutionFile;
                    if (persistedFile != null)
                    {
                        mPersistedFile = new AvePersistedFile(persistedFile);
                    }
                }
                return mPersistedFile;
            }
        }

        public IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path)
        {
            return new AveFarmSolutionPackage(AveAssemblyUtility.InvokeStaticMethod(typeof(SPSolutionLanguagePack), "CreateSolutionPackageFromFile", new Type[] { typeof(string) }, new object[] { path }));
        }

        public IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path, string name)
        {
            return new AveFarmSolutionPackage(AveAssemblyUtility.InvokeStaticMethod(typeof(SPSolutionLanguagePack), "CreateSolutionPackageFromFile", new Type[] { typeof(string), typeof(string) }, new object[] { path, name }));
        }

        public void Upgrade(string path)
        {
            mSolutionLanguagePack.Upgrade(path);
        }

        public bool ContainsPreviousVersion
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "ContainsPreviousVersion");
            }
        }

        public bool WfeGlobalDeployment
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionLanguagePack, "WfeGlobalDeployment");
            }
        }

        public void DeleteTempDir(string dir)
        {
            AveAssemblyUtility.InvokeStaticMethod(typeof(SPSolutionLanguagePack), "DeleteTempDir", new Type[] { typeof(string) }, new object[] { dir });
        }

        public List<IAveWebApplication> DeployedWebApplicationsInLocalServer()
        {
            List<SPWebApplication> spWebApps = (List<SPWebApplication>)AveAssemblyUtility.InvokeMethod(mSolutionLanguagePack, "DeployedWebApplicationsInLocalServer", new Type[] { }, new object[] { });
            List<IAveWebApplication> webApps = null;
            if (spWebApps != null)
            {
                webApps = new List<IAveWebApplication>();
                foreach (SPWebApplication webapp in spWebApps)
                {
                    if (webapp != null)
                    {
                        webApps.Add(new AveWebApplication(webapp));
                    }
                    else
                    {
                        webApps.Add(null);
                    }
                }
            }
            return webApps;
        }
    }
}
