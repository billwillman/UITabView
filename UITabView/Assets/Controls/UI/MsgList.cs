using System;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    // 消息节点
    internal class MsgNode {

        internal MsgNode(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0) {
            this.Msg = msgId;
            this.Obj = obj;
            this.wParam = wParam;
            this.lParam = lParam;
            Node = new LinkedListNode<MsgNode>(this);
        }

        public long Msg {
            get;
            internal set;
        }

        public System.Object Obj {
            get;
            internal set;
        }

        public long wParam {
            get;
            internal set;
        }

        public long lParam {
            get;
            internal set;
        }

        internal LinkedListNode<MsgNode> Node {
            get;
            private set;
        }
    }


    // 消息队列
    internal class MsgList {

        // 创建消息
        public MsgNode CreateMsg(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0) {
            MsgNode ret = CreateMsgNode(msgId, obj, wParam, lParam);
            if (ret != null)
                m_MsgList.AddLast(ret);
            return ret;
        }

        public MsgNode PopMsg() {
            var node = m_MsgList.First;
            if (node != null) {
                m_MsgList.Remove(node);
                return node.Value;
            }
            return null;
        }

        // 删除消息
        public void DestroyMsg(MsgNode node) {
            if (node == null)
                return;
            if (node.Node != null && node.Node.List == m_MsgList)
                m_MsgList.Remove(node.Node);
            DestroyMsgNode(node);
        }


        private LinkedList<MsgNode> m_MsgList = new LinkedList<MsgNode>();

        // 创建消息节点
        protected static MsgNode CreateMsgNode(long msgId, System.Object obj = null, 
            long wParam = 0, long lParam = 0) {
            var firstNode = m_MsgPool.First;
            if (firstNode == null) {
                MsgNode ret = new MsgNode(msgId, obj, wParam, lParam);
                return ret;
            }

            m_MsgPool.Remove(firstNode);

            MsgNode ret1 = firstNode.Value;
            ret1.Msg = msgId;
            ret1.Obj = obj;
            ret1.wParam = wParam;
            ret1.lParam = lParam;
            return ret1;
        }

        // 删除消息节点
        protected static void DestroyMsgNode(MsgNode node) {
            if (node == null)
                return;
            LinkedListNode<MsgNode> m = node.Node;
            if (m == null || (m.List != null))
                return;
            m_MsgPool.AddLast(m);
        }

        // 消息池
        private static LinkedList<MsgNode> m_MsgPool = new LinkedList<MsgNode>();
    }
}
