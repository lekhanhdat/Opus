import React from "react";

export default class CheckAgentAvailable extends React.Component {
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
            url: this.props.url,
            method: "Get",
        };
        fetchUtility(option).then((result) => {
            if (!result) {
                let content = <$g.I18NProvider msg={RMResx.RM_FS_NOAvailableAgent_BrowserTree}>
                    <a style={{ color: "#0072d0" }} href="/Root/CP/AgentManagement">{RMResx.RM_CP_Agent_Management}</a>
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