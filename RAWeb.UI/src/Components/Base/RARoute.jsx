import React from 'react';
import { Route } from 'react-router-dom';
import { Prompt } from 'react-router';
import { checkPermission } from '../../Utilities/permissionManager';
import { EnvironmentHelper, LicenseHelper } from '../../Utilities/CommonUtil';
import { JobType } from '../../Constants/Constants';
import RouterUrls from '../../Constants/RouterUrls';

export default class RARoute extends React.Component {
    constructor(props) {
        super(props);
        props.setActiveNav(props.routeConfig.navId);
        this._checkingProfile = false;
        this._checkedProfileAllowed = undefined; // undefined = not checked, true = allowed, false = blocked
    }

    isBlockInGCPEnv(path) {
        if (!EnvironmentHelper.IsGCPEnvironment) {
            return false;
        }

        const blockList = [RouterUrls.CP_EndUserRestore, RouterUrls.CP_AgentManagement,
            RouterUrls.BCM_ContentRepositoryManagement_FS, RouterUrls.BCM_FSConnGroup, RouterUrls.BCM_ContentRepositoryManagement_LSP];
                            
        return blockList.some(item => item.toLocaleLowerCase() === path);
    }

    isBlockInJPMCFeature(path) {
        if (!LicenseHelper.EnableJPMCFileSystemFeature()) {
            return false;
        }

        const type = RM.Url.getParam(window.location.href, "type");
        const blockedRoutes = {
            [RouterUrls.RC_TermUsageReportProfile.toLocaleLowerCase()]: String(JobType.FSBCSTermUsageReport),
            [RouterUrls.RC_DueDisposalReportProfile.toLocaleLowerCase()]: String(JobType.FSItemsFilesDueDisposal),
            [RouterUrls.RC_CreationAndDestructionProfile.toLocaleLowerCase()]: String(JobType.FSCreateAndDestroyedFileReport),
        };

        return blockedRoutes[path] === type;
    }

    isJPMCShowReportCandidate(path) {
        if (!LicenseHelper.EnableJPMCFileSystemFeature()) {
            return false;
        }
        const showRoutes = [
            RouterUrls.RC_TermUsageShowReport.toLocaleLowerCase(),
            RouterUrls.RC_DueDisposalShowReport.toLocaleLowerCase(),
            RouterUrls.RC_CreationAndDestructionShowReport.toLocaleLowerCase()
        ];
        return showRoutes.includes(path);
    }

    checkProfileSourceAndRedirect(path) {
        if (this._checkingProfile || this._checkedProfileAllowed !== undefined) return;
        this._checkingProfile = true;

        const id = RM.Url.getParam(window.location.href, "id");
        const jobId = RM.Url.getParam(window.location.href, "jobid");
        if (!id) {
            // skip profile API check and allow the show-report flow to continue.
            if (jobId) {
                console.log('RARoute: skipping profile API check');
                this._checkedProfileAllowed = true;
                this._checkingProfile = false;
                return;
            }
            this._checkedProfileAllowed = false;
            this._checkingProfile = false;
            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            return;
        }

        let apiUrl = null;
        if (path === RouterUrls.RC_TermUsageShowReport.toLocaleLowerCase()) {
            apiUrl = "/api/TermUsageReportApi/LoadProfileById";
        } else if (path === RouterUrls.RC_DueDisposalShowReport.toLocaleLowerCase()) {
            apiUrl = "/api/DueDisposalApi/LoadProfileById";
        } else if (path === RouterUrls.RC_CreationAndDestructionShowReport.toLocaleLowerCase()) {
            apiUrl = "/api/TimeFrameProfileApi/LoadProfileById";
        }

        if (!apiUrl) {
            this._checkedProfileAllowed = false;
            this._checkingProfile = false;
            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            return;
        }

        const option = { url: apiUrl, method: "POST", data: id };
        fetchUtility(option).then((data) => {
            this._checkingProfile = false;
            let isFS = false;
            try {
                if (data && data.Type) {
                    const t = parseInt(data.Type, 10);
                    if (t === JobType.FSBCSTermUsageReport || t === JobType.FSItemsFilesDueDisposal || t === JobType.FSCreateAndDestroyedFileReport || t === JobType.FSOrphanedTermReport || t === JobType.FSRetiredTermReport) {
                        isFS = true;
                    }
                }
            } catch (e) {
                isFS = false;
            }

            this._checkedProfileAllowed = !isFS;
            if (isFS) {
                window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            } else {
                this.forceUpdate();
            }
        }).catch(() => {
            this._checkingProfile = false;
            this._checkedProfileAllowed = false;
            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
        });
    }

    render() {
        let isAllowed = false;
        let isGCPEnvConfig = false;
        let isJPMCFeatureConfig = false;
        if(window.location.pathname){
            let currentResource = window.location.pathname.toLocaleLowerCase();
            currentResource =  currentResource.endsWith("/") ? currentResource.slice(0,currentResource.length - 1) : currentResource;
            
            if (this.isBlockInGCPEnv(currentResource)) {
                isAllowed = false;
                isGCPEnvConfig = true;
            } else if (this.isBlockInJPMCFeature(currentResource)) {
                isAllowed = false;
                isJPMCFeatureConfig = true;
            } else if (this.isJPMCShowReportCandidate(currentResource)) {
                if (this._checkedProfileAllowed === undefined) {
                    this.checkProfileSourceAndRedirect(currentResource);
                    return null;
                }

                if (this._checkedProfileAllowed === false) {
                    isAllowed = false;
                    isJPMCFeatureConfig = true;
                } else {
                    isAllowed = checkPermission(currentResource, RM.UserResources);
                }
                isGCPEnvConfig = false;
            } else {
                isAllowed = checkPermission(currentResource, RM.UserResources);
                isGCPEnvConfig = false;
            }
        }
        if(!isAllowed){
            if (isGCPEnvConfig) {
                window.location.href = window.location.origin + "/ErrorPage/PageNotFound";
                return;
            }

            if (isJPMCFeatureConfig) {
                window.location.href = window.location.origin + "/ErrorPage/NoPermission";
                return;
            }

            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            return true;
        }

        let { routeConfig, exact } = this.props;
        return <React.Fragment>
            <Prompt message={RARoute.PromptMsg} when={true} />
            <Route exact={exact} path={routeConfig.url} component={routeConfig.component} />
        </React.Fragment>;
    }
}

RARoute.PromptMsg = "Prompt_RARoute";