//#define NGUI_3_6_8

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIWidget))]
public class UIDesignView : MonoBehaviour {

	public enum UIDesingType
	{
		None,
		BaseOnWidth,
		BaseOnHeight,
		InAspect,
		OutAaspect
	}

	public enum UIDesignRunTime
	{
		None,
		Width,
		Height
	}

	// Use this for initialization
	void Start () {
	//	AddDesginItemScript ();
		mRoot = this.GetComponentInParent<UIRoot> ();
		mChildBase = GetComponentsInChildren<UIDesignBase> ();
	}

	void AddDesginItemScript()
	{
		var scrollViews = GetComponentsInChildren<UIScrollView> ();
		if ((scrollViews != null) && (scrollViews.Length > 0)) {
			for (int i = 0; i < scrollViews.Length; ++i)
			{
				var item = scrollViews[i];
				if (item == null)
					continue;
				UIDesignScrollView scroll = item.GetComponent<UIDesignScrollView>();
				if (scroll == null)
				{
					item.gameObject.AddComponent<UIDesignScrollView>();
				}
			}
		}

		var panels = GetComponentsInChildren<UIPanel> ();
		if ((panels != null) && (panels.Length > 0)) {
			for (int i = 0; i < panels.Length; ++i)
			{
				var item = panels[i];
				if ((item == null) || (item.clipping == UIDrawCall.Clipping.None))
					continue;
				if (item.GetComponent<UIScrollView>() != null)
					continue;
				UIDesignPanel design = item.GetComponent<UIDesignPanel>();
				if (design == null)
				{
					item.gameObject.AddComponent<UIDesignPanel>();
				}
			}
		}
	}

	void UpdateChildPanels()
	{
		if ((mChildBase == null) || (mChildBase.Length <= 0))
			return;
		for (int i = 0; i < mChildBase.Length; ++i) {
			var item = mChildBase[i];
			if (item != null)
				item.OnDesignUpdate(this);
		}
	}

	Vector2 GetScreenSize(bool isRelative)
	{
		 Vector2 ret = NGUITools.screenSize;
		//Vector2 ret = new Vector2 (UICamera.mainCamera.pixelWidth, UICamera.mainCamera.pixelHeight);
		if (mRoot == null)
			return ret;

		//if ((DesignType == UIDesingType.BaseOnWidth) || (DesignType == UIDesingType.BaseOnHeight))
		//	return ret;

#if NGUI_3_6_8
		// 3.6.8
		if (isRelative) {
			if (mRoot.scalingStyle == UIRoot.Scaling.FixedSize) {
				ret.x = DesignWidth / DesignHeight * ret.y;
			} else
				if (mRoot.scalingStyle == UIRoot.Scaling.FixedSizeOnMobiles)
			{
				// 判断是否是Mobile
				if (Application.isMobilePlatform)
				{
					ret.x = DesignWidth / DesignHeight * ret.y;
				}
			}
		}
#else
		// 3.6.8后面的版本
		if (isRelative) {
			if (mRoot.activeScaling == UIRoot.Scaling.Constrained) {
				if (mRoot.constraint == UIRoot.Constraint.FitHeight) {
					ret.x = DesignWidth / DesignHeight * ret.y;
				} else 
		if (mRoot.constraint == UIRoot.Constraint.FitWidth) {
					ret.y = DesignHeight / DesignWidth * ret.x;
				} else
		if (mRoot.constraint == UIRoot.Constraint.Fit) {
					float scale1 = ret.x / DesignWidth;
					float scale2 = ret.y / DesignHeight;
					if (scale1 < scale2) {
						ret.y = DesignHeight / DesignWidth * ret.x;
					} else {
						ret.x = DesignWidth / DesignHeight * ret.y;
					}
				} else
		if (mRoot.constraint == UIRoot.Constraint.Fill) {
					float scale1 = ret.x / DesignWidth;
					float scale2 = ret.y / DesignHeight;
					if (scale1 < scale2) {
						ret.x = DesignWidth / DesignHeight * ret.y;
					} else {
						ret.y = DesignHeight / DesignWidth * ret.x;
					}
				}
			}
		}
#endif

		return ret;
	}

	void UpdateDesign()
	{
		if (!IsVaildDesignWH)
			return;
		//return;
		Vector2 size = NGUITools.screenSize;
		Vector2 screenSize = GetScreenSize (IsRelative);
	
		float designW = DesignWidth;
		float designH = DesignHeight;
		/*
		if (mRoot != null) {
			var trans = mRoot.transform;
			designW *= trans.localScale.x;
			designH *= trans.localScale.y;
		}*/
		float aspect = 1.0f;
		switch (DesignType) {
		case UIDesingType.BaseOnWidth:
		{
			if (IsRelative)
				aspect = size.x/screenSize.x;
			else
				aspect = screenSize.x/designW;
			mDesignRunTime = UIDesignRunTime.Width;
			break;
		}
		case UIDesingType.BaseOnHeight:
		{
			if (IsRelative)
				aspect = size.y/screenSize.y;
			else
				aspect = screenSize.y/designH;
			mDesignRunTime = UIDesignRunTime.Height;
			break;
		}
		case UIDesingType.InAspect:
		{
			float xScale;
			if (IsRelative)
				xScale = size.x/screenSize.x;
			else
				xScale = screenSize.x/designW;
			float yScale;
			if (IsRelative)
				yScale = size.y/screenSize.y;
			else
				yScale = screenSize.y/designH;
			if (xScale < yScale)
			{
				aspect = xScale;
				mDesignRunTime = UIDesignRunTime.Width;
			}
			else
			{
				aspect = yScale;
				mDesignRunTime = UIDesignRunTime.Height;
			}
			break;
		}
		case UIDesingType.OutAaspect:
		{
			float xScale;
			if (IsRelative)
				xScale = size.x/screenSize.x;
			else
				xScale = screenSize.x/designW;
			float yScale;
			if (IsRelative)
				yScale = size.y/screenSize.y;
			else
				yScale = screenSize.y/designH;
			if (xScale > yScale)
			{
				aspect = xScale;
				mDesignRunTime = UIDesignRunTime.Width;
			}
			else
			{
				aspect = yScale;
				mDesignRunTime = UIDesignRunTime.Height;
			}
			break;
		}
		}

		Vector3 nowScale = CacheTransform.localScale;
		// Vector3 chgScale = new Vector3 (aspect, aspect, nowScale.z);
		Vector3 chgScale = new Vector3 (aspect, aspect, aspect);
		if (nowScale != chgScale) {
			CacheTransform.localScale = chgScale;
			//UpdateChildPanels();
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (((!IsRunOnce) || mIsFirstRun) && (DesignType != UIDesingType.None)) {
			UpdateDesign ();
			mIsFirstRun = false;
		}
	}

	protected bool IsVaildDesignWH
	{
		get {
			return (DesignWidth > 0) && (DesignHeight > 0);
		}
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

	public UIDesignRunTime DesignRunTime
	{
		get {
			return mDesignRunTime;
		}
	}

	//[HideInInspector]
	public bool IsRunOnce = true;
	public bool IsRelative = true;
	public UIDesingType DesignType = UIDesingType.None;
	public float DesignWidth = 0;
	public float DesignHeight = 0;
	private Transform mCacheTransform = null;
	private bool mIsFirstRun = true;
	private UIDesignBase[] mChildBase = null;
	protected UIRoot mRoot = null;
	private UIDesignRunTime mDesignRunTime = UIDesignRunTime.None;
}
