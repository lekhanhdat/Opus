import { withRouter } from 'react-router';
import * as JMConstants from './JMConstants';
import SiteMapLinks from '../../Constants/SiteMapLinks';
import { bindEvents } from '../../Utilities/CommonUtil';
import JMDetailList from './JMDetailList';
import { DisposalJobTemplate } from "./JMTableTemplate";
import JMTable from "./JMTable";
import React from 'react';

class PlanDetail extends R.Component {
    constructor(props) {
        super(props);
        let tempId = RM.Url.getParam(window.location.href, "id");
        if (tempId && tempId != null) {
            this.Jobid = tempId;
            this.JobType = JMConstants.JobType.DisposalActivityManagement;
        }
        this.state = {
            jobsChecked: [],
            jobsCount: 0,             //分页数据总数
            jobsPagerIndex: 0,         //分页每页的条数
            jobsPagerSize: 15,           //分页每页条数
            summaryModel: {},
            index: 0,
            ManagedColumns: [
                { isChecked: true, value: RMResx.RM_JS_JM_JobOrder, Id: 0, isDynamic: true },
                { isChecked: true, value: RMResx.RM_JS_JM_JobID, Id: 1, isDynamic: true },
                { isChecked: true, value: RMResx.RM_JS_JM_Module, Id: 2 },
                { isChecked: true, value: RMResx.RM_JS_JM_Progress, Id: 3 },
                { isChecked: true, value: RMResx.RM_JS_JM_Status, Id: 4 },
                { isChecked: true, value: RMResx.RM_JS_JM_StartTime, Id: 5 },
                { isChecked: true, value: RMResx.RM_JS_JM_EndTime, Id: 6 },
            ],
            allColumns: this.getColumns(),
            items: [],
        };
        bindEvents(this, "handleSelectedIndexChanged", 'viewDetail', 'selectChange', 'managedColumnChanged', 'refreshAction');
    }

