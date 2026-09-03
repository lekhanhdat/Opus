function ArchivingSettingComponent({ nodeSetting }) {
    const getSettingRuleNames = () => {
        const associatedRules = nodeSetting.ArchiverRuleInfos;
        if (associatedRules && associatedRules.length) {
            return (
                <div className="ra-setting-ruleNames-container">
                    {associatedRules.map((o, index) => (
                        <div key={index}>{`${index + 1}. ${o.RuleName}`}</div>
                    ))}
                </div>
            );
        }
    };

    const getOptions = () => {
        if (Object.keys(nodeSetting).length > 0) {
            const optionList = [];
            // if (this.state.nodeSettingInfo.IsWorkflowDefinition && this.props.mode != TabIndex.Archive) {
            //     optionList.push({ "name": RMResx.RM_AR_SPS_Options_Workflow });
            // }
            if (nodeSetting.isIncludeManagedMetadataService) {
                optionList.push({ name: RMResx.RM_AR_SPS_Options_Managed });
            }
            if (nodeSetting.isEnableSuperUserDecrypt) {
                optionList.push({ name: RMResx.RM_AR_SPS_Options_SuperUser });
            }
            if (nodeSetting.isEnableRemoveRetentionLabel) {
                optionList.push({
                    name: RMResx.RM_AR_SPS_Options_Remove_RetentionLabel,
                });
            }
            return optionList.map((op) => op.name).join("; ");
        }
    };

    return (
        <R.Expander title={RMResx.RM_AR_SPS_EditTitle_ArchiveSetting} level={2}>
            <div>
                <$g.DetailList>
                    <$g.DetailRow>
                        <$g.DetailCell label={RMResx.RM_JS_SPS_RuleNames_Title}>
                            <span tabIndex="0">{getSettingRuleNames()}</span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell label={RMResx.RM_AR_SPS_Title_Options}>
                            <span tabIndex="0">{getOptions()}</span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                </$g.DetailList>
            </div>
        </R.Expander>
    );
}

export default ArchivingSettingComponent;
