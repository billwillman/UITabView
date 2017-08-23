using System;

namespace NsLib.UI {
    
    // UI层级关系
    public enum UILayerType {
        // 主面板层级=>会和主界面同层，关闭则都关闭
        UILayer_MainWindow = 0,
        // 普通界面层，普通界面和主界面以及其他普通界面互斥，会关闭
        UILayer_NormalWindow = 1,
        // 弹框层，可覆盖在普通界面以及主界面上，不会关闭其他界面
        UILayer_PopupWindow = 2
    }

    // UI层级
    public interface IUILayer {

    }
}
