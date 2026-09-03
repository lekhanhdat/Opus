import React from "react";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import {
    InactiveSummaryHistoryVersion,
    InactiveSummaryV3,
    SFInactiveSummaryV3,
    // GoogleDriveInactiveSummaryV3,
} from "./Summary";
import {
    InactiveOptimizationHistoryVersion,
    InactiveOptimizationV3,
} from "./Optimization";
import SiteMap from "../Components/SiteMap";
import { JobMangerRequester } from "../requests";
import { GoogleDriveJobManagerRequester } from "../requests/GoogleDrive";
import { SalesforceJobManagerRequester } from "../requests/Salesforce";
import { DiscoveryJobVersion, ScopeSource } from "../Constants";
import SFSiteMap from "../Components/SiteMap/Salesforce";
// import GoogleDriveSiteMap from "../Components/SiteMap/GoogleDrive";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

class Inactive extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            jobInfo: null,
            scopeSourceSelected: localStorage.getItem("scopeSourceSelected") || ScopeSource.O365,
            activeTab: ActionTab.Summary,
            selectedO365TenantId: undefined,
            selectedGGContainerId: null,
        };
    }

    componentDidMount() {
        this.getJobInfo();
        this.dispatch(
            "raScopeSource",
            true,
            this.renderScopeSourceNavigation()
        );
    }

    componentDidUpdate() {
        this.dispatch(
            "raScopeSource",
            true,
            this.renderScopeSourceNavigation()
        );
    }

    componentDestroy() {
        this.dispatch("raScopeSource", false, <></>);
    }

    getJobInfo = async () => {
        let responseJobInfo = null;
        switch (Number(this.state.scopeSourceSelected)) {
            case ScopeSource.GoogleDrive:
                responseJobInfo = await GoogleDriveJobManagerRequester.getLatest();
                break;
            case ScopeSource.Salesforce:
                responseJobInfo = await SalesforceJobManagerRequester.getLatest();
                break;
            default:
                // For office 365
                responseJobInfo = await JobMangerRequester.getLatest();
                break;
        }
        this.setState({
            jobInfo: responseJobInfo,
        });
    };

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderScopeSourceNavigation = () => {
        const navList = [
            {
                id: ScopeSource.O365,
                name: RMResx.RM_FA_Discovery_Common_O365_Source,
            },
            {
                id: ScopeSource.GoogleDrive,
                name: RMResx.RM_FA_Discovery_Common_GoogleDrive_Source,
            },
            {
                id: ScopeSource.Salesforce,
                name: RMResx.RM_FA_Discovery_Common_Salesforce_Source,
            },
        ];

        return (
            <div className="reco-scope-source-wrapper">
                <div className="reco-scope-source-content">
                    <p className="reco-scope-source-title">{RMResx.RM_FA_Discovery_Common_SourceTitle}</p>
                    <div style={{ gap: 4 }} className="flex flex-column">
                        {navList.map((item) => (
                            <p
                                key={item.id}
                                className={`reco-scope-source-nav-item ${
                                    this.state.scopeSourceSelected ==
                                        item.id && "active"
                                }`}
                                onKeyDown={this.onKeyDown}
                                onClick={() => {
                                    // localStorage.setItem("scopeSourceSelected", item.id);
                                    this.setState({
                                        scopeSourceSelected: item.id,
                                    }, () => {
                                        this.getJobInfo();
                                    });
                                }}
                            >
                                {item.name}
                            </p>
                        ))}
                    </div>
                </div>
            </div>
        );
    };

    renderSiteMap = () => {
        if (this.state.jobInfo === null) {
            return (
                <section className="reco-sitemap">
                    <div className="margin-top-l">
                        <$g.SiteMap data={[SiteMapLinks.FA_Inactive]} />
                    </div>
                </section>
            );
        }

        switch (Number(this.state.scopeSourceSelected)) {
            case ScopeSource.Salesforce:
                return <SFSiteMap URL={[SiteMapLinks.FA_Inactive]} />;
            case ScopeSource.GoogleDrive:
                return (
                    // <GoogleDriveSiteMap
                    //     url={[SiteMapLinks.FA_Inactive]}
                    //     onChange={(value) =>
                    //         this.setState({ selectedGGContainerId: value })
                    //     }
                    // />
                    <></>
                );
            case ScopeSource.O365:
                return (
                    <SiteMap
                        URL={[SiteMapLinks.FA_Inactive]}
                        onChange={(value) =>
                            this.setState({ selectedO365TenantId: value })
                        }
                    />
                );
            default:
                return <div></div>;
        }
    };

    renderPageBasedOnDataSource = () => {
        const {
            jobInfo,
            scopeSourceSelected,
            activeTab,
            selectedO365TenantId,
            selectedGGContainerId,
        } = this.state;
        
        if (!jobInfo) return <div></div>;

        // Salesforce
        if (scopeSourceSelected == ScopeSource.Salesforce) {
            return (
                <R.Tabcontrol
                    maxWidth={"none"}
                    destroy={true}
                    onChange={(index) => this.setState({ activeTab: index })}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_SummaryTab}
                        aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                    >
                        <SFInactiveSummaryV3
                            key={"sfTenantId" + "_inactive_summary"}
                        />
                    </R.TabPanel>

                    {/** 
                    ** Hidden in this release, It'll be implemented later
                    ** Hide the "Inactive data optimization" tab

                    <R.TabPanel
                        tab={RMResx.RM_FA_SF_Inactive_OptimizationTab}
                        aria-label={
                            RMResx.RM_FA_SF_Inactive_OptimizationTab
                        }
                        disabled={true}
                    >
                        <div></div>
                    </R.TabPanel>
                    */}
                </R.Tabcontrol>
            );
        }

        // Google drive
        if (scopeSourceSelected == ScopeSource.GoogleDrive) {
            return (
                <R.Tabcontrol
                    maxWidth={"none"}
                    destroy={true}
                    onChange={(index) => this.setState({ activeTab: index })}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_Inactive_SummaryTab}
                        aria-label={RMResx.RM_FA_Inactive_SummaryTab}
                    >
                        {/* <GoogleDriveInactiveSummaryV3
                            key={"ggContainerId" + "_inactive_summary"}
                            containerId={selectedGGContainerId}
                            jobInfo={jobInfo}
                        /> */}
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        }

        // M365
        // if (scopeSourceSelected == ScopeSource.O365) {
        //     if (jobInfo.version == DiscoveryJobVersion.V3 || jobInfo.version == DiscoveryJobVersion.V4) {
        //         return (
        //             <R.Tabcontrol
        //                 maxWidth={"none"}
        //                 destroy={true}
        //                 onChange={(index) => this.setState({ activeTab: index })}
        //                 active={activeTab}
        //             >
        //                 <R.TabPanel
        //                     tab={RMResx.RM_FA_Inactive_SummaryTab}
        //                     aria-label={RMResx.RM_FA_Inactive_SummaryTab}
        //                 >
        //                     <InactiveSummaryV3
        //                         key={selectedO365TenantId + "_inactive_summary"}
        //                         o365TenantId={selectedO365TenantId}
        //                         scopeSourceSelected={scopeSourceSelected}
        //                         jobInfo={jobInfo}
        //                     />
        //                 </R.TabPanel>
        //                 <R.TabPanel
        //                     tab={RMResx.RM_FA_Inactive_OptimizationTab}
        //                     aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
        //                 >
        //                     <InactiveOptimizationV3
        //                         key={selectedO365TenantId + "_inactive_optimization"}
        //                         o365TenantId={selectedO365TenantId}
        //                         jobInfo={jobInfo}
        //                     />
        //                 </R.TabPanel>
        //             </R.Tabcontrol>
        //         );
        //     } else {
        //         return (
        //             <R.Tabcontrol
        //                 maxWidth={"none"}
        //                 destroy={true}
        //                 onChange={(index) => setActiveTab(index)}
        //                 active={activeTab}
        //             >
        //                 <R.TabPanel
        //                     tab={RMResx.RM_FA_Inactive_SummaryTab}
        //                     aria-label={RMResx.RM_FA_Inactive_SummaryTab}
        //                 >
        //                     <InactiveSummaryHistoryVersion
        //                         key={selectedO365TenantId + "_inactive_summary"}
        //                         o365TenantId={selectedO365TenantId}
        //                         scopeSourceSelected={scopeSourceSelected}
        //                     />
        //                 </R.TabPanel>
        //                 <R.TabPanel
        //                     tab={RMResx.RM_FA_Inactive_OptimizationTab}
        //                     aria-label={RMResx.RM_FA_Inactive_OptimizationTab}
        //                 >
        //                     <InactiveOptimizationHistoryVersion
        //                         key={selectedO365TenantId + "_inactive_optimization"}
        //                         o365TenantId={selectedO365TenantId}
        //                     />
        //                 </R.TabPanel>
        //             </R.Tabcontrol>
        //         );
        //     }
        // }
    };

    render() {
        return (
            <div id="raInactive">
                {this.renderSiteMap()}
                <div>{this.renderPageBasedOnDataSource()}</div>
            </div>
        );
    }
}

export default Inactive;
