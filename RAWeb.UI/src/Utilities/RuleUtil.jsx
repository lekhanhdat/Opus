//rule DisposalAction util
import { string } from "prop-types";
import {
    DisposalAction,
    NewLogicDisposalAction,
    NewLogicMainOptionSet,
    FSDisposalAction,
    SPDisposalAction,
    SourceFlags,
} from "../Constants/Constants";
import { LicenseHelper } from "./CommonUtil";

export default {
    getOldLogicDisposalAction: function (disposalAction) {
        Object.keys(NewLogicDisposalAction).forEach(key => {
            const action = parseInt(key, 10); 
            if ((disposalAction & action) === action) {
                disposalAction -= action;
            }
        });
        return disposalAction;
    },

    
    parseDisposalAction: function (disposalAction) {
        let res = "";

        const oldLogicKey =  this.getOldLogicDisposalAction(disposalAction);
        if (DisposalAction[oldLogicKey]) {
            res = DisposalAction[oldLogicKey]; 
        }

        Object.keys(NewLogicDisposalAction).forEach(key => {
            const action = parseInt(key, 10); 
            if ((disposalAction & action) === action) {
                if(!(res == null || res.trim() === "")){
                    res += "; "
                }
                res += NewLogicDisposalAction[key];
            }
        });
        return res;
    },

    parseDisposalActionForSP: function (disposalAction, source = SourceFlags.SP) {
        let res = "";
        // RECO-30632 - Remove declare option for OneDrive
        let type = RM.Url.getParam(window.location.href, "type");
        if ((type == 6103 || source == SourceFlags.OneDrive) && disposalAction == 1 && LicenseHelper.EnableRecordsArchiver()) {
            const newActionName = RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent;
            SPDisposalAction[disposalAction] = newActionName;
        }
        
        if (!LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver() && disposalAction == 1 && [SourceFlags.SP, SourceFlags.OneDrive, SourceFlags.Teams].includes(source)) {
            SPDisposalAction[disposalAction] = RMResx.RM_JS_RDM_CreateRule_Options_TagOrLock;
        }

        if (LicenseHelper.EnableRecordsArchiver() && disposalAction == 4194304) {
            return RMResx.RM_JS_RDM_CreateRule_Options_StoreInM365Archive;
        }

        const oldLogicKey =  this.getOldLogicDisposalAction(disposalAction);
        if (SPDisposalAction[oldLogicKey] && !this.disposalActionUseNewLogicMainOption(disposalAction)) {
            res = SPDisposalAction[oldLogicKey]; 
        }

        Object.keys(NewLogicDisposalAction).forEach(key => {
            const action = parseInt(key, 10); 
            if ((disposalAction & action) === action) {
                if(!(res == null || res.trim() === "")){
                    res += "; "
                }
                res += NewLogicDisposalAction[key];
            }
        });
        return res;
    },

    disposalActionUseNewLogicMainOption : function(disposalAction){
        let res = false;
        Object.keys(NewLogicMainOptionSet).forEach(key => {
            const action = parseInt(key, 10); 
            if ((disposalAction & action) === action) {
                res = true;
            }
        });
        return res;
    },

    parseDisposalActionForFS : function (disposalAction){
        let res = "";

        const oldLogicKey =  this.getOldLogicDisposalAction(disposalAction);
        if (FSDisposalAction[oldLogicKey] && !this.disposalActionUseNewLogicMainOption(disposalAction)) {
            res = FSDisposalAction[oldLogicKey]; 
        }

        Object.keys(NewLogicDisposalAction).forEach(key => {
            const action = parseInt(key, 10); 
            if ((disposalAction & action) === action) {
                if(!(res == null || res.trim() === "")){
                    res += "; "
                }
                res += NewLogicDisposalAction[key];
            }
        });
        return res;
    }


};