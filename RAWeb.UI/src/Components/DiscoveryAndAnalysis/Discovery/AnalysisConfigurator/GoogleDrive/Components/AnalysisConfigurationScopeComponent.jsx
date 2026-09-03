import { forwardRef, useEffect, useImperativeHandle, useState } from "react";

const driverNameMap = {
    "Default_ Google_ SharedDrive_ Group": RMResx.RM_GoogleSharedDrive_Default_Container,
    "Default_ GoogleUser_ Group": RMResx.RM_GoogleUser_Default_Container
};

export const AutoFitDriverName = (driverUrl) => driverNameMap[driverUrl] ?? driverUrl;

const getDriveContainers = (driveOptions) => {
    const newOptions = [];
    driveOptions.forEach((item) => {
        newOptions.push({
            name: AutoFitDriverName(item.Url),
            value: item.Id,
            email: item.Email,
            checked: false,
        });
    });

    return newOptions;
};

function AnalysisConfigurationScopeComponent(props, ref) {
    const { info, allDriveContainer, onChange } = props;

    const [driveContainers, setDriveContainers] = useState([]);
    const [validateInfo, setValidateInfo] = useState({ isValidated: true });

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_ScopeConfig_ErrorMsg],
            };
            if (driveContainers.every((item) => !item.checked)) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }
            return true;
        },
    }));

    useEffect(() => {
        const driveContainers = getDriveContainers(allDriveContainer);
        if (
            info.specifyContainerIds.length &&
            driveContainers.length
        ) {
            driveContainers.forEach((item) => {
                info.specifyContainerIds.forEach((id) => {
                    if (item.value === id) {
                        item.checked = true;
                    }
                });
            });
        }
        setDriveContainers(driveContainers);
    }, [allDriveContainer, info]);

    const onSelectDriveContainer = (args) => {
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
                        items={driveContainers}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        onChange={onSelectDriveContainer}
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
