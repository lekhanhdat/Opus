import { createSlice } from "@reduxjs/toolkit";

const initialState = {
    externalActionRequest: null
};

const avaDialogSlice = createSlice({
    name: "avaDialog",
    initialState,
    reducers: {
        setAvaExternalActionRequest: (state, action) => {
            state.externalActionRequest = action.payload ?? null;
        },
        clearAvaExternalActionRequest: (state) => {
            state.externalActionRequest = null;
        }
    }
});

export const { setAvaExternalActionRequest, clearAvaExternalActionRequest } = avaDialogSlice.actions;
export default avaDialogSlice.reducer;