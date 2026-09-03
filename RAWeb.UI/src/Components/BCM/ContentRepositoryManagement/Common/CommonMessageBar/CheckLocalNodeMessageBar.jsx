import React from "react";

export default class CheckLocalNodeMessageBar extends React.Component {
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
            url: "/api/SPOnPremBrowse/CheckLocalNodesIsInit",
            method: "Get",
        };
        fetchUtility(option).then((result) => {
            if (!result) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_DAM_SPORemoteNodesNotInit}>
                    <a  className="ra-link-a" href="/Root/CP/TimerJobSettings">{RMResx.RM_CP_TimerJob_PageTitle}</a>
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