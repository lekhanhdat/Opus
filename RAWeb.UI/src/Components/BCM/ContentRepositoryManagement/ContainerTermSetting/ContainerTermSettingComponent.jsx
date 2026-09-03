import ContainerTermSettingPanel from "./ContainerTermSettingPanel";
import { DetailCell, DetailList, DetailRow } from "../Common/DetailList";
import StringUtil from "../../../../Utilities/StringUtil";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import "../../../../Less/BCM/ContentRepositoryManagement/containerTermSetting.less";

export default class ContainerTermSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        this.state = {
            isShowContainerTermSettingsPanel: { show: false },
            containerSettingInfo: {},
        };
        this.containerTermSetting = "containerTermSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "containerData":
                this.node = args;
                this.setState({ containerSettingInfo: args });
                break;
        }
    }

    showContainerTermSettingsClick = (e) => {
        if (!(this.props.checkMissingConfig && this.props.checkMissingConfig())) {
            this.setState({ isShowContainerTermSettingsPanel: { show: true } });
        }
    };

    saveContainerTermSettings = (e) => {
        this.dispatch(this.containerTermSetting, 'onSave', (success, data) => {
            this.props.refreshNodeSettings();
            this.setState({ isShowContainerTermSettingsPanel: { show: false } });
        });
        return false;
    }

    cancelContainerTermSettings = () => {
        this.setState({ isShowContainerTermSettingsPanel: { show: false } });
    }

    render() {
        let containerSettingInfo = this.state.containerSettingInfo;
        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div data-tooltip="ifneed" className="ra-expander-fontStyle">{RMResx.RM_JS_SPS_EditTitle_ContainerLevelTermSetting}</div>
                    <R.Scope>
                        <R.Button
                            id="raCrmContainerLevelEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_JS_SPS_EditTitle_ContainerLevelTermSetting}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showContainerTermSettingsClick} />
                    </R.Scope>
                </div>
                <div>
                    {containerSettingInfo && containerSettingInfo.Level == 2 && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_TermName)}>
                                    <span tabIndex="0">
                                        {(containerSettingInfo.IsClassificationTermDeprecated || containerSettingInfo.IsClassificationTermRemoved) && <div className="info-error">
                                            <div className="info-error-icon"><span className="fia-status-error info-error-tab"></span></div>
                                        </div>}
                                        <div className="ra-setting-termPath">
                                            <span>{containerSettingInfo.ContainerTermFullPath}</span>
                                        </div>
                                        {containerSettingInfo.IsClassificationTermRemoved &&
                                            <span className="info-error-font">{RMResx.RM_JS_SPS_TermDelete}</span>}
                                        {!containerSettingInfo.IsClassificationTermRemoved && containerSettingInfo.IsClassificationTermDeprecated &&
                                            <span className="info-error-font">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
                                    </span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EditKey_ColumnNameDescription)}>
                                    <span tabIndex="0">{containerSettingInfo.DescriptionOfContainer}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {this.enableRecordsArchiver && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_EnableInheritParentTerm}>
                                    <span tabIndex="0">{containerSettingInfo.IsInheritParentTerm ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                    {containerSettingInfo && containerSettingInfo.Level != 2 && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EditKey_EnableClassification)}>
                                    <span tabIndex="0">{containerSettingInfo.isEnableClassification ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_TermName)}>
                                    <span tabIndex="0">
                                        {(containerSettingInfo.IsClassificationTermDeprecated || containerSettingInfo.IsClassificationTermRemoved) && <div className="info-error">
                                            <div className="info-error-icon"><span className="fia-status-error info-error-tab"></span></div>
                                        </div>}
                                        <div className="ra-setting-termPath">
                                            <span>{containerSettingInfo.ContainerTermFullPath}</span>
                                        </div>
                                        {containerSettingInfo.IsClassificationTermRemoved &&
                                            <span className="info-error-font">{RMResx.RM_JS_SPS_TermDelete}</span>}
                                        {!containerSettingInfo.IsClassificationTermRemoved && containerSettingInfo.IsClassificationTermDeprecated &&
                                            <span className="info-error-font">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
                                    </span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EditKey_ColumnNameDescription)}>
                                    <span tabIndex="0">{containerSettingInfo.DescriptionOfContainer}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {this.enableRecordsArchiver && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_EnableInheritParentTerm}>
                                    <span tabIndex="0">{containerSettingInfo.IsInheritParentTerm ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>

            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowContainerTermSettingsPanel}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{RMResx.RM_JS_SPS_EditTitle_ContainerLevelTermSetting}</span>
                </div>
                <ContainerTermSettingPanel
                    context={this.props.context}
                    id={this.containerTermSetting}
                    data={this.node}
                ></ContainerTermSettingPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelContainerTermSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveContainerTermSettings} />
                </>
            </R.Panel>
        </div>;
    }
}