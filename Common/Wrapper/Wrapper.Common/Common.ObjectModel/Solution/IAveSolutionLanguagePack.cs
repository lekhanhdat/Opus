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
using System.Collections.ObjectModel;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSolutionLanguagePack : IAvePersistedObject
    {
        bool ApplicationServerDeployment { get; }
        bool ContainsCasPolicy { get; }
        bool ContainsGlobalAssembly { get; }
        bool ContainsWebApplicationResource { get; }
        bool Deployed { get; }
        Collection<IAveWebApplication> DeployedWebApplications { get; }
        AveServerRole DeploymentServerType { get; }
        IAveDeploymentConfigCollection DeploymentConfigs { get; }
        AveSolutionDeploymentState DeploymentState { get; }
        bool GlobalDeployment { get; }
        bool IsWebPartPackage { get; }
        string LastOperationDetails { get; }
        DateTime LastOperationEndTime { get; }
        AveSolutionOperationResult LastOperationResult { get; }
        uint LocaleId { get; }
        bool ResetWebServer { get; }
        IAvePersistedFile SolutionFile { get; }
        Guid SolutionId { get; }
        bool ContainsPreviousVersion { get; }
        bool WfeGlobalDeployment { get; }

        void DeleteTempDir(string dir);
        List<IAveWebApplication> DeployedWebApplicationsInLocalServer();
        void Deploy(DateTime dt, bool globalInstallWPPackDlls, bool force);
        void Deploy(DateTime dt, bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force);
        void DeployLocal(bool globalInstallWPPackDlls, bool force);
        void DeployLocal(bool globalInstallWPPackDlls, Collection<IAveWebApplication> webApplications, bool force);
        bool IsDeployedToWebApplication(Guid id);
        void Retract(DateTime dt);
        void Retract(DateTime dt, Collection<IAveWebApplication> webApplications);
        IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path);

        IAveFarmSolutionPackage CreateSolutionPackageFromStream(Stream packageStream,string name);
        IAveFarmSolutionPackage CreateSolutionPackageFromFile(string path, string name);
        void Upgrade(string path);
    }
}
