export default class Rackspace extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            params: { cdn: false },
        };
    }

    componentInit() {
        let that = this;
        $(document).ready(function () {
            $("#raStorageSettingsAPIKeyIpt").focus(function () {
                that.state.params.secret = "";
                that.setState({ params: RM.deepcopy(that.state.params) });
            });
        });
    }

    getParams = () => {
        return this.state.params;
    }

    setData = (args) => {
        if (args.Id) {
            args.mCurrentXRI.Params.cdn = args.mCurrentXRI.Params.cdn.toLowerCase() == "true" ? true : false;
        } else {
            Object.assign(args.mCurrentXRI.Params, this.state.params);
        }
        this.setState({ params: args.mCurrentXRI.Params });
    }

    onContainerNameChanged = (value) => {
        this.state.params.containername = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onUsernameChanged = (value) => {
        this.state.params.name = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onAPIKeyChanged = (value) => {
        this.state.params.secret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onCDNEnableChanged = (value) => {
        this.state.params.cdn = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_RackSpace_Container_Name"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsContainerNameIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Storage}
                                    value={this.state.params.containername}
                                    onChange={this.onContainerNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_RackSpace_Container_Name"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_RackSpace_Username"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsUsernameIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Admin}
                                    value={this.state.params.name}
                                    onChange={this.onUsernameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_RackSpace_Username"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_RackSpace_API_Key"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsAPIKeyIpt"
                                    type="password"
                                    value={this.state.params.secret}
                                    onChange={this.onAPIKeyChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_RackSpace_API_Key"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <R.Checkbox
                            id="raCDNChk"
                            text={RMResx["MediaStorage_RackSpace_CDN_Enabled"]}
                            title={RMResx["MediaStorage_RackSpace_CDN_Enabled"]}
                            checked={this.state.params.cdn}
                            onChange={this.onCDNEnableChanged}
                        />
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}