using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIPanel))]
public class UIDesignPanel: UIDesignBase
{
	public override void OnDesignUpdate (UIDesignView view)
	{
		base.OnDesignUpdate (view);

		if (view.DesignRunTime == UIDesignView.UIDesignRunTime.None)
			return;

		float scaleX = 1;
		float scaleY = 1;
		float scaleZ = 1;

		if(view.DesignRunTime == UIDesignView.UIDesignRunTime.Height)  
		{  
			scaleY = CacheTransform.lossyScale.y;
			scaleZ = scaleX = scaleY;
		}  
		else if(view.DesignRunTime == UIDesignView.UIDesignRunTime.Width)  
		{  
			scaleX = CacheTransform.lossyScale.x;
			scaleZ = scaleY = scaleX;
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

	protected UIPanel Panel
	{
		get {
			if (mPanel == null)
				mPanel = GetComponent<UIPanel>();
			return mPanel;
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

	private UIPanel mPanel = null;
	private Transform mCacheTransform = null;
}
