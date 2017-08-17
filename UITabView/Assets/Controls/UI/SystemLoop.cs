﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    // 系统循环
    public sealed class SystemLoop: ISystemLoop {

        public override IPageDefaultMsgHandler GetMsgHandler(HWND handle) {
            IPageLoop page;
            if (m_PageMap.TryGetValue(handle, out page)) {
                if (page == null)
                    return null;
                return page as IPageDefaultMsgHandler;
            }

            return null;
        }

        // 系统页
        private Dictionary<HWND, IPageLoop> m_PageMap = new Dictionary<HWND, IPageLoop>();
    }

}
