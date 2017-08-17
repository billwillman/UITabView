using System;

namespace NsLib.UI.Message {

    using HWND = String;
    using LRESULT = Int64;

    // 获得消息中心接口需要获得数据
    public interface IMsgCenterData {
        // 根据HWND获得窗体消息处理
        IPageMsgHandler GetMsgHandler(HWND handle);
    }

    // 消息中心
    public class MsgCenter: SystemLoop, IPageMsgDispatch {

        // 发送消息等待返回
        public virtual LRESULT SendMsg(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0) {

            IPageMsgHandler handler = GetMsgHandler(handle);
            if (handler == null) {
                return LRESULT_VALUE.FAIL;
            }

            // 直接发送返回
            return handler.OnMsg(msg, obj, wParam, lParam);
        }

        // 发送消息不等待返回
        public virtual bool PostMsg(HWND handle, long msg, System.Object obj = null, long wParam = 0, long lParam = 0) {

            IPageMsgHandler handler = GetMsgHandler(handle);
            if (handler == null)
                return false;

            return handler.PushMsgToList(msg, obj, wParam, lParam);
        }
    }
}