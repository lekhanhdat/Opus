import React from "react";
import { SourceFlags } from "../../../../../Constants/Constants";

export default class CheckRemoteNodeMessageBar extends React.Component {
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
        this.validate();
    }

    validate() {
        let option = {
            url: this.props.treeSource === SourceFlags.Teams ? "/api/TeamsSettingApi/CheckRemoteNodesIsInit" : "/api/SPSettingApi/CheckRemoteNodesIsInit",
            method: "Get",
        };
        fetchUtility(option).then((result) => {
            if (!result) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_DAM_RemoteNodesNotInit}>
                    <a style={{ color: "#0072d0" }} className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                this.setState({ classify: "warn", status: { show: true }, message: content, hasClose: false });
            }
        }).catch((e) => {
        });
    }

    render() {
        return <R.Messagebar classify={this.state.classify} message={this.state.message} status={this.state.status} onClose={this.onClose} hasClose={this.state.hasClose} />;
    }
}