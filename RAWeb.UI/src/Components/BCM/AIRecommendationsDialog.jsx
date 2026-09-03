import {
    forwardRef,
    useEffect,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";

import { IndustryList } from "./Constants";
import StringUtil from "../../Utilities/StringUtil";
import { LicenseHelper } from "../../Utilities/CommonUtil";

const initialIndustryState = {
    list: _.cloneDeep(IndustryList),
    selected: RMResx.RM_TM_AI_Recommendations_IndustryNone,
    disabled: true,
    input: "",
    isValid: true,
};

// Be careful when editing this file because it involves too many types of reference data

function AIRecommendationsDialog({ result }, ref) {
    const uploadRef = useRef(null);

    const [industry, setIndustry] = useState(initialIndustryState);
    const [countryState, setCountryState] = useState("");
    const [requirement, setRequirement] = useState("");
    const [exampleFiles, setExampleFiles] = useState([]);
    const [isShowIndustry, setIsShowIndustry] = useState(false);

    const [cachedSelectedIndustryList, setCachedSelectedIndustryList] =
        useState(_.cloneDeep(IndustryList));
    const [cachedSelectedIndustry, setCachedSelectedIndustry] = useState(null);

    useImperativeHandle(ref, () => ({
        resetIndustry: handleClearIndustry,
        isValidIndustry: () => {
            // Do not return because will validate by $$.verify for country below in the same place.
            const isOther = industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryOther;
            const isNone = industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryNone;
            let isValid = false;
            if (isOther) {
                isValid = !!industry.input.trim();
            } else {
                isValid = isNone ? false : !!industry.selected;
            }
            setIndustry((prev) => ({
                ...prev,
                isValid,
            }));
        },
        getAllData: () => ({
            industry:
                industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryOther
                    ? industry.input
                    : industry.selected,
            country: countryState,
            requirement: requirement,
            file: exampleFiles[0]?.file || "",
        }),
    }));

    // Reset industry list when selected industry but close dialog
    useEffect(() => {
        setIndustry({
            ...initialIndustryState,
            list: _.cloneDeep(IndustryList),
        });
    }, []);

    // Industry handlers
    const isInPredefinedIndustryList = (value) => {
        if (value === RMResx.RM_TM_AI_Recommendations_IndustryOther) return false;

        if (value) {
            return _.cloneDeep(IndustryList).some(
                (item) =>
                    item.text !== RMResx.RM_TM_AI_Recommendations_IndustryOther &&
                    item.text.toLowerCase() === value.trim().toLowerCase()
            );
        }
        return true;
    };

    const handleClearIndustry = () => {
        setCachedSelectedIndustryList(_.cloneDeep(IndustryList));
        setCachedSelectedIndustry(null);
        setIndustry({
            ...initialIndustryState,
            list: _.cloneDeep(IndustryList),
            isValid: false,
        });
    };

    const handleOnShowIndustry = () => {
        if (industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryOther && industry.input) {
            setCachedSelectedIndustry(industry.input);
        }
    };

    const handleChangeIndustry = (newValue) => {
        setCachedSelectedIndustry(newValue);
        setIsShowIndustry(true);
    };

    const handleCancelIndustry = () => {
        console.log(industry);
        if (!isInPredefinedIndustryList(cachedSelectedIndustry)) {
            setCachedSelectedIndustry(null);
        }
        setIndustry((prev) => ({
            ...prev,
            list: _.cloneDeep(cachedSelectedIndustryList),
        }));
    };

    const handleApplyIndustry = () => {
        const newValue = cachedSelectedIndustry;
        const clonedCachedSelectedIndustryList = _.cloneDeep(
            cachedSelectedIndustryList
        );
        const isInput = !cachedSelectedIndustryList.some(
            (item) => item.value === newValue
        );
        const newIndustries = clonedCachedSelectedIndustryList.map((item) => ({
            ...item,
            checked: isInput ? item.text === RMResx.RM_TM_AI_Recommendations_IndustryOther : item.value === newValue,
        }));
        const textMap = new Map(newIndustries.map((item) => [item.text, true]));
        const isExist = textMap.has(newValue);

        setCachedSelectedIndustryList(newIndustries);
        setIndustry((prev) => ({
            ...prev,
            list: newIndustries,
            selected: isExist ? newValue : RMResx.RM_TM_AI_Recommendations_IndustryOther,
            input: isExist ? "" : newValue,
            disabled: !newValue,
            isValid: !!newValue,
        }));
        setIsShowIndustry(false);
    };

    // Template handlers
    const handleDownloadTemplate = () => {
        $$.loading(true);
        const divElement = document.getElementById("downloadTemplate");
        const downloadUrl = "/api/TermManagementApi/DownloadTemplateAIRecommendation";
        ReactDOM.render(
            <form action={downloadUrl} method="POST"></form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
        $$.loading(false);
    }

    const onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    const handleUpload = (args) => {
        if (args.isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            setExampleFiles(args.files);
            uploadRef.current = args.files[0];
        }
    };

    const handleDelete = (args) => {
        if (args.isSucceed) {
            setExampleFiles([]);
            uploadRef.current = null;
        }
    };

    const verifyLength = (value) => {
        if (value && value.length > 20000) {
            return RMResx.RM_TM_AI_Recommendations_RequirementValidate;
        }
        return true;
    }

    // Renders
    const renderRetentionPolicyAction = (item) => {
        switch (item.retention_policy.action) {
            case "destroy":
                return RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove;
            case "archive":
                if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
                    return RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove;
                }
                return RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying;
            default:
                return "";
        }
    }

    if (result) {
        return (
            <div id="termAIRecommendationPanel" className="tm-result">
                <div tabIndex={0}>
                    {RMResx.RM_TM_AI_Recommendations_ResultDesc}
                </div>
                <div
                    tabIndex={0}
                    className="font-semibold font-xs margin-top-l"
                >
                    {RMResx.RM_TM_AI_Recommendations_Result_IL}
                </div>
                <div className="flex flex-column gap-xs margin-top-l">
                    {result.map((item, index) => (
                        <div key={index}>
                            <div tabIndex={0} className="font-bold font-xs">
                                {item.name}
                            </div>
                            <ul className="margin-top-xs flex flex-column gap-xs">
                                <li>
                                    <span tabIndex={0} className="font-semibold">
                                        {RMResx.RM_TM_AI_Recommendations_Result_RetentionPeriod}
                                    </span>
                                    <span tabIndex={0}>
                                        {item.retention_policy.retention_time.policy_description}
                                    </span>
                                </li>
                                <li>
                                    <span tabIndex={0} className="font-semibold">
                                        {RMResx.RM_TM_AI_Recommendations_Result_ModifiedTime}
                                    </span>
                                    <div tabIndex={0} className="tm-result-trigger">
                                        <$g.I18NProvider  msg={RMResx.RM_TM_AI_Recommendations_Result_Trigger}>
                                            <span>
                                                {item.retention_policy.retention_time.retention_time_number}
                                            </span>
                                            <span>
                                                {item.retention_policy.retention_time.unit}
                                            </span>
                                        </$g.I18NProvider>
                                    </div>
                                </li>
                                <li>
                                    <span tabIndex={0} className="font-semibold">
                                        {RMResx.RM_TM_AI_Recommendations_Result_DisposalAction}
                                    </span>{" "}
                                    <span tabIndex={0}>
                                        {renderRetentionPolicyAction(item)}
                                    </span>
                                </li>
                                <li>
                                    <span tabIndex={0} className="font-semibold">
                                        {RMResx.RM_TM_AI_Recommendations_Result_ManualReview}
                                    </span>{" "}
                                    <span tabIndex={0}>
                                        {item.retention_policy.manual_review === "yes" 
                                            ? RMResx.RM_JS_Common_Yes 
                                            : RMResx.RM_JS_Common_No}
                                    </span>
                                </li>
                                <li>
                                    <span tabIndex={0} className="font-semibold">
                                        {RMResx.RM_TM_AI_Recommendations_Result_Description}
                                    </span>{" "}
                                    <span tabIndex={0}>
                                        {item.description}
                                    </span>
                                </li>
                                {(item.retention_policy.reference ||
                                    item.retention_policy.reference_link) && (
                                    <li>
                                        <span tabIndex={0} className="font-semibold">
                                            {RMResx.RM_TM_AI_Recommendations_Result_Reference}
                                        </span>{" "}
                                        {(item.retention_policy.reference_link && item.retention_policy.reference_link != "") ? (
                                            <a
                                                tabIndex={0}
                                                href={item.retention_policy.reference_link}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="tm-link-template"
                                            >
                                                {item.retention_policy.reference}
                                            </a>
                                        ) : (
                                            <span
                                                tabIndex={0}
                                                className="tm-reference-link-template"
                                            >
                                                {item.retention_policy.reference}
                                            </span>
                                        )}
                                    </li>
                                )}
                            </ul>
                        </div>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div id="termAIRecommendationPanel" className="flex flex-column gap-m">
            <p style={{ margin: 0 }} tabIndex={0}>
                {RMResx.RM_TM_AI_Recommendations_Desc}
            </p>
            <R.Validation>
                <div id="allValidation" className="flex flex-column gap-m">
                    <div>
                        <div id="ariaIndustry" tabIndex={0}className="tm-label-control require">
                            {RMResx.RM_TM_AI_Recommendations_IndustryLabel}
                        </div>
                        <div
                            className={`${
                                !industry.isValid && "tm-control-validation"
                            }`}
                        >
                            <R.ComboboxShell
                                id="raTmAIIndustry"
                                content={
                                    industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryOther
                                        ? industry.input
                                        : industry.selected
                                }
                                width="100%"
                                block={false}
                                triggerType="all"
                                offClose={true}
                                clearable={
                                    industry.selected === RMResx.RM_TM_AI_Recommendations_IndustryOther
                                        ? !!industry.input
                                        : industry.selected !== RMResx.RM_TM_AI_Recommendations_IndustryNone
                                }
                                compact={true}
                                status={{ show: isShowIndustry }}
                                onClear={handleClearIndustry}
                                onShow={handleOnShowIndustry}
                            >
                                <div className="padding-s">
                                    <R.Radio.Group
                                        block
                                        name="common-industry"
                                        items={industry.list}
                                        onChange={handleChangeIndustry}
                                    />
                                    <div className="margin-top-s">
                                        <R.Input
                                            id="raTmAIIndustryInput"
                                            type="text"
                                            value={industry.input}
                                            disabled={isInPredefinedIndustryList(
                                                cachedSelectedIndustry
                                            )} // cachedSelectedIndustry: null
                                            width="100%"
                                            placeholder={RMResx.RM_TM_AI_Recommendations_IndustryCustomPlaceholder}
                                            onChange={handleChangeIndustry}
                                        />
                                    </div>
                                </div>
                                <>
                                    <R.Button
                                        slot="buttons"
                                        name="cancel"
                                        text={RMResx.RM_JS_Common_Cancel}
                                        value="close"
                                        onClick={handleCancelIndustry}
                                    />
                                    <R.Button
                                        slot="buttons"
                                        name="save"
                                        primary={true}
                                        classify="theme"
                                        text={RMResx.RM_JS_Common_Save}
                                        value="close"
                                        disabled={!(
                                            cachedSelectedIndustry &&
                                            cachedSelectedIndustry !== RMResx.RM_TM_AI_Recommendations_IndustryOther
                                        )}
                                        onClick={handleApplyIndustry}
                                    />
                                </>
                            </R.ComboboxShell>
                            <R.ValidationFaker
                                valid={industry.isValid}
                                of={`#raTmAIIndustry`}
                                message={RMResx.RM_TM_AI_Recommendations_IndustryValidation}
                            />
                        </div>
                    </div>
                    <div>
                        <div id="ariaCountryState" tabIndex={0} className="tm-label-control">
                            {RMResx.RM_TM_AI_Recommendations_CountryStateLabel}
                        </div>
                        <div>
                            <R.Input
                                id="raTmAICountryState"
                                type="text"
                                value={countryState}
                                width="100%"
                                placeholder={RMResx.RM_TM_AI_Recommendations_CountryStatePlaceholder}
                                onChange={setCountryState}
                            />
                        </div>
                    </div>
                    <label style={{ width: "100%" }}>
                        <div tabIndex={0} className="tm-label-control">
                            {RMResx.RM_TM_AI_Recommendations_Requirement}
                        </div>
                        <R.Validation
                            element="Input"
                            rules={{
                                verifyLength,
                            }}
                        >
                            <R.Input
                                id="raTermClassification"
                                type="textarea"
                                value={requirement}
                                height={80}
                                width="100%"
                                resize="vertical"
                                placeholder={RMResx.RM_TM_AI_Recommendations_RequirementPlaceholder}
                                onChange={setRequirement}
                            />
                        </R.Validation>
                    </label>
                    <div>
                        <div tabIndex={0} className="tm-label-control">
                            {RMResx.RM_TM_AI_Recommendations_AIReference}
                        </div>
                        <div tabIndex={0} className="margin-bottom-xs">
                            <$g.I18NProvider msg={RMResx.RM_TM_AI_Recommendations_AIReferenceUpload}>
                                <span tabIndex={0} className="tm-link-template" onClick={handleDownloadTemplate} onKeyDown={onKeyDown}>{RMResx.RM_TM_AI_Recommendations_AIReferenceTemplate}</span>
                            </$g.I18NProvider>
                        </div>
                        <div id='downloadTemplate' style={{ display: "none" }} />
                        <R.Uploader
                            ref={uploadRef}
                            files={exampleFiles}
                            showTypes
                            fileTypes={["XLSX"]}
                            showMaxSize
                            maxSize="10MB"
                            showLoading
                            onUpload={handleUpload}
                            onDelete={handleDelete}
                        />
                    </div>
                </div>
            </R.Validation>
        </div>
    );
}

export default forwardRef(AIRecommendationsDialog);
