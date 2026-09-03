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
using System.Collections.ObjectModel;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server13
{
    class AveSolution : AvePersistedObject, IAveSolution
    {
        private SPSolution mSolution;
        private AveSolutionLanguagePackCollection mLanguagePacks;
        private Collection<IAveWebApplication> mDeployedWebApplications;
        private AvePersistedFile mPersistedFile;
        private Collection<IAveServer> mDeployedServers;

        public AveSolution(SPSolution solution)
            : base(solution)
        {
            mSolution = solution;
        }

        public AveSolution()
            : this(new SPSolution())
        { }

        internal SPSolution Solution
        {
            get { return mSolution; }
        }

        #region IAveSolution Members

        public void Upgrade(string path, DateTime dt)
        {
            mSolution.Upgrade(path, dt);
        }

        public IAveSolutionLanguagePackCollection LanguagePacks
        {
            get
            {
                if (mLanguagePacks == null)
                {
                    mLanguagePacks = new AveSolutionLanguagePackCollection(mSolution.LanguagePacks);
                }
                return mLanguagePacks;
            }
        }

        public Guid SolutionId
        {
            get
            {
                return mSolution.SolutionId;
            }
        }

        public IAveSolutionLanguagePack GetLanguagePack(uint lcid)
        {
            SPSolutionLanguagePack solutionLanguagePack = mSolution.GetLanguagePack(lcid);
            if (solutionLanguagePack == null)
            {
                return null;
            }
            return new AveSolutionLanguagePack(solutionLanguagePack);
        }

        public bool IsOperationResultError(AveSolutionOperationResult result)
        {
            return Convert.ToBoolean(AveAssemblyUtility.InvokeStaticMethod(typeof(SPSolution), "IsOperationResultError", new object[] { (SPSolutionOperationResult)result }));
        }

        public bool JobExists
        {
            get { return mSolution.JobExists; }
        }

        public AveRunningJobStatus JobStatus
        {
            get { return (AveRunningJobStatus)mSolution.JobStatus; }
        }

        public bool Deployed
        {
            get { return mSolution.Deployed; }
        }

        public AveSolutionDeploymentState DeploymentState
        {
            get { return (AveSolutionDeploymentState)mSolution.DeploymentState; }
        }

        public bool ContainsCasPolicy
        {
            get { return mSolution.ContainsCasPolicy; }
        }

        public bool ContainsGlobalAssembly
        {
            get { return mSolution.ContainsGlobalAssembly; }
        }

        public bool ContainsWebApplicationResource
        {
            get { return mSolution.ContainsWebApplicationResource; }
        }

        public bool IsWebPartPackage
        {
            get { return mSolution.IsWebPartPackage; }
        }

        public string LastOperationDetails
        {
            get { return mSolution.LastOperationDetails; }
        }

        public AveSolutionOperationResult LastOperationResult
        {
            get { return (AveSolutionOperationResult)mSolution.LastOperationResult; }
        }

        public Collection<IAveWebApplication> DeployedWebApplications
        {
            get
            {
                if (mDeployedWebApplications == null)
                {
                    Collection<SPWebApplication> webApplications = mSolution.DeployedWebApplications;
                    mDeployedWebApplications = new Collection<IAveWebApplication>();
                    foreach (var webApplication in webApplications)
                    {
                        mDeployedWebApplications.Add(new AveWebApplication(webApplication));
                    }
                }
                return mDeployedWebApplications;
            }
        }

        public void Upgrade(string path)
        {
            mSolution.Upgrade(path);
        }

        public IAvePersistedFile SolutionFile
        {
            get
            {
                if (mPersistedFile == null)
                {
                    SPPersistedFile persistedFile = mSolution.SolutionFile;
                    if (persistedFile != null)
                    {
                        mPersistedFile = new AvePersistedFile(persistedFile);
                    }
                }
                return mPersistedFile;
            }
        }

        public DateTime LastOperationEndTime
        {
            get
            {
                return mSolution.LastOperationEndTime;
            }
        }

        public void DeployLocal(bool globalInstallWPPackDlls, bool force)
        {
            mSolution.DeployLocal(globalInstallWPPackDlls, force);
        }

        public void DeployLocal(bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force)
        {
            mSolution.DeployLocal(globalInstallWPPackDlls, GetSPWebApplication(webApplications), force);
        }

        public void RetractLocal()
        {
            mSolution.RetractLocal();
        }

        public void RetractLocal(Collection<IAveWebApplication> webApplications)
        {
            mSolution.RetractLocal(GetSPWebApplication(webApplications));
        }

        private Collection<SPWebApplication> GetSPWebApplication(Collection<IAveWebApplication> webApplications)
        {
            Collection<SPWebApplication> webApps = null;
            if (webApplications != null)
            {
                webApps = new Collection<SPWebApplication>();
                foreach (IAveWebApplication webApp in webApplications)
                {
                    webApps.Add((webApp as AveWebApplication).WebApplication);
                }
            }
            return webApps;
        }

        public Collection<IAveServer> DeployedServers
        {
            get
            {
                if (mDeployedServers == null)
                {
                    Collection<SPServer> deployedServers = mSolution.DeployedServers;
                    mDeployedServers = new Collection<IAveServer>();
                    foreach (SPServer server in deployedServers)
                    {
                        mDeployedServers.Add(new AveServer(server));
                    }
                }
                return mDeployedServers;
            }
        }

        public void Retract(DateTime dt)
        {
            mSolution.Retract(dt);
        }

        public void Retract(DateTime dt, Collection<IAveWebApplication> webApplications)
        {
            Collection<SPWebApplication> spWebApplications = null;
            if (webApplications != null)
            {
                spWebApplications = new Collection<SPWebApplication>();
                foreach (IAveWebApplication webApp in webApplications)
                {
                    if (webApp != null)
                    {
                        spWebApplications.Add((webApp as AveWebApplication).WebApplication);
                    }
                    else
                    {
                        spWebApplications.Add(null);
                    }
                }
            }
            mSolution.Retract(dt, spWebApplications);
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, bool force)
        {
            mSolution.Deploy(dt, globalInstallWPPackDlls, force);
        }

        public void Deploy(DateTime dt, bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force)
        {
            Collection<SPWebApplication> spWebApplications = null;
            if (webApplications != null)
            {
                spWebApplications = new Collection<SPWebApplication>();
                foreach (IAveWebApplication webApp in webApplications)
                {
                    if (webApp != null)
                    {
                        spWebApplications.Add((webApp as AveWebApplication).WebApplication);
                    }
                    else
                    {
                        spWebApplications.Add(null);
                    }
                }
            }
            mSolution.Deploy(dt, globalInstallWPPackDlls, spWebApplications, force);
        }

        #endregion
    }
}
