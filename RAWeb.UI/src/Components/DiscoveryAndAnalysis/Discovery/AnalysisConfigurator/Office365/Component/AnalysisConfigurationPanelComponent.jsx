import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfigurationRequester } from "../../../../Analysis/requests";
import {
    NewAnalysisOptionType,
    NewAnalysisOptionTypeI18ns,
} from "../../Constants";
import { useStableCallback } from "../../../../../Common/Hooks";
import RouterUrls from "../../../../../../Constants/RouterUrls";
import { showToast } from "../../../../../../Utilities/CommonUtil";

const getNewAnalysisOptiions = (selectedOption) => {
    return [
        {
            name: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.New),
            text: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.New),
            value: NewAnalysisOptionType.New,
            checked: selectedOption === NewAnalysisOptionType.New,
        },
        {
            name: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.Append),
            text: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.Append),
            value: NewAnalysisOptionType.Append,
            checked: selectedOption === NewAnalysisOptionType.Append,
        },
    ];
};

export const CONTAINER_MAPPING_URL = {
    "Default_ SharePoint Sites_ Group" : RMResx.RM_SPS_DefaultSharePointSitesGroup,
    "Default Office 365 Group Sites Group" : RMResx.RM_SPS_DefaultGroupTeamSiteContainer,
    "Default Private Channel Sites Container" : RMResx.RM_SPS_DefaultPrivateChannelSitesContainer,
    "Default OneDrive for Business Group": RMResx.RM_SPS_DefaultOneDriveforBusinessGroup,
}

const AnalysisConfigurationPanelComponent = ({}, ref) => {
    const [showPanel, setShowPanel] = useState(false);

    const [isValidated, setIsValidated] = useState(true);

    const [history, setHistory] = useState(null);

    const [jobInfo, setJobInfo] = useState(null);

    const [selectedAnalysisOption, setSelectedAnalysisOption] = useState(
        NewAnalysisOptionType.New
    );

    const [analysisOptions, setAnalysisOptions] = useState(
        getNewAnalysisOptiions(NewAnalysisOptionType.New)
    );

    const [containerOptions, setContainerOptions] = useState([]);

    const [specifyContainerIds, setSpecifyContainerIds] = useState([]);

    const [btnText, setBtnText] = useState(RMResx.RM_FA_Discovery_NewlyPanel_NextBtn);

    useImperativeHandle(ref, () => ({
        onShow: async (jobInfo, history) => {
            const containers =
                await ConfigurationRequester.getCanAppendOpusContainer();
                containers.forEach((item) => {
                    item.url = CONTAINER_MAPPING_URL[item.url] ?? item.url;
                });

            const options = containers.map((item) => ({
                name: item.url,
                value: item.id,
                checked: false,
            }));

            setSelectedAnalysisOption(NewAnalysisOptionType.New);
            setAnalysisOptions(
                getNewAnalysisOptiions(NewAnalysisOptionType.New)
            );
            setBtnText(RMResx.RM_FA_Discovery_NewlyPanel_NextBtn);
            setHistory(history);
            setSpecifyContainerIds([]);
            setContainerOptions(options);
            setJobInfo(jobInfo);
            setShowPanel(true);
        },
    }));

    const onHide = () => {
        setSpecifyContainerIds([]);
        setShowPanel(false);
    };

    const onAanlysisOptionChange = (value) => {
        setAnalysisOptions(getNewAnalysisOptiions(value));
        setSelectedAnalysisOption(value);
        if (value === NewAnalysisOptionType.New) {
            setIsValidated(true);
            setBtnText(RMResx.RM_FA_Discovery_NewlyPanel_NextBtn);
        } else {
            setBtnText(RMResx.RM_FA_Discovery_NewlyPanel_AnalyzeBtn);
        }
    };

    const onSelectContainer = (args) => {
        const newValue = args.newValue;
        const newIds = newValue.map((value) => {
            return value.value;
        });
        const clonedOptions = _.cloneDeep(containerOptions);
        clonedOptions.forEach((item) => {
            item.checked = newIds.some((i) => i === item.value);
        });
        setContainerOptions(clonedOptions);
        setSpecifyContainerIds(newIds);

        if (newIds.length > 0) {
            setIsValidated(true);
        }
    };

    const onSave = useStableCallback(() => {
        if (selectedAnalysisOption === NewAnalysisOptionType.New) {
            history.push({
                pathname: RouterUrls.FA_Discovery_Configuration,
                state: jobInfo,
            });
            return true;
        }

        if (specifyContainerIds.length === 0) {
            setIsValidated(false);
            return false;
        }

        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_EnsureDiscovery}</span>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.messagedialog(false);
                        const res =
                            await ConfigurationRequester.saveAppendDiscoveryConfig(
                                specifyContainerIds
                            );
                        if (res.MessageType !== 0) {
                            showToast.error(res.ErrorMessage);
                            return false;
                        }
                        history.push({
                            pathname: RouterUrls.FA_Discovery_RunJob,
                        });
                        return true;
                    },
                },
            ],
        });

        return true;
    });

    return (
        <R.Panel
            id="reco-discovery-new-analysis-panel"
            header={RMResx.RM_FA_Discovery_SuccessPage_AgainBtn}
            size={660}
            status={{ show: showPanel }}
            onHide={onHide}
            destroy={false}
        >
            <div>
                <div className="reco-ac-component-title-secondary">
                    {RMResx.RM_FA_Discovery_NewlyPanel_Desc}
                </div>
                <div style={{ marginBottom: "8px" }}>
                    <R.Radio.Group
                        id="raAnalysisRadioGroup"
                        name="reco-dc-scopes"
                        block={true}
                        items={analysisOptions}
                        onChange={onAanlysisOptionChange}
                    />
                </div>
                {selectedAnalysisOption === NewAnalysisOptionType.Append && (
                    <div style={{ paddingLeft: "30px" }}>
                        <R.Multicombobox
                            id="raContainer"
                            width={280}
                            popupMaxHeight={400}
                            items={containerOptions}
                            textField="name"
                            valueField="value"
                            checkedField="checked"
                            onChange={onSelectContainer}
                        />
                    </div>
                )}
                {!isValidated && (
                    <div className="reco-error-messages margin-top-s">
                        <div className="reco-error-message" tabIndex="0">
                            {RMResx.RM_FA_Discovery_ScopeConfig_ErrorMsg}
                        </div>
                    </div>
                )}
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
                    text={btnText}
                    onClick={onSave}
                />
            </>
        </R.Panel>
    );
};

export default forwardRef(AnalysisConfigurationPanelComponent);
