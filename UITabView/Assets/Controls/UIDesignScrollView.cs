using UnityEngine;
using System.Collections;


[RequireComponent(typeof(UIScrollView))]
public class UIDesignScrollView : UIDesignBase
{

	protected UIScrollView ScrollView
	{
		get
		{
			if (mScrollView == null)
				mScrollView = GetComponent<UIScrollView>();
			return mScrollView;
		}
	}

	protected UIPanel Panel
	{
		get
		{
			if (mPanel == null)
				mPanel = GetComponent<UIPanel>();
			return mPanel;
		}
	}

	void UpdateDesign(UIDesignView view)
	{
		float scaleX = 1;
		float scaleY = 1;
		float scaleZ = 1;
		if (ScrollView.movement == UIScrollView.Movement.Vertical)
		{
			scaleY = CacheTransform.lossyScale.y;
			scaleZ = scaleX = scaleY;
		}
		else if (ScrollView.movement == UIScrollView.Movement.Horizontal)
		{
			scaleX = CacheTransform.lossyScale.x;
			scaleZ = scaleY = scaleX;
		}
		else
		{
			Debug.LogError("Unsupported UIScrollView Movement");
			return;
		}

		CacheTransform.localScale = new Vector3(scaleX / CacheTransform.lossyScale.x, scaleY / CacheTransform.lossyScale.y, scaleZ / CacheTransform.lossyScale.z);
		Panel.baseClipRegion = new Vector4(Panel.baseClipRegion.x / CacheTransform.localScale.x, Panel.baseClipRegion.y / CacheTransform.localScale.y
		                                    , Panel.baseClipRegion.z / CacheTransform.localScale.x, Panel.baseClipRegion.w / CacheTransform.localScale.y);
		Panel.clipOffset = new Vector2(Panel.clipOffset.x / CacheTransform.localScale.x, Panel.clipOffset.y / CacheTransform.localScale.y);
		for (int i = 0; i < CacheTransform.childCount; i++)
		{
			CacheTransform.GetChild(i).localScale = new Vector3(1 / CacheTransform.localScale.x, 1 / CacheTransform.localScale.y, 1 / CacheTransform.localScale.z);
		}
	}

	/*
	Vector3 GetOrSetChildLocalScale(Transform trans, Vector3 localScale)
	{
		if (!mChildLocalScales.ContainsKey (trans.GetInstanceID ())) {
			mChildLocalScales.Add (trans.GetInstanceID (), localScale);
			return localScale;
		}

		return mChildLocalScales [trans.GetInstanceID ()];
	}*/

	public override void OnDesignUpdate (UIDesignView view)
	{
		base.OnDesignUpdate (view);
		UpdateDesign (view);
	}

	public Transform CacheTransform
	{
		get
		{
			if (mCacheTransform == null)
				mCacheTransform = this.transform;
			return mCacheTransform;
		}
	}


	private UIScrollView mScrollView = null;
	private UIPanel mPanel = null;
	private Transform mCacheTransform = null;
	//private System.Collections.Generic.Dictionary<int, Vector3> mChildLocalScales = new System.Collections.Generic.Dictionary<int, Vector3>();
	//private float mBaseScale = 1.0f;
}