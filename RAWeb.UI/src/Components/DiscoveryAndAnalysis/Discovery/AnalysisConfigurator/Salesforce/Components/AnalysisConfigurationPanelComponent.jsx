import { forwardRef, useImperativeHandle, useState } from "react";
import {
    DiscoveryDataSource,
    NewAnalysisOptionType,
    NewAnalysisOptionTypeI18ns,
} from "../../Constants";
import { useStableCallback } from "../../../../../Common/Hooks";
import RouterUrls from "../../../../../../Constants/RouterUrls";

const getNewAnalysisOptions = () => {
    return [
        {
            name: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.New),
            text: NewAnalysisOptionTypeI18ns.get(NewAnalysisOptionType.New),
            value: NewAnalysisOptionType.New,
            checked: true,
        },
    ];
};

const AnalysisConfigurationPanelComponent = ({}, ref) => {
    const [showPanel, setShowPanel] = useState(false);
    const [jobInfo, setJobInfo] = useState(null);
    const [history, setHistory] = useState(null);

    useImperativeHandle(ref, () => ({
        onShow: (jobInfo, history) => {
            setJobInfo(jobInfo);
            setHistory(history);
            setShowPanel(true);
        },
    }))

    const onHide = () => {
        setShowPanel(false);
    };

    const onSave = useStableCallback(() => {
        history.push({
            pathname: RouterUrls.FA_Discovery_Configuration,
            state: jobInfo,
            search: `?dataSource=${DiscoveryDataSource.Salesforce}`
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
                        items={getNewAnalysisOptions()}
                        onChange={() => {}}
                    />
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
                    text={RMResx.RM_FA_Discovery_NewlyPanel_NextBtn}
                    onClick={onSave}
                />
            </>
        </R.Panel>
    );
};

export default forwardRef(AnalysisConfigurationPanelComponent);
