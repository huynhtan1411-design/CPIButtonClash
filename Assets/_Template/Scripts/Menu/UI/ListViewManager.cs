using UnityEngine;
using System.Collections.Generic;
using UISystems;

public class ListViewManager : MonoBehaviour
{
    [SerializeField] private GameObject listViewItemPrefab; 
    [SerializeField] private Transform content;

    private List<ListViewItem> items = new List<ListViewItem>();

    public void UpdateList(List<string> ids)
    {
        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }
        items.Clear();

        foreach (var id in ids)
        {
            int starCount = UIManager.instance.MenuGameCtr.GetSkillCardCount(id);
            AddItem(id, starCount);
        }
    }

    private void AddItem(string id, int countStar)
    {
        GameObject itemObj = Instantiate(listViewItemPrefab, content);
        ListViewItem item = itemObj.GetComponent<ListViewItem>();
        item.Init(id, countStar);
        items.Add(item);
    }


}