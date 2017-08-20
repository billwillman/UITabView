using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestTabView : MonoBehaviour, ITableViewData {

	public UITableView TabView = null;
    private List<int> m_DataList = new List<int>();

	void Awake()
	{
        if (TabView != null) {
            TabView.InitItemCount(200);
            TabView.Data = this;
        }	
	}

	public void OnUpClick()
	{
        if (TabView != null)
			TabView.Scroll(500);
			//TabView.Scroll(100);
	}

	public void OnDownClick()
	{
        // if (TabView != null)
        //     TabView.AddItem(100, true);
        //  return;

        if (TabView != null) {
            //TabView.AddItem(20);
            TabView.Scroll(-500);
            TabView.Scroll(200);
        }
           // TabView.RemoveIndex(10);
           // TabView.Scroll(-29496);
           //TabView.Scroll(-100);
    }

	private static readonly int m_SplitCnt = 5;

   // private bool m_IsFirst = true;
	public void OnTabViewItemSize(int index, UIWidget item)
	{
     //   if (m_IsFirst)
       //     return;

		if (index != 0 && index%m_SplitCnt == 0)
		{
			item.height = 10;
		} else {
			item.height = 100;
		}
        
       // item.height = 100;
    }

	public void OnTabViewData (int index, UIWidget item, int subIndex)
	{

      //  m_IsFirst = false;
     //   if (subIndex == 0)
      //       Debug.LogFormat("FillItem: {0:D}", index);
   //     Debug.LogErrorFormat("FillItem: Index=>{0:D} SubIndex=>{1:D}", index, subIndex);

		GameObject obj = item.cachedTransform.FindChild("Items").gameObject;
		GameObject btn = item.cachedTransform.FindChild("Btn").gameObject;
		if (index != 0 && index%m_SplitCnt == 0)
		{
			btn.SetActive(true);
			obj.SetActive(false);

			return;
		}
			
		btn.SetActive(false);
		obj.SetActive(true);
		int idx = index * 4 + subIndex;
		if (subIndex == 0)
		{
			UISprite sp = item.cachedTransform.FindChild("Items/Item").GetComponent<UISprite>();
			sp.spriteName = string.Format("dish_0{0:D}", subIndex + 1);
			sp.enabled = true;
		//	UILabel lb = item.cachedTransform.FindChild("Items/Item/Lb").GetComponent<UILabel>();
		//	lb.text = idx.ToString();
		//	lb.enabled = true;
		} else
		{
			string name = string.Format("Items/Item ({0:D})", subIndex);
			UISprite sp = item.cachedTransform.FindChild(name).GetComponent<UISprite>();
			sp.spriteName = string.Format("dish_0{0:D}", subIndex + 1);
			sp.enabled = true;
		//	UILabel lb = item.cachedTransform.FindChild(string.Format("{0}/Lb", name)).GetComponent<UILabel>();
		//	lb.text = idx.ToString();
		//	lb.enabled = true;
		}
	}
}
