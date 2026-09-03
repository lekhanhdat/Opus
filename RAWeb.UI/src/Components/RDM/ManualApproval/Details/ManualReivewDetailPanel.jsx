
import ManualReviewTable from "./ManualReviewTable";
import ManaulOverview from "./ManualOverView";

const ManualReviewDetailPanel = ({details, manualInfos, isShow, onHide , handleTabIndexChanged, tabIndex, template,columns,onClick}) =>{

    return (
        <div>
            <R.Panel
                header={RMResx.RM_JS_MA_ViewDetails_Title}
                size={670}
                status={{ show : isShow }}
                destroy={true}
                onHide={onHide}
            >
                <R.Tabcontrol
                    flex
                    type="underline"
                    active={tabIndex}
                    onChange={handleTabIndexChanged}
                    destroy={true}
                >   
                    <R.TabPanel tab = {RMResx.RM_JS_MA_ViewDetails_DetailTab} key = {0}>
                        <ManaulOverview details={details} />
                    </R.TabPanel>
                    <R.TabPanel tab = {RMResx.RM_JS_MA_ViewDetails_ReviewTab} key = {1}>
                        <ManualReviewTable 
                            items={manualInfos}
                            columns={columns}
                            template={template}
                        >
                        </ManualReviewTable>
                    </R.TabPanel>
                </R.Tabcontrol>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={onHide} />
            </R.Panel>
        </div>
    );
};

export default ManualReviewDetailPanel;