using UnityEngine;

public class UIDesignWindow: MonoBehaviour
{
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


	void Awake()
	{
		mRoot = this.GetComponentInParent<UIRoot> ();
		UpdateDesign();
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
	
	public bool IsRelative = true;
	public UIDesingType DesignType = UIDesingType.BaseOnWidth;
	public float DesignWidth = 1280;
	public float DesignHeight = 720;
	private Transform mCacheTransform = null;
	protected UIRoot mRoot = null;
	private UIDesignRunTime mDesignRunTime = UIDesignRunTime.None;
}

