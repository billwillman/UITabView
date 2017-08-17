using System;
using UnityEngine;
using NsLib.UI.Message;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    // UI管理器, 可以发送消息 SendMsg和PostMsg
    public sealed class UIManager: MsgCenter {

        // 打开界面

        // 查找界面
        public IPageLoop FindPage(HWND handle) {
            var handler = GetMsgHandler(handle);
            return handler as IPageLoop;
        }
        

        /* ---------Static区域--------- */

        public static UIManager GetInstance() {
            if (m_Mgr == null) {
                GameObject gameObj = new GameObject("UIManager");
                m_Mgr = gameObj.AddComponent<UIManager>();
                
            }
            return m_Mgr;
        }

        public static UIManager Instance {
            get {
                return GetInstance();
            }
        }

        private static UIManager m_Mgr = null;
        /*---------------------*/
    }
}