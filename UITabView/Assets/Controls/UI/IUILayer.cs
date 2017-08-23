using System;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.UI {
    
    // UI层级关系
    public enum UILayerType {
        // Depth: -99 ~ 0
        // 主面板层级=>会和主界面同层，关闭则都关闭
        UILayer_MainWindow = 0,
        // Depth: 1 ~ 100
        // 普通界面层，普通界面和主界面以及其他普通界面互斥，会关闭
        UILayer_NormalWindow = 1,
        // Depth: 101 ~ 200
        // 弹框层，可覆盖在普通界面以及主界面上，不会关闭其他界面
        UILayer_PopupWindow = 2,
        UILayer_NUM = 3
    }

    // UI层级
    public interface IUILayer {
        UILayerType LayerType {
            get;
            set;
        }

        // 返回层级
        int Depth {
            get;
            set;
        }
    }

    // Layer管理器
    internal sealed class LayerManager {

        public LayerManager() {
            int layerCnt = (int)UILayerType.UILayer_NUM;
            m_CurrentLayerMaxDepths = new int[layerCnt];
            for (int i = 0; i < layerCnt; ++i) {
                m_CurrentLayerMaxDepths[i] = int.MinValue;
            }
        }

        // 设置层级
        public void ShowLayer(IUILayer layer) {
            if (layer == null)
                return;
            UILayerType layerType = layer.LayerType;
            int maxDepth = GetCurrentLayerMaxDepth(layerType) + 1;
            layer.Depth = maxDepth;
            ChangeCurrentLayerMaxDepth(layerType, maxDepth);
        }

        public bool CanClose(IUILayer layer) {
            if (layer == null)
                return false;
            UILayerType layerType = layer.LayerType;
            int maxDepth = GetCurrentLayerMaxDepth(layerType);
            return layer.Depth >= maxDepth;
        }

        public bool ChangeLayerMaxDepth(UILayerType layerType, int depth) {
            int minDepth = GetLayerDepthMin(layerType);
            int maxDepth = GetLayerDepthMax(layerType);
            if (depth < minDepth || depth > maxDepth)
                return false;
            m_CurrentLayerMaxDepths[(int)layerType] = depth;
            return true;
        }

        // 当前界面最大Depth
        private int[] m_CurrentLayerMaxDepths = null;

        // 当前最大层级关系
        private int GetCurrentLayerMaxDepth(UILayerType layerType) {
            int layer = (int)layerType;
            if (m_CurrentLayerMaxDepths == null || layer < 0 || 
                layer >= (int)UILayerType.UILayer_NUM)
                return GetLayerDepthMin(layerType);
            int maxDepth = m_CurrentLayerMaxDepths[layer];
            if (maxDepth == int.MinValue)
                maxDepth = GetLayerDepthMin(layerType);
            return maxDepth;
        }

        // 修改层级关系
        public void ChangeCurrentLayerMaxDepth(UILayerType layerType, int depth) {
            int layer = (int)layerType;
            if (m_CurrentLayerMaxDepths == null || layer < 0 ||
                layer >= (int)UILayerType.UILayer_NUM)
                return;
            int minLayer = GetLayerDepthMin(layerType);
            int maxLayer = GetLayerDepthMax(layerType);
            if (depth >= minLayer && depth <= maxLayer) {
                m_CurrentLayerMaxDepths[layer] = depth;
            }
        }

        public static int GetLayerDepthMin(UILayerType layerType) {
            switch (layerType) {
                case UILayerType.UILayer_MainWindow:
                    return -100;
                case UILayerType.UILayer_NormalWindow:
                    return 0;
                case UILayerType.UILayer_PopupWindow:
                    return 100;
                default:
                    return 0;
            }
        } 

        // 层级Depth最大值
        public static int GetLayerDepthMax(UILayerType layerType) {
            switch (layerType) {
                case UILayerType.UILayer_MainWindow:
                    return 0;
                case UILayerType.UILayer_NormalWindow:
                    return 100;
                case UILayerType.UILayer_PopupWindow:
                    return 200;
                default:
                    return 100;
            }
        }
    }

}
