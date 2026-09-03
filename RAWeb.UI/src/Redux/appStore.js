import { configureStore } from "@reduxjs/toolkit";
import jmSlice from "./slices/jmSlice";
import pickListSlice from "./slices/pickListSlice";
import avaDialogSlice from "./slices/avaDialogSlice";

export const store = configureStore({
    reducer: {
        jobMonitor: jmSlice,
        pickList: pickListSlice,
        avaDialog: avaDialogSlice
    },
});
