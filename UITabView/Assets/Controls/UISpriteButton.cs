using UnityEngine;

// Sprite Button
[RequireComponent(typeof(UIButton))]
public class UISpriteButton: MonoBehaviour
{
	// 普通状态
	public UISprite NormalSprite = null;
	// 按下状态
	public UISprite DownSprite = null;
	// 鼠标进入
	public UISprite HoveredSprite = null;

	void SetCtlActive(UISprite sprite, bool isActived)
	{
		if (sprite == null)
			return;
		sprite.gameObject.SetActive (isActived);
	}

	void OnHover(bool isHovered)
	{
		DisableAllSprites ();
		if (isHovered) {
			if (HoveredSprite != null)
				SetCtlActive(HoveredSprite, true);
			else
				if (NormalSprite != null)
					SetCtlActive(NormalSprite, true);
		} else {
			if (NormalSprite != null)
				SetCtlActive(NormalSprite, true);
		}
	}

	void OnPress(bool isDown)
	{
		DisableAllSprites ();
		if (isDown) {
			if (DownSprite != null)
				SetCtlActive(DownSprite, true);
			else
				if (HoveredSprite != null)
					SetCtlActive(HoveredSprite, true);
			else
				if (NormalSprite != null)
					SetCtlActive(NormalSprite, true);
		} else {
			if (NormalSprite != null)
				SetCtlActive(NormalSprite, true);
		}
	}

	void DisableAllSprites()
	{
		SetCtlActive (NormalSprite, false);
		SetCtlActive (DownSprite, false);
		SetCtlActive (HoveredSprite, false);
	}
}