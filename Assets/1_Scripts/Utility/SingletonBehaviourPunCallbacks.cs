using Photon.Pun;
using UnityEngine;

// TODO: 싱글톤이 파괴된 이후에 참조되는 경우(특히 게임 종료 시)를 막기 위해 IsAlive 속성 필요
public class SingletonBehaviourPunCallbacks<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T instance = null;
    
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning($"{typeof(T).Name}를 찾을 수 없습니다. 임의로 객체를 생성합니다.");
                
                GameObject obj = new GameObject(nameof(T), typeof(T));
                instance = obj.GetComponent<T>();
                DontDestroyOnLoad(instance);
            }
            
            return instance;
        }
    }

    public static bool IsAlive => instance != null;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
