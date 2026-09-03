import React, { useEffect, useState } from "react";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";

import { DateFrameType, ActionTypeName, DateFrameTypeName } from "./Constants";

const getViewRequestOption = (id) => ({
    url: "/api/CreateAndDestryoedReport/Get",
    data: id
});

const View = ({ id, source }) => {

    const [reportInfo, setReportInfo] = useState({
        profileName: "",
        description: "",
        actionType: "",
        dateFrame: "",
        checkedTreeStructure: null,
    });

    useEffect(() => {
        const fetchData = async () => {
            const requestOption = getViewRequestOption(id);
            const reportInfo = await fetchUtility(requestOption);
            let dateFrame = DateFrameTypeName.get(reportInfo.dateFrameType);
            if (reportInfo.dateFrameType === DateFrameType.Custom) {
                dateFrame = dateFrame + " " + reportInfo.customStartDate + " - " + reportInfo.customEndDate;
            }
            setReportInfo({
                profileName: reportInfo.profileName,
                description: reportInfo.description,
                actionType: ActionTypeName.get(reportInfo.actionType),
                dateFrame: dateFrame,
                checkedTreeStructure: JSON.parse(reportInfo.checkedTreeStructure)
            });
        };

        fetchData();
    }, []);

    return (
        <div className="reco-cad-view-wrapper">
            <section className="reco-cad-view-section">
                <div className="reco-cad-section-title" tabIndex="1">
                    {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                </div>
                <div className="reco-cad-section-value" tabIndex="1">
                    {reportInfo.profileName}
                </div>
            </section>
            <section className="reco-cad-view-section">
                <div className="reco-cad-section-title" tabIndex="1">
                    {RMResx.RM_JS_Profile_Description}
                </div>
                <div className="reco-cad-section-value" tabIndex="1">
                    {reportInfo.description}
                </div>
            </section>
            <section className="reco-cad-view-section">
                <div className="reco-cad-section-title" tabIndex="1">
                    {RMResx.RM_JS_RC_TimeFrame_OprationType}
                </div>
                <div className="reco-cad-section-value" tabIndex="1">
                    {reportInfo.actionType}
                </div>
            </section>
            <section className="reco-cad-view-section">
                <div className="reco-cad-section-title" tabIndex="1">
                    {RMResx.RM_JS_RC_TimeFrame_Range}
                </div>
                <div className="reco-cad-section-value" tabIndex="1">
                    {reportInfo.dateFrame}
                </div>
            </section>
            <section className="reco-cad-view-section">
                <div className="reco-cad-section-title" tabIndex="1">
                    {RMResx.RM_RC_DueDisposalViewDetail_ReportingScope}
                </div>
                <div className="reco-cad-section-tree-value">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        checkedTreeStructure={reportInfo.checkedTreeStructure}
                        isReadonly={true}
                    />
                </div>
            </section>
        </div>
    );

};

export default View;