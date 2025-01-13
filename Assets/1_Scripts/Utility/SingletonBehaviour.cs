using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning($"{nameof(T)}를 찾을 수 없습니다. 임의로 객체를 생성합니다.");
                
                GameObject obj = new GameObject(nameof(T), typeof(T));
                _instance = obj.GetComponent<T>();
                DontDestroyOnLoad(_instance);
            }
            
            return _instance;
        }
    }
    private static T _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
