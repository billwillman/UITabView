using UnityEngine;
using System.Collections;

public class WndChat : MonoBehaviour, ITableViewData {

    private UITableView m_ChatView = null;
    private UIInput m_InputView = null;

    void Start()
    {
        var trans = this.transform;
        m_ChatView = trans.FindChild ("ChatRoot/ChatView").GetComponent<UITableView>();
        m_ChatView.Data = this;
        m_ChatView.InitItemCount (0);

        m_InputView = trans.FindChild ("ChatRoot/InputRoot").GetComponent<UIInput> ();
    }

    public void OnTabViewData (int index, UIWidget item, int subIndex)
    {
        if (subIndex == 0) {
            ChatData data = ChatDataManager.Instance.GetChatData (index);
            if (data == null)
                return;
            var trans = item.cachedTransform;
            UILabel lb = trans.FindChild ("Label").GetComponent<UILabel> ();
            lb.text = data.Text;
        }
    }

    public void OnTabViewItemSize (int index, UIWidget item)
    {
       // Debug.Log ("ItemSize");
        if (ChatDataManager.Instance.ChatCount <= 0)
            return;
        
        var trans = item.cachedTransform;
        var lb = trans.FindChild ("Label").GetComponent<UILabel>();
        int height = (int)(lb.height + lb.cachedTransform.localPosition.y);
        if (height < 100)
            item.height = 100;
        else
            item.height = height;
    }

    public void OnBtnSendClick()
    {
        // 点击事件
        string text = m_InputView.value;
        if (string.IsNullOrEmpty (text))
            return;
        
        ChatDataManager.Instance.AddChat (string.Empty, text, "123");
        m_ChatView.AddItem ();
    }
}
