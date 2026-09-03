import React from "react";
import { ValidateResultType } from "../CRMCommonUtil";

export default class ValidateMessageBar extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            classify: "",
            status: { show: false },
            message: null,
            hasClose: false
        };
    }

    componentDidMount() {
        this.validateCommonSettings();
    }

    validateCommonSettings() {
        let option = {
            url: "/api/DAMApi/ValidateCommonSettings",
            method: "Post",
        };
        fetchUtility(option).then((result) => {
            if (result != ValidateResultType.AllCorrect) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_GlobalStorageNotAvailable}>
                    <a style={{ color: "#0072d0" }} className="ra-link-a" href="/Root/cp/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
                </$g.I18NProvider>;
                this.setState({ classify: "warn", status: { show: true }, message: content, hasClose: false });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    render() {
        return <R.Messagebar classify={this.state.classify} message={this.state.message} status={this.state.status} onClose={this.onClose} hasClose={this.state.hasClose} />;
    }
}