package utils;

public class SynchUtils {
    public static final long INFINITE = -1;

    public static long remainingTimeout(long refTime, long timeout) {
        if (timeout == INFINITE) return timeout;
        return Math.max(0, timeout - (System.currentTimeMillis() - refTime));
    }
}
