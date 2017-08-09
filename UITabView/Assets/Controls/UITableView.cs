using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITableViewData
{
	void OnTabViewData (int index, UIWidget item, int subIndex);

	void OnTabViewItemSize (int index, UIWidget item);
}

// TabViewScrollBar接口, 需要让UIScrollView调用
public interface ITabViewScrollBar
{
	void OnScrollViewScrollBarUpdate ();
}

[RequireComponent (typeof(UIScrollView))]
public class UITableView: MonoBehaviour, ITabViewScrollBar
{
	void Start ()
	{
		mScrollView = GetComponent<UIScrollView> ();
		mPanel = GetComponent<UIPanel> ();
		// 设置TabViewScrollBar接口

		if (mScrollView != null) {
			mScrollView.TabViewScrollBar = this;
		}
		
		if (ItemObject != null) {
			ItemObject.gameObject.SetActive (false);
		}

		//	ReCalcCreateItems ();
	}

	public void OnScrollViewScrollBarUpdate ()
	{
		if (!mIsFirstRun)
			Check2NoVisible ();
		UpdateScrollBar ();
	}

	public UIScrollView ScrollView {
		get {
			return mScrollView;
		}
	}

	public bool IsHorizontal {
		get {
			return mScrollView.movement == UIScrollView.Movement.Horizontal;
		}
	}

	public bool IsVertical {
		get {
			return mScrollView.movement == UIScrollView.Movement.Vertical; 
		}
	}

	// 可见的最大数量
	public int ItemViewMaxCount {
		get {
			return mViewMaxCount;
		}
	}

	public ITableViewData Data {
		get;
		set;
	}

	public Action<int, UIWidget, int> OnDataEvent {
		get;
		set;
	}

	public int ItemTopIndex {
		get {
			return mItemTopIndex;
		}
	}

	public int ItemBottomIndex {
		get {
			return mItemBottomIndex;
		}
	}


	#if UNITY_EDITOR
	[System.NonSerialized]
	public int 当前Top索引 = 0;
	[System.NonSerialized]
	public int 当前Bottom索引 = 0;
	[System.NonSerialized]
	public int 可见Item最大数量 = 0;
	#endif

	/*
	// 往下或者往右拖拽
	public bool IsDragingRightOrBottom
	{
		get {
			Vector2 offset = mScrollView.currentMomentum;
			if (IsHorizontal)
			{
				// 水平
				if (mScrollView.isDragging)
					return (offset.x - mLastOffset.x < -float.Epsilon);
			} else
				if (IsVertical)
			{
				// 垂直
				if (mScrollView.isDragging)
					return (offset.y - mLastOffset.y < -float.Epsilon);
			}

			return false;
		}
	}

	public bool IsDragingLeftOrTop
	{
		get {
			Vector2 offset = mScrollView.currentMomentum;
			if (IsHorizontal)
			{
				// 水平
				if (mScrollView.isDragging)
					return (offset.x - mLastOffset.x > float.Epsilon);
			} else
				if (IsVertical)
			{
				// 垂直
				if (mScrollView.isDragging)
					return (offset.y - mLastOffset.y > float.Epsilon);
			}
			
			return false;
		}
	}*/

	// 是否需要显示滚动条
	void CheckScrollBarVisible ()
	{
		if ((ItemObject == null) || (mPanel == null) || (ScrollBar == null))
			return;

		if (IsNeetShowScrollBar) {
			if (ItemCount < ItemViewMaxCount) {
				ScrollBar.alpha = 0;
			} else if (ItemViewMaxCount < ItemCount) {
				ScrollBar.alpha = 1;
			} else {
				Vector2 viewSize = mPanel.GetViewSize ();
				// 相等情况下也可能会需要滚动条
				if (IsHorizontal) {
					// 水平
					if (viewSize.x < ItemCount * ItemObject.width)
						ScrollBar.alpha = 1;
					else
						ScrollBar.alpha = 0;
				} else if (IsVertical) {
					// 垂直
					if (viewSize.y < ItemCount * ItemObject.height)
						ScrollBar.alpha = 1;
					else
						ScrollBar.alpha = 0;
				}
			}
		}


	}

	void ClearItemList ()
	{
		if (mItemList == null)
			return;
		var node = mItemList.First;
		while (node != null) {
			if (node.Value != null)
				NGUITools.DestroyImmediate (node.Value.cachedGameObject);
			node = node.Next;
		}
		mItemList.Clear ();
	}

	IEnumerator NodeListDoGetDataAsync ()
	{
		if (mScrollView == null)
			yield break;

		// 防止滚动了
		mScrollView.enabled = false;

		var node = mItemList.First;
		int index = mItemTopIndex;
		while (node != null) {
			yield return null;

			if (node.Value == null) {
				node = node.Next;
				++index;
				continue;
			}

			// 直到设置完值再返回
			int subCnt = SubItemCount;
			if (subCnt <= 0)
				subCnt = 1;
			for (int subIndex = 0; subIndex < subCnt; ++subIndex) {
				DoGetData (node.Value, index, subIndex);
				yield return null;
			}
			
			++index;
			node = node.Next;
		}

		mScrollView.enabled = true;
	}

	void NodeListDoGetData ()
	{
		if (IsAsynInitData) {
			if (mScrollView != null) {
				StopCreateCoroutne ();
				m_CreateCoroutne = StartCoroutine (NodeListDoGetDataAsync ());
			}
			return;
		}
		// 循环获得数据
		var node = mItemList.First;
		int index = mItemTopIndex;
		while (node != null) {
			if (node.Value == null) {
				node = node.Next;
				++index;
				continue;
			}

			RefreshSubItem (node.Value, index);

			++index;
			node = node.Next;
		}
	}

	void refreshItems ()
	{
		if (mIsFirstRun) {
			
			Vector3 offset = mScrollView.transform.localPosition;
				
			if (IsHorizontal) {
				offset.z = 0;
				offset.y = 0;
			} else if (IsVertical) {
				offset.z = 0;
				offset.x = 0;
			}
				
			mScrollView.MoveRelative (-offset);
			//	mScrollView.MoveAbsolute(Vector3.zero);
			ClearItemList ();

			ReCalcCreateItems ();
			
			mOrgOffset = mScrollView.transform.localPosition;
            mLastOffset = mOrgOffset;

        }

		Check2NoVisible ();

		if (mIsFirstRun) {
			if (mItemList != null) {
				NodeListDoGetData ();
			}

			CheckScrollBarVisible ();

			mIsFirstRun = false;

			// 调一次
			UpdateScrollBar ();
		}

		/*

		if (IsDragingRightOrBottom) {
			// 往右，往下
			ExpandMoveFirst (1);
			#if UNITY_EDITOR
			DEBUG_SubClipOffset = (Vector2)mScrollView.currentMomentum - mLastOffset;
			#endif
		} else
		if (IsDragingLeftOrTop) {
			// 往左，往上
			ExpandMoveLast (1);
			#if UNITY_EDITOR
			DEBUG_SubClipOffset = (Vector2)mScrollView.currentMomentum - mLastOffset;
			#endif
		} else {
			#if UNITY_EDITOR
			DEBUG_SubClipOffset = (Vector2)mScrollView.currentMomentum - mLastOffset;
			#endif
			// 判断是否存在两个不可显示情况
			Check2NoVisible();
		}
		mLastOffset = mScrollView.currentMomentum;
		*/
	}

	private void UpdateEditor ()
	{
#if UNITY_EDITOR
		当前Top索引 = mItemTopIndex;
		当前Bottom索引 = mItemBottomIndex;
		可见Item最大数量 = mViewMaxCount;
#endif
	}

	protected float AllContentSize(bool isCheckUseItemSize = true) {
			if (ItemObject == null)
				return 0;
		if (isCheckUseItemSize && IsUseTabItemSize && Data != null)
			{
				float ret = 0;
				for (int i = 0; i < ItemCount; ++i)
				{
					int w = ItemObject.width;
					int h = ItemObject.height;
					Data.OnTabViewItemSize(i, ItemObject);
					if (IsHorizontal)
						ret += ItemObject.width;
					else
						ret += ItemObject.height;
					ItemObject.width = w;
					ItemObject.height = h;
				}

				return ret;
			} else
			{
				if (IsHorizontal)
					return ItemCount * ItemObject.width;
				else if (IsVertical)
					return ItemCount * ItemObject.height;

			}
			return 0;
	}

	private void UpdateScrollBar ()
	{
		// 更新ScrollBar
		if ((ScrollBar == null) || mIsFirstRun)
			return;

		if ((mItemList == null) || (mItemList.Count <= 0) || (mPanel == null) || (ItemObject == null)) {
			ScrollBar.value = 0;
			return;
		}

		var firstNode = mItemList.First;
		if ((firstNode == null) || (firstNode.Value == null)) {
			ScrollBar.value = 0;
			return;
		}

		float all = AllContentSize(false);
		if (Mathf.Abs (all) <= float.Epsilon)
			return;

		Vector2 offset = firstNode.Value.pivotOffset;
		Vector2 viewSize = mPanel.GetViewSize ();
		Vector4 clip = mPanel.finalClipRegion;
		float availdOffset = 0;
		if (IsHorizontal) {
			all -= viewSize.x;
			availdOffset = -(mItemTopIndex * ItemObject.width + (1 - offset.x) * ItemObject.width - firstNode.Value.cachedTransform.localPosition.x - clip.x - viewSize.x / 2);
		} else if (IsVertical) {
			all -= viewSize.y;
			availdOffset = mItemTopIndex * ItemObject.height + (1 - offset.y) * ItemObject.height + firstNode.Value.cachedTransform.localPosition.y - clip.y - viewSize.y / 2;
		}



		float scrollValue = availdOffset / all;

		if (scrollValue < 0) {
			scrollValue = 0;
		} else {
			if (scrollValue > 1.0f) {
				scrollValue = 1.0f;
			}
		}
		// ScrollBar.value = scrollValue;
		//	Debug.Log (string.Format ("offset={0}", ScrollBar.value.ToString()));

	
		// 不让其出圈
		UIWidget thumb = null;
		if (ScrollBar.thumb != null) {
			thumb = ScrollBar.thumb.GetComponent<UIWidget> ();
		}

		UIWidget back = ScrollBar.backgroundWidget;
		if ((thumb != null) && (back != null)) {
			bool isHorBar = (ScrollBar.fillDirection == UIProgressBar.FillDirection.LeftToRight) || (ScrollBar.fillDirection == UIProgressBar.FillDirection.RightToLeft);
			bool isVertBar = (ScrollBar.fillDirection == UIProgressBar.FillDirection.TopToBottom) || (ScrollBar.fillDirection == UIProgressBar.FillDirection.BottomToTop);
			if (isHorBar && (back.width > float.Epsilon)) {
				float start = (1 - thumb.pivotOffset.x) * thumb.width;
				float end = (back.width - thumb.pivotOffset.x * thumb.width);
				float dist = end - start;
				float v = start + scrollValue * dist;
				scrollValue = v / back.width;
			} else if (isVertBar && (back.height > float.Epsilon)) {
				float start = (1 - thumb.pivotOffset.y) * thumb.height;
				float end = (back.height - thumb.pivotOffset.y * thumb.height);
				float dist = end - start;
				float v = start + scrollValue * dist;
				scrollValue = v / back.height;
			}
		}


		ScrollBar.value = scrollValue;

	}

	private void RefreshSubItem (UIWidget widget, int index)
	{
		// 直到设置完值再返回
		int subCnt = SubItemCount;
		if (subCnt <= 0)
			subCnt = 1;
		for (int subIndex = 0; subIndex < subCnt; ++subIndex) {
			DoGetData (widget, index, subIndex);
		}
	}

	private IEnumerator RefreshDataAtViewRectAsync ()
	{
		if (mItemList == null || mItemList.Count <= 0 || mScrollView == null) {
			mItemTopIndex = -1;
			mItemBottomIndex = -1;
			yield break;
		}

		if (mItemBottomIndex >= ItemCount) {
			mItemBottomIndex = ItemCount - 1;
			if (mItemBottomIndex < 0) {
				mItemBottomIndex = 0;
				mItemTopIndex = 0;
			} else {
				mItemTopIndex = mItemBottomIndex - mViewMaxCount;
				if (mItemTopIndex < 0)
					mItemTopIndex = 0;
			}
		}

		mScrollView.enabled = false;

		var node = mItemList.First;
		int idx = mItemTopIndex;
		while (node != null && node.Value != null) {
			yield return null;

			// 直到设置完值再返回
			int subCnt = SubItemCount;
			if (subCnt <= 0)
				subCnt = 1;
			for (int subIndex = 0; subIndex < subCnt; ++subIndex) {
				DoGetData (node.Value, idx, subIndex);
				yield return null;
			}

			++idx;
			node = node.Next;
		}

		mScrollView.enabled = true;

		if (mViewMaxCount >= ItemCount && mItemList.Count - 2 == mViewMaxCount) {
			mScrollView.ResetPosition ();
		} else {
			mScrollView.DisableSpring ();
			mScrollView.InvalidateBounds ();
			if (mMustResetInWith)
				mScrollView.RestrictWithinBounds (true);

			CheckScrollBarVisible ();
			UpdateScrollBar ();
		}
	}

	public void RefreshDataAtViewRect ()
	{
		/*
		if (IsAsynInitData) {
			if (mScrollView != null && mScrollView.enabled)
			{
				StopCoroutine(m_CreateCoroutne);
				m_CreateCoroutne = StartCoroutine(RefreshDataAtViewRectAsync());
			}
		} else*/
		{
            if (mItemList == null || mItemList.Count <= 0) {
				mItemTopIndex = -1;
				mItemBottomIndex = -1;
				return;
			}
		
			if (mItemBottomIndex >= ItemCount) {
				mItemBottomIndex = ItemCount - 1;
				if (mItemBottomIndex < 0) {
					mItemBottomIndex = 0;
					mItemTopIndex = 0;
				} else {
					mItemTopIndex = mItemBottomIndex - mViewMaxCount;
					if (mItemTopIndex < 0)
						mItemTopIndex = 0;
				}
			}

            // 防止正在初始化立马被调用
            StopCreateCoroutne();

            var node = mItemList.First;
			int idx = mItemTopIndex;
			while (node != null && node.Value != null) {
				// 直到设置完值再返回
				RefreshSubItem (node.Value, idx);
				++idx;
				node = node.Next;
			}

			if (mViewMaxCount >= ItemCount && mItemList.Count - 2 == mViewMaxCount) {
				// Scroll Top 0
				mScrollView.ResetPosition ();
				//ScrollIndex(0);
				//DoScrollIndex();
				/*
			Vector3 offset = mOrgOffset;
			if (IsHorizontal)
				offset.y = 0;
			else if (IsVertical)
				offset.x = 0;
			mScrollView.MoveAbsolute(-offset);*/

				//mScrollView.transform.localPosition = mOrgOffset;
			} else {
				mScrollView.DisableSpring ();
				mScrollView.InvalidateBounds ();
				if (mMustResetInWith)
					mScrollView.RestrictWithinBounds (true);

				CheckScrollBarVisible ();
				UpdateScrollBar ();
			}
		}
	}

	/*
	 //  has problem
	// 并不是真的Delete mItemCount
	public void RemoveCount(int decCnt)
	{
		if (decCnt <= 0)
			return;

		if (mItemList == null || mItemList.Count <= 0 || mViewMaxCount <= 0)
			return;

		int itemCnt = ItemCount - decCnt;
		if (itemCnt < 0)
		{
			itemCnt = 0;
			decCnt = ItemCount;
		}

		ItemCount = itemCnt;

		bool isMoveDownOffset = (mScrollView != null) && (ItemObject != null) &&
								(mItemBottomIndex >= ItemCount) &&(mViewMaxCount < ItemCount);

		if (isMoveDownOffset)
		{
			mScrollView.DisableSpring();
			// Scroll Down
			Vector3 offset;
			if (IsHorizontal)
				offset = new Vector3(-ItemObject.width * decCnt, 0, 0);
			else
				offset = new Vector3(0, -ItemObject.height * decCnt, 0);
			mScrollView.MoveRelative(offset);
		} else
		{
			RefreshDataAtViewRect();
		}
	}*/
	
	// 删除当前数据
	public void RemoveIndex (int index)
	{
		if (index < 0 || index >= ItemCount || mItemList == null || mItemList.Count <= 0 || mViewMaxCount <= 0)
			return;
		
		int itemCnt = ItemCount - 1;
		if (itemCnt < 0)
			return;

		ItemCount = itemCnt;

		bool isMoveDownOffset = (mScrollView != null) && (ItemObject != null) &&
		                        (mItemBottomIndex >= ItemCount) && (mViewMaxCount < ItemCount);

		bool isMoveUpOffset = (mScrollView != null) && (ItemObject != null) &&
		                      (index < mItemTopIndex) && (mViewMaxCount < ItemCount) &&
		                      (mItemBottomIndex < ItemCount);
		if (isMoveDownOffset) {
			// Scroll Down
			if (mItemTopIndex == 0 && mItemBottomIndex == ItemCount) {
				mScrollView.ResetPosition ();
			} else {
				mScrollView.DisableSpring ();
				Vector3 offset;
				if (IsHorizontal)
					offset = new Vector3 (ItemObject.width, 0, 0);
				else
					offset = new Vector3 (0, -ItemObject.height, 0);
				mScrollView.MoveRelative (offset);
				RefreshDataBottomToTopIdx ();
			}
		} else if (isMoveUpOffset) {
			// scroll up
			mScrollView.DisableSpring ();
			Vector3 offset;
			if (IsHorizontal)
				offset = new Vector3 (-ItemObject.width, 0, 0);
			else
				offset = new Vector3 (0, ItemObject.height, 0);
			mScrollView.MoveRelative (offset);
		} else
			RefreshDataAtViewRect ();
	}

	private void RefreshDataBottomToTopIdx ()
	{
		if (mItemList == null || mItemList.Count <= 0)
			return;
		var node = mItemList.Last;
		while (node != null && node.Value != null) {
			UITabViewItem item = node.Value.GetComponent<UITabViewItem> ();
			if (item != null) {
				RefreshSubItem (node.Value, item.Index);
			}
			node = node.Previous;
		}
	}

	private void DoScrollIndex ()
	{
		if (mScrollIndex < 0 || mScrollView == null)
			return;

		if (!mScrollView.enabled)
			return;

		// 无法滚动
		if ((mViewMaxCount > ItemCount) || (mViewMaxCount <= 0) || (ItemCount <= 0) || (mItemList == null) || (mItemList.Count <= 0)) {
			mScrollIndex = -1;
			return;
		}
		
		var Node = mItemList.First;
		if ((Node == null) || (Node.Value == null)) {
			mScrollIndex = -1;
			return;
		}

		//StopCreateCoroutne();

        if (IsUseTabItemSize && Data != null) {
#if DEBUG
            Debug.LogErrorFormat("UITabView TabSize is not same: don't support ScrollIndex");
#endif
        }
		
		// 1.在可滚动范围内
		int itemTopIndex;
		if (mScrollIndex + mViewMaxCount <= ItemCount) {
			itemTopIndex = mScrollIndex;
		} else {
			itemTopIndex = ItemCount - mViewMaxCount - 1 + 2;
		}
		
		if (ItemObject != null) {
			mScrollView.DisableSpring ();
			
			float allSize = this.AllContentSize();
			
			if (IsHorizontal) {
				allSize -= mPanel.GetViewSize ().x;

                float y;
                /*
                if (IsUseTabItemSize && Data != null) {
                    y = 0;
                    for (int i = 0; i < itemTopIndex; ++i) {
                        int w = ItemObject.width;
                        int h = ItemObject.height;
                        Data.OnTabViewItemSize(i, ItemObject);
                        y += ItemObject.width;
                        ItemObject.width = w;
                        ItemObject.height = h;
                    }
                } else*/
                    y = itemTopIndex * ItemObject.width;

				if (y > allSize)
					y = allSize;
				Vector3 offset = new Vector3 (-y, 0, 0);
				Vector3 pos = mScrollView.transform.localPosition;
				offset.x -= pos.x - mOrgOffset.x;
				mScrollView.MoveRelative (offset);
			} else if (IsVertical) {
				allSize -= mPanel.GetViewSize ().y;

                float y;
                /*
                if (IsUseTabItemSize && Data != null) {
                    y = 0;
                    for (int i = 0; i < itemTopIndex; ++i) {
                        int w = ItemObject.width;
                        int h = ItemObject.height;
                        Data.OnTabViewItemSize(i, ItemObject);
                        y += ItemObject.height;
                        ItemObject.width = w;
                        ItemObject.height = h;
                    }
                } else*/
                    y = itemTopIndex * ItemObject.height;

				if (y > allSize)
					y = allSize;
				Vector3 offset = new Vector3 (0, y, 0);
				Vector3 pos = mScrollView.transform.localPosition;
				offset.y -= pos.y - mOrgOffset.y;
				mScrollView.MoveRelative (offset);
			}
		}

		mScrollIndex = -1;
		
		CheckScrollBarVisible ();
		UpdateScrollBar ();
	}

	private void DoScroll ()
	{
		if (mScrollView == null || !mIsScroll)
			return;

		if (!mScrollView.enabled)
			return;

		if (mScrollView.isDragging) {
			mScroll = 0;
			mIsScroll = false;
			return;
		}
		
		if (Mathf.Abs (mScroll) <= float.Epsilon) {
			mScroll = 0;
			mIsScroll = false;
			return;
		}
		
		float deltaTime = Time.unscaledDeltaTime;
		float scrollValue = Mathf.SmoothDamp (mScroll, 0, ref mScrollSpeed, deltaTime);
		
		Vector3 offset;
		if (IsHorizontal) {
			offset = new Vector3 (-scrollValue, 0, 0);
		} else if (IsVertical) {
			offset = new Vector3 (0, -scrollValue, 0);
		} else {
			mScroll = 0;
			mIsScroll = false;
			return;
		}

		//StopCreateCoroutne();

		mScrollView.MoveRelative (offset);
		float lastScroll = mScroll;
		mScroll -= scrollValue;
		if ((lastScroll > 0 && mScroll < 0) || (lastScroll < 0 || mScroll > 0)) {
			mScroll = 0;
			mIsScroll = false;
		}
	}
	
	// 已经滚动到头
	public bool IsScrollTop {
		get {
			if (mScrollView == null || ItemObject == null)
				return false;
			
			if (mViewMaxCount >= ItemCount)
				return true;
			
			Vector3 offset = mScrollView.transform.localPosition;
			if (IsHorizontal) {
				float delta = offset.x - mOrgOffset.x;
				return delta >= -ItemObject.width / 2;
			} else if (IsVertical) {
				float delta = offset.y - mOrgOffset.y;
				return delta <= ItemObject.height / 2;
			} else
				return false;
		}
	}

	public bool IsScrollBottom {
		get {
			if (mScrollView == null || mPanel == null)
				return false;
			if (mViewMaxCount >= ItemCount)
				return true;
			
			float allSize = this.AllContentSize();
			Vector3 offset = mScrollView.transform.localPosition;
			if (IsHorizontal) {
				allSize -= mPanel.GetViewSize ().x;
				float maxSize = -mOrgOffset.x + allSize;
				float delta = offset.x + maxSize;
				return delta <= ItemObject.width / 2;
			} else if (IsVertical) {
				allSize -= mPanel.GetViewSize ().y;
				float maxSize = mOrgOffset.y + allSize;
				float delta = offset.y - maxSize;
				return delta >= -ItemObject.height / 2;
			} else
				return false;
		}
	}

	// Relative about ScrollView
	/*
	public bool GetScrollViewPosition(UITabViewItem target, out Vector2 r)
	{
		r = Vector2.zero;
		if (target == null || target.Widget == null || mIsFirstRun)
			return false;

		if (mScrollView == null)
			return false;

		Transform scrollViewTrans = mScrollView.transform;
		Vector3 offset = scrollViewTrans.localPosition;
		Vector3 targetPos = target.Widget.cachedTransform.localPosition;
		if (IsHorizontal)
		{

		} else if (IsVertical)
		{
			float off = offset.y - mOrgOffset.y;
			r.x = targetPos.x;
			r.y = targetPos.y + off;
		} else
			return false;

		return true;
	}*/
	
    public void Scroll (float delta)
	{
		if (mScrollView == null || mPanel == null || ItemCount <= 0 || mItemList == null || mItemList.Count <= 0 || mViewMaxCount >= ItemCount)
			return;
		
		Vector3 offset = mScrollView.transform.localPosition;
		if (delta > 0) {
			if (IsHorizontal) {
				if (offset.x + delta > mOrgOffset.x)
					delta = mOrgOffset.x - offset.x;
				
				delta = -delta;
			} else if (IsVertical) {
				if (offset.y - delta < mOrgOffset.y)
					delta = offset.y - mOrgOffset.y;
			} else
				return;
		} else if (delta < 0) {
			float allSize = this.AllContentSize();
			if (IsHorizontal) {
				allSize -= mPanel.GetViewSize ().x;
				float maxSize = -mOrgOffset.x + allSize;
				if (-offset.x - delta > maxSize)
					delta = -(maxSize + offset.x);
				delta = -delta;
			} else if (IsVertical) {
				allSize -= mPanel.GetViewSize ().y;
				float maxSize = mOrgOffset.y + allSize;
				if (offset.y - delta > maxSize)
					delta = offset.y - maxSize;
			} else
				return;
		} else
			return;
		
		if (Mathf.Abs (delta) <= float.Epsilon)
			return;
		
		mScrollView.DisableSpring ();
		mScroll = delta;
		mIsScroll = true;
		mScrollIndex = -1;
		mScrollSpeed = 0;
	}

	// 滚动到位置
	public void ScrollIndex (int Index, bool isAnim = false)
	{
		if (ItemCount <= 0 || ItemObject == null || mScrollView == null)
			return;
		
		if (Index < 0)
			Index = 0;
		else if (Index >= ItemCount)
			Index = ItemCount - 1;
		
		if (isAnim) {
            StopCreateCoroutne();
            mScrollView.enabled = true;

            if (IsHorizontal) {
                float absoluteDistance;
                if (IsUseTabItemSize && Data != null) {
                    absoluteDistance = 0;
                    for (int i = 0; i < Index; ++i) {
                        int w = ItemObject.width;
                        int h = ItemObject.height;

                        Data.OnTabViewItemSize(i, ItemObject);
                        absoluteDistance += ItemObject.width;

                        ItemObject.width = w;
                        ItemObject.height = h;
                    }
                } else
                    absoluteDistance = Index * ItemObject.width;
				float currDistance = -(mScrollView.transform.localPosition.x - mOrgOffset.x);
				float relativeDistance = currDistance - absoluteDistance;
				Scroll (relativeDistance);
			} else if (IsVertical) {
                float absoluteDistance;
                if (IsUseTabItemSize && Data != null) {
                    absoluteDistance = 0;
                    for (int i = 0; i < Index; ++i) {
                        int w = ItemObject.width;
                        int h = ItemObject.height;

                        Data.OnTabViewItemSize(i, ItemObject);
                        absoluteDistance += ItemObject.height;

                        ItemObject.width = w;
                        ItemObject.height = h;
                    }
                }
                else
                    absoluteDistance = Index * ItemObject.height;
				float currDistance = mScrollView.transform.localPosition.y - mOrgOffset.y;
				float relativeDistance = currDistance - absoluteDistance;
				Scroll (relativeDistance);
			}
		} else {	
			mScrollIndex = Index;
		
			mScroll = 0;
			mIsScroll = false;
			
			
		}
	}

	public void ScrollTopItemCount (int cnt)
	{
		if (cnt <= 0 || ItemObject == null)
			return;
		float delta;
		if (IsHorizontal)
			delta = -cnt * ItemObject.width;
		else if (IsVertical)
			delta = -cnt * ItemObject.height;
		else
			return;
		Scroll (delta);
	}

	public void ScrollBottomItemCount (int cnt)
	{
		if (cnt <= 0 || ItemObject == null)
			return;
		float delta;
		if (IsHorizontal)
			delta = cnt * ItemObject.width;
		else if (IsVertical)
			delta = cnt * ItemObject.height;
		else
			return;
		Scroll (delta);
	}

    bool CheckIsMoving() {
        if (mScrollView == null || ItemObject == null)
            return false;

        float checkOffset;
        if (IsHorizontal)
            checkOffset = ItemObject.width;
        else if (IsVertical)
            checkOffset = ItemObject.height;
        else
            return false;

        if (mIsFirstRun)
            return true;

        Vector3 currOffset = mScrollView.transform.localPosition;
        Vector3 delta = currOffset - mLastOffset;
        checkOffset *= checkOffset;
        if (delta.sqrMagnitude >= checkOffset) {
            mLastOffset = currOffset;
            return true;
        }
        return false;
    }

	void LateUpdate ()
	{
		if (!Application.isPlaying)
			return;

        if (!CheckIsMoving())
            return;

		refreshItems ();
		//	UpdateScrollBar ();
		DoScrollIndex ();
		DoScroll ();
		UpdateEditor ();
	}

	public void SetItemCount (int newCount)
	{
		if (ItemCount == newCount)
			return;
		ItemCount = newCount;
		if (ItemCount < 0)
			ItemCount = 0;
		// 重新刷一次数据
		mIsFirstRun = true;
		/*
		if (refreshAll || mItemList == null || mItemList.Count <= 0)
			mIsFirstRun = true;
		else
		{
			RefreshDataAtViewRect();
		}*/
	}

	public void RefreshData ()
	{
		mIsFirstRun = true;
	}

	// 重新计算生成Item缓冲
	public void ReCalcCreateItems ()
	{
		mViewMaxCount = 0;
		if ((ItemObject == null) || (ItemCount <= 0))
			return;
		if ((!IsHorizontal) && (!IsVertical))
			return;
	
		Vector2 size = mPanel.GetViewSize ();
		Vector2 childSize = new Vector2 (ItemObject.width, ItemObject.height);
		if ((Mathf.Abs (size.x) < float.Epsilon) || (Mathf.Abs (size.y) < float.Epsilon) ||
		    (Mathf.Abs (childSize.x) < float.Epsilon) || (Mathf.Abs (childSize.y) < float.Epsilon))
			return;


		int Cnt = 0;
		if (IsHorizontal) {
			// 水平
			Cnt = Mathf.CeilToInt (size.x / childSize.x);
		} else if (IsVertical) {
			// 垂直
			Cnt = Mathf.CeilToInt (size.y / childSize.y);
		}
		if (Cnt <= 0)
			return;

		mViewMaxCount = Cnt;
		if (ItemCount <= Cnt) {
			Cnt = ItemCount;
			mMustResetInWith = false;
			mScrollView.restrictWithinPanel = true;
		} else {
			Cnt += 2;
			//Cnt += 4;
			if (mScrollView.restrictWithinPanel) {
				mMustResetInWith = true;
				mScrollView.restrictWithinPanel = false;
			}
		}

		if (Cnt > 0) {
			mItemTopIndex = 0;
			mItemBottomIndex = Cnt - 1;
		} else {
			mItemBottomIndex = -1;
			mItemTopIndex = -1;
		}

		if (mItemList == null)
			mItemList = new LinkedList<UIWidget> ();
		
		ReCountList (Cnt);
	}

	public void RemoveAllItems ()
	{
		ReCountList (0);
		mItemTopIndex = -1;
		mItemBottomIndex = -1;

	}

	private Coroutine m_CreateCoroutne = null;

	private void StopCreateCoroutne ()
	{
		if (m_CreateCoroutne != null) {
			StopCoroutine (m_CreateCoroutne);
			m_CreateCoroutne = null;
		}
	}

	void DoDestroy ()
	{
		StopCreateCoroutne ();
	}

	void OnDestroy ()
	{
		DoDestroy ();
	}

	private void DoGetData (UIWidget item, int index, int subIndex)
	{
		if (index < ItemCount) {
			UITabViewItem iter = item.GetComponent<UITabViewItem> ();
			if (iter != null)
				iter.Index = index;
		}

		// 处理调用赋值接口
		if (Data != null) {
			if (index < ItemCount) {
				item.cachedGameObject.SetActive (true);
				Data.OnTabViewData (index, item, subIndex);
			} else
				item.cachedGameObject.SetActive (false);
		} else if (OnDataEvent != null) {
			if (index < ItemCount) {
				item.cachedGameObject.SetActive (true);
				OnDataEvent (index, item, subIndex);
			} else
				item.cachedGameObject.SetActive (false);
		}
#if UNITY_EDITOR
		//	LogMgr.Instance.Log(string.Format("get {0:d} data!", index));
#endif
	}

	private bool IsItemVisible (UIWidget item)
	{
		if (mPanel.clipping == UIDrawCall.Clipping.None || mPanel.clipping == UIDrawCall.Clipping.ConstrainButDontClip) {
			Vector3[] corners = item.worldCorners;
			if (!mPanel.IsVisible (corners [0], corners [1], corners [2], corners [3]))
				return false;
			return true;
		} else
			return item.isVisible;
	}

	private void Check2NoVisible ()
	{
		if (mItemList == null)
			return;
		
		if ((mItemList.Count <= 1) || (mViewMaxCount >= ItemCount))
			return;

		LinkedListNode<UIWidget> node1 = mItemList.First;
		LinkedListNode<UIWidget> node2 = node1.Next;
		if ((node1.Value == null) || (node2.Value == null))
			return;

		if (mItemBottomIndex + 1 < ItemCount) {
			while ((node1 != null) && (node1.Value != null) && (node2 != null) && (node2.Value != null) &&
			       (!IsItemVisible (node1.Value)) && (!IsItemVisible (node2.Value))) {
				if ((IsHorizontal && (node1.Value.cachedTransform.position.x < -float.Epsilon)) ||
				    (IsVertical && (node1.Value.cachedTransform.position.y > float.Epsilon))) {
					if (!ExpandMoveLast (1))
						break;
					node1 = mItemList.First;
					node2 = node1.Next;
				} else
					break;
			}
		}

		node1 = mItemList.Last;
		node2 = node1.Previous;
		if (mItemTopIndex > 0) {
			while (
				(node1 != null) && (node1.Value != null) && (node2 != null) && (node2.Value != null) &&
				(!IsItemVisible (node1.Value)) && (!IsItemVisible (node2.Value))) {
				if ((IsHorizontal && (node1.Value.cachedTransform.position.x > float.Epsilon)) ||
				    (IsVertical && (node1.Value.cachedTransform.position.y < -float.Epsilon))) {
					if (!ExpandMoveFirst (1))
						break;
					node1 = mItemList.Last;
					node2 = node1.Previous;
				} else
					break;
			}
		}

		if (mMustResetInWith) {
			if ((mItemTopIndex <= 0) || (mItemBottomIndex + 1 >= ItemCount)) {
				if (!mScrollView.restrictWithinPanel)
					mScrollView.restrictWithinPanel = true;
			} else {
				if (ScrollView.restrictWithinPanel)
					ScrollView.restrictWithinPanel = false;
			}
		}
	}

	private bool ExpandMoveFirst (int count)
	{
		if ((mItemList == null) || (count <= 0))
			return false;

		if ((mItemList.Count <= 1) || (mViewMaxCount >= ItemCount) || (mItemTopIndex <= 0))
			return false;

		bool isChg = false;
		LinkedListNode<UIWidget> node = mItemList.Last;
		while (node != null) {
			LinkedListNode<UIWidget> preNode = node.Previous;

			if (node.Value != null) {
				if (IsItemVisible (node.Value))
					break;

				LinkedListNode<UIWidget> firstNode = mItemList.First;
				if ((firstNode == null) || (firstNode.Value == null))
					break;

				mItemList.Remove (node);

				// 修改位置
				Vector2 nOffset = node.Value.pivotOffset;
				Vector2 lOffset = firstNode.Value.pivotOffset;

				if (/*IsUseTabItemSize &&*/ Data != null) {
					int newIdx = mItemTopIndex;
					if (!mIsFirstRun)
						--newIdx;
					Data.OnTabViewItemSize (newIdx, node.Value);
				}

				Vector3 pos = node.Value.cachedTransform.localPosition;
				if (IsHorizontal) {
					pos.x = firstNode.Value.cachedTransform.localPosition.x - firstNode.Value.width * lOffset.x - node.Value.width * (1 - nOffset.x);
				} else if (IsVertical) {
					pos.y = firstNode.Value.cachedTransform.localPosition.y + firstNode.Value.height * lOffset.y + node.Value.height * (1 - nOffset.y);
				}
				node.Value.cachedTransform.localPosition = pos;

				mItemList.AddFirst (node);
				--count;
				if (!mIsFirstRun) {
					--mItemTopIndex;
					--mItemBottomIndex;
					RefreshSubItem (node.Value, mItemTopIndex);
				}
				isChg = true;
				if (count == 0)
					break;
			}

			node = preNode;
		}

		if (isChg) {
			mScrollView.InvalidateBounds ();
			// mScrollView.DisableSpring();
			return true;
		}

		return false;
	}

	private bool ExpandMoveLast (int count)
	{
		if ((mItemList == null) || (count <= 0))
			return false;

		if ((mItemList.Count <= 1) || (mViewMaxCount >= ItemCount) || (mItemBottomIndex + 1 >= ItemCount))
			return false;

		bool isChg = false;
		LinkedListNode<UIWidget> node = mItemList.First;
		while (node != null) {
			LinkedListNode<UIWidget> nextNode = node.Next;

			if (node.Value != null) {
				if (IsItemVisible (node.Value))
					break;

				LinkedListNode<UIWidget> lastNode = mItemList.Last;
				if ((lastNode == null) || (lastNode.Value == null))
					break;

				mItemList.Remove (node);

				Vector2 nOffset = node.Value.pivotOffset;
				Vector2 lOffset = lastNode.Value.pivotOffset;

				if (/*IsUseTabItemSize &&*/ Data != null) {
					int newIdx = mItemBottomIndex;
					if (!mIsFirstRun)
						++newIdx;
					Data.OnTabViewItemSize (newIdx, node.Value);
				}

				// 修改位置
				Vector3 pos = node.Value.cachedTransform.localPosition;
				if (IsHorizontal) {
					pos.x = lastNode.Value.cachedTransform.localPosition.x + lastNode.Value.width * (1 - lOffset.x) + node.Value.width * nOffset.x;
				} else if (IsVertical) {
					pos.y = lastNode.Value.cachedTransform.localPosition.y - lastNode.Value.height * (1 - lOffset.y) - node.Value.height * nOffset.y;
				}
				node.Value.cachedTransform.localPosition = pos;

				mItemList.AddLast (node);
				--count;
				if (!mIsFirstRun) {
					++mItemTopIndex;
					++mItemBottomIndex;
					RefreshSubItem (node.Value, mItemBottomIndex);
				}
				isChg = true;
				if (count == 0)
					break;
			}

			node = nextNode;
		}

		if (isChg) {
			mScrollView.InvalidateBounds ();
			//mScrollView.DisableSpring();
			return true;
		}

		return false;
	}


	private void ReCountList (int newCount)
	{
		if (mItemList == null)
			return;

		if (newCount < 0)
			newCount = 0;

		if (newCount == mItemList.Count)
			return;
		if (newCount < mItemList.Count) {
			for (int i = mItemList.Count; i > newCount; --i) {
				LinkedListNode<UIWidget> node = mItemList.Last;
				if (node != null) {
					if ((node.Value != null) && (node.Value.cachedGameObject != null)) {
						NGUITools.DestroyImmediate (node.Value.cachedGameObject);
					}
					mItemList.RemoveLast ();
				}
			}
		} else {
			if ((ItemObject == null) || (ItemObject.cachedGameObject == null))
				return;

			Vector2 parentSize = mPanel.GetViewSize ();
			//	Vector3 s = new Vector3(1, 1, 1);
			//	Vector2 offset = NGUIMath.GetPivotOffset(mScrollView.contentPivot);
			Vector2 childSize = new Vector2 (ItemObject.width, ItemObject.height);

			// Vector4 clipRange = mScrollView.panel.baseClipRegion;
			for (int i = mItemList.Count; i < newCount; ++i) {
				GameObject obj = GameObject.Instantiate (ItemObject.cachedGameObject) as GameObject;
				// 激活
				obj.SetActive (true);
				UITabViewItem item = obj.GetComponent<UITabViewItem> ();
				if (item == null)
					item = obj.AddComponent<UITabViewItem> ();
				item.Index = i;
				item.TabView = this;
				UIWidget widget = obj.GetComponent<UIWidget> ();
				widget.cachedTransform.parent = mPanel.cachedTransform;
				if (widget.panel == null)
					widget.panel = mPanel;
				LinkedListNode<UIWidget> last = mItemList.Last;
				Vector3 tPos = widget.cachedTransform.localPosition;
				widget.cachedTransform.localScale = Vector3.one;
				int subDepth = ItemDepth - widget.depth;
				NGUITools.AdjustDepth (obj, subDepth);
				Vector2 offset = widget.pivotOffset;

				if ((last != null) && (last.Value != null)) {

					// 上一个Size
					if (/*IsUseTabItemSize &&*/ Data != null) {
						var target = last.Value;
						var targetIdx = item.Index - 1;
						Data.OnTabViewItemSize (targetIdx, target);
						Vector2 targetSize = new Vector2 (target.width, target.height);

						Data.OnTabViewItemSize (item.Index, widget);
						childSize = new Vector2 (widget.width, widget.height);

						childSize.x = (childSize.x + targetSize.x) / 2f;
						childSize.y = (childSize.y + targetSize.y) / 2f;
					}

					if (IsHorizontal) {
						tPos.x = last.Value.cachedTransform.localPosition.x + childSize.x;
						tPos.y = 0;
					} else {
						tPos.y = last.Value.cachedTransform.localPosition.y - childSize.y;
						tPos.x = 0;
					}
				} else {

					// 自己的Size
					if (/*IsUseTabItemSize &&*/ Data != null) {
						Data.OnTabViewItemSize (item.Index, widget);
						childSize = new Vector2 (widget.width, widget.height);
					}

					if (IsHorizontal) {
						tPos.x = -parentSize.x / 2 + childSize.x * offset.x;
						tPos.y = 0;
					} else {
						tPos.y = parentSize.y / 2 - childSize.y * offset.y;
						tPos.x = 0;
					}
				}
				
				BoxCollider collider = obj.GetComponent<BoxCollider> ();
				if (collider != null) {
					Vector3 center = collider.center;
					center.z = tPos.z;
					collider.center = center;
				}
				tPos.z = 0;
				widget.cachedTransform.localPosition = tPos;
				mItemList.AddLast (widget);
			}

		}

		mScrollView.ResetPosition ();
	}


	private LinkedList<UIWidget> ItemList {
		get {
			if (mItemList == null)
				mItemList = new LinkedList<UIWidget> ();
			return mItemList;
		}
	}

	// 基于模型
	public UIWidget ItemObject = null;
	public int ItemCount = 0;
	public int ItemDepth = 0;
	public bool IsAsynInitData = true;
	// 是否会动态ItemSize
	public bool IsUseTabItemSize = false;
	public int SubItemCount = 1;

	// 是否按需显示ScrollBar
	public bool IsNeetShowScrollBar = false;
	public UIScrollBar ScrollBar = null;

	/*
	public void CalcTabViewAllSize (ref float contentMin, ref float contentMax, ref float contentSize)
	{
		if ((mItemList == null) || mIsFirstRun || (ItemCount <= 0))
			return;

		if (IsHorizontal) {
			// 水平
			var first = mItemList.First;
			var end = mItemList.Last;
			if ((first != null) && (first.Value != null))
			{
				float w = first.Value.width;
				float v = mItemTopIndex * w;
				contentMin += v;
				contentSize += v;
			}

			if ((end != null) && (end.Value != null))
			{
				float w = end.Value.width;
				float v = (ItemCount - 1 - mItemBottomIndex) * w;
				contentMax -= v;
				contentSize += v;
			}
		} else
		if (IsVertical) {
			// 垂直
			var first = mItemList.First;
			var end = mItemList.Last;
			if ((first != null) && (first.Value != null))
			{
				float h = first.Value.height;
				float v = mItemTopIndex * h;
				contentMin += v;
				contentSize += v;
			}
			
			if ((end != null) && (end.Value != null))
			{
				float h = end.Value.height;
				float v = (ItemCount - 1 - mItemBottomIndex) * h;
				contentMax -= v;
				contentSize += v;
			}
		}
	}*/

	public LinkedListNode<UIWidget> FirstNode
	{
		get
		{
			if (mItemList == null || mItemList.Count <= 0)
				return null;
			return mItemList.First;
		}
	}

	public LinkedListNode<UIWidget> LastNode
	{
		get
		{
			if (mItemList == null || mItemList.Count <= 0)
				return null;
			return mItemList.Last;
		}
	}

	public UIWidget FirstWidget {
		get {
			var node = FirstNode;
			if (node == null)
				return null;
			return node.Value;
		}
	}

	public UITabViewItem FirstItem {
		get {
			UIWidget widget = FirstWidget;
			if (widget == null)
				return null;
			return widget.GetComponent<UITabViewItem> ();
		}
	}

	public UIWidget LastWidget {
		get {
			var node = LastNode;
			if (node == null)
				return null;
			return node.Value;
		}
	}

	public UITabViewItem LastItem {
		get {
			UIWidget widget = LastWidget;
			if (widget == null)
				return null;
			return widget.GetComponent<UITabViewItem> ();
		}
	}

	#if UNITY_EDITOR
	//public Vector2 DEBUG_SubClipOffset = Vector2.zero;mIsFirstUpdate
	#endif

	#region private vars

	private UIScrollView mScrollView = null;
	private UIPanel mPanel = null;
	private LinkedList<UIWidget> mItemList = null;
	private int mItemTopIndex = -1;
	private int mItemBottomIndex = -1;
	private int mViewMaxCount = 0;
	private bool mMustResetInWith = false;
	private bool mIsFirstRun = true;
	private int mScrollIndex = -1;
	private Vector3 mOrgOffset = Vector3.zero;
    private Vector3 mLastOffset = Vector3.zero;
	//	private bool mIsFirstUpdate = true;
	//private Vector2 mLastOffset = Vector2.zero;
	
	// 滚动相关
	private bool mIsScroll = false;
	private float mScroll = 0.0f;
	private float mScrollSpeed = 0.0f;

	#endregion private vars
}