export default class AzureBlob extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            params: { accesspoint: "https://blob.core.windows.net" },
            storageConfigDisabled: false,
        };
    }

    componentInit() {
        let that = this;
        $(document).ready(function () {
            $("#raStorageSettingsAccountKeyIpt").focus(function () {
                that.state.params.secret = "";
                that.setState({ params: RM.deepcopy(that.state.params) });
            });
        });
    }

    getParams = () => {
        return this.state.params;
    }

    setData = (args) => {
        if (!args.Id) {
            Object.assign(args.mCurrentXRI.Params, this.state.params);
        }
        this.setState({ params: args.mCurrentXRI.Params });
    }

    setDisabled = (disabled) => {
        this.setState({ storageConfigDisabled: disabled });
    }

    onAccessPointChanged = (value) => {
        this.state.params.accesspoint = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onContainerNameChanged = (value) => {
        this.state.params.containername = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onAccountNameChanged = (value) => {
        this.state.params.name = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onAccountKeyChanged = (value) => {
        this.state.params.secret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Azure_Access_Point"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsAccessPointIpt"
                                    type="text"
                                    disabled={this.state.storageConfigDisabled}
                                    value={this.state.params.accesspoint ? this.state.params.accesspoint : "https://blob.core.windows.net"}
                                    onChange={this.onAccessPointChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Azure_Access_Point"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Azure_Container_Name"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsContainerNameIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Storage}
                                    disabled={this.state.storageConfigDisabled}
                                    value={this.state.params.containername}
                                    onChange={this.onContainerNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Azure_Container_Name"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Azure_Account_Name"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsAccountNameIpt"
                                    type="text"
                                    disabled={this.state.storageConfigDisabled}
                                    value={this.state.params.name}
                                    onChange={this.onAccountNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Azure_Account_Name"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Azure_Account_Key"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsAccountKeyIpt"
                                    type="password"
                                    disabled={this.state.storageConfigDisabled}
                                    value={this.state.params.secret}
                                    onChange={this.onAccountKeyChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Azure_Account_Key"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}