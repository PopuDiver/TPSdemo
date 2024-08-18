using System;
using System.Collections.Generic;

public enum EventType {
    PlayerAttack,
    PlayerHealthChange,
    GameOverPlayerUI
}

public class EventControl {
    private static EventControl instance;
    private EventControl() { }
    public static EventControl Instance {
        get {
            if (instance == null) {
                instance = new EventControl();
            }
            return instance;
        }
    }
    
    public Dictionary<EventType, List<Delegate>> handlers = new Dictionary<EventType, List<Delegate>>();
    
    public void Invoke(EventType eventType) {
        if (handlers.TryGetValue(eventType, out var list)) {
            foreach (var e in list) {
                ((Action)e).Invoke();
            }
        }
    }
    
    public void Invoke<T>(EventType eventType, T para) {
        if (handlers.TryGetValue(eventType, out var list)) {
            foreach (var e in list) {
                ((Action<T>)e).Invoke(para);
            }
        }
    }
    
    public void Invoke<T1, T2>(EventType eventType, T1 para1, T2 para2) {
        if (handlers.TryGetValue(eventType, out var list)) {
            foreach (var e in list) {
                ((Action<T1, T2>)e).Invoke(para1, para2);
            }
        }
    }
    
    public void Register(EventType eventType, Action onEvent) {
        if (!handlers.TryGetValue(eventType, out var deleList)) {
            deleList = new List<Delegate>();
            handlers.Add(eventType, deleList);
        }
        deleList.Add(onEvent);
    }

    public void Register<T>(EventType eventType, Action<T> onEvent) {
        if (!handlers.TryGetValue(eventType, out var deleList)) {
            deleList = new List<Delegate>();
            handlers.Add(eventType, deleList);
        }
        deleList.Add(onEvent);
    }
    
    public void Register<T1, T2>(EventType eventType, Action<T1, T2> onEvent) {
        if (!handlers.TryGetValue(eventType, out var deleList)) {
            deleList = new List<Delegate>();
            handlers.Add(eventType, deleList);
        }
        deleList.Add(onEvent);
    }
    
    public void UnRegister(EventType eventType) {
        if (handlers.TryGetValue(eventType, out var list)) {
            handlers.Remove(eventType);
        }
    }
    
    public void UnRegister(EventType eventType, Action onEvent) {
        if (handlers.TryGetValue(eventType, out var list)) {
            list.Remove(onEvent);
        }
    }
    
    public void UnRegister<T>(EventType eventType, Action<T> onEvent) {
        if (handlers.TryGetValue(eventType, out var list)) {
            list.Remove(onEvent);
        }
    }
    
    public void UnRegister<T1, T2>(EventType eventType, Action<T1, T2> onEvent) {
        if (handlers.TryGetValue(eventType, out var list)) {
            list.Remove(onEvent);
        }
    }
}