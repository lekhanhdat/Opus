import _ from "lodash";
import TenantUpgradeOptions from "./TenantUpgradeOptions";

export default class TenantUpgradeManager {

    static async getInstance() {
        try {
            if (_.isNil(this._instance)) {
                this._upgradeInfo = await fetchUtility({ url: "/api/ManualApproval/GetTenantUpgradeInfo" });
                if(_.isNil(this._upgradeInfo)) {
                    console.error("Get upgrade information has an error.");
                    return null;
                }
                this._instance = new TenantUpgradeManager();
            }
            return this._instance;
        }
        catch (e) {
            console.error(`An error occurred while init tenant upgrade manager. Error: ${e}`);
            return null;
        }
    }

    isComplete(upgradeOption) {
        return TenantUpgradeManager._upgradeInfo.Completed | upgradeOption === TenantUpgradeManager._upgradeInfo.Completed;
    }

    isSucceed(upgradeOption) {
        return TenantUpgradeManager._upgradeInfo.Succeed | upgradeOption === TenantUpgradeManager._upgradeInfo.Succeed;
    }

    isException(upgradeOption) {
        return TenantUpgradeManager._upgradeInfo.HasException | upgradeOption === TenantUpgradeManager._upgradeInfo.HasException;
    }

    isFailed(upgradeOption) {
        return TenantUpgradeManager._upgradeInfo.Failed | upgradeOption === TenantUpgradeManager._upgradeInfo.Failed;
    }
}

export { TenantUpgradeOptions };