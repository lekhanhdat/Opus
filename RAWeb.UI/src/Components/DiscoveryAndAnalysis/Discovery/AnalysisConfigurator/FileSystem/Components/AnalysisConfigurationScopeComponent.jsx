import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import RouterUrls from "../../../../../../Constants/RouterUrls";
import { DiscoveryDataSource } from "../../Constants";

const getConnectionGroups = (allGroupOptions) =>
  allGroupOptions?.map(gr => ({
    name: gr.Name,
    value: gr.Id,
    checked: false
  })) || [];

function AnalysisConfigurationScopeComponent(props, ref) {
    const { history, info, allGroups, onChange } = props;

    const [connectionGroups, setConnectionGroups] = useState([]);
    const [validateInfo, setValidateInfo] = useState({ isValidated: true });

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_ScopeConfig_ErrorMsg],
            };
            if (connectionGroups.every((item) => !item.checked)) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }
            return true;
        },
    }));

    useEffect(() => {
        const connectionGroups = getConnectionGroups(allGroups);
        if (info.specifyContainerIds.length && connectionGroups.length) {
            connectionGroups.forEach((item) => {
                info.specifyContainerIds.forEach((id) => {
                    if (item.value === id) {
                        item.checked = true;
                    }
                });
            });
        }
        setConnectionGroups(connectionGroups);
    }, [allGroups, info]);

    const onCreateConnectionGroup = () => {
        history.push({
            pathname: RouterUrls.FA_Discovery_Configuration_FSConfigConnection,
            search: `?dataSource=${DiscoveryDataSource.FileSystem}`,
        });
    }

    const onSelectConnectionGroup = (args) => {
        const newValue = args.newValue;
        const clonedScopeInfo = _.cloneDeep(info);
        const newIds = newValue.map((item) => {
            return item.value;
        });
        clonedScopeInfo.specifyContainerIds = newIds;
        onChange(clonedScopeInfo);
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
                <div className="margin-top-s margin-bottom-s">
                    <R.Multicombobox
                        id="raContainer"
                        width={400}
                        popupMaxHeight={400}
                        items={connectionGroups}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        createNewText={RMResx.RM_FA_Discovery_ConfigConnection}
                        doCreateNew={onCreateConnectionGroup}
                        onChange={onSelectConnectionGroup}
                    />
                </div>
                {!validateInfo.isValidated && (
                    <div className="reco-error-messages margin-top-s margin-bottom-s">
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
}

export default forwardRef(AnalysisConfigurationScopeComponent);
