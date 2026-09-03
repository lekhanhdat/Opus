import * as Constants from "../Constants";
class FolderCheckBoxGroup extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            pushToChild: false,
            childInheritsValue: false,
            allowModifyValue: false,
        };
    }
    componentReceive(action, data0, data1) {
        switch (action) {
            case Constants.dispatchAction.save:
                this.props.getFolderCheckedSettings({
                    pushToChild: this.state.pushToChild,
                    childInheritsValue: this.state.childInheritsValue,
                    allowModifyValue: this.state.allowModifyValue
                });
                break;
            case Constants.dispatchAction.setData:
                this.setState({
                    pushToChild: data0.pushToChild,
                    childInheritsValue: data0.childInheritsValue,
                    allowModifyValue: data0.allowModifyValue,
                });
                break;
            default:
                break;
        }
    }

    handlePushToChildClick = () => {
        if (this.state.pushToChild) {
            this.setState({
                pushToChild: !this.state.pushToChild,
                childInheritsValue: false,
                allowModifyValue: false
            });
        } else {
            this.setState({ pushToChild: !this.state.pushToChild });
        }
    }
    handleChildInheritsValueClick = () => {
        if (this.state.childInheritsValue) {
            this.setState({
                childInheritsValue: !this.state.childInheritsValue,
                allowModifyValue: false
            });
        } else {
            this.setState({ childInheritsValue: !this.state.childInheritsValue });
        }
    }
    handleAllowModifyValueClick = () => {
        this.setState({ allowModifyValue: !this.state.allowModifyValue });
    }


    render() {
        return (
            <div id={this.props.id}>
                <div className="margin-top-10">
                    <label>
                        <input
                            className="type-radio"
                            type="checkbox"
                            checked={this.state.pushToChild}
                            onChange={this.handlePushToChildClick}
                        />
                        <span className="ra-white-text">{this.props.templateType == Constants.panelType.Folder
                            ? "Push the column to child Record Template"
                            : "Push the column to child Folder Template"}</span>


                    </label>

                    <div className="margin-top-10 margin-left-20">
                        <label>
                            <input
                                className="archiveType-radio"
                                type="checkbox"
                                disabled={!this.state.pushToChild}
                                checked={this.state.childInheritsValue}
                                onChange={this.handleChildInheritsValueClick}
                            />
                            <span className="ra-white-text">{this.props.templateType == Constants.panelType.Folder
                                ? "Child Record inherits the value fill in the Folder"
                                : "Child Record inherits the value fill in the Box"}</span>
                        </label>

                        <div className="margin-top-10 margin-left-20">
                            <label>
                                <input
                                    className="archiveType-radio"
                                    type="checkbox"
                                    disabled={!this.state.childInheritsValue}
                                    checked={this.state.allowModifyValue}
                                    onChange={this.handleAllowModifyValueClick}
                                />
                                <span className="ra-white-text">{"Allow to modify value"}</span>
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

export { FolderCheckBoxGroup };