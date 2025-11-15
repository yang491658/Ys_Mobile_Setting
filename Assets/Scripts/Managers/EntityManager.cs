using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { private set; get; }

#if UNITY_EDITOR
    private void OnValidate()
    {
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region 소환
    #endregion

    #region 제거
    #endregion

    #region SET
    #endregion

    #region GET
    #endregion
}
