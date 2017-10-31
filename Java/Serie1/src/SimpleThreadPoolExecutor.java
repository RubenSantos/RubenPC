import sun.reflect.generics.reflectiveObjects.NotImplementedException;
import utils.SynchUtils;

import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.RejectedExecutionException;

public class SimpleThreadPoolExecutor {

    private int maxPoolSize;
    private int keepAliveTime;
    private boolean shuttingDown;
    private Object monitor = new Object();
    private List<WorkerThread> workingThreads = new LinkedList<WorkerThread>();

    private class WorkerThread extends Thread{
        Thread thread;
        long aliveSince;

        WorkerThread(Runnable cmd){
            super(() ->{
                Thread t = new Thread(cmd);
                t.run();
                try {
                    t.join();
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }
            });
        }

        @Override
        public void run() {

        }
    }

    public SimpleThreadPoolExecutor(int maxPoolSize, int keepAliveTime){
        this.keepAliveTime = keepAliveTime;
        this.maxPoolSize = maxPoolSize;
    }

    public boolean execute(Runnable command, long timeout) throws InterruptedException {
        synchronized (monitor){
            if(shuttingDown)
                throw new RejectedExecutionException();
            WorkerThread workerThread;
            if(workingThreads.size() < maxPoolSize) {
                workerThread = workingThreads.stream().filter(wt -> !wt.isAlive()).findFirst().orElse(null);
                if(workerThread == null){
                    workerThread = new WorkerThread(command);
                    workingThreads.add(workerThread);
                    workerThread.run();
                    return true;
                }
                workerThread = new WorkerThread(command);
                workerThread.run();
                return true;
            }
            do{
                monitor.wait(timeout);
                timeout = SynchUtils.remainingTimeout(System.currentTimeMillis(), timeout);
                if(timeout == 0)
                    return false;
                workerThread = workingThreads.stream().filter(wt -> !wt.isAlive()).findFirst().orElse(null);
            }while(true);
        }
    }

    public void shutdown(){
        throw new NotImplementedException();
    }

    public boolean awaitTermination(int timeout){
        throw new NotImplementedException();
    }
}
