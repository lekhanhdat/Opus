import {Component} from "react";
import RouterUrls from "../../Constants/RouterUrls";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import {bindEvents} from "../../Utilities/CommonUtil";
import "../../Less/CP/dashboardSettings.less";

export default class DashboardSettings extends Component {
    constructor(props) {
        super(props);
        bindEvents(this, "handleChange", "DashboardSettingsSave", "onCancel", "hideMessageTip");
        this.state = {
            FRUITS: [
                {name: "0", value: "0"},
                {name: "1", value: "1"},
                {name: "2", value: "2"},
                {name: "3", value: "3"},
                {name: "4", value: "4"},
                {name: "5", value: "5"},
                {name: "6", value: "6"},
                {name: "7", value: "7"},
                {name: "8", value: "8"},
                {name: "9", value: "9"},
                {name: "10", value: "10"},
                {name: "11", value: "11"},
                {name: "12", value: "12"},
                {name: "13", value: "13"},
                {name: "14", value: "14"},
                {name: "15", value: "15"},
                {name: "16", value: "16"},
                {name: "17", value: "17"},
                {name: "18", value: "18"},
                {name: "19", value: "19"},
                {name: "20", value: "20"},
                {name: "21", value: "21"},
                {name: "22", value: "22"},
                {name: "23", value: "23"}
            ],                                   //下拉数据
            radioValue: true,                    //判断选中yes or no
            dashPretermit_hour: {},              //获取默认小时数
            dashMinute: null,                       //获取当前分钟数
            messageShow: false,                    //提示框的显示隐藏
            showTip: false,
            type: "success"
        };
    }

    componentDidMount() {
        //数据回显调用
        this.getData();

        this.setState({
            showTip: false,
            type: "success"
        });
    }

    //点击radio切换
    handleChange(e) {
        this.setState({
            radioValue: !this.state.radioValue
        }
        );
    }

    //保存数据回显
    getData() {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/LoadDashboardSetting",
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data.isActive) {
                //回显小时数
                this.setState({
                    dashPretermit_hour: {
                        name: data.hour,
                        value: data.hour
                    }
                });
                //回显分钟数
                this.setState({
                    dashMinute: data.minute
                });
            } else {
                //判断是否有数据返回
                let date = new Date();
                // 获取当前时间小时
                let pretermit_hour = date.getHours();
                //获取分钟数
                let pretermit_minute = date.getMinutes();
                //初始化数据（无数据默认当前）
                this.setState({
                    dashPretermit_hour: {
                        name: pretermit_hour,
                        value: pretermit_hour
                    }
                });
                //获取默认分钟（无数据默认当前）
                this.setState({
                    dashMinute: pretermit_minute
                });
                //回显radio的状态
                this.setState({
                    radioValue: data.isActive
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    //点击保存
    DashboardSettingsSave() {
        //获取当前需要的参数
        let urlData = "/api/CPApi/SaveDashboardSetting" + "?" + "isActive=" + this.state.radioValue + "&" + "hour=" + this.state.dashPretermit_hour.name + "&" + "minute=" + this.state.dashMinute;
        $$.loading(true);
        let option = {
            url: urlData,
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            //弹出保存成功提示框
            this.setState({
                showTip: true,
                type: "success"
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    //路由跳转
    onCancel() {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    //选择切换小时
    onSelectionChanged(e, args) {
        let newObj = args.newValue;
        this.setState({
            dashPretermit_hour: {
                name: newObj.name,
                value: newObj.value
            }
        });
    }

    //选择切换分钟
    contentChanged(e, args) {
        this.setState({
            dashMinute: args.newValue
        });
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    //获取
    render() {
        return <div id="rmDashboardSettings">
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_DashboardSettings]}/>
            <R.Messagebar message={RMResx.RM_JS_CP_DS_SaveSuccessfull}
                classify={this.state.tipType} onClose={this.hideMessageTip}
                status={{show: this.state.showTip}}/>
            <div className="body-div">
                <div className="set-active">
                    <span className="question-icon">*</span>
                    <span className="question-title">{RMResx.RM_CP_DS_Active_Title}</span>
                    <div style={{margin: "10px 0 0px 10px"}}>
                        <label>
                            <input
                                type="radio"
                                checked={!this.state.radioValue}
                                onChange={this.handleChange}/>
                            No
                        </label>

                    </div>
                    <div style={{margin: "0 0 10px 10px"}}>
                        <label>
                            <input
                                type="radio"
                                checked={this.state.radioValue}
                                onChange={this.handleChange}/>
                            Yes
                        </label>

                    </div>

                </div>
                <div className="set-time" style={{display: (this.state.radioValue) ? "block" : "none"}}>
                    <span className="question-title">{RMResx.RM_CP_DS_SetTime_Title}</span>
                    <table className="set-time-table">
                        <tbody>
                            <tr>
                                <td>
                                    <div>{RMResx.RM_CP_DS_SetTime_StartAt}</div>
                                </td>
                                <td>
                                    <div style={{marginLeft: "20px"}}>
                                        <R.Combobox
                                            searchPlaceholder=''
                                            width={60}
                                            textField='name'
                                            checkedField='checked'
                                            items={this.state.FRUITS}
                                            onChange={this.onSelectionChanged.bind(this)}
                                            disabled={this.state.disabled}
                                        />
                                    </div>

                                </td>
                                <td>
                                    <div style={{margin: "0 5px"}}>
                                        :
                                    </div>
                                </td>
                                <td>
                                    <div>
                                        <R.Numericbox
                                            style={{height: "25px"}}
                                            value={this.state.dashMinute}
                                            minValue={0}
                                            maxValue={59}
                                            width={40}
                                            contentChanged={this.contentChanged.bind(this)}
                                        />
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div id="btnGroups">
                <div id="btnCancel" className="btnBase blackBtn" onClick={this.onCancel}
                    style={{height: "28px"}}>{RMResx.RM_JS_Common_Cancel}</div>
                <div id="btnSave" className="btnBase blueBtn" onClick={this.DashboardSettingsSave}
                    style={{height: "28px"}}>{RMResx.RM_JS_Common_Save}</div>
            </div>
        </div>;
    }
}
