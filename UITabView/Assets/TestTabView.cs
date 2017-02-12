using UnityEngine;
using System.Collections;

public class TestTabView : MonoBehaviour, ITableViewData {

	public UITableView TabView = null;

	void Awake()
	{
		if (TabView != null)
			TabView.Data = this;	
	}

	public void OnUpClick()
	{
		if (TabView != null)
			TabView.Scroll(500);
			//TabView.Scroll(100);
	}

	public void OnDownClick()
	{
		if (TabView != null)
			TabView.Scroll(-500);
			//TabView.Scroll(-100);
	}

	private static readonly int m_SplitCnt = 5;

	public void OnTabViewItemSize(int index, UIWidget item)
	{
		if (index != 0 && index%m_SplitCnt == 0)
		{
			item.height = 10;
		} else {
			item.height = 100;
		}
	}

	public void OnTabViewData (int index, UIWidget item, int subIndex)
	{
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
			UILabel lb = item.cachedTransform.FindChild("Items/Item/Lb").GetComponent<UILabel>();
			lb.text = idx.ToString();
			lb.enabled = true;
		} else
		{
			string name = string.Format("Items/Item ({0:D})", subIndex);
			UISprite sp = item.cachedTransform.FindChild(name).GetComponent<UISprite>();
			sp.spriteName = string.Format("dish_0{0:D}", subIndex + 1);
			sp.enabled = true;
			UILabel lb = item.cachedTransform.FindChild(string.Format("{0}/Lb", name)).GetComponent<UILabel>();
			lb.text = idx.ToString();
			lb.enabled = true;
		}
	}
}
