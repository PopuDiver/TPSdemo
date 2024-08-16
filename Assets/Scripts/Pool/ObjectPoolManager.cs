using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPoolManager : MonoBehaviour {
    private Dictionary<Type, object> poolDictionary = new();

    public ObjectPool<T> GetOrCreatePool<T>(T prefab, int initialSize, Transform parent = null) where T : MonoBehaviour {
        Type type = typeof(T);

        if (poolDictionary.TryGetValue(type, out object pool)) {
            return (ObjectPool<T>)pool;
        } else {
            ObjectPool<T> newPool = new ObjectPool<T>(prefab, initialSize, parent);
            poolDictionary[type] = newPool;
            return newPool;
        }
    }

    public ObjectPool<T> GetPool<T>() where T : MonoBehaviour {
        Type type = typeof(T);

        if (poolDictionary.TryGetValue(type, out object pool)) {
            return (ObjectPool<T>)pool;
        } else {
            Debug.LogError($"No pool found for type {type}");
            return null;
        }
    }

    public T GetObject<T>(T prefab, int initialSize, Transform parent = null) where T : MonoBehaviour {
        ObjectPool<T> pool = GetOrCreatePool(prefab, initialSize, parent);
        return pool.GetObject();
    }

    public void ReturnObject<T>(T obj) where T : MonoBehaviour {
        ObjectPool<T> pool = GetPool<T>();
        if (pool != null) {
            pool.ReturnObject(obj);
        } else {
            Debug.LogError($"No pool found for type {typeof(T)}");
        }
    }
}