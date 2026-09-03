import React, { useEffect, useState } from "react";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";

const getViewRequestOption = (id) => ({
    url: "/api/DisposalReport/Get",
    data: id
});

const View = ({ id, source }) => {

    const [disposalReportInfo, setDisposalReportInfo] = useState({
        profileName: "",
        description: "",
        applyRuleBeforeTime: "",
        checkedTreeStructure: null,
    });

    useEffect(() => {
        const fetchData = async () => {
            const requestOption = getViewRequestOption(id);
            const reportInfo = await fetchUtility(requestOption);
            reportInfo.checkedTreeStructure = JSON.parse(reportInfo.checkedTreeStructure);
            setDisposalReportInfo(reportInfo);
        };
        fetchData();
    }, []);

    return (
        <div className="reco-disposal-view-wrapper">
            <section className="reco-disposal-view-section">
                <div className="reco-disposal-section-title" tabIndex="1">
                    {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                </div>
                <div className="reco-disposal-section-value" tabIndex="1">
                    {disposalReportInfo.profileName}
                </div>
            </section>
            <section className="reco-disposal-view-section">
                <div className="reco-disposal-section-title" tabIndex="1">
                    {RMResx.RM_JS_Profile_Description}
                </div>
                <div className="reco-disposal-section-value" tabIndex="1">
                    {disposalReportInfo.description}
                </div>
            </section>
            <section className="reco-disposal-view-section">
                <div className="reco-disposal-section-title" tabIndex="1">
                    {RMResx.RM_RC_DueDisposalViewDetail_Time}
                </div>
                <div className="reco-disposal-section-value" tabIndex="1">
                    {disposalReportInfo.applyRuleBeforeTime}
                </div>
            </section>
            <section className="reco-disposal-view-section">
                <div className="reco-disposal-section-title" tabIndex="1">
                    {RMResx.RM_RC_DueDisposalViewDetail_ReportingScope}
                </div>
                <div className="reco-disposal-section-tree-value">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        checkedTreeStructure={disposalReportInfo.checkedTreeStructure}
                        isReadonly={true}
                    />
                </div>
            </section>
        </div>
    );

};

export default View;