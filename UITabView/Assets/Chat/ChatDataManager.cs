using System;
using System.Collections;
using System.Collections.Generic;

public class ChatData
{
    public string Icon {
        get;
        set;
    }

    public string Text {
        get;
        set;
    }

    public string Name {
        get;
        set;
    }
}

public class ChatDataManager
{

    public ChatDataManager()
    {

    }

    public ChatData GetChatData(int index)
    {
        if (index < 0 || index >= m_ChatList.Count)
            return null;
        return m_ChatList [index];
    }

    public void AddChat(string icon, string text, string name)
    {
        ChatData chat = new ChatData ();
        chat.Icon = icon;
        chat.Text = text;
        chat.Name = name;
        m_ChatList.Add (chat);
    }

    public int ChatCount
    {
        get {
            return m_ChatList.Count;
        }
    }

    private List<ChatData> m_ChatList = new List<ChatData> ();


    public static ChatDataManager GetInstance()
    {
        if (m_Mgr == null)
            m_Mgr = new ChatDataManager ();
        return m_Mgr;
    }

    public static ChatDataManager Instance
    {
        get {
            return GetInstance ();
        }
    }

    private static ChatDataManager m_Mgr = null;
}
