using UnityEngine;
using System.Collections;

public class TestTabView : MonoBehaviour {

	public UITableView TabView = null;

	public void OnUpClick()
	{
		if (TabView != null)
			TabView.ScrollIndex(0);
	}

	public void OnDownClick()
	{
		if (TabView != null)
			TabView.ScrollIndex(100);
	}
}
