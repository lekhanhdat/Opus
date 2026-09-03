import React, { useEffect, useState } from "react";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";
import { MultipleChoiceTermTree } from "../../Common/TreeComponents/TermTree";

import { TermUsageReportTypeName, TermUsageReportType } from "./Constants";

const getViewRequestOption = (id) => ({
    url: "/api/TermUsageReport/Get",
    data: id
});

const View = ({ id, source }) => {

    const [reportInfo, setReportInfo] = useState({
        profileName: "",
        description: "",
        termUsageReportType: TermUsageReportType.None,
        termUsageReportTypeName: "",
        checkedTermTreeStructure: null,
        checkedSourceTreeStructure: null,
    });

    useEffect(() => {
        const fetchReportInfo = async () => {
            const requestOption = getViewRequestOption(id);
            const reportInfo = await fetchUtility(requestOption);
            setReportInfo({
                profileName: reportInfo.profileName,
                description: reportInfo.description,
                termUsageReportType: reportInfo.termUsageReportType,
                termUsageReportTypeName: TermUsageReportTypeName.get(reportInfo.termUsageReportType),
                checkedTermTreeStructure: JSON.parse(reportInfo.checkedTermTreeStructure),
                checkedSourceTreeStructure: JSON.parse(reportInfo.checkedSourceTreeStructure)
            });
        };

        fetchReportInfo();
    }, []);

    return (
        <div className="reco-termusage-view-wrapper">
            <section className="reco-termusage-view-section">
                <div className="reco-termusage-section-title" tabIndex="1">
                    {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                </div>
                <div className="reco-termusage-section-value" tabIndex="1">
                    {reportInfo.profileName}
                </div>
            </section>
            <section className="reco-termusage-view-section">
                <div className="reco-termusage-section-title" tabIndex="1">
                    {RMResx.RM_JS_Profile_Description}
                </div>
                <div className="reco-termusage-section-value" tabIndex="1">
                    {reportInfo.description}
                </div>
            </section>
            <section className="reco-termusage-view-section">
                <div className="reco-termusage-section-title" tabIndex="1">
                    {RMResx.RM_JS_TermUsageReport_SelectReportType}
                </div>
                <div className="reco-termusage-section-value" tabIndex="1">
                    {reportInfo.termUsageReportTypeName}
                </div>
            </section>
            <section className="reco-termusage-view-section" hidden={reportInfo.termUsageReportType !== TermUsageReportType.Active}>
                <div className="reco-termusage-section-title" tabIndex="1">
                    {RMResx.RM_JS_TermUsageReport_TermIncludeReport}
                </div>
                <div className="reco-termusage-section-tree-value" tabIndex="1">
                    <MultipleChoiceTermTree
                        checkedTreeStructure={reportInfo.checkedTermTreeStructure}
                        isReadonly={true}
                    />
                </div>
            </section>
            <section className="reco-termusage-view-section">
                <div className="reco-termusage-section-title" tabIndex="1">
                    {RMResx.RM_RC_Common_ElectronicScope}
                </div>
                <div className="reco-termusage-section-tree-value">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        checkedTreeStructure={reportInfo.checkedSourceTreeStructure}
                        isReadonly={true}
                    />
                </div>
            </section>
        </div>
    );

};

export default View;