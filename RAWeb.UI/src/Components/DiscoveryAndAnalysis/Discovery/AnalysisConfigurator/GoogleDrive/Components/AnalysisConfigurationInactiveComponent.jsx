import { useImperativeHandle, forwardRef } from "react";

const AnalysisConfigurationInactiveComponent = ({}, ref) => {
    useImperativeHandle(ref, () => ({
        onValidate: () => {
            return true;
        },
    }));

    return (
        <div className="reco-analysis-configurator-inactive-definition">
            <section className="reco-ac-component-title-main">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_Inactive}</span>
            </section>
            <section className="flex flex-column gap-xs">
                <strong className="reco-ac-component-title-secondary" tabIndex={0}>{RMResx.RM_FA_Discovery_InactiveConfig_SF_Record}</strong>
                <span tabIndex={0}>{RMResx.RM_FA_Discovery_InactiveConfig_GoogleDrive_Record_Desc}</span>
            </section>
        </div>
    );
};

export default forwardRef(AnalysisConfigurationInactiveComponent);
