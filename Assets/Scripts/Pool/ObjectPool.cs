using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPool<T> where T : MonoBehaviour {
    private Stack<T> pool;
    private T prefab;
    private Transform parentTransform;

    public ObjectPool(T prefab, int initialSize, Transform parentTra = null) {
        this.prefab = prefab;
        parentTransform = parentTra;
        pool = new Stack<T>();

        for (int i = 0; i < initialSize; i++) {
            CreateNewObject();
        }
    }

    private void CreateNewObject() {
        T obj = Object.Instantiate(prefab, parentTransform);
        obj.gameObject.SetActive(false);
        pool.Push(obj);
    }

    public T GetObject() {
        if (pool.Count > 0) {
            T obj = pool.Pop();
            obj.gameObject.SetActive(true);
            return obj;
        } else {
            T obj = Object.Instantiate(prefab, parentTransform);
            obj.gameObject.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(T obj) {
        obj.gameObject.transform.parent = parentTransform;
        obj.gameObject.SetActive(false);
        pool.Push(obj);
    }
}