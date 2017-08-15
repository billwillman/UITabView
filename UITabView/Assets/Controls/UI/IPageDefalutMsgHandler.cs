using System;
using UnityEngine;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    public static class PageDefaultMsgId {
        // 创建消息
        public static readonly long WM_CREATE = -1;
        // 显示消息
        public static readonly long WM_SHOW = -2;
        // 关闭消息
        public static readonly long WM_CLOSE = -3;
        // 释放消息
        public static readonly long WM_FREE = -4;
        // 用户自定义消息开始
        public static readonly long WM_USER = 0;
    }

    public interface IPageDefaultMsgHandler {
        // 界面打开
        void OnOpen();
        // 界面关闭
        void OnClose();
        // 界面释放
        void OnFree();

        LRESULT OnMsg(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0);
    }

    // Page消息分发器
    public interface IPageMsgDispatch {
        // 发送消息等待返回
        LRESULT SendMessage(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0);
        // 发送消息不等待返回
        bool PostMessage(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0);
        
    }

    /* 主循环接口 */

    // 页面的循环
    public abstract class IPageLoop: MonoBehaviour { }

    // 系统循环
    public abstract class ISystemLoop : MonoBehaviour { }
}