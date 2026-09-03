import React, {
    useEffect,
    useState,
    useImperativeHandle,
    forwardRef,
} from "react";
import _ from "lodash";
import { DiscoveryM365Scope } from "../../Constants";
import { SourceFlags } from "../../../../../../Constants/Constants";
import { SourceFlagI18Ns } from "../../../../../Common/Constants";

const getComboxContainers = (containers) => {
    var newArr = [];
    containers.forEach((element) => {
        let url = element.Url;
        if (element.Url == "Default_ SharePoint Sites_ Group") {
            url = RMResx.RM_SPS_DefaultSharePointSitesGroup;
        } else if (element.Url == "Default Office 365 Group Sites Group") {
            url = RMResx.RM_SPS_DefaultGroupTeamSiteContainer;
        } else if (element.Url == "Default OneDrive for Business Group") {
            url = RMResx.RM_SPS_DefaultOneDriveforBusinessGroup;
        } else if (element.Url == "Default Private Channel Sites Container") {
            url = RMResx.RM_SPS_DefaultPrivateChannelSitesContainer;
        }

        newArr.push({
            name: url,
            value: element.Id,
            checked: false,
        });
    });
    return newArr;
};

const getContentSourceOptions = (checkedContentSources) => {
    return [
        {
            value: SourceFlags.SP,
            text: SourceFlagI18Ns.get(SourceFlags.SP),
            checked: checkedContentSources.some(
                (item) => item === SourceFlags.SP
            ),
        },
        {
            value: SourceFlags.OneDrive,
            text: SourceFlagI18Ns.get(SourceFlags.OneDrive),
            checked: checkedContentSources.some(
                (item) => item === SourceFlags.OneDrive
            ),
        },
    ];
};

const AnalysisConfigurationScopeComponent = ({ allContainer, info, onChange }, ref) => {
    const [contentSourceOptions, setContentSourceOptions] = useState(
        getContentSourceOptions([])
    );

    const [containers, setContainers] = useState([]);

    const [validateInfo, setValidateInfo] = useState({ isValidated: true });

    useEffect(() => {
        setContentSourceOptions(
            getContentSourceOptions(info.contentSources)
        );
        let convertContainers = getComboxContainers(allContainer);
        if (
            info.specifyContainerIds.length > 0 &&
            convertContainers.length > 0
        ) {
            convertContainers.forEach((value) => {
                info.specifyContainerIds.forEach((id) => {
                    if (value.value === id) {
                        value.checked = true;
                    }
                });
            });
        }
        setContainers(convertContainers);
    }, [info, allContainer]);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_ScopeConfig_ErrorMsg],
            };
            if (
                (info.scopeType === DiscoveryM365Scope.Specify &&
                    containers.every((i) => !i.checked)) ||
                (info.scopeType === DiscoveryM365Scope.DataSource &&
                    info.contentSources.length === 0)
            ) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }
            return true;
        },
    }));

    const onContentSourceChange = (value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.contentSources = value;
        onChange(clonedInfo);

        if (value.length > 0) {
            setValidateInfo({ isValidated: true });
        }
    };

    const onScopeOptionChange = (value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.scopeType = value;
        onChange(clonedInfo);

        if (value === DiscoveryM365Scope.DataSource) {
            setValidateInfo({ isValidated: true });
        }
    };

    const onSelectContainer = (args) => {
        const newValue = args.newValue;
        const clonedInfo = _.cloneDeep(info);
        const newIds = newValue.map((value) => {
            return value.value;
        });
        clonedInfo.specifyContainerIds = newIds;
        onChange(clonedInfo);
        setValidateInfo({ isValidated: true });
    };

    return (
        <div className="reco-analysis-configurator-scope-info">
            <section className="reco-ac-component-title-main">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_Scope}</span>
            </section>
            <section>
                <div className="reco-ac-component-title-secondary" tabIndex="0">
                    {RMResx.RM_FA_Discovery_Config_ScopeTitle}
                    <span className="reco-ac-required-input">*</span>
                </div>
                <div style={{ marginBottom: "8px" }}>
                    <R.Radio
                        name="raScopeRadioGroup"
                        text={RMResx.RM_FA_Discovery_JobPage_Scope_DataSource}
                        value={DiscoveryM365Scope.DataSource}
                        checked={info.scopeType == DiscoveryM365Scope.DataSource}
                        onChange={() =>
                            onScopeOptionChange(DiscoveryM365Scope.DataSource)
                        }
                    />
                </div>
                {info.scopeType === DiscoveryM365Scope.DataSource && (
                    <div style={{ paddingLeft: "30px" }}>
                        <R.Checkbox.Group
                            name="raScopeCheckboxGroup"
                            items={contentSourceOptions}
                            onChange={onContentSourceChange}
                            block={true}
                        />
                    </div>
                )}
                {info.scopeType === DiscoveryM365Scope.DataSource &&
                    !validateInfo.isValidated && (
                        <div className="reco-error-messages margin-top-s">
                            {validateInfo.errorMessages.map((item, index) => (
                                <div
                                    className="reco-error-message"
                                    key={index}
                                    tabIndex="0"
                                >
                                    {item}
                                </div>
                            ))}
                        </div>
                    )}
                <div style={{ marginBottom: "8px", marginTop: "8px" }}>
                    <R.Radio
                        name="raScopeRadioGroup"
                        text={
                            RMResx.RM_FA_Discovery_ScopeConfig_SpecifyDiscover
                        }
                        value={DiscoveryM365Scope.Specify}
                        checked={info.scopeType == DiscoveryM365Scope.Specify}
                        onChange={() =>
                            onScopeOptionChange(DiscoveryM365Scope.Specify)
                        }
                    />
                </div>
                {info.scopeType === DiscoveryM365Scope.Specify && (
                    <div style={{ paddingLeft: "30px" }}>
                        <R.Multicombobox
                            id="raContainer"
                            width={280}
                            popupMaxHeight={400}
                            items={containers}
                            textField="name"
                            valueField="value"
                            checkedField="checked"
                            onChange={onSelectContainer}
                        />
                    </div>
                )}
                {info.scopeType === DiscoveryM365Scope.Specify &&
                    !validateInfo.isValidated && (
                        <div className="reco-error-messages margin-top-s">
                            {validateInfo.errorMessages.map((item, index) => (
                                <div
                                    className="reco-error-message"
                                    key={index}
                                    tabIndex="0"
                                >
                                    {item}
                                </div>
                            ))}
                        </div>
                    )}
            </section>
        </div>
    );
};

export default forwardRef(AnalysisConfigurationScopeComponent);
