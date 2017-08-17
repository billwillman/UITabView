using System;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.UI.Message {

    using HWND = String;
    using LRESULT = Int64;

    // 系统循环
    public class SystemLoop: ISystemLoop {

        public override IPageMsgHandler GetMsgHandler(HWND handle) {
            IPageLoop page;
            if (m_PageMap.TryGetValue(handle, out page)) {
                if (page == null)
                    return null;
                return page as IPageMsgHandler;
            }

            return null;
        }

        public IPageLoop FindPage(HWND handle) {
            var handler = GetMsgHandler(handle);
            return handler as IPageLoop;
        }

        // 增加Page
        protected bool AddPage(HWND handle, IPageLoop page) {
            if (string.IsNullOrEmpty(handle))
                return false;
            m_PageMap[handle] = page;
            return true;
        }

        // 系统页
        protected Dictionary<HWND, IPageLoop> m_PageMap = new Dictionary<HWND, IPageLoop>();
    }

}
