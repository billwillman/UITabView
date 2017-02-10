using UnityEngine;

public class UITabViewItem: MonoBehaviour
{
	// 索引
	[HideInInspector]
	public int Index {
		get;
		set;
	}

	[HideInInspector]
	public UITableView TabView
	{
		get;
		set;
	}

	public UIWidget Widget
	{
		get
		{
			if (mWidget == null)
				mWidget = GetComponent<UIWidget>();
			return mWidget;
		}
	}

	/*
	public bool GetScrollViewPosition(out Vector2 pos)
	{
		pos = Vector2.zero;
		if (TabView == null)
			return false;
		return TabView.GetScrollViewPosition(this, out pos);
	}*/

	private UIWidget mWidget = null;
}

