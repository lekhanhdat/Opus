import React, { useEffect, useState , useImperativeHandle,forwardRef} from "react";
import _ from "lodash";
import { ExtendType, ExtendTypeI18Ns } from "../Constants/index";

const BuildSelectorItems = (
    updateExtendType,
    options = [ ExtendType.Month, ExtendType.Year ]
) => {
    const result = [];
    for (const option of options) {
        const optionValue = ExtendTypeI18Ns.get(option);
        result.push({
            key: option,
            value: optionValue,
            checked: updateExtendType === option,
        });
    }
    return result;
};

const DisposalExtention = ({disposalExtentionSetting, onChange}, ref) => {

    const [selectorItemsType, setSelectorItemsType] = useState([]);

    useEffect(() => {
        const buildedSelectorItems = BuildSelectorItems(disposalExtentionSetting.LatestExtendType);
        setSelectorItemsType(buildedSelectorItems);
    }, [disposalExtentionSetting]);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            if ((disposalExtentionSetting.LatestExtendNumber > 120 || disposalExtentionSetting.LatestExtendNumber < 1 )&& disposalExtentionSetting.LatestExtendType == 5 ) {
                return false;
            }
            if ((disposalExtentionSetting.LatestExtendNumber > 10|| disposalExtentionSetting.LatestExtendNumber < 1 ) && disposalExtentionSetting.LatestExtendType == 6 ) {
                return false;
            }
            return true;
        }
    }));

    const onChangeMaxDelayTimes = (value) => {
        const clonedSetting = _.cloneDeep(disposalExtentionSetting);
        if(_.isNil(value) || value === "") {
            value = "1";
        }
        clonedSetting.MaxDelayTimes = parseInt(value);
        onChange(clonedSetting);
    };

    const onChangeLatestExtendNumber = (args) => {
        const clonedSetting = _.cloneDeep(disposalExtentionSetting);
        clonedSetting.LatestExtendNumber = args;
        onChange(clonedSetting);
    };

    const onChangeLatestExtendType = (args) => {
        const clonedSetting = _.cloneDeep(disposalExtentionSetting);
        clonedSetting.LatestExtendType = args.newValue.key;
        onChange(clonedSetting);
    };

    return (
        <section className="reco-manual-setting-section">
            <div className="reco-manual-setting-section-title" tabIndex="0">
                {RMResx.RM_MA_Setting_Disposal_Extention}
            </div>
            <div className="reco-manual-setting-disposal-extention">
                <div className="disposal-delay-times">
                    <div className="reco-manual-setting-text">
                        {RMResx.RM_MA_Setting_Disposal_Extention_Delay_Times}
                    </div>
                    <R.Input
                        key={Math.random()}
                        type="number"
                        min={1}
                        max={10}
                        width={100}
                        value={disposalExtentionSetting.MaxDelayTimes}
                        hasControl
                        onChange={onChangeMaxDelayTimes}
                    />
                </div>
                <div className="disposal-delay-latest">
                    <div className="reco-manual-setting-text">
                        {RMResx.RM_MA_Setting_Disposal_Extention_Delay_Latest}
                    </div>
                    <div className="reco-manual-setting-options">
                        <R.Input
                            key={Math.random()}
                            type="number"
                            width={100}
                            value={disposalExtentionSetting.LatestExtendNumber}
                            hasControl
                            onChange={onChangeLatestExtendNumber}
                        />
                        <R.Combobox
                            checkedField="checked"
                            textField="value"
                            valueField="key"
                            width={90}
                            hasFilter={false}
                            searchable={false}
                            items={selectorItemsType}
                            onChange={onChangeLatestExtendType}
                        />
                    </div>
                </div>
                <$g.ValidationMsg show={selectorItemsType.filter(item => item.checked).map(item => item.key)[0] == 5 && (disposalExtentionSetting.LatestExtendNumber > 120 || disposalExtentionSetting.LatestExtendNumber < 1 ) }>
                    {RMResx.RM_MA_Setting_Disposal_Extention_MoreThanMonth}
                </$g.ValidationMsg>
                <$g.ValidationMsg show={selectorItemsType.filter(item => item.checked).map(item => item.key)[0] == 6 && (disposalExtentionSetting.LatestExtendNumber > 10|| disposalExtentionSetting.LatestExtendNumber < 1 )}>
                    {RMResx.RM_MA_Setting_Disposal_Extention_MoreThanYear}
                </$g.ValidationMsg>
            </div>
        </section>
    );
};

export default forwardRef(DisposalExtention);