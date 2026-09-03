import { createRef } from "react";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import SiteMap from "../Components/SiteMap";
import Progress from "./Progress";
import Planned from "./Planned";
import Projection from "./Projection";
import ConfigurationPanel from "./Projection/ConfigurationPanel";
import { ConfigurationRequester } from "../requests";
import ProgressRequester from "../requests/ProgressRequester";
import { DataSizeType, ScopeSource } from "../Constants";
import Enviroments from "../../../../Constants/Enviroments";
import { SourceFlag } from "../../../Common/Constants";
import "./index.less";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

class ProgressReport extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedO365TenantId: null,
            savingInfo: {
                spFreeStorage: 0,
                spStoragePrice: 0,
                odFreeStorage: 0,
                odStoragePrice: 0,
                archivedDataStoragePrice: 0,
            },
            projectionConfigurationInfo: {
                o365TenantId: null,
                latestYear: currentDate.getFullYear(),
                latestMonth: currentDate.getMonth() + 1,
                latestStorageSize: 0,
                oldestYear: sixMonthsAgoDate.getFullYear(),
                oldestMonth: sixMonthsAgoDate.getMonth() + 1,
                oldestStorageSize: 0,
                realityMonthlyGrowthRate: 0,
                monthlyGrowthRate: 0,
                odLatestYear: currentDate.getFullYear(),
                odLatestMonth: currentDate.getMonth() + 1,
                odLatestStorageSize: 0,
                odOldestYear: sixMonthsAgoDate.getFullYear(),
                odOldestMonth: sixMonthsAgoDate.getMonth() + 1,
                odOldestStorageSize: 0,
                odRealityMonthlyGrowthRate: 0,
                odMonthlyGrowthRate: 0,
                realityDailyOptimizationSpeed: 0,
                dailyOptimizationSpeed: 0,
                dataSizeUnitType: DataSizeType.TB,
                contentSource: SourceFlag.SharePoint
            },
            activeTab: 0,
            scopeSourceSelected: ScopeSource.O365,
        };
        this.projectionPanelRef = createRef();
        this.formRef = createRef();
    }

    componentDidMount() {
        this.getProjectionConfigurationInfo();
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
    // const projectionPanelRef = useRef(null);

    // const [selectedO365TenantId, setSelectedO365TenantId] = useState();

    // const [savingInfo, setSavingInfo] = useState({
    //     spFreeStorage: 0,
    //     spStoragePrice: 0,
    //     odFreeStorage: 0,
    //     odStoragePrice: 0,
    //     archivedDataStoragePrice: 0,
    // });

    // const [projectionConfigurationInfo, setProjectionConfigurationInfo] =
    //     useState({
    //         o365TenantId: null,
    //         latestYear: currentDate.getFullYear(),
    //         latestMonth: currentDate.getMonth() + 1,
    //         latestStorageSize: 0,
    //         oldestYear: sixMonthsAgoDate.getFullYear(),
    //         oldestMonth: sixMonthsAgoDate.getMonth() + 1,
    //         oldestStorageSize: 0,
    //         realityMonthlyGrowthRate: 0,
    //         monthlyGrowthRate: 0,
    //         odLatestYear: currentDate.getFullYear(),
    //         odLatestMonth: currentDate.getMonth() + 1,
    //         odLatestStorageSize: 0,
    //         odOldestYear: sixMonthsAgoDate.getFullYear(),
    //         odOldestMonth: sixMonthsAgoDate.getMonth() + 1,
    //         odOldestStorageSize: 0,
    //         odRealityMonthlyGrowthRate: 0,
    //         odMonthlyGrowthRate: 0,
    //         realityDailyOptimizationSpeed: 0,
    //         dailyOptimizationSpeed: 0,
    //         dataSizeUnitType: DataSizeType.TB,
    //         contentSource: SourceFlag.SharePoint
    //     });

    // const [activeTab, setActiveTab] = useState(0);

    onActiveTabChange = (index) => {
        this.setState({ activeTab: index });
    };

    onProjectionConfiguration = () => {
        this.projectionPanelRef.current.onShow(this.state.projectionConfigurationInfo);
    };

    getProjectionConfigurationInfo = async () => {
        if (_.isNil(selectedO365TenantId)) {
            return;
        }

        const { selectedO365TenantId, projectionConfigurationInfo } = this.state;
        const responseConfigurationInfo = await ProgressRequester.getProjectionConfigurationInfo(selectedO365TenantId);
        responseConfigurationInfo.contentSource = projectionConfigurationInfo.contentSource;
        const responseSavingInfo = await ConfigurationRequester.getCostSavingConfigurationInfo();
        this.setState({
            savingInfo: responseSavingInfo,
            projectionConfigurationInfo: responseConfigurationInfo,
        })
    }

    onReloadConfigurationInfo = async () => {
        const { selectedO365TenantId, projectionConfigurationInfo } = this.state;
        const responseConfigurationInfo = await ProgressRequester.getProjectionConfigurationInfo(selectedO365TenantId);
        responseConfigurationInfo.contentSource = projectionConfigurationInfo.contentSource;
        this.setState({ projectionConfigurationInfo: responseConfigurationInfo });
    };

    onProjectionContentSourceChange = async (contentSource) => {
        const clonedConfigurationInfo = _.cloneDeep(this.state.projectionConfigurationInfo);
        clonedConfigurationInfo.contentSource = contentSource;
        this.setState({ projectionConfigurationInfo: clonedConfigurationInfo });
    };

    onChangeO365TenantId = (uniqueId) => {
        this.setState({
            selectedO365TenantId: uniqueId
        }, () => {
            this.getProjectionConfigurationInfo();
        });
    }

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
                                    this.setState({
                                        scopeSourceSelected: item.id,
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

    render() {
        const {
            activeTab,
            savingInfo,
            projectionConfigurationInfo,
            selectedO365TenantId,
         } = this.state;
        return (
            <div id="raProgressReport">
                <SiteMap
                    URL={[SiteMapLinks.FA_Discovery_Progress]}
                    onChange={this.onChangeO365TenantId}
                />
                <div className="reco-progress-tabs">
                    <div className="reco-project-config-btn">
                        {activeTab === 0 && (
                            <R.Button
                                className="theme"
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_FA_Progress_ProjectionButton}
                                type="button"
                                tooltip={RMResx.RM_FA_Progress_ProjectionButton}
                                onClick={this.onProjectionConfiguration}
                            />
                        )}
                    </div>
    
                    <R.Tabcontrol
                        flex
                        onChange={(index) => onActiveTabChange(index)}
                        active={activeTab}
                    >
                        {[
                            Enviroments.GCC,
                            Enviroments.ChinaNorth,
                            Enviroments.GOV,
                            Enviroments.PHProduction,
                            Enviroments.PHTest,
                        ].every((item) => item !== RM.gData.enviromentName) && (
                            <R.TabPanel
                                tab={RMResx.RM_FA_Progress_ProjectionTab}
                                aria-label={RMResx.RM_FA_Progress_ProjectionTab}
                            >
                                <div className="reco-container">
                                    <Projection
                                        savingInfo={savingInfo}
                                        configurationInfo={projectionConfigurationInfo}
                                        onContentSourceChange={this.onProjectionContentSourceChange}
                                    />
                                </div>
                            </R.TabPanel>
                        )}
    
                        <R.TabPanel
                            tab={RMResx.RM_FA_Progress_ProgressTab}
                            aria-label={RMResx.RM_FA_Progress_ProgressTab}
                        >
                            <div className="reco-container">
                                <Progress
                                    key={selectedO365TenantId}
                                    o365TenantId={selectedO365TenantId}
                                />
                            </div>
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_FA_Progress_PlannedTab}
                            aria-label={RMResx.RM_FA_Progress_PlannedTab}
                        >
                            <div className="reco-container">
                                <Planned
                                    key={selectedO365TenantId}
                                    o365TenantId={selectedO365TenantId}
                                />
                            </div>
                        </R.TabPanel>
                    </R.Tabcontrol>
                </div>
                <ConfigurationPanel
                    onReload={this.onReloadConfigurationInfo}
                    ref={this.projectionPanelRef}
                />
            </div>
        );
    }
};

export default ProgressReport;
