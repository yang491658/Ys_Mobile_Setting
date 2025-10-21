#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

static public class ManagerEditor
{
    static private void ResetLikeInspector(Component _comp)
    {
        if (_comp == null) return;
        Undo.RegisterCompleteObjectUndo(_comp, "Reset");
        Unsupported.SmartReset(_comp);
        EditorUtility.SetDirty(_comp);
        EditorSceneManager.MarkSceneDirty(_comp.gameObject.scene);
    }

    static private void ResetAllOfType(Type _type)
    {
        var _objs = UnityEngine.Object.FindObjectsByType(
            _type,
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        for (int i = 0; i < _objs.Length; i++)
        {
            var _comp = _objs[i] as Component;
            if (_comp != null) ResetLikeInspector(_comp);
        }
    }

    [MenuItem("Tools/매니저 초기화")]
    static private void Init_AllManagersInScene()
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
        };
        for (int i = 0; i < _types.Length; i++) ResetAllOfType(_types[i]);
    }
}
#endif
