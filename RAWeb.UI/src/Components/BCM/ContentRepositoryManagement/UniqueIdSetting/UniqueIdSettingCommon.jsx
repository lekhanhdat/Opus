import { isShowActionByDC } from "../../../../Utilities/CommonUtil";
import UniqueIdSettingPanel from "./UniqueIdSettingPanel";
const isMultiGeoMainDC = isShowActionByDC();
export default class UniqueIdSettingCommon extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isShowUniqueIdSettingsPanel: { show: false },
        };
    }

    componentReceive(type, args) {
        switch (type) {
            case "showUniqueIdPanel":
                this.showUniqueIdSettingsPanel(args);
                break;
        }
    }

    showUniqueIdSettingsPanel() {
        this.setState({ isShowUniqueIdSettingsPanel: { show: true } });
    }

    saveUniqueIdSettings = (e) => {
        this.dispatch("uniqueIdSettingPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowUniqueIdSettingsPanel: { show: false } },() => {
                    this.dispatch("generalManagementSettingPanel", 'reloadUniqueId');
                });
            }
        });
        return false;
    }

    cancelUniqueIdSettings = () => {
        this.setState({ isShowUniqueIdSettingsPanel: { show: false } });
    }

    render() {
        return <R.Panel
            header={RMResx.RM_JS_SPS_EditSetting}
            size={670}
            status={this.state.isShowUniqueIdSettingsPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="ra-setting-panel-header">{RMResx.RM_EditTemplate_BoxSettingsTitle}</span>
            </div>
            <UniqueIdSettingPanel
                supportCustomColumn={this.props.supportCustomColumn}
                id="uniqueIdSettingPanel"
                context={this.props.context}
                sourceFlag={this.props.sourceFlag}
            ></UniqueIdSettingPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelUniqueIdSettings} />
                {isMultiGeoMainDC && <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveUniqueIdSettings} />}
            </>
        </R.Panel>;
    }
}