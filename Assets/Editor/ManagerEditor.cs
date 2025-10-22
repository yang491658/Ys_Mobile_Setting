#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

static public class ManagerEditor
{
    static private bool IsPlaying() => !EditorApplication.isPlaying;

    #region �ʱ�ȭ
    static private void ResetInspector(Component _comp)
    {
        Undo.RegisterCompleteObjectUndo(_comp, "Reset");
        Unsupported.SmartReset(_comp);
        EditorUtility.SetDirty(_comp);
        EditorSceneManager.MarkSceneDirty(_comp.gameObject.scene);
    }

    static private void ResetAll(Type _type)
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

    [MenuItem("Tools/�Ŵ��� �ʱ�ȭ", true)]
    static private bool ResetManagers_Validate() => IsPlaying();
    [MenuItem("Tools/�Ŵ��� �ʱ�ȭ", false, 1)]
    static private void ResetManagers()
    {
        var _types = new Type[]
        {
            typeof(GameManager), typeof(SoundManager), typeof(EntityManager),
            typeof(HandleManager), typeof(UIManager), typeof(ADManager), typeof(TestManager),
        };
        for (int i = 0; i < _types.Length; i++) ResetAll(_types[i]);
    }
    #endregion

    #region �ѱ�/����
    static private T FindSingle<T>() where T : Component
    {
        var c = UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        return c;
    }

    static private bool AnyActive<T>() where T : Component
    {
        var c = FindSingle<T>();
        if (!c) return false;
        var go = c.gameObject;
        return go && go.activeSelf;
    }

    static private void SetActive<T>(bool on, string onLabel, string offLabel) where T : Component
    {
        var c = FindSingle<T>();
        if (!c) return;
        var go = c.gameObject;
        if (!go) return;
        Undo.RegisterFullObjectHierarchyUndo(go, on ? onLabel : offLabel);
        go.SetActive(on);
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
    #endregion

    #region UI
    [MenuItem("Tools/UI �ѱ�", true)]
    static private bool UIOn_Validate() => IsPlaying() && !AnyActive<UIManager>();
    [MenuItem("Tools/UI �ѱ�", false, 101)]
    static private void UIOn() => SetActive<UIManager>(true, "UI �ѱ�", "UI ����");

    [MenuItem("Tools/UI ����", true)]
    static private bool UIOff_Validate() => IsPlaying() && AnyActive<UIManager>();
    [MenuItem("Tools/UI ����", false, 102)]
    static private void UIOff() => SetActive<UIManager>(false, "UI �ѱ�", "UI ����");
    #endregion

    #region ����
    [MenuItem("Tools/���� �ѱ�", true)]
    static private bool AdsOn_Validate() => IsPlaying() && !AnyActive<ADManager>();
    [MenuItem("Tools/���� �ѱ�", false, 201)]
    static private void AdsOn() => SetActive<ADManager>(true, "���� �ѱ�", "���� ����");

    [MenuItem("Tools/���� ����", true)]
    static private bool AdsOff_Validate() => IsPlaying() && AnyActive<ADManager>();
    [MenuItem("Tools/���� ����", false, 202)]
    static private void AdsOff() => SetActive<ADManager>(false, "���� �ѱ�", "���� ����");
    #endregion
}
#endif