    convertStatusStr(statusCode) {
        return JMConstants.JobStatusI18N[statusCode];
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    componentInit() {
        this.getDetailFromServer();
        if (this.Jobid) {
            $$.loading(true);
            let urlData = '/api/JMApi/GetJobSummary';
            let option = {
                url: urlData,
                method: 'POST',
                data: this.Jobid
            };
            fetchUtility(option).then((data) => {
                if (data) { this.setState({ summaryModel: data }); }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    handleSelectedIndexChanged(newIndex) {
        this.setState({ index: newIndex });
    }

    getDetailFromServer() {
        $$.loading(true);
        let urlData = '/api/JMApi/QueryPagerForDisposal?id=' + this.Jobid;
        let option = {
            url: urlData,
            method: 'Get'
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data) {
                if (data.IsDeleted) {
                    $$.messagedialog(true, {
                        // classify: "warn",
                        width: '550px',
                        hideActions: false,
                        title: RMResx.RM_JS_Common_Confirmation,
                        content: RMResx.RM_JS_JM_SelectedJobIdError,
                        buttons: [
                            {
                                text: RMResx.RM_JS_Common_OK,
                                primary: true,
                                classify: "theme",
                                onClick: () => {
                                    $$.messagedialog(false);
                                }
                            },
                        ],
                    });
                } else {
                    this.setState({
                        items: data.Items,
                        jobsCount: data.Items.length
                    });
                    this.dispatch("disposalJobTable", { columns: this.state.allColumns, items: data.Items });
                }
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    managedColumnChanged(args) {
        let allColumn = RM.deepcopy(this.getColumns());
        let IdArr = args.newValue.map((item) => { return item.Id; });
        allColumn.map((item, index) => { item.visible = IdArr.includes(index); });
        this.setState({ allColumns: allColumn });
        this.dispatch("disposalJobTable", { columns: this.state.allColumns });
    }

    viewDetail(args) {
        let jobId = args.JobId;
        let jobType = args.JobTypeCode;
        this.routerTo("/Root/JM/Detail", { id: jobId, type: jobType, DisposalId: this.Jobid });
    }

    refreshAction() {
        this.getDetailFromServer();
    }

    getColumns() {
        let column = [
            {
                header: RMResx.RM_JS_JM_JobOrder,
                resizeable: true,
                width: 80
            }, {
                headerTemplate: RMResx.RM_JS_JM_JobID,
                width: 200,
                resizeable: true
            }, {
                header: RMResx.RM_JS_JM_Module,
                width: 200,
                resizeable: true,
            }, {
                header: RMResx.RM_JS_JM_Progress,
                resizeable: true,
                width: 250
            }, {
                header: RMResx.RM_JS_JM_Status,
                resizeable: true,
                width: 200
            }, {
                headerTemplate: RMResx.RM_JS_JM_StartTime,
                resizeable: true,
                width: 300
            }, {
                headerTemplate: RMResx.RM_JS_JM_EndTime,
                resizeable: true,
                width: 300
            }];
        return column;
    }

    getJobDetailContent(){
        return [
            {
                name: RMResx.RM_JS_JMD_Summary_JobType,
                value: RMResx.RM_JS_JM_JobType_DisposalActivityManagement
            },
            {
                name: RMResx.RM_JS_JMD_Summary_JobID,
                value: this.state.summaryModel.JobId
            },
            {
                name: RMResx.RM_JS_JMD_Summary_StartTime,
                value: this.state.summaryModel.StartTime
            },
            {
                name: RMResx.RM_JS_JMD_Summary_EndTime,
                value: this.state.summaryModel.EndTime
            },
            {
                name: RMResx.RM_JS_JM_JobRunBy,
                value: this.state.summaryModel.JobRunBy
            },
            {
                name: RMResx.RM_JS_JMD_Summary_Status,
                value: JMConstants.JobStatusI18N[this.state.summaryModel.Status]
            },
            {
                name: RMResx.RM_JS_JMD_Summary_Comment,
                value: this.state.summaryModel.Comment
            }
        ];
    }

    getSummary() {
        if (this.state.summaryModel.JobType) {
            let jobDetailContent = this.getJobDetailContent();
            return <div>
                <JMDetailList
                    textField={"name"}
                    valueField={"value"}
                    title={RMResx.RM_JS_JMD_GeneralSetting}
                    data={jobDetailContent}>
                </JMDetailList>
            </div>;
        }
    }

    selectChange(items) {
        this.setState({ jobsChecked: items });
    }

    onClose() {
        this.props.history.push({
            pathname: "/Root/JM/Index",
            query: { id: this.Jobid }
        });
    }

    renderNavBar() {
        return < div className="ra-main-header">
            <R.Button
                text={RMResx.RM_JS_JM_Refresh_Btn}
                primary={true}
                classify="theme"
                onClick={this.refreshAction}
            />
            <R.Multicombobox
                checkedField="isChecked"
                textField="value"
                valueField="Id"
                hasFilter={false}
                height={34}
                required={true}
                customTrigger={true}
                items={this.state.ManagedColumns}
                allText={RMResx.RM_JS_JM_CustomColumns}
                disabledField='isDynamic'
                onChange={this.managedColumnChanged}
                triggerBySource={true}
            >
                <R.Button icon="fia-manage-column" text={RMResx.RM_JS_JM_CustomColumns} tooltip={RMResx.RM_JS_JM_CustomColumns} />
            </R.Multicombobox>
        </div>;
    }

    renderTable() {
        return <div className="ra-main-table padding-bottom-l">
            <JMTable
                id="disposalJobTable"
                template={DisposalJobTemplate}
                cellClick={this.viewDetail}
            />
        </div>;
    }

    render() {
        return <React.Fragment>
            <div id="rmPlanDetails">
                <$g.SiteMap data={[SiteMapLinks.JM, SiteMapLinks.JM_PLAN_DETAIL]} />
                <div id="planDetailModule">
                    <R.Tabcontrol
                        maxWidth={"none"}
                        active={this.state.index}
                        onChange={this.handleSelectedIndexChanged}
                    >
                        <R.TabPanel tab={RMResx.RM_JS_JMD_Tab_Summary}>
                            {this.getSummary()}
                        </R.TabPanel>
                        <R.TabPanel tab={RMResx.RM_JS_JM_PlanQueue}>
                            <div>
                                <div className="ra-page-container">
                                    {this.renderNavBar()}
                                    {this.renderTable()}
                                </div>
                            </div>
                        </R.TabPanel>
                    </R.Tabcontrol>
                </div>
            </div>
            <div className="jm-footer">
                <div className="jm-footer-btn">
                    <R.Button
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={this.onClose.bind(this)} />
                </div>
            </div>
        </React.Fragment>;
    }
}

export default withRouter(PlanDetail);