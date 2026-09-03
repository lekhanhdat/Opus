import { useCallback, useLayoutEffect, useRef } from "react";

export const useStableCallback = (handler) => {
    const handlerRef = useRef(null);

    useLayoutEffect(() => {
        handlerRef.current = handler;
    });

    return useCallback((...args) => {
        const fn = handlerRef.current;
        return fn(...args);
    }, []);
};
