import { useState } from "react";
import { UnitConvertsionUtil } from "../../../Utils";
import { DataSizeType, DataSizeTypeI18ns } from "../../../Constants";
import { useStableCallback } from "../../../../../Common/Hooks";
import ProgressRequester from "../../../requests/ProgressRequester";
import { useImperativeHandle } from "react";
import { forwardRef } from "react";


const ConfigurationSpeedArr = RMResx.RM_FA_Progress_ProjectionConfig_SpeedExplain.split("==Ave==");
const ConfigurationRateArr = RMResx.RM_FA_Progress_ProjectionConfig_RateExplain.split("==Ave==");
const ConfigurationPanel = ({onReload}, ref) => {
    const [showPanel, setShowPanel] = useState(false);

    const [configurationInfo, setConfigurationInfo] = useState({
        monthlyGrowthRate: 0,
        realityMonthlyGrowthRate: 0,
        realityDailyOptimizationSpeed: 0,
        odMonthlyGrowthRate: 0,
        odRealityMonthlyGrowthRate: 0,
        odRealityDailyOptimizationSpeed: 0,
        dailyOptimizationSpeed: 0,
        dataSizeUnitType: DataSizeType.TB,
    });

    useImperativeHandle(ref, () => ({
        onShow: async (configurationInfoParam) => {
            setShowPanel(true);
            setConfigurationInfo(configurationInfoParam);
        },
    }));

    const onHide = () => {
        setShowPanel(false);
    };

    const onValueChange = (field, value) => {
        const clonedInfo = _.cloneDeep(configurationInfo);
        clonedInfo[field] = value;
        setConfigurationInfo(clonedInfo);
    };

    const getDataSizeUnitOptions = () => {
        return [{
            name: DataSizeTypeI18ns.get(DataSizeType.GB),
            value: DataSizeType.GB,
        }, {
            name: DataSizeTypeI18ns.get(DataSizeType.TB),
            value: DataSizeType.TB,
        }].map((item) => {
            item.checked = item.value === configurationInfo.dataSizeUnitType;
            return item;
        });
    };

    const onSave = useStableCallback(async () => {
        await ProgressRequester.updateProjectionConfigurationInfo(configurationInfo);
        onReload();
        setShowPanel(false);
    });

    return (
        <R.Panel
            id="reco-discovery-projection-panel"
            header={RMResx.RM_FA_Progress_ProjectionButton}
            size={660}
            status={{ show: showPanel }}
            onHide={() => onHide()}
            destroy={true}
        >
            <div className="reco-projection-panel">
                <div className="reco-panel-item">
                    <div className="reco-title-font" tabIndex="0">
                        {RMResx.RM_FA_Progress_ProjectionConfig_Speed}
                        <$g.Popover>{RMResx.RM_FA_Progress_ProjectionConfig_SpeedTips}</$g.Popover>
                    </div>
                    <div className="reco-description" tabIndex="0">
                        {`${ConfigurationSpeedArr[0]} ${UnitConvertsionUtil.Convert(
                            configurationInfo.realityDailyOptimizationSpeed,
                            DataSizeType.GB
                        )}${ConfigurationSpeedArr[1]}`}
                    </div>
                    <div className="reco-content">
                        <div className="reco-input">
                            <R.Input
                                value={Math.ceil(
                                    configurationInfo.dailyOptimizationSpeed /
                                        1024 /
                                        1024 /
                                        1024
                                )}
                                type="number"
                                width={508}
                                min={0}
                                onChange={(newValue) => {
                                    onValueChange(
                                        "dailyOptimizationSpeed",
                                        newValue * 1024 * 1024 * 1024
                                    );
                                }}
                            />
                        </div>
                        <div className="reco-input-desc" tabIndex="0">{RMResx.RM_FA_Progress_ProjectionUnit_GB_Day}</div>
                    </div>
                </div>
                <div className="reco-panel-item">
                    <div className="reco-title-font" tabIndex="0">
                        {RMResx.RM_FA_Progress_ProjectionConfig_GrowthRateTitle_SP}
                        <$g.Popover>{RMResx.RM_FA_Progress_ProjectionConfig_GrowthRateTitle_Tips}</$g.Popover>
                    </div>
                    <div className="reco-description" tabIndex="0">
                        {`${ConfigurationRateArr[0]} ${UnitConvertsionUtil.Convert(
                            configurationInfo.realityMonthlyGrowthRate,
                            DataSizeType.GB
                        )}${ConfigurationRateArr[1]}`}
                    </div>
                    <div className="reco-content">
                        <div className="reco-input">
                            <R.Input
                                value={Math.ceil(
                                    configurationInfo.monthlyGrowthRate /
                                        1024 /
                                        1024 /
                                        1024 
                                )}
                                type="number"
                                width={489}
                                onChange={(newValue) => {
                                    onValueChange(
                                        "monthlyGrowthRate",
                                        newValue * 1024 * 1024 * 1024
                                    );
                                }}
                            />
                        </div>
                        <div className="reco-input-desc" tabIndex="0">{RMResx.RM_FA_Progress_ProjectionUnit_GB_Month}</div>
                    </div>
                </div>
                <div className="reco-panel-item">
                    <div className="reco-title-font" tabIndex="0">
                        {RMResx.RM_FA_Progress_ProjectionConfig_GrowthRateTitle_Od}
                        <$g.Popover>{RMResx.RM_FA_Progress_ProjectionConfig_GrowthRateTitle_Tips}</$g.Popover>
                    </div>
                    <div className="reco-description" tabIndex="0">
                        {`${ConfigurationRateArr[0]} ${UnitConvertsionUtil.Convert(
                            configurationInfo.odRealityMonthlyGrowthRate,
                            DataSizeType.GB
                        )}${ConfigurationRateArr[1]}`}
                    </div>
                    <div className="reco-content">
                        <div className="reco-input">
                            <R.Input
                                value={Math.ceil(
                                    configurationInfo.odMonthlyGrowthRate /
                                        1024 /
                                        1024 /
                                        1024 
                                )}
                                type="number"
                                width={489}
                                onChange={(newValue) => {
                                    onValueChange(
                                        "odMonthlyGrowthRate",
                                        newValue * 1024 * 1024 * 1024
                                    );
                                }}
                            />
                        </div>
                        <div className="reco-input-desc" tabIndex="0">{RMResx.RM_FA_Progress_ProjectionUnit_GB_Month}</div>
                    </div>
                </div>
                <div className="reco-panel-item">
                    <div id="ariaUnit" className="reco-title-font reco-title-bottom">
                        {RMResx.RM_FA_Progress_ProjectionConfig_Units}
                    </div>
                    <div className="reco-content">
                        <R.Combobox
                            width={"100%"}
                            popupMaxHeight={400}
                            searchable={false}
                            items={getDataSizeUnitOptions()}
                            textField="name"
                            valueField="value"
                            onChange={(args) => {
                                onValueChange("dataSizeUnitType", args.newValue.value);
                            }}
                            aria="#ariaUnit"
                        />
                    </div>
                </div>
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => onHide()}
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

export default forwardRef(ConfigurationPanel);
