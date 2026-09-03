import { useState } from "react";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import SiteMap from "../../Components/SiteMap";
import Progress from "./Progress";
import "./index.less";
import Planned from "./Planned";
import Projection from "./Projection";
import ConfigurationPanel from "./Projection/ConfigurationPanel";
import { useRef } from "react";
import { useEffect } from "react";
import { ConfigurationRequester } from "../../requests";
import ProgressRequester from "../../requests/ProgressRequester";
import { DataSizeType } from "../../Constants";
import Enviroments from "../../../../../Constants/Enviroments";
import { SourceFlag } from "../../../../Common/Constants";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

const ProgressReport = () => {
    const projectionPanelRef = useRef(null);

    const [selectedO365TenantId, setSelectedO365TenantId] = useState();

    const [savingInfo, setSavingInfo] = useState({
        spFreeStorage: 0,
        spStoragePrice: 0,
        odFreeStorage: 0,
        odStoragePrice: 0,
        archivedDataStoragePrice: 0,
    });

    const [projectionConfigurationInfo, setProjectionConfigurationInfo] =
        useState({
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
        });

    const [activeTab, setActiveTab] = useState(0);

    const onActiveTabChange = (index) => {
        setActiveTab(index);
    };

    const onProjectionConfiguration = () => {
        projectionPanelRef.current.onShow(projectionConfigurationInfo);
    };

    useEffect(() => {
        const fetchData = async () => {
            if (_.isNil(selectedO365TenantId)) {
                return;
            }

            const responseConfigurationInfo =
                await ProgressRequester.getProjectionConfigurationInfo(
                    selectedO365TenantId
                );
            responseConfigurationInfo.contentSource = projectionConfigurationInfo.contentSource;
            const responseSavingInfo =
                await ConfigurationRequester.getCostSavingConfigurationInfo();
            setSavingInfo(responseSavingInfo);
            setProjectionConfigurationInfo(responseConfigurationInfo);
        };

        fetchData();
    }, [selectedO365TenantId]);

    const onReloadConfigurationInfo = async () => {
        const responseConfigurationInfo =
            await ProgressRequester.getProjectionConfigurationInfo(
                selectedO365TenantId
            );
        responseConfigurationInfo.contentSource = projectionConfigurationInfo.contentSource;
        setProjectionConfigurationInfo(responseConfigurationInfo);
    };

    const onProjectionContentSourceChange = async (contentSource) => {
        const clonedConfigurationInfo = _.cloneDeep(projectionConfigurationInfo);
        clonedConfigurationInfo.contentSource = contentSource;
        setProjectionConfigurationInfo(clonedConfigurationInfo);
    };

    return (
        <div id="raProgressReport">
            <SiteMap
                URL={[SiteMapLinks.FA_Discovery_Progress]}
                onChange={setSelectedO365TenantId}
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
                            onClick={onProjectionConfiguration}
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
                                    configurationInfo={
                                        projectionConfigurationInfo
                                    }
                                    onContentSourceChange={onProjectionContentSourceChange}
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
                onReload={onReloadConfigurationInfo}
                ref={projectionPanelRef}
            />
        </div>
    );
};

export default ProgressReport;
