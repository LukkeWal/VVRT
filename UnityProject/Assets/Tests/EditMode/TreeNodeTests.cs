// Assets/Tests/EditMode/TreeNodeTests.cs
using NUnit.Framework;
using _Project.Ray_Tracer.Scripts.Utility;

[TestFixture]
public class TreeNodeTests
{
    [Test]
    public void Constructor_SetsData_NoParent_NoChildren()
    {
        var node = new TreeNode<int>(42);
        Assert.AreEqual(42, node.Data);
        Assert.IsTrue(node.IsRoot());
        Assert.IsTrue(node.IsLeaf());
    }

    [Test]
    public void AddChild_ByData_AddsChildAndSetsParent()
    {
        var root = new TreeNode<string>("root");
        root.AddChild("leaf");
        Assert.AreEqual(1, root.Children.Count);
        var leaf = root.Children[0];
        Assert.AreEqual("leaf", leaf.Data);
        Assert.IsFalse(leaf.IsRoot());
        Assert.IsTrue(leaf.IsLeaf());
    }

    [Test]
    public void RemoveChild_WorksAndReturnsCorrectBool()
    {
        var root = new TreeNode<int>(0);
        root.AddChild(1);
        var child = root.Children[0];
        Assert.IsTrue(root.RemoveChild(child));
        Assert.IsFalse(root.RemoveChild(child));
        Assert.AreEqual(0, root.Children.Count);
    }

    [Test]
    public void Clear_RemovesAllChildren()
    {
        var root = new TreeNode<int>(0);
        root.AddChild(1);
        root.AddChild(2);
        Assert.AreEqual(2, root.Children.Count);
        root.Clear();
        Assert.AreEqual(0, root.Children.Count);
    }

    [Test]
    public void Depth_CalculatesLongestChain()
    {
        // root → child → grandchild = depth 3
        var root = new TreeNode<char>('a');
        root.AddChild('b');
        var child = root.Children[0];
        child.AddChild('c');
        Assert.AreEqual(3, root.Depth());
        // leaf alone has depth 1
        Assert.AreEqual(1, child.Children[0].Depth());
    }
}
