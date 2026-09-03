import React, { useEffect, useState } from "react";
import _ from "lodash";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { ApprovalStatus, EndType, EscalateSettingType, ExtendType, IntervalType, ManualTab, OrderOptions } from "./Constants/index";
import "./index.less";
import { CacheKeys} from "./Constants/index";
import { CustomColumnType } from "../../BCM/ContentRepositoryManagement/CustomMetadataSetting/Constants";

import RelatedRecords from "./RelatedRecords";
import UnderReview from "./UnderReview";
import History from "./History";
import WaitDisposal from "./WaitDisposal";
import Extend from "./Extend";

const DefaultSettingModel = {
    EmailNotificationSetting: {
        Interval: 1,
        IntervalType: IntervalType.Days,
        EndType: EndType.EndOccurrences,
        OccurrencesTimes: 3
    },
    EscalationSetting: {
        EscalateSettingType: EscalateSettingType.WorkflowNextStep,
        ApprovalStatus: ApprovalStatus.Rejected,
        ReassignUsers: [],
    },
    DisposalExtentionSetting: {
        MaxDelayTimes: 3 ,
        LatestExtendType: ExtendType.Month ,
        LatestExtendNumber: 1 ,
    }
};

const ManualApproval = (props) => {

    const [filterAvailableOptions, setFilterAvailableOptions] = useState(new Map());

    const [settingModel, setSettingModel] = useState(DefaultSettingModel);

    const [activeTab, setActiveTab] = useState(ManualTab.UnderReview);

    const [customColumns, setCustomColumns] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            await initDefaultActiveTab();
            await initSetting();
            await initFilterAvailableOptions();
        };
        fetchData();
    }, []);

    useEffect(() => {
        getInUsedCustomMetadataColumns();
    }, []);

    useEffect(()=>{
        return()=>{
            sessionStorage.removeItem(CacheKeys.URIsFiltered);
        };
    },[]);

    const getInUsedCustomMetadataColumns = async () => {
        $$.loading(true);
        const option = {
            url: "/api/ManualApproval/GetInUsedCustomMetadataColumns",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        if (res) {
            const orderOptionMap = new Map([
                [CustomColumnType.SingleText, OrderOptions.CustomText],
                [CustomColumnType.YesOrNo, OrderOptions.CustomYesOrNo],
                [CustomColumnType.DateTime, OrderOptions.CustomDateTime],
                [CustomColumnType.Number, OrderOptions.CustomNumber],
            ]);
            const newColumn = res.map((item) => ({
                id: item.UniqueId,
                header : item.ColumnName,
                columnType : item.ColumnType,
                width: [180],
                resizeable : true,
                sortable: item.EnableSort,
                orderOption: orderOptionMap.get(item.ColumnType),
            }));
            setCustomColumns(newColumn);
        }
    }

    const initDefaultActiveTab = () => {
        const Tab_Param_Name = "tab";
        const tab = RM.Url.getParam(window.location.href, Tab_Param_Name);
        if (tab === "") {
            return;
        }
        setActiveTab(parseInt(tab));
    };

    const initSetting = async () => {
        const setting = await fetchUtility({ url: "/api/ManualApproval/GetSettingInfo" });
        setSettingModel(setting);
    };

    const initFilterAvailableOptions = async () => {
        const options = await fetchUtility({ url: "/api/ManualApproval/GetFilterDefaultOptions" });
        const map = new Map();
        for (const option of options) {
            map.set(option.defaultOption, option.value);
        }
        setFilterAvailableOptions(map);
    };

    return (
        <div className="reco-manual-review">
            <div className="reco-manual-review-header">
                <$g.SiteMap data={[SiteMapLinks.RDM_ManualApprovalReview]} />
            </div>
            <div className="reco-manual-review-content">
                <section className="reco-manual-review-tabs">
                    <R.Tabcontrol
                        flex
                        onChange={(index) => setActiveTab(index)}
                        active={activeTab}
                    >
                        <R.TabPanel
                            tab={RMResx.RM_MA_InReviewing}
                            aria-label={RMResx.RM_MA_InReviewing}
                        >
                            {
                                activeTab === ManualTab.UnderReview &&
                                <UnderReview
                                    filterAvailableOptions={filterAvailableOptions}
                                    settingModel={settingModel}
                                    customColumns={customColumns}
                                />
                            }
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_MA_WaitDisposal}
                            aria-label={RMResx.RM_MA_WaitDisposal}
                        >
                            {
                                activeTab === ManualTab.WaitDisposal &&
                                <WaitDisposal
                                    filterAvailableOptions={filterAvailableOptions}
                                    customColumns={customColumns}
                                />
                            }
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_MA_Extended}
                            aria-label={RMResx.RM_MA_Extended}
                        >
                            {
                                activeTab === ManualTab.Extend &&
                                <Extend
                                    filterAvailableOptions={filterAvailableOptions}
                                    customColumns={customColumns}
                                />
                            }
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_MA_RelatedRecords}
                            aria-label={RMResx.RM_MA_RelatedRecords}
                        >
                            {
                                activeTab === ManualTab.RelatedRecords &&
                                <RelatedRecords
                                    filterAvailableOptions={filterAvailableOptions}
                                    customColumns={customColumns}
                                />
                            }
                        </R.TabPanel>
                        <R.TabPanel
                            tab={RMResx.RM_MA_History}
                            aria-label={RMResx.RM_MA_History}
                        >
                            {
                                activeTab === ManualTab.History &&
                                <History
                                    filterAvailableOptions={filterAvailableOptions}
                                />
                            }
                        </R.TabPanel>
                    </R.Tabcontrol>
                </section>
            </div>
        </div>
    );
};

export default ManualApproval;