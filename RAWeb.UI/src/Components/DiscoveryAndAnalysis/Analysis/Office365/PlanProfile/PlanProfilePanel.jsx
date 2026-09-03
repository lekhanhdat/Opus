import { useEffect, useRef, useState } from "react";
import { RuleModuleTypes } from "../../../../Common/RuleItem/Components/Constants";
import RuleCriteriaManager from "../../../RuleManagement/RuleCriteriaManager";
import { AnalyseMethodConstants } from "../../../RuleManagement/Constants";
import StubPanel from "../../../../CP/StubSettings/StubPanel";

export const ActionEnums = {
    None: 0,
    ArchiveAndDestroy: 1,
    DestroyFile: 2
};

export const ActionOptionEnums = {
    KeepCurrentAndSpecifiedArchiveRest: 1,
    ArchiveCurrentAndPrevious: 2,
    LeaveStubInPlace: 4,
    IncludeDeclaredRecords: 8,
    IncludeLockedByRecordsLabel: 16,
    KeepCurrentAndPrevious: 32,
    DeleteToRecycleBin: 64
};

export const ScheduleTypeEnums = {
    NoSchedule: 0,
    Configure: 1
};

export const EndTypeEnums = {
    NoEnd: 0,
    EndByTime: 1,
    EndByOccurrences: 2
};

export const IntervalTypeEnums = {
    None: 0,
    Weekly: 1,
    Daily: 2,
    Hourly: 3,
    Monthly: 4
};

export const SiteMappingTypeEnums = {
    None: 0,
    SPOAndOD: 1,
    PHL: 2
};

const formatBackendDate = (rawDate) => {
    if (!rawDate) return "";
    const parsedDate = new Date(rawDate);
    if (isNaN(parsedDate.getTime())) return "";

    const pad = (n) => String(n).padStart(2, "0");
    const y = parsedDate.getFullYear();
    const M = pad(parsedDate.getMonth() + 1);
    const d = pad(parsedDate.getDate());
    const H = pad(parsedDate.getHours());
    const m = pad(parsedDate.getMinutes());
    const s = pad(parsedDate.getSeconds());
    
    // Requested format: yyyy/mm/dd hh:mm:ss
    return `${y}/${M}/${d} ${H}:${m}:${s}`;
};

const ArchiveOptionKeys = {
    KeepCurrentAndSpecifiedArchiveRest: "KeepCurrentAndSpecifiedArchiveRest",
    ArchiveCurrentAndPrevious: "ArchiveCurrentAndPrevious",
    LeaveStubInPlace: "LeaveStubInPlace",
    IncludeDeclaredRecords: "IncludeDeclaredRecords",
    IncludeLockedByRecordsLabel: "IncludeLockedByRecordsLabel"
};

const DestroyOptionKeys = {
    KeepCurrentAndPrevious: "KeepCurrentAndPrevious",
    IncludeDeclaredRecords: "IncludeDeclaredRecords",
    IncludeLockedByRecordsLabel: "IncludeLockedByRecordsLabel",
    DeleteToRecycleBin: "DeleteToRecycleBin"
};

const getEmptyArchiveChecks = () => ({
    [ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]: false,
    [ArchiveOptionKeys.ArchiveCurrentAndPrevious]: false,
    [ArchiveOptionKeys.LeaveStubInPlace]: false,
    [ArchiveOptionKeys.IncludeDeclaredRecords]: false,
    [ArchiveOptionKeys.IncludeLockedByRecordsLabel]: false
});

const getEmptyDestroyChecks = () => ({
    [DestroyOptionKeys.KeepCurrentAndPrevious]: false,
    [DestroyOptionKeys.IncludeDeclaredRecords]: false,
    [DestroyOptionKeys.IncludeLockedByRecordsLabel]: false,
    [DestroyOptionKeys.DeleteToRecycleBin]: false
});

const hasFlag = (options, flag) => (options & flag) === flag;

const getArchiveChecksFromOptions = (options) => ({
    [ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]: hasFlag(options, ActionOptionEnums.KeepCurrentAndSpecifiedArchiveRest),
    [ArchiveOptionKeys.ArchiveCurrentAndPrevious]: hasFlag(options, ActionOptionEnums.ArchiveCurrentAndPrevious),
    [ArchiveOptionKeys.LeaveStubInPlace]: hasFlag(options, ActionOptionEnums.LeaveStubInPlace),
    [ArchiveOptionKeys.IncludeDeclaredRecords]: hasFlag(options, ActionOptionEnums.IncludeDeclaredRecords),
    [ArchiveOptionKeys.IncludeLockedByRecordsLabel]: hasFlag(options, ActionOptionEnums.IncludeLockedByRecordsLabel)
});

const getDestroyChecksFromOptions = (options) => ({
    [DestroyOptionKeys.KeepCurrentAndPrevious]: hasFlag(options, ActionOptionEnums.KeepCurrentAndPrevious),
    [DestroyOptionKeys.IncludeDeclaredRecords]: hasFlag(options, ActionOptionEnums.IncludeDeclaredRecords),
    [DestroyOptionKeys.IncludeLockedByRecordsLabel]: hasFlag(options, ActionOptionEnums.IncludeLockedByRecordsLabel),
    [DestroyOptionKeys.DeleteToRecycleBin]: hasFlag(options, ActionOptionEnums.DeleteToRecycleBin)
});

const getOptionsFromArchiveChecks = (checks) => {
    let result = 0;
    if (checks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]) result |= ActionOptionEnums.KeepCurrentAndSpecifiedArchiveRest;
    if (checks[ArchiveOptionKeys.ArchiveCurrentAndPrevious]) result |= ActionOptionEnums.ArchiveCurrentAndPrevious;
    if (checks[ArchiveOptionKeys.LeaveStubInPlace]) result |= ActionOptionEnums.LeaveStubInPlace;
    if (checks[ArchiveOptionKeys.IncludeDeclaredRecords]) result |= ActionOptionEnums.IncludeDeclaredRecords;
    if (checks[ArchiveOptionKeys.IncludeLockedByRecordsLabel]) result |= ActionOptionEnums.IncludeLockedByRecordsLabel;
    return result;
};

const getOptionsFromDestroyChecks = (checks) => {
    let result = 0;
    if (checks[DestroyOptionKeys.KeepCurrentAndPrevious]) result |= ActionOptionEnums.KeepCurrentAndPrevious;
    if (checks[DestroyOptionKeys.IncludeDeclaredRecords]) result |= ActionOptionEnums.IncludeDeclaredRecords;
    if (checks[DestroyOptionKeys.IncludeLockedByRecordsLabel]) result |= ActionOptionEnums.IncludeLockedByRecordsLabel;
    if (checks[DestroyOptionKeys.DeleteToRecycleBin]) result |= ActionOptionEnums.DeleteToRecycleBin;
    return result;
};

const defaultCriteriaTemplate = [{ 
    order: 1, 
    logic: 0, 
    criteriaType: 1, 
    conditionInfo: { 
        category: 5, 
        logic: 3, 
        value: "[\"*\"]", 
        extraValue: null 
    } 
}];

