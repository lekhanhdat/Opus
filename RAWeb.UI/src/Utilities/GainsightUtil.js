import { RoleType } from "../Constants/Constants";
import AssertUtil from "./AssertUtil";
import { HashMd5 } from "./GenerateHashCode";
import _ from "lodash";

export default class GainsightUtil {
    static GainsightIsInit() {
        // if (RM.gData.disabledGainsight) {
        //     return false;
        // }

        // initialize property: If the real analytics.js is already on the page return.
        // const isInit = window.analytics && window.analytics.initialize;
        // const isInit = window.analytics;
        // if (!isInit) {
        //     console.warn("Gainsight is not initialized.");
        // }
        return !RM.gData.disabledGainsight;
    }

    static PageSimple() {
        if (!GainsightUtil.GainsightIsInit()) {
            return;
        }

    }

    static Page(pageName, pageCategory = undefined) {
        if (!GainsightUtil.GainsightIsInit()) {
            return;
        }

        try {
            AssertUtil(
                pageName !== undefined &&
                    pageName !== null &&
                    pageName.length > 0,
                `Illegal parameter: ${pageName}`
            );

            if (
                pageCategory === undefined ||
                pageCategory === null ||
                pageCategory.length === 0
            ) {
                window.analytics.page(pageName);
                return;
            }
            window.analytics.page(pageCategory, pageName);
        } catch (error) {
            console.error(error);
        }
    }

    static Identity() {
        try {
            if (!GainsightUtil.GainsightIsInit()) {
                return;
            }
            const {
                userName,
                emailAddress,
                userId,
                dataCenter,
                logonGroupId,
                company,
                accountNumber,
                enviromentName,
            } = RM.gData;
            if (!userName) {
                console.warn("User name doesn't exist.");
                return;
            }
            if (!emailAddress) {
                console.warn("User emial doesn't exist.");
                return;
            }

            if (!dataCenter) {
                console.warn("Data center doesn't exist.");
            }

            if (!userId) {
                console.warn("User Id doesn't exist.");
            }

            // trail license account number is null
            let tempAccountNumber = !_.isNil(accountNumber)
                ? accountNumber
                : "null";
            //user group info
            const index = emailAddress.indexOf("@");
            const domain = emailAddress.substr(index);
            const usertype =
                RM.RoleType === RoleType.SupAdmin ? "admin" : "non-admin";

            if (window.aptrinsic) {
                window.aptrinsic("identify", {
                    id: userId,
                    name: HashMd5(userName),
                    email: HashMd5(emailAddress) + domain,
                    usertype: usertype,
                    datacenter: dataCenter,
                    environment: enviromentName,
                    groupid: tempAccountNumber,
                }, {
                    id: tempAccountNumber,
                    accountid: logonGroupId,
                    name: company,
                    environment: enviromentName,
                    accountnumber: tempAccountNumber,
                    datacenter: dataCenter,
                });
            }
            
        } catch (error) {
            console.error(error);
        }
    }
}
