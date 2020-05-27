using System;
using System.Collections.Generic;
using System.Text;

//Finite State Tree(FST)
// https://docs.google.com/presentation/d/1xQo5Axy0GR7l1cGDWupx_d5_nOaVyNPzuO99-svNgI4/edit#slide=id.g782da3613b_0_208
// 심플버전... 이게 낫다
// 된거같네...
// Method Chaining  으로... > 완료
// friend class나 internal로 숨기기
// OnActive/OnDeactive 가 더 나을듯??

namespace Dogfoot.AI
{

    public class FSTNode
    {
        public string Name { get; protected set; } = "NoName";

        public Func<FSTNode, bool> OnThink;
        public Action<FSTNode> OnBeginAct;
        public Action<FSTNode> OnAct;
        public Action<FSTNode> OnEndAct;

        public bool IsSelected { get; set; } = false;

        public List<FSTNode> Children { get; protected set; }

        public FSTNode(string name, Func<FSTNode, bool> onThink = null, Action<FSTNode> onBeginAct = null, Action<FSTNode> onAct = null, Action < FSTNode> onEndAct = null, params FSTNode[] children)
        {
            Name = name;
            OnThink = onThink;
            OnBeginAct = onBeginAct;
            OnAct = onAct;
            OnEndAct = onEndAct;
            Children = new List<FSTNode>(children);
        }

        public FSTNode(string name, Func<FSTNode, bool> onThink, Action<FSTNode> onAct, params FSTNode[] children) : this(name, onThink, null, onAct, null, children)
        {
        }

        public FSTNode(string name, params FSTNode[] children) : this(name, _ => true, null, children)
        {
        }

        //For Chaining
        public FSTNode Think(Func<FSTNode, bool> onThink)
        {
            this.OnThink = onThink;
            return this;
        }
        public FSTNode BeginAct(Action<FSTNode> onBeginAct)
        {
            this.OnBeginAct = onBeginAct;
            return this;
        }
        public FSTNode Act(Action<FSTNode> onAct)
        {
            this.OnAct = onAct;
            return this;
        }
        public FSTNode EndAct(Action<FSTNode> onEndAct)
        {
            this.OnEndAct = onEndAct;
            return this;
        }
        public FSTNode AddChildren(params FSTNode[] children)
        {
            this.Children = new List<FSTNode>(children);
            return this;
        }
    }

    public class FST
    {
        public List<FSTNode> RootNodes { get; set; }

        private List<FSTNode> LastNodePath = new List<FSTNode>();   //지난 Tick의 NodePath
        private List<FSTNode> CurrentNodePath = new List<FSTNode>();    //현재 Tick의 NodePath

        public FST(params FSTNode[] nodes)
        {
            RootNodes = new List<FSTNode>(nodes);
        }

        public FSTNode GetCurrentNode()
        {
            return CurrentNodePath[CurrentNodePath.Count - 1];
        }

        public string GetCurrentNodePath()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in CurrentNodePath)
                sb.Append("/").Append(node.Name);
            return sb.ToString();
        }


        public void Think()
        {
            //Exchange Last & Current
            LastNodePath = CurrentNodePath;
            CurrentNodePath = new List<FSTNode>();

            foreach (var child in RootNodes)
            {
                if (RunRecursive(child))
                    break;
            }
        }

        //true일 경우는 하위 노드만 재귀적으로 호출...
        //false일 경우는 다음 sibling node 호출...
        private bool RunRecursive(FSTNode node)
        {
            bool result = node.OnThink(node);
            if(result)
            {
                CurrentNodePath.Add(node);

                //LastNodePath 의 현재 node와 동일 level 포함 이후로 OnEndAct 해야함
                for (int i = 0; i < LastNodePath.Count; i++)
                {
                    if (i < CurrentNodePath.Count
                        && LastNodePath[i] != CurrentNodePath[i])
                    {
                        for (int j = i; j < LastNodePath.Count; j++)
                        {
                            if (LastNodePath[j].IsSelected == true)
                            {
                                LastNodePath[j].IsSelected = false;
                                LastNodePath[j].OnEndAct?.Invoke(LastNodePath[j]);
                            }
                        }
                        break;
                    }
                }
  
                if (node.IsSelected == false)
                {
                    node.IsSelected = true;
                    node.OnBeginAct?.Invoke(node);
                }

                node.OnAct?.Invoke(node);

                foreach(var child in node.Children)
                {
                    if (RunRecursive(child))
                        break;
                }
            }
            else
            {
                if (node.IsSelected == true)
                {
                    //리커시브하게 OnEndAct 해야함
                    for (int i = LastNodePath.IndexOf(node); i < LastNodePath.Count; i++)
                    {
                        LastNodePath[i].IsSelected = false;
                        LastNodePath[i].OnEndAct?.Invoke(LastNodePath[i]);
                    }
                }
            }

            return result;
        }
    }

}