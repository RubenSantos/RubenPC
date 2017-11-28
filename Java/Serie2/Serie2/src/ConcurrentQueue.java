import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.concurrent.atomic.AtomicReference;

public class ConcurrentQueue<T> {

    private class Node<T>{
        final T item;
        final AtomicReference<Node> next;
        private Node(T item, Node next){
            this.item = item;
            this.next = new AtomicReference(next);
        }
    }

    private final Node dummy;
    private final AtomicReference<Node> head;
    private final AtomicReference<Node> tail;

    public ConcurrentQueue(){
        dummy = new Node(null, null);
        head = new AtomicReference(dummy);
        tail = new AtomicReference(dummy);
    }

    public void put(T t){
        Node newNode = new Node(t, null);
        while(true){
            Node curTail = tail.get();
            Node tailNext = (Node)curTail.next.get();
            if(curTail == tail.get()){
                if(tailNext != null){
                    tail.compareAndSet(curTail, tailNext);
                } else {
                    if(curTail.next.compareAndSet(null, newNode)){
                        tail.compareAndSet(curTail, newNode);
                        return;
                    }
                }
            }
        }
    }

    public boolean isEmpty(){
        return head.equals(tail);
    }

    public T tryTake(){
        while (true){
            Node curHead = head.get();
            Node headNext = (Node)curHead.next.get();
            if(headNext == null){
                return null;
            }
            if(head.compareAndSet(curHead, headNext)){
                return (T)curHead.item;
            }

        }
    }
}