const PlanProfilePanel = ({ mode = "edit", tenantId, data, onReady }) => {
    const criteriaRef = useRef(null);
    const formRef = useRef(null);
    const stubPanelRef = useRef(null);

    // API Payload Mapped States
    const [name, setName] = useState("");
    const [action, setAction] = useState(ActionEnums.ArchiveAndDestroy);
    const [archiveKeepRestVersion, setArchiveKeepRestVersion] = useState("0");
    const [archivePrevVersion, setArchivePrevVersion] = useState("0");
    const [destroyPrevVersion, setDestroyPrevVersion] = useState("0");

    // UI Logic States
    const [selectedScopes, setSelectedScopes] = useState([]);
    const [archiveChecks, setArchiveChecks] = useState(getEmptyArchiveChecks());
    const [destroyChecks, setDestroyChecks] = useState(getEmptyDestroyChecks());

    const [stubOptions, setStubOptions] = useState([]);
    const [stubTemplateId, setStubTemplateId] = useState("");
    const [isStubValid, setIsStubValid] = useState(true);
    const [showStubSettingsPanel, setShowStubSettingsPanel] = useState(false);

    const [criteriaInfoes, setCriteriaInfoes] = useState(defaultCriteriaTemplate);

    const scopeLazyStep = 10;
    const [scopePager, setScopePager] = useState({ pageIndex: 1, pageSize: 10, key: "", total: 0 });

    // Schedule States
    const [scheduleType, setScheduleType] = useState(ScheduleTypeEnums.NoSchedule);
    const [startTime, setStartTime] = useState(new Date());
    const [intervalValue, setIntervalValue] = useState(1);
    const [intervalUnit, setIntervalUnit] = useState(1);
    const [endType, setEndType] = useState(0);
    const [endAfterCount, setEndAfterCount] = useState(10);
    const [endByDate, setEndByDate] = useState(new Date());

    // Storage location selection
    const [storageOptions, setStorageOptions] = useState([]);
    const [selectedStorage, setSelectedStorage] = useState(null);

    const [initialSelectedSiteIds, setInitialSelectedSiteIds] = useState([]);
    const [loadedScopeIds, setLoadedScopeIds] = useState(new Set());

    const archiveOptionTexts = [
        RMResx.RM_FA_PlanProfile_Action_KeepVersionAndArchiveOther,
        RMResx.RM_FA_PlanProfile_Action_ArchiveVersionAndDestroyFile,
        RMResx.RM_FA_PlanProfile_Action_LeaveStubOption,
        RMResx.RM_FA_PlanProfile_Action_ArchivingRecordsOption,
        RMResx.RM_FA_PlanProfile_Action_RecordsLabelOption
    ];
    const destroyOptionTexts = [
        RMResx.RM_FA_PlanProfile_Action_ArchivingRecordsOption,
        RMResx.RM_FA_PlanProfile_Action_RecordsLabelOption,
        RMResx.RM_FA_PlanProfile_Action_DeleteToRecycleBin
    ];

    const intervalOptions = [
        { id: IntervalTypeEnums.Weekly, Name: RMResx.RM_FA_PlanProfile_ConfigureSchedule_Weeks }, 
        { id: IntervalTypeEnums.Daily, Name: RMResx.RM_FA_PlanProfile_ConfigureSchedule_Days },
        { id: IntervalTypeEnums.Hourly, Name: RMResx.RM_FA_PlanProfile_ConfigureSchedule_Hours }
    ];

    const mergeLoadedIds = (ids) => {
        if (!ids || ids.length === 0) return;
        setLoadedScopeIds((prev) => new Set([...prev, ...ids]));
    };

    const handleScopeLoad = (args) => {
        const pageIndex = Math.floor(args.start / scopeLazyStep) + 1;
        const key = args.key || "";

        const request = mode === "edit" && data?.id
            ? {
                url: "/api/RMDiscoveryPlanProfileApi/GetMappedSitesPaged",
                method: "POST",
                data: { planProfileId: data.id, pageIndex, pageSize: args.count, key }
            }
            : {
                url: "/api/RMDiscoveryPlanProfileApi/GetSiteCollectionsInfo",
                method: "POST",
                data: { pageIndex, pageSize: args.count, key }
            };

        return fetchUtility(request).then((resp) => {
            const list = resp?.items || [];

            if (mode === "edit") {
                mergeLoadedIds(list.map((item) => item.id));
            }

            return list.map((item) => ({
                id: item.id,
                url: item.url || ""
            }));
        }).catch(() => []);
    };

    const loadMappedSites = async (profileId) => {
        try {
            const allSelectedIds = await fetchUtility({
                url: "/api/RMDiscoveryPlanProfileApi/GetAllSelectedSiteByProfileId",
                method: "POST",
                data: profileId
            });

            const baselineIds = Array.isArray(allSelectedIds) ? allSelectedIds : [];
            setInitialSelectedSiteIds(baselineIds);

            const result = await fetchUtility({
                url: "/api/RMDiscoveryPlanProfileApi/GetMappedSitesPaged",
                method: "POST",
                data: { planProfileId: profileId, pageIndex: 1, pageSize: 10, key: "" }
            });

            const list = result?.items || [];
            mergeLoadedIds(list.map((item) => item.id));

            const activeSelections = list
                .filter((x) => x.isPlanProfileSelected)
                .map((x) => ({ id: x.id, url: x.url || "" }));

            setSelectedScopes(activeSelections);
        } catch (error) {
            console.error("[PlanProfilePanel] Failed to load mapped sites.", error);
            setSelectedScopes([]);
            setInitialSelectedSiteIds([]);
            setLoadedScopeIds(new Set());
        }
    };

    const loadStorageOptions = async () => {
        const option = {
            url: "/api/StorageDevice/GetAllActiveStorage",
            method: "POST",
            data: {
                PageIndex: -1,
                PageSize: 10,
                TotalNumber: 0,
                SearchValue: ""
            }
        };

        return fetchUtility(option).then((result) => {
            const list = result?.StorageDeviceUIDtosList || [];
            const defaultId = result?.IndexDeviceId;

            const mapped = list.map((item) => ({
                id: item.Id,
                name: item.Name
            }));
            setStorageOptions(mapped);

            if (mode === "create" && defaultId) {
                const defaultStorage = mapped.find((item) => item.id === defaultId);
                if (defaultStorage) {
                    setSelectedStorage(defaultStorage);
                }
            }
        }).catch(() => {
            setStorageOptions([]);
        });
    };

    const loadStubOptions = async () => {
        try {
            const result = await fetchUtility({
                url: "/api/StubSetting/GetAllStubSettingsNotPaged",
                method: "POST"
            });
            setStubOptions(Array.isArray(result) ? result : []);
        } catch (error) {
            console.error(error);
            setStubOptions([]);
        }
    };

    const isNonNegativeInteger = (value) => /^(0|[1-9]\d*)$/.test(String(value ?? "").trim());

    const clampPositiveInteger = (value) => {
        if (value === "" || value === null || value === undefined) {
            return "";
        }
        const num = parseInt(value, 10);
        if (Number.isNaN(num)) {
            return value;
        }
        if (num < 1) {
            return "1";
        }
        if (num > 65535) {
            return "65535";
        }
        return String(num);
    };

    const isValidDate = (value) => {
        if (!value) return false;
        const date = new Date(value);
        return !Number.isNaN(date.getTime());
    };

    const isFutureOrPresent = (value) => {
        if (!isValidDate(value)) return false;
        return new Date(value) >= new Date();
    };

    const isStartBeforeEnd = (start, end) => {
        if (!isValidDate(start) || !isValidDate(end)) return false;
        return new Date(start) < new Date(end);
    };

    const handleIntervalChange = (value) => {
        setIntervalValue(clampPositiveInteger(value));
    };

    const handleEndAfterCountChange = (value) => {
        setEndAfterCount(clampPositiveInteger(value));
    };

    const isArchiveKeepRestVersionValid = !archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest] || isNonNegativeInteger(archiveKeepRestVersion);
    const isArchivePrevVersionValid = !archiveChecks[ArchiveOptionKeys.ArchiveCurrentAndPrevious] || isNonNegativeInteger(archivePrevVersion);
    const isDestroyPrevVersionValid = !destroyChecks[DestroyOptionKeys.KeepCurrentAndPrevious] || isNonNegativeInteger(destroyPrevVersion);

    useEffect(() => {
        loadStorageOptions();
        loadStubOptions();
    }, [tenantId, mode]);

    useEffect(() => {
        if (mode === "edit" && data) {
            setName(data.name || "");
            const incomingAction = data.action === ActionEnums.DestroyFile
                ? ActionEnums.DestroyFile
                : ActionEnums.ArchiveAndDestroy;

            setAction(incomingAction);
            
            const prevVerStr = String(data.previousVersion ?? 10);
            setArchiveKeepRestVersion(prevVerStr);
            setArchivePrevVersion(prevVerStr);
            setDestroyPrevVersion(prevVerStr);

            loadMappedSites(data.id);
            
            const opts = Number(data.actionOptions || 0);

            if (incomingAction === ActionEnums.ArchiveAndDestroy) {
                setArchiveChecks(getArchiveChecksFromOptions(opts));
                setDestroyChecks(getEmptyDestroyChecks());
                setStubTemplateId(data.stubSetting?.Id || "");
            } else {
                setDestroyChecks(getDestroyChecksFromOptions(opts));
                setArchiveChecks(getEmptyArchiveChecks());
            }

            if (data.storageLocationId && data.storageName) {
                setSelectedStorage({
                    id: data.storageLocationId,
                    name: data.storageName
                });
            }

            if (data.scheduleSetting) {
                setScheduleType(
                    data.scheduleSetting.noSchedule 
                        ? ScheduleTypeEnums.NoSchedule 
                        : ScheduleTypeEnums.Configure
                );

                if (data.scheduleSetting.startTime) {
                    const parsedStart = new Date(data.scheduleSetting.startTime);
                    if (!isNaN(parsedStart.getTime())) setStartTime(parsedStart);
                }

                if (data.scheduleSetting.endTime) {
                    const parsedEnd = new Date(data.scheduleSetting.endTime);
                    if (!isNaN(parsedEnd.getTime())) setEndByDate(parsedEnd);
                }

                setEndType(data.scheduleSetting.endType ?? EndTypeEnums.NoEnd);
                setIntervalValue(data.scheduleSetting.interval ?? 1);
                setIntervalUnit(data.scheduleSetting.intervalType ?? IntervalTypeEnums.Weekly);
                setEndAfterCount(data.scheduleSetting.occurrencesTotal ?? 10);
            }

            const initialRules = (data.criteriaInfoes && data.criteriaInfoes.length > 0)
                ? data.criteriaInfoes
                : defaultCriteriaTemplate;
            setCriteriaInfoes(initialRules);
        } else {
            setSelectedScopes([]);
            setCriteriaInfoes(defaultCriteriaTemplate);
            setInitialSelectedSiteIds([]);
            setLoadedScopeIds(new Set());
        }
    }, [mode, data]);

    const validate = () => {
        const isFormValid = Boolean($$.verify(formRef.current));
        const isCriteriaValid = criteriaRef.current ? criteriaRef.current.onValidate() : true;

        let isStubTemplateValid = true;
        if (archiveChecks[ArchiveOptionKeys.LeaveStubInPlace] && !stubTemplateId) {
            isStubTemplateValid = false;
            setIsStubValid(false);
        } else {
            setIsStubValid(true);
        }

        return isFormValid && isCriteriaValid && isStubTemplateValid;
    };

    const onActionRadioChange = (nextAction) => {
        setAction(nextAction);
    };

    const toggleArchiveCheck = (key) => {
        setArchiveChecks((prev) => {
            const next = { ...prev, [key]: !prev[key] };
            
            if (key === ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest && next[key]) {
                next[ArchiveOptionKeys.ArchiveCurrentAndPrevious] = false;
                next[ArchiveOptionKeys.LeaveStubInPlace] = false;
                setStubTemplateId("");
                setIsStubValid(true);
            }

            if (key === ArchiveOptionKeys.LeaveStubInPlace && !next[key]) {
                setStubTemplateId("");
                setIsStubValid(true);
            }

            return next;
        });
    };

    const toggleDestroyCheck = (key) => {
        setDestroyChecks((prev) => {
            const next = { ...prev, [key]: !prev[key] };
            return next;
        });
    };

    const getPayload = () => {
        const actionOptions = action === ActionEnums.ArchiveAndDestroy
            ? getOptionsFromArchiveChecks(archiveChecks)
            : getOptionsFromDestroyChecks(destroyChecks);

        let finalPreviousVersion = 0;
        if (action === ActionEnums.ArchiveAndDestroy) {
            if (archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]) {
                finalPreviousVersion = Number(archiveKeepRestVersion) || 0;
            } else if (archiveChecks[ArchiveOptionKeys.ArchiveCurrentAndPrevious]) {
                finalPreviousVersion = Number(archivePrevVersion) || 0;
            }
        } else if (action === ActionEnums.DestroyFile) {
            if (destroyChecks[DestroyOptionKeys.KeepCurrentAndPrevious]) {
                finalPreviousVersion = Number(destroyPrevVersion) || 0;
            }
        }
        
        const currentIds = selectedScopes.map((item) => item.id);

        let siteMappings = [];

        if (mode === "create") {
            siteMappings = currentIds.map((siteId) => ({ siteId, isAdd: true }));
        } else {
            const baselineSet = new Set(initialSelectedSiteIds);
            const currentSet = new Set(currentIds);

            currentIds.forEach((siteId) => {
                if (!baselineSet.has(siteId)) {
                    siteMappings.push({ siteId, isAdd: true });
                }
            });

            initialSelectedSiteIds.forEach((siteId) => {
                const wasVisibleToUser = loadedScopeIds.has(siteId);
                if (!currentSet.has(siteId) && wasVisibleToUser) {
                    siteMappings.push({ siteId, isAdd: false });
                }
            });
        }

        const selectedStubObject = stubOptions.find(opt => opt.Id === stubTemplateId) || null;

        const payload = {
            id: mode === "edit" ? (data?.id || 0) : 0,
            name: name.trim(),
            action,
            actionOptions,
            previousVersion: finalPreviousVersion,
            extension1: "",
            extension2: "",
            storageLocationId: selectedStorage?.id || "",
            storageName: selectedStorage?.name || "",
            criteriaInfoes: criteriaInfoes,
            siteMappings: siteMappings,
            stubSetting: archiveChecks[ArchiveOptionKeys.LeaveStubInPlace] ? selectedStubObject : null,
            scheduleSetting: scheduleType === ScheduleTypeEnums.NoSchedule
                ? { noSchedule: true }
                : {
                    id: mode === "edit" ? (data?.scheduleSetting?.id || "") : "",
                    noSchedule: false,
                    startTime: formatBackendDate(startTime),
                    endTime: endType === EndTypeEnums.EndByTime ? formatBackendDate(endByDate) : "",
                    endType,
                    occurrencesTotal: endType === EndTypeEnums.EndByOccurrences ? (Number(endAfterCount) || 0) : 0,
                    interval: Number(intervalValue) || 1,
                    intervalType: intervalUnit,
                    TimeZoneId: RM.TimeSettingModel.TimeZoneId
                }
        };

        return payload;
    };

    useEffect(() => {
        if (typeof onReady === "function") {
            onReady({ validate, getPayload });
        }
    }, [
        name, action, archiveChecks, destroyChecks, archiveKeepRestVersion, archivePrevVersion, destroyPrevVersion, 
        scheduleType, startTime, endType, intervalUnit, intervalValue, 
        endAfterCount, endByDate, selectedStorage, selectedScopes, criteriaInfoes,
        initialSelectedSiteIds, loadedScopeIds,
        stubTemplateId, stubOptions 
    ]);

    const renderScope = () => (
         <div className="margin-bottom-l">
            <R.Expander title={RMResx.RM_FA_PlanProfile_Scope_Title} level={2} status={{ show: true }} togglable={false}>
                <div id="planProfileScopeForm">
                    <div className="margin-bottom-m">
                        <R.Validation 
                            element="Input" 
                            require={RMResx.RM_FA_PlanProfile_ProfileName_Required} 
                            block 
                            label={RMResx.RM_FA_PlanProfile_ProfileName}
                        >
                            <R.Input
                                id="planProfileName"
                                value={name}
                                placeholder="Enter profile name"
                                width="100%"
                                onChange={setName}
                            />
                        </R.Validation>
                    </div>

                    <div>
                        <label className="reco-label require strong">{RMResx.RM_FA_PlanProfile_Scope}</label>
                        <div id="planProfileScopeWrapper">
                            <R.Multicombobox
                                id="planProfileScope"
                                value={selectedScopes}
                                textField="url"
                                valueField="id"
                                width="100%"
                                popupMaxHeight={300}
                                searchable={true}
                                hasSelectAll={false}
                                lazyStep={scopeLazyStep}
                                doLoad={handleScopeLoad}
                                onChange={({ newValue }) => setSelectedScopes(newValue || [])}
                            />
                        </div>
                        <R.ValidationFaker
                            valid={Boolean(selectedScopes && selectedScopes.length > 0)}
                            of="#planProfileScopeWrapper"
                            message={RMResx.RM_FA_PlanProfile_Scope_Required}
                        />
                    </div>
                </div>
            </R.Expander>
         </div>
    );

    const renderCriteria = () => {
        return (
            <div className="margin-bottom-l">
                <R.Expander title={RMResx.RM_FA_PlanProfile_RuleCriteria_Title} level={2} status={{ show: true }} togglable={false}>
                    <div className="plan-profile-criteria-wrapper">
                        <RuleCriteriaManager
                            ref={criteriaRef}
                            analyseMethod={AnalyseMethodConstants.type.AVADocument} 
                            criteriaInfoes={criteriaInfoes}
                            onChange={setCriteriaInfoes}
                        />
                    </div>
                </R.Expander>
            </div>
        );
    };

    const renderArchiveOptions = () => (
        <div className="margin-left-m margin-top-xs flex-column gap-s">
            <div>
                <R.Checkbox
                    checked={archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]}
                    text={archiveOptionTexts[0] || ""}
                    onChange={() => toggleArchiveCheck(ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest)}
                />
                {archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest] && (
                    <div className="margin-top-xs margin-left-m" id="planProfileKeepArchiveVersionWrapper">
                        <R.Validation
                            element="Input"
                            require={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                            block
                        >
                            <R.Input 
                                id="planProfileArchiveKeepPreviousVersion"
                                value={archiveKeepRestVersion} 
                                onChange={setArchiveKeepRestVersion} 
                                width="100%" 
                                type="number"
                            />
                        </R.Validation>
                        <R.ValidationFaker
                            valid={isArchiveKeepRestVersionValid}
                            of="#planProfileKeepArchiveVersionWrapper"
                            message={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                        />
                    </div>
                )}
            </div>
            <div>
                <R.Checkbox
                    checked={archiveChecks[ArchiveOptionKeys.ArchiveCurrentAndPrevious]}
                    text={archiveOptionTexts[1] || ""}
                    onChange={() => toggleArchiveCheck(ArchiveOptionKeys.ArchiveCurrentAndPrevious)}
                    disabled={archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]}
                />
                {archiveChecks[ArchiveOptionKeys.ArchiveCurrentAndPrevious] && (
                    <div className="margin-top-xs margin-left-m" id="planProfileArchivePrevWrapper">
                        <R.Validation
                            element="Input"
                            require={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                            block
                        >
                            <R.Input 
                                id="planProfileArchivePrevVersion"
                                value={archivePrevVersion} 
                                onChange={setArchivePrevVersion} 
                                width="100%" 
                                type="number"
                            />
                        </R.Validation>
                        <R.ValidationFaker
                            valid={isArchivePrevVersionValid}
                            of="#planProfileArchivePrevWrapper"
                            message={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                        />
                    </div>
                )}
            </div>
            <div>
                <R.Checkbox
                    checked={archiveChecks[ArchiveOptionKeys.LeaveStubInPlace]}
                    text={archiveOptionTexts[2] || ""}
                    onChange={() => toggleArchiveCheck(ArchiveOptionKeys.LeaveStubInPlace)}
                    disabled={archiveChecks[ArchiveOptionKeys.KeepCurrentAndSpecifiedArchiveRest]}
                />
                
                {archiveChecks[ArchiveOptionKeys.LeaveStubInPlace] && (
                    <div className="margin-top-xs margin-left-m" id="planProfileLeaveStubWrapper">
                        <R.Combobox
                            items={stubOptions}
                            textField="Name"
                            valueField="Id"
                            value={stubOptions.find(opt => opt.Id === stubTemplateId) || null}
                            onChange={(e) => {
                                setStubTemplateId(e?.newValue?.Id || "");
                                if (e?.newValue?.Id) setIsStubValid(true);
                            }}
                            placeholder=""
                            width="100%"
                            createNewText={RMResx.RM_FA_PlanProfile_CreateStubTemplate}
                            doCreateNew={() => setShowStubSettingsPanel(true)}
                        />
                        <R.ValidationFaker
                            valid={isStubValid}
                            of="#planProfileLeaveStubWrapper"
                            message={RMResx.RM_FA_PlanProfile_LeaveStub_Required}
                        />
                    </div>
                )}
            </div>
            <R.Checkbox
                checked={archiveChecks[ArchiveOptionKeys.IncludeDeclaredRecords]}
                text={archiveOptionTexts[3] || ""}
                onChange={() => toggleArchiveCheck(ArchiveOptionKeys.IncludeDeclaredRecords)}
            />
            <R.Checkbox
                checked={archiveChecks[ArchiveOptionKeys.IncludeLockedByRecordsLabel]}
                text={archiveOptionTexts[4] || ""}
                onChange={() => toggleArchiveCheck(ArchiveOptionKeys.IncludeLockedByRecordsLabel)}
            />
        </div>
    );

    const renderDestroyOptions = () => (
        <div className="margin-left-m margin-top-xs flex-column gap-s">
            <div>
                <R.Checkbox
                    checked={destroyChecks[DestroyOptionKeys.KeepCurrentAndPrevious]}
                    text={RMResx.RM_FA_PlanProfile_Action_KeepCurrentVersionNumber}
                    onChange={() => toggleDestroyCheck(DestroyOptionKeys.KeepCurrentAndPrevious)}
                />
                {destroyChecks[DestroyOptionKeys.KeepCurrentAndPrevious] && (
                    <div className="margin-top-xs margin-left-m" id="planProfilePreviousVersionWrapper">
                        <R.Validation
                            element="Input"
                            require={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                            block
                        >
                            <R.Input 
                                id="planProfilePreviousVersion"
                                value={destroyPrevVersion} 
                                onChange={setDestroyPrevVersion} 
                                width="100%"
                                type="number"
                            />
                        </R.Validation>
                        <R.ValidationFaker
                            valid={isDestroyPrevVersionValid}
                            of="#planProfilePreviousVersionWrapper"
                            message={RMResx.RM_FA_PlanProfile_Validation_PreviousVersion_Invalid}
                        />
                    </div>
                )}
            </div>

            <R.Checkbox
                checked={destroyChecks[DestroyOptionKeys.IncludeDeclaredRecords]}
                text={destroyOptionTexts[0] || ""}
                onChange={() => toggleDestroyCheck(DestroyOptionKeys.IncludeDeclaredRecords)}
            />
            <R.Checkbox
                checked={destroyChecks[DestroyOptionKeys.IncludeLockedByRecordsLabel]}
                text={destroyOptionTexts[1] || ""}
                onChange={() => toggleDestroyCheck(DestroyOptionKeys.IncludeLockedByRecordsLabel)}
            />
            <R.Checkbox
                checked={destroyChecks[DestroyOptionKeys.DeleteToRecycleBin]}
                text={destroyOptionTexts[2] || ""}
                onChange={() => toggleDestroyCheck(DestroyOptionKeys.DeleteToRecycleBin)}
            />
        </div>
    );

    const renderAction = () => (
        <div className="margin-bottom-l">
            <R.Expander title={RMResx.RM_FA_PlanProfile_Action_Title} level={2} status={{ show: true }} togglable={false}>
                <div className="padding-m" id="planProfileActionWrapper">
                    <p className="margin-bottom-s strong">{RMResx.RM_FA_PlanProfile_Action_RadioGroup_Title}</p>
                    
                    <div className="margin-bottom-m">
                        <R.Radio 
                            group="actionSettings" 
                            checked={action === ActionEnums.ArchiveAndDestroy} 
                            text={RMResx.RM_FA_PlanProfile_Action_Radio_ArchiveAndDestroy}
                            onChange={() => onActionRadioChange(ActionEnums.ArchiveAndDestroy)} 
                        />
                        {action === ActionEnums.ArchiveAndDestroy && renderArchiveOptions()}
                    </div>

                    <div>
                        <R.Radio 
                            group="actionSettings" 
                            checked={action === ActionEnums.DestroyFile} 
                            text={RMResx.RM_FA_PlanProfile_Action_Radio_Destroy}
                            onChange={() => onActionRadioChange(ActionEnums.DestroyFile)} 
                        />
                        {action === ActionEnums.DestroyFile && renderDestroyOptions()}
                    </div>
                </div>
            </R.Expander>
        </div>
    );

    const renderConfigureTimePanel = () => {
        const selectedIntervalObj = intervalOptions.find(opt => opt.id === intervalUnit) || null;

        const isStartTimeNotEarlierThanNow = !isValidDate(startTime) || isFutureOrPresent(startTime);
        const isStartBeforeEndValid =
            endType !== EndTypeEnums.EndByTime ||
            !isValidDate(startTime) ||
            !isValidDate(endByDate) ||
            isStartBeforeEnd(startTime, endByDate);

        const isEndAfterCountValid =
            endType !== EndTypeEnums.EndByOccurrences ||
            (Boolean(endAfterCount) && Number(endAfterCount) >= 1 && Number(endAfterCount) <= 65535);
        
        const isIntervalValid =
            Boolean(intervalValue) &&
            Number(intervalValue) >= 1 &&
            Number(intervalValue) <= 65535;

        return (
            <div className="margin-left-m margin-top-s flex-column gap-m">
                <div className="flex align-center gap-s">
                    <span style={{ width: "102px" }}>{RMResx.RM_FA_PlanProfile_ConfigureSchedule_StartTime}</span>
                    <div style={{ width: "200px" }} id="planProfileStartTimeWrapper">
                        <R.Validation
                            element="Datepicker"
                            require={RMResx.RM_FA_PlanProfile_ConfigureSchedule_StartTime_Required || true}
                            block
                        >
                            <R.Datepicker 
                                clearable 
                                hasTimePicker 
                                hasToday="withTime" 
                                selectedDate={startTime} 
                                dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                                onChange={(args) => setStartTime(args?.newValue || null)} 
                            />
                        </R.Validation>
                    </div>
                </div>
                <div>
                    <R.ValidationFaker
                        valid={isStartTimeNotEarlierThanNow}
                        of="#planProfileStartTimeWrapper"
                        message={RMResx.RM_FA_PlanProfile_ConfigureSchedule_StartTime_NotEarlierThanNow}
                    />
                </div>

                <div className="flex align-center gap-s">
                    <span style={{ width: "90px" }}>{RMResx.RM_FA_PlanProfile_ConfigureSchedule_Interval}</span>
                    <div style={{ width: "100px" }} id="planProfileIntervalWrapper">
                        <R.Input 
                            type="number" 
                            hasControl 
                            loop 
                            min={1} 
                            value={intervalValue} 
                            onChange={handleIntervalChange} 
                        />
                    </div>
                    <div style={{ width: "140px" }}>
                        <R.Combobox 
                            items={intervalOptions} 
                            textField="Name" 
                            valueField="id"
                            value={selectedIntervalObj}
                            searchable={false}
                            onChange={(e) => {
                                if (e?.newValue) setIntervalUnit(e.newValue.id);
                            }} 
                            placeholder=""
                        />
                    </div>
                </div>
                <div>
                    <R.ValidationFaker
                        valid={isIntervalValid}
                        of="#planProfileIntervalWrapper"
                        message={RMResx.RM_FA_PlanProfile_ConfigureSchedule_Interval_Required}
                    />
                </div>

                <div className="flex gap-s">
                    <span style={{ width: "90px" }}>{RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndTime}</span>
                    <div className="flex-column gap-s">
                        <R.Radio 
                            group="scheduleEndType" 
                            checked={endType === EndTypeEnums.NoEnd} 
                            text={RMResx.RM_FA_PlanProfile_ConfigureSchedule_NoEndDate}
                            onChange={() => setEndType(EndTypeEnums.NoEnd)} 
                        />
                        
                        <div className="flex align-center gap-s">
                            <R.Radio 
                                group="scheduleEndType" 
                                checked={endType === EndTypeEnums.EndByOccurrences} 
                                text={RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndAfter}
                                onChange={() => setEndType(EndTypeEnums.EndByOccurrences)} 
                            />
                            <div style={{ width: "90px" }} id="planProfileEndAfterWrapper">
                                <R.Input 
                                    type="number" 
                                    hasControl 
                                    loop 
                                    min={1} 
                                    value={endAfterCount} 
                                    onChange={handleEndAfterCountChange} 
                                    disabled={endType !== EndTypeEnums.EndByOccurrences} 
                                />
                            </div>
                            <span>{RMResx.RM_FA_PlanProfile_ConfigureSchedule_Occurrences}</span>
                        </div>
                        <div>
                            <R.ValidationFaker
                                valid={isEndAfterCountValid}
                                of="#planProfileEndAfterWrapper"
                                message={RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndAfter_Required}
                            />
                        </div>

                        <div className="flex align-center gap-s">
                            <R.Radio 
                                group="scheduleEndType" 
                                checked={endType === EndTypeEnums.EndByTime} 
                                text={RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndBy} 
                                onChange={() => setEndType(EndTypeEnums.EndByTime)} 
                            />
                            <div id="planProfileEndByWrapper">
                                <R.Validation
                                    element="Datepicker"
                                    require={endType === EndTypeEnums.EndByTime ? (RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndBy_Required || true) : false}
                                    block
                                >
                                    <R.Datepicker 
                                        clearable 
                                        hasTimePicker 
                                        hasToday="withTime" 
                                        selectedDate={endByDate} 
                                        dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                                        onChange={(args) => setEndByDate(args?.newValue || null)} 
                                        disabled={endType !== EndTypeEnums.EndByTime} 
                                    />
                                </R.Validation>
                            </div>
                        </div>
                        <div>
                            <R.ValidationFaker
                                valid={isStartBeforeEndValid}
                                of="#planProfileEndByWrapper"
                                message={RMResx.RM_FA_PlanProfile_ConfigureSchedule_EndBy_MustBeLaterThanStart}
                            />
                        </div>
                    </div>
                </div>
            </div>
        );
    };

    const renderSchedule = () => (
        <div className="margin-bottom-l">
            {action === ActionEnums.ArchiveAndDestroy && (
                <div className="margin-bottom-m">
                    <label className="reco-label require strong">{RMResx.RM_FA_PlanProfile_Schedule_Storage_Location}</label>
                    <div id="planProfileStorageLocationWrapper">
                        <R.Combobox
                            id="planProfileStorageLocation"
                            items={storageOptions}
                            textField="name"
                            valueField="id"
                            value={selectedStorage} 
                            placeholder="Select storage location"
                            width="100%"
                            onChange={({ newValue }) => {
                                setSelectedStorage(
                                    newValue
                                        ? { id: newValue.id, name: newValue.name }
                                        : null
                                );
                            }}
                        />
                    </div>
                    <R.ValidationFaker
                        valid={Boolean(selectedStorage?.id && selectedStorage?.name)}
                        of="#planProfileStorageLocationWrapper"
                        message={RMResx.RM_FA_PlanProfile_Schedule_Storage_Location_Required || RMResx.RM_AR_CP_Common_SelEmpty}
                    />
                </div>
            )}

            <p className="strong">{RMResx.RM_FA_PlanProfile_Schedule_StartTime}</p>

            <div className="margin-bottom-s">
                <R.Radio 
                    group="mainScheduleType" 
                    checked={scheduleType === ScheduleTypeEnums.NoSchedule} 
                    text={RMResx.RM_FA_PlanProfile_Schedule_NoSchedule}
                    onChange={() => setScheduleType(ScheduleTypeEnums.NoSchedule)} 
                />
            </div>

            <div>
                <R.Radio 
                    group="mainScheduleType" 
                    checked={scheduleType === ScheduleTypeEnums.Configure} 
                    text={RMResx.RM_FA_PlanProfile_Schedule_ConfigureSchedule}
                    onChange={() => setScheduleType(ScheduleTypeEnums.Configure)} 
                />
                
                {scheduleType === ScheduleTypeEnums.Configure && renderConfigureTimePanel()}
            </div>
        </div>
    );

    const renderStubSettingsPanel = () => {
        return (
            <R.Panel
                id="planProfileStubSettingsPanel"
                header={RMResx.RM_JS_Rule_Stub_PanelTitle_CreateTemplate}
                size={670}
                status={{ show: showStubSettingsPanel }}
                destroy={true}
                onClose={() => setShowStubSettingsPanel(false)}
            >
                <StubPanel
                    ref={stubPanelRef}
                    id="stubSettingsPanel"
                    cellStubId={null}
                    recordsLabelValue={""}
                />
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={() => setShowStubSettingsPanel(false)}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={() => {
                            if (stubPanelRef.current && stubPanelRef.current.onSave) {
                                stubPanelRef.current.onSave((success) => {
                                    if (success) {
                                        setShowStubSettingsPanel(false);
                                        loadStubOptions();
                                    }
                                });
                            }
                        }}
                    />
                </>
            </R.Panel>
        );
    };

    return (
        <div className="reco-plan-profile-panel-container">
            <R.Validation>
                <div id="planProfilePanelForm" ref={formRef}>
                    {renderScope()}
                    {renderCriteria()}
                    {renderAction()}
                    {renderSchedule()}
                    {renderStubSettingsPanel()}
                </div>
            </R.Validation>
        </div>
    );
};

export default PlanProfilePanel;