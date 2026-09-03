import RelatedAppSettingPanel from "./RelatedAppSettingPanel";
import "../../../../Less/BCM/ContentRepositoryManagement/relatedAppSetting.less";

export default class RelatedAppSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isShowPanel: { show: false },
        };
    }

    componentReceive(type) {
        switch (type) {
            case "showRelatedAppSettingPanel":
                this.showPanel();
                break;
        }
    }

    showPanel() {
        this.setState({ isShowPanel: { show: true } });
    }

    saveSettings = (e) => {
        this.dispatch("relatedAppSettingPanel", 'onSave', (success) => {
            if (success) {
                this.setState({ isShowPanel: { show: false } });
            }
        });
        return false;
    }

    cancelSettings = () => {
        this.setState({ isShowPanel: { show: false } });
    }

    render() {
        return <R.Panel
            header={RMResx.RM_JS_SPS_EditSetting}
            size={670}
            status={this.state.isShowPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="ra-setting-panel-header">{RMResx.RM_JS_LSP_RelatedRecordsAppSetting}</span>
            </div>
            <RelatedAppSettingPanel id="relatedAppSettingPanel" />
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSettings} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveSettings} />
            </>
        </R.Panel>;
    }
}