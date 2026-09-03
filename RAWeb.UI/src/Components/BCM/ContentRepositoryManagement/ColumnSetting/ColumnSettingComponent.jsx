import ColumnSettingPanel from "./ColumnSettingPanel";
import { DetailCell, DetailList, DetailRow } from "../Common/DetailList";
import StringUtil from "../../../../Utilities/StringUtil";
import "../../../../Less/BCM/ContentRepositoryManagement/columnSetting.less";
import { NodeLevel } from "../../../../Constants/DAEnums";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import Enviroments from "../../../../Constants/Enviroments";
import { SourceFlag } from "../../../Common/Constants";

export default class ColumnSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isShowColumnSettingsPanel: { show: false },
            columnSettingInfo: {},
        };
        this.columnSettingComponent = "columnSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "columnData":
                this.data = args;
                this.setState({ columnSettingInfo: args });
                break;
        }
    }

    showColumnSettingsClick = (e) => {
        this.setState({ isShowColumnSettingsPanel: { show: true } });
    }

    saveColumnSettings = (e) => {
        this.dispatch(this.columnSettingComponent, 'onSave', (success, data) => {
            if (success) {
                this.props.refreshNodeSettings();
                this.setState({ isShowColumnSettingsPanel: { show: false } });
            }
        });
        return false;
    }

    cancelColumnSettings = () => {
        this.setState({ isShowColumnSettingsPanel: { show: false } });
    }

    hasConfigColumn() {
        return !CRMCommonUtil.guidIsEmpty(this.state.columnSettingInfo.ColumnName) || !CRMCommonUtil.guidIsEmpty(this.state.columnSettingInfo.IsUsingExistColumnName);
    }

    getKeepSpDefaultValueSettingContent = (setting) => {
        let defaultValueTitle = RMResx.RM_SPS_NoSetTermForEmptyDefaultValue_Title;
        if (this.props.sourceFlag == SourceFlag.Teams) {
            defaultValueTitle = RMResx.RM_SPS_Teams_NoSetTermForEmptyDefaultValue_Title;
        }
        if(setting.IsKeepSharePointDefaultValue && setting.SetTermForEmptyDefaultValue)
        {
            return `${RMResx.RM_JS_Common_Yes}; ${defaultValueTitle}`;
        }   
        return setting.IsKeepSharePointDefaultValue? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
    }

    render() {
        let columnSettingInfo = this.state.columnSettingInfo;

        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div className="ra-expander-fontStyle">{RMResx.RM_JS_SPS_EditTitle_ColumnSetting}</div>
                    {columnSettingInfo.Level == NodeLevel.WebApplication && <R.Scope>
                        <R.Button
                            id="raCrmColumnSettingEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_JS_SPS_EditTitle_ColumnSetting}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showColumnSettingsClick} />
                    </R.Scope>}
                </div>
                <div>
                    {columnSettingInfo && columnSettingInfo.IsUsingExistColumnName && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc)}>
                                    <span tabIndex="0">{columnSettingInfo.ExistColumnName}</span>
                                    {columnSettingInfo.SetDocLevelTermForExistColumn && <span tabIndex="0">{RMResx.RM_JS_SPS_ExistingColumn.replace("{0}", RMResx.RM_JS_SPS_UseTermSettingsDefinedInRecords)}</span>}
                                    {!columnSettingInfo.SetDocLevelTermForExistColumn && <span tabIndex="0">{RMResx.RM_JS_SPS_ExistingColumn.replace("{0}", RMResx.RM_JS_SPS_UseTermSettingsDefinedInSP)}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_ShowUniqueID}>
                                    <span tabIndex="0">{columnSettingInfo.IsShowUniqueId ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {!this.props.isCSDTenant && this.props.context.configurations.enableKeepSharePointDefaultValue && 
                                 <$g.DetailRow>
                                     <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_KeepSPDefaultValue}>
                                         <span tabIndex="0">{this.getKeepSpDefaultValueSettingContent(columnSettingInfo)}</span>
                                     </$g.DetailCell>
                                 </$g.DetailRow>
                            }
                            {RM.gData.enviromentName !== Enviroments.ChinaNorth  && columnSettingInfo.Level == NodeLevel.WebApplication && this.props.context.configurations.enableRelatedRecords && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_SP_SettingRelatedRecords}>
                                    <span tabIndex="0">{columnSettingInfo.EnableRelatedRecords ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                    {columnSettingInfo && !columnSettingInfo.IsUsingExistColumnName && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc)}>
                                    <span tabIndex="0">{columnSettingInfo.ColumnName}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_ColumnNameDescription}>
                                    <span tabIndex="0">{columnSettingInfo.Description}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {!this.props.isCSDTenant && this.props.context.configurations.enableHiddenColumn && <$g.DetailRow>
                                <$g.DetailCell label={this.props.context.configurations.hiddenColumnDetail}>
                                    {this.hasConfigColumn() && <span tabIndex="0">
                                        {columnSettingInfo.ColumnHidden ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}
                                    </span>}
                                </$g.DetailCell>
                            </$g.DetailRow>}
                            <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_DisplayColumnRequired)}>
                                    {this.hasConfigColumn() && <span tabIndex="0">{columnSettingInfo.ColumnRequired ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={this.props.context.configurations.uniqueIdDetail}>
                                    {this.hasConfigColumn() && <span tabIndex="0">{columnSettingInfo.IsShowUniqueId ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {!this.props.isCSDTenant && this.props.context.configurations.enableKeepSharePointDefaultValue && 
                                 <$g.DetailRow>
                                     <$g.DetailCell label={this.props.context.configurations.defaultTermDetail}>
                                         {this.hasConfigColumn() && <span tabIndex="0">{this.getKeepSpDefaultValueSettingContent(columnSettingInfo)}</span>}
                                     </$g.DetailCell>
                                 </$g.DetailRow>
                            }
                            {RM.gData.enviromentName !== Enviroments.ChinaNorth  && columnSettingInfo.Level == NodeLevel.WebApplication && this.props.context.configurations.enableRelatedRecords && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_SP_SettingRelatedRecords}>
                                    {this.hasConfigColumn() && <span tabIndex="0">{columnSettingInfo.EnableRelatedRecords ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>

            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowColumnSettingsPanel}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{RMResx.RM_JS_SPS_EditTitle_ColumnSetting}</span>
                </div>
                <ColumnSettingPanel
                    context={this.props.context}
                    id={this.columnSettingComponent}
                    data={this.data}
                    isCSDTenant={this.props.isCSDTenant}
                    sourceFlag={this.props.sourceFlag}
                ></ColumnSettingPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelColumnSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveColumnSettings} />
                </>
            </R.Panel>
        </div>;
    }
}