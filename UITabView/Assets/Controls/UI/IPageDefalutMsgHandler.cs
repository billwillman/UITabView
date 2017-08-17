using System;
using UnityEngine;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    // 负数为保留消息，不要主动发送保留消息，都是系统来发

    public enum PageDefaultMsgId {
        WM_CREATE = -1,
        WM_SHOW = -2,
        WM_CLOSE = -3,
        WM_FREE = -4,
        WM_USER = 0
    }

    public static class LRESULT_VALUE {
        public static readonly LRESULT FAIL = -1;
        public static readonly LRESULT OK = 0;
    }

    public interface IPageDefaultMsgHandler {
        // 界面打开
        void OnShow();
        // 界面关闭
        void OnClose();
        // 界面释放
        void OnFree();

        void OnCreate();

        LRESULT OnMsg(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0);
        
        // 放到消息队列里
        bool PushMsgToList(long msgId, System.Object obj, long wParam, long lParam);
    }

    // Page消息分发器
    public interface IPageMsgDispatch {
        // 发送消息等待返回
        LRESULT SendMessage(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0);
        // 发送消息不等待返回
        bool PostMessage(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0);
        
    }

    /* 主循环接口 */

    public abstract class IPageLoop: MonoBehaviour, IPageDefaultMsgHandler {
        // 更新
        public virtual void Update() {

        }
        
        public virtual void OnShow() { }

        public virtual void OnClose() { }

        public virtual void OnFree() { }

        public virtual void OnCreate() { }

        public virtual LRESULT OnMsg(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0) {
            LRESULT ret;
            if (DispatchDefaultMsgFunc(msgId, obj, wParam, lParam))
                ret = LRESULT_VALUE.OK;
            else
                ret = LRESULT_VALUE.FAIL;
            return ret;
        }

        public virtual bool PushMsgToList(long msgId, System.Object obj, long wParam, long lParam) {
            MsgNode node = m_MsgList.CreateMsg(msgId, obj, wParam, lParam);
            return node != null;
        }

        protected bool DispatchDefaultMsgFunc(long msgId, System.Object obj, long wParam, long lParam) {
            bool ret = false;
            switch (msgId) {
                case (long)PageDefaultMsgId.WM_CLOSE:
                    OnClose();
                    ret = true;
                    break;
                case (long)PageDefaultMsgId.WM_CREATE:
                    OnCreate();
                    ret = true;
                    break;
                case (long)PageDefaultMsgId.WM_FREE:
                    OnFree();
                    ret = true;
                    break;
                case (long)PageDefaultMsgId.WM_SHOW:
                    OnShow();
                    ret = true;
                    break;
            }

            return ret;
        }

        private MsgList m_MsgList = new MsgList();

    }

    // 系统循环
    public abstract class ISystemLoop : MonoBehaviour, IMsgCenterData {
        public abstract IPageDefaultMsgHandler GetMsgHandler(HWND handle);
    }
}