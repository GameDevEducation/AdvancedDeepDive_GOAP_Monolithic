using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif // UNITY_EDITOR

[System.Serializable]
public class ActivateCategory : UnityEvent<string> {}

public class SettingsUI_Category : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private TMPro.TextMeshProUGUI CategoryTitle;
    [SerializeField] private string CategoryName;
    [SerializeField] private Animator AnimController;
#pragma warning restore 0649

    public ActivateCategory OnActivateCategory;

    public string Name
    {
        get
        {
            return CategoryName;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCategoryIsShown(bool isShown)
    {
        AnimController.SetBool("Category Shown", isShown);
    }

    public void CategorySelected()
    {
        OnActivateCategory?.Invoke(CategoryName);
    }

#if UNITY_EDITOR
    public void BindCategory(string newName, UnityAction<string> listener)
    {
        CategoryTitle.text = CategoryName = newName;
        UnityEventTools.AddPersistentListener(OnActivateCategory, listener);
    }
#endif // UNITY_EDITOR
}
