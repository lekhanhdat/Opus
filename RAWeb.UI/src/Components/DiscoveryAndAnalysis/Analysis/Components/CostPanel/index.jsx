import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import _ from "lodash";
import "./index.less";
import { useStableCallback } from "../../../../Common/Hooks";
import { showToast } from "../../../../../Utilities/CommonUtil";

const CostPanel = ({}, ref) => {
    const validationRef = useRef(null);

    const [costInfo, setCostInfo] = useState({});

    const [showPanel, setShowPanel] = useState(false);

    useImperativeHandle(ref, () => ({
        onShow: (costInfos) => {
            let clonedCostInfo = _.cloneDeep(costInfos);
            if (_.isNil(clonedCostInfo)) {
                clonedCostInfo = {
                    spFreeStorage: 0,
                    spStoragePrice: 0.2,
                    odFreeStorage: 0,
                    odStoragePrice: 0.2,
                    archivedDataStoragePrice: 0,
                };
            }
            setCostInfo(clonedCostInfo);
            setShowPanel(true);
        },
    }));

    const onValueChange = (column, value) => {
        const clonedCostInfo = _.cloneDeep(costInfo);
        clonedCostInfo[column] = value;
        setCostInfo(clonedCostInfo);
    };

    const onSave = useStableCallback(async () => {
        if (!$$.verify(validationRef.current)) {
            return false;
        }

        const clonedCostInfo = _.cloneDeep(costInfo);
        $$.loading(true);
        const requestOption = {
            url: "/api/RMDiscoveryOffice365ConfigurationApi/AddOrUpdateCostSavingInfo",
            data: clonedCostInfo,
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            setShowPanel(false);
            showToast.success(RMResx.RM_FA_CostSaving_SaveSuccessful);
        } else {
            showToast.error(response.ErrorMessage);
            return false;
        }
    });

    return (
        <R.Panel
            id="reco-analysis-cost-panel"
            header={RMResx.RM_FA_CostSaving_Configuration}
            size={660}
            status={{ show: showPanel }}
            onHide={() => setShowPanel(false)}
            destroy={true}
        >
            <div className="reco-cost-content">
                <R.Validation>
                    <div ref={validationRef}>
                        <R.Expander
                            title={RMResx.RM_FA_Discovery_Saving_Opt_SP}
                            level={2}
                            status={{ show: true }}
                        >
                            <div>
                                <div
                                    className="reco-cost-option"
                                    style={{ marginTop: 0 }}
                                >
                                    <div>
                                        <span className="reco-cost-title require">
                                            {
                                                RMResx.RM_FA_CostSaving_TotalStorageTitle
                                            }
                                        </span>
                                        <$g.Popover>
                                            {
                                                RMResx.RM_FA_CostSaving_TotalStorageDes
                                            }
                                        </$g.Popover>
                                    </div>
                                    <div className="reco-cost-value">
                                        <div className="reco-cost-input">
                                            <R.Validation
                                                element="Input"
                                                require={
                                                    RMResx[
                                                        "Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"
                                                    ]
                                                }
                                            >
                                                <R.Input
                                                    id="raTotalNumIpt"
                                                    type="number"
                                                    width={"100%"}
                                                    min={0}
                                                    value={
                                                        costInfo.spFreeStorage
                                                    }
                                                    onChange={(value) =>
                                                        onValueChange(
                                                            "spFreeStorage",
                                                            value
                                                        )
                                                    }
                                                    aria={{
                                                        ariaLabel:
                                                            RMResx.RM_FA_CostSaving_TotalStorageTitle,
                                                    }}
                                                />
                                            </R.Validation>
                                        </div>
                                        <div
                                            className="reco-cost-unit"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_DSB_Unit_GB}
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-cost-option">
                                    <div>
                                        <span className="reco-cost-title require">
                                            {
                                                RMResx.RM_FA_CostSaving_SPStoragePriceTitle
                                            }
                                        </span>
                                        <$g.Popover>
                                            {
                                                RMResx.RM_FA_CostSaving_SPStoragePriceDes
                                            }
                                        </$g.Popover>
                                    </div>
                                    <div className="reco-cost-value">
                                        <div className="reco-cost-input">
                                            <R.Validation
                                                element="Input"
                                                require={
                                                    RMResx[
                                                        "Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"
                                                    ]
                                                }
                                            >
                                                <R.Input
                                                    id="raSPPriceNumIpt"
                                                    type="number"
                                                    width={"100%"}
                                                    min={0}
                                                    float={4}
                                                    value={
                                                        costInfo.spStoragePrice
                                                    }
                                                    onChange={(value) =>
                                                        onValueChange(
                                                            "spStoragePrice",
                                                            value
                                                        )
                                                    }
                                                    aria={{
                                                        ariaLabel:
                                                            RMResx.RM_FA_CostSaving_SPStoragePriceTitle,
                                                    }}
                                                />
                                            </R.Validation>
                                        </div>
                                        <div
                                            className="reco-cost-unit"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_DSB_ConfigUnit}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </R.Expander>

                        <R.Expander
                            title={RMResx.RM_FA_Discovery_Saving_Opt_OD}
                            level={2}
                            status={{ show: true }}
                        >
                            <div>
                                <div
                                    className="reco-cost-option"
                                    style={{ marginTop: 0 }}
                                >
                                    <div>
                                        <span className="reco-cost-title require">
                                            {RMResx.RM_FA_CostSaving_TotalStorageTitle_Od}
                                        </span>
                                        <$g.Popover>
                                            {
                                                RMResx.RM_FA_CostSaving_TotalStorageDes_Od
                                            }
                                        </$g.Popover>
                                    </div>
                                    <div className="reco-cost-value">
                                        <div className="reco-cost-input">
                                            <R.Validation
                                                element="Input"
                                                require={
                                                    RMResx[
                                                        "Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"
                                                    ]
                                                }
                                            >
                                                <R.Input
                                                    id="raTotalNumIpt"
                                                    type="number"
                                                    width={"100%"}
                                                    min={0}
                                                    value={
                                                        costInfo.odFreeStorage
                                                    }
                                                    onChange={(value) =>
                                                        onValueChange(
                                                            "odFreeStorage",
                                                            value
                                                        )
                                                    }
                                                    aria={{
                                                        ariaLabel:
                                                            RMResx.RM_FA_CostSaving_TotalStorageTitle_Od,
                                                    }}
                                                />
                                            </R.Validation>
                                        </div>
                                        <div
                                            className="reco-cost-unit"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_DSB_Unit_GB}
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-cost-option">
                                    <div>
                                        <span className="reco-cost-title require">
                                            {
                                                RMResx.RM_FA_CostSaving_StoragePriceTitle_Od
                                            }
                                        </span>
                                        <$g.Popover>
                                            {
                                                RMResx.RM_FA_CostSaving_StoragePriceDes_Od
                                            }
                                        </$g.Popover>
                                    </div>
                                    <div className="reco-cost-value">
                                        <div className="reco-cost-input">
                                            <R.Validation
                                                element="Input"
                                                require={
                                                    RMResx[
                                                        "Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"
                                                    ]
                                                }
                                            >
                                                <R.Input
                                                    id="raSPPriceNumIpt"
                                                    type="number"
                                                    width={"100%"}
                                                    min={0}
                                                    float={4}
                                                    value={
                                                        costInfo.odStoragePrice
                                                    }
                                                    onChange={(value) =>
                                                        onValueChange(
                                                            "odStoragePrice",
                                                            value
                                                        )
                                                    }
                                                    aria={{
                                                        ariaLabel:
                                                            RMResx.RM_FA_CostSaving_StoragePriceTitle_Od
                                                    }}
                                                />
                                            </R.Validation>
                                        </div>
                                        <div
                                            className="reco-cost-unit"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_DSB_ConfigUnit}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </R.Expander>

                        <R.Expander
                            title={RMResx.RM_FA_CostSaving_Archived_Title}
                            level={2}
                            status={{ show: true }}
                        >
                            <div className="reco-cost-option" style={{marginTop: 0}}>
                                <div className="margin-bottom-s">
                                    <span className="reco-cost-title require">
                                        {
                                            RMResx.RM_FA_CostSaving_ArchivedDataPriceTitle
                                        }
                                    </span>
                                </div>
                                <div className="reco-cost-value">
                                    <div className="reco-cost-input">
                                        <R.Validation
                                            element="Input"
                                            require={
                                                RMResx[
                                                    "Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"
                                                ]
                                            }
                                        >
                                            <R.Input
                                                id="raArchivedDataNumIpt"
                                                type="number"
                                                width={"100%"}
                                                min={0}
                                                float={4}
                                                value={
                                                    costInfo.archivedDataStoragePrice
                                                }
                                                onChange={(value) =>
                                                    onValueChange(
                                                        "archivedDataStoragePrice",
                                                        value
                                                    )
                                                }
                                                aria={{
                                                    ariaLabel:
                                                        RMResx.RM_FA_CostSaving_ArchivedDataPriceTitle,
                                                }}
                                            />
                                        </R.Validation>
                                    </div>
                                    <div
                                        className="reco-cost-unit"
                                        tabIndex="0"
                                    >
                                        {RMResx.RM_DSB_ConfigUnit}
                                    </div>
                                </div>
                            </div>
                        </R.Expander>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setShowPanel(false)}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSave}
                />
            </>
        </R.Panel>
    );
};

export default forwardRef(CostPanel);
