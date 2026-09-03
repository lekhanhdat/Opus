import { forwardRef, useEffect, useImperativeHandle, useState } from "react";

const getOrganizationOptions = (organizationOptions) => {
    const newOptions = [];
    organizationOptions.forEach((item) => {
        newOptions.push({
            name: item.Name, // Apply i18n in here
            value: item.Id,
            email: item.Email,
            checked: false,
        });
    });

    return newOptions;
};

function AnalysisConfigurationScopeComponent(props, ref) {
    const { info, allOrganization, onChange } = props;

    const [organizationOptions, setOrganizationOptions] = useState([]);
    const [validateInfo, setValidateInfo] = useState({ isValidated: true });

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_ScopeConfig_ErrorMsg],
            };
            if (organizationOptions.every((item) => !item.checked)) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }
            return true;
        },
    }));

    useEffect(() => {
        const fetchData = async () => {
            const organizationInfos = getOrganizationOptions(allOrganization);
            if (info?.organizations.length && organizationInfos.length) {
                organizationInfos.forEach((item) => {
                    info.organizations.forEach((org) => {
                        if (item.value === org.Id) {
                            item.checked = true;
                        }
                    });
                });
            }
            setOrganizationOptions(organizationInfos);
        };

        fetchData();
    }, [info, allOrganization]);

    const onSelectOrganization = (args) => {
        // Comment, will change to this in the future!!!
        // const newValues = args.newValue;
        // const clonedScopeInfo = _.cloneDeep(info);
        // const newIds = newValues.map((item) => {
        //     return {
        //         Id: item.value,
        //         Name: item.name,
        //         Email: item.email,
        //     };
        // });
        // clonedScopeInfo.organizations = newIds;
        // onChange(clonedScopeInfo);
        // setValidateInfo({ isValidated: true });
        const newValue = args.newValue;
        const clonedScopeInfo = _.cloneDeep(info);
        clonedScopeInfo.organizations = [{
            Id: newValue.value,
            Name: newValue.name,
            Email: newValue.email,
        }];
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
                    {/* Comment, will change to this in the future!!! */}
                    {/* <R.Multicombobox
                        id="raContainer"
                        width={400}
                        popupMaxHeight={400}
                        items={organizationOptions}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        onChange={onSelectOrganization}
                    /> */}
                    <R.Combobox
                        id="raContainer"
                        width={400}
                        popupMaxHeight={400}
                        items={organizationOptions}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        onChange={onSelectOrganization}
                        searchPlaceholder={RMResx.RM_FA_Discovery_SF_SearchPlaceholder}
                        matchFields={{ name: false, email: false }}
                        template={(item) => {
                            return (
                                <div className="reco-salesforce-discovery-custom-combobox-item">
                                    <div className="reco-salesforce-discovery-custom-combobox-item-name">
                                        {item.name}
                                    </div>
                                    <div className="reco-salesforce-discovery-custom-combobox-item-email">
                                        {item.email}
                                    </div>
                                </div>
                            );
                        }}
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
