using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor
{
    public interface IReadOnlyTree<T>
    {
        Tree<T>.TreeNode RootNode { get; }
        Tree<T>.TreeNode FindNode(string key);
    }

    public class Tree<T> : IReadOnlyTree<T>
    {
        public class TreeNode
        {
            T? _value;
            public bool IsLeaf { get; private set; }
            readonly char _delimiter;
            readonly Dictionary<string, TreeNode> _nodes;
            public IReadOnlyDictionary<string, TreeNode> Nodes => _nodes;
            public T? Value => _value;
            public TreeNode(char delimiter = '/')
            {
                _delimiter = delimiter;
                _nodes = new Dictionary<string, TreeNode>();
            }

            public void SetValue(T value)
            {
                if (IsLeaf)
                {
                    throw new ArgumentException("Cannot add node to leaf");
                }
                IsLeaf = true;
                _value = value;
            }

            public void Add(ReadOnlySpan<char> keySpan, T value)
            {
                keySpan = keySpan.Trim();
                keySpan = keySpan.Trim(_delimiter);
                int firstDelimiterIndex = keySpan.IndexOf(_delimiter);
                if (firstDelimiterIndex > 0) //We need a subNode
                {
                    var firstKey = keySpan.Slice(0, firstDelimiterIndex).ToString();
                    var node = GetOrAdd(firstKey);
                    if (node.IsLeaf)
                    {
                        throw new ArgumentException("Part of the given key already exists as a leafnode");
                    }
                    var keyRest = keySpan.Slice(firstDelimiterIndex).ToString();
                    node.Add(keyRest, value);
                }
                else if (keySpan.Length > 0) //We are at the level
                {
                    string name = keySpan.ToString();
                    var node = new TreeNode();
                    node.SetValue(value);
                    _nodes.Add(name, node);
                }
                else
                {
                    throw new ArgumentException("The key must consist of atleast 1 letter");
                }
            }

            public TreeNode Get(ReadOnlySpan<char> keySpan)
            {
                keySpan = keySpan.Trim();
                keySpan = keySpan.Trim(_delimiter);

                int firstDelimiterIndex = keySpan.IndexOf(_delimiter);
                if (firstDelimiterIndex > 0) //We need a subNode
                {
                    var firstKey = keySpan.Slice(0, firstDelimiterIndex).ToString();
                    return _nodes[firstKey].Get(keySpan);
                }
                else if (keySpan.Length > 0) //We are at the level
                {
                    string name = keySpan.ToString();
                    return _nodes[name];
                }
                else
                {
                    throw new ArgumentException("The key must consist of atleast 1 letter");
                }
            }

            TreeNode GetOrAdd(string key)
            {
                if (!_nodes.TryGetValue(key, out var node))
                {
                    node = new TreeNode();
                    _nodes.Add(key, node);
                }
                return node;
            }
        }

        readonly TreeNode _rootNode;

        public Tree<T>.TreeNode RootNode => _rootNode;

        public Tree(char delimiter = '/')
        {
            _rootNode = new TreeNode(delimiter);
        }

        public void AddNode(string key, T value)
        {
            _rootNode.Add(key, value);
        }

        public TreeNode FindNode(string key)
        {
            return _rootNode.Get(key);
        }
    }
}
