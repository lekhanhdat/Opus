import SiteMap from "../Components/SiteMap";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import {
    // GoogleDriveROTSummaryV3,
    ROTSummaryHistoryVersion,
    ROTSummaryV3,
} from "./Summary";
import {
    ROTOptimizationHistoryVersion,
    ROTOptimizationV3,
} from "./Optimization";
import JobManagerRequester from "../requests/JobMangerRequester";
import { DiscoveryJobVersion, ScopeSource } from "../Constants";
// import GoogleDriveSiteMap from "../Components/SiteMap/GoogleDrive";

const ActionTab = {
    Summary: 0,
    Optimization: 1,
};

class ROT extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            jobInfo: null,
            scopeSourceSelected: localStorage.getItem("scopeSourceSelected") && localStorage.getItem("scopeSourceSelected") === ScopeSource.GoogleDrive ? ScopeSource.GoogleDrive : ScopeSource.O365,
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
        const responseJobInfo = await JobManagerRequester.getLatest();
        this.setState({ jobInfo: responseJobInfo });
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
                                    this.state.scopeSourceSelected ===
                                        item.id && "active"
                                }`}
                                onKeyDown={this.onKeyDown}
                                onClick={() => {
                                    localStorage.setItem("scopeSourceSelected", `${item.id}`);
                                    this.setState({
                                        scopeSourceSelected: item.id,
                                    })
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
        if (this.state.scopeSourceSelected === ScopeSource.GoogleDrive) {
            return (
                // <GoogleDriveSiteMap
                //     url={[SiteMapLinks.FA_ROT]}
                //     onChange={(value) =>
                //         this.setState({ selectedGGContainerId: value })
                //     }
                // />
                <></>
            );
        }

        if (this.state.scopeSourceSelected === ScopeSource.O365) {
            return (
                <SiteMap
                    URL={[SiteMapLinks.FA_ROT]}
                    onChange={(value) =>
                        this.setState({ selectedO365TenantId: value })
                    }
                />
            );
        }

        return <div></div>;
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

        // Google drive
        if (scopeSourceSelected === ScopeSource.GoogleDrive) {
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
                        {/* <GoogleDriveROTSummaryV3
                            key={"ggContainerId" + "_inactive_summary"}
                            containerId={selectedGGContainerId}
                            jobInfo={jobInfo}
                        /> */}
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        }

        // M365
        if (
            jobInfo.version == DiscoveryJobVersion.V3 ||
            jobInfo.version == DiscoveryJobVersion.V4 ||
            jobInfo.version == DiscoveryJobVersion.V5
        ) {
            return (
                <R.Tabcontrol
                    flex
                    destroy={true}
                    onChange={(index) => this.setState({ activeTab: index })}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_ROT_SummaryTab}
                        aria-label={RMResx.RM_FA_ROT_SummaryTab}
                    >
                        <ROTSummaryV3
                            key={selectedO365TenantId + "_rot_summary"}
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FA_ROT_OptimizationTab}
                        aria-label={RMResx.RM_FA_ROT_OptimizationTab}
                    >
                        <ROTOptimizationV3
                            key={selectedO365TenantId + "_rot_optimization"}
                            o365TenantId={selectedO365TenantId}
                            jobInfo={jobInfo}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        } else {
            return (
                <R.Tabcontrol
                    flex
                    destroy={true}
                    onChange={(index) => this.setState({ activeTab: index })}
                    active={activeTab}
                >
                    <R.TabPanel
                        tab={RMResx.RM_FA_ROT_SummaryTab}
                        aria-label={RMResx.RM_FA_ROT_SummaryTab}
                    >
                        <ROTSummaryHistoryVersion
                            key={selectedO365TenantId + "_rot_summary"}
                            o365TenantId={selectedO365TenantId}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_FA_ROT_OptimizationTab}
                        aria-label={RMResx.RM_FA_ROT_OptimizationTab}
                    >
                        <ROTOptimizationHistoryVersion
                            key={selectedO365TenantId + "_rot_optimization"}
                            o365TenantId={selectedO365TenantId}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            );
        }
    };

    render() {
        return (
            <div id="raROT">
                {this.renderSiteMap()}
                <div>{this.renderPageBasedOnDataSource()}</div>
            </div>
        );
    }
}

export default ROT;
