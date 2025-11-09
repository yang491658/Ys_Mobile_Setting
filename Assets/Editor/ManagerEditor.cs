#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ManagerEditor
{
    private static bool IsPlaying() => !EditorApplication.isPlaying;

    #region 초기화
    private static void ResetInspector(Component _comp)
    {
        Undo.RegisterCompleteObjectUndo(_comp, "Reset");
        Unsupported.SmartReset(_comp);
        EditorUtility.SetDirty(_comp);
        EditorSceneManager.MarkSceneDirty(_comp.gameObject.scene);
    }

    private static void ResetAll(Type _type)
    {
        var _objs = UnityEngine.Object.FindObjectsByType(
            _type, FindObjectsInactive.Include, FindObjectsSortMode.None
        );
        for (int i = 0; i < _objs.Length; i++)
        {
            var _comp = _objs[i] as Component;
            if (_comp != null) ResetInspector(_comp);
        }
    }

    [MenuItem("Tools/스크립트 초기화", true)]
    private static bool ResetManagers_Validate() => IsPlaying();
    [MenuItem("Tools/스크립트 초기화", false, 1)]
    private static void ResetManagers()
    {
        var _types = new Type[]
        {
            typeof(GameManager),
            typeof(SoundManager),
            typeof(EntityManager),
            typeof(HandleManager),
            typeof(UIManager),
            typeof(ADManager),
            typeof(TestManager),
            typeof(AutoCamera),
            typeof(AutoUICanvas),
            typeof(AutoBackground),
        };
        for (int i = 0; i < _types.Length; i++) ResetAll(_types[i]);
    }
    #endregion

    #region 켜기/끄기
    private static T FindSingle<T>() where T : Component
        => UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);

    private static bool AnyActive<T>() where T : Component
    {
        var c = FindSingle<T>();
        if (c == null) return false;
        var go = c.gameObject;
        return (go != null) && go.activeSelf;
    }

    private static void SetActive<T>(bool _on, string _onLabel, string _offLabel) where T : Component
    {
        var c = FindSingle<T>();
        if (c == null) return;
        var go = c.gameObject;
        if (go == null) return;

        Undo.RegisterFullObjectHierarchyUndo(go, _on ? _onLabel : _offLabel);
        go.SetActive(_on);
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
    #endregion

    #region UI
    [MenuItem("Tools/UI 켜기", true)]
    private static bool UIsOnValidate() => IsPlaying() && !AnyActive<UIManager>();
    [MenuItem("Tools/UI 켜기", false, 101)]
    private static void UIsOn() => SetActive<UIManager>(true, "UI 켜기", "UI 끄기");

    [MenuItem("Tools/UI 끄기", true)]
    private static bool UIsOffValidate() => IsPlaying() && AnyActive<UIManager>();
    [MenuItem("Tools/UI 끄기", false, 102)]
    private static void UIsOff() => SetActive<UIManager>(false, "UI 켜기", "UI 끄기");
    #endregion

    #region 광고
    [MenuItem("Tools/광고 켜기", true)]
    private static bool ADsOnValidate() => IsPlaying() && !AnyActive<ADManager>();
    [MenuItem("Tools/광고 켜기", false, 201)]
    private static void ADsOn() => SetActive<ADManager>(true, "광고 켜기", "광고 끄기");

    [MenuItem("Tools/광고 끄기", true)]
    private static bool ADsOff_Validate() => IsPlaying() && AnyActive<ADManager>();
    [MenuItem("Tools/광고 끄기", false, 202)]
    private static void ADsOff() => SetActive<ADManager>(false, "광고 켜기", "광고 끄기");
    #endregion
}
#endif
