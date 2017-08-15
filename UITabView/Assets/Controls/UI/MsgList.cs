using System;
using System.Collections;
using System.Collections.Generic;

namespace NsLib.UI {

    using HWND = String;
    using LRESULT = Int64;

    // 消息节点
    public class MsgNode {

        internal MsgNode(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0) {
            Msg = msgId;
            Obj = obj;
            wParam = wParam;
            lParam = lParam;
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
    public class MsgList {

        public MsgNode CreateMsg(long msgId, System.Object obj = null, long wParam = 0, long lParam = 0) {
            MsgNode ret = CreateMsgNode(msgId, obj, wParam, lParam);
            return ret;
        }

        public void DestroyMsg(MsgNode node) {
            if (node == null)
                return;
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
