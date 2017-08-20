using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WndChat : MonoBehaviour, ITableViewData {

    private UITableView m_ChatView = null;
    private UIInput m_InputView = null;
    private List<int> m_ChatSize = new List<int> ();

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
            int height = (int)(lb.height - lb.cachedTransform.localPosition.y);
            if (height < 100)
                height = 100;
            m_ChatSize [index] = height;
        }
    }

    public void OnTabViewItemSize (int index, UIWidget item)
    {
        if (index >= m_ChatSize.Count)
            return;

        item.height = m_ChatSize [index];
        
       
    }

    public void OnBtnSendClick()
    {
        // 点击事件
        string text = m_InputView.value;
        if (string.IsNullOrEmpty (text))
            return;
        
        ChatDataManager.Instance.AddChat (string.Empty, text, "123");
        m_ChatSize.Add (100);
        m_ChatView.AddItem (1, true);
    }
}
