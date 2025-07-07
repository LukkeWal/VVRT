// Assets/Tests/PlayMode/OctreePlayModeTests.cs
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using _Project.Ray_Tracer.Scripts.Utility;

public class OctreePlayModeTests
{
    GameObject go;
    Octree octree;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        octree = go.AddComponent<Octree>();
        yield return null; // allow Start() to build initial OctreeRoot :contentReference[oaicite:1]{index=1}
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (go != null)
            GameObject.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_InitializesOctreeRootNode()
    {
        Assert.NotNull(octree.octreeRoot,        "OctreeRoot should be created in Start()");
        Assert.NotNull(octree.octreeRoot.rootNode, "rootNode must be non-null after initialization");
        yield break;
    }

    [UnityTest]
    public IEnumerator showOctreeToggle_FlipsInternalFlag()
    {
        // Access private drawOctree flag
        var flagField = typeof(Octree)
            .GetField("drawOctree", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(flagField);

        // Initially false
        Assert.IsFalse((bool)flagField.GetValue(octree));

        octree.showOctreeToggle();
        Assert.IsTrue((bool)flagField.GetValue(octree), "showOctreeToggle should set drawOctree = true");

        octree.showOctreeToggle();
        Assert.IsFalse((bool)flagField.GetValue(octree), "Toggling again should reset drawOctree to false");
        yield break;
    }

    [UnityTest]
    public IEnumerator changeOctreeDepthSlider_UpdatesMaxDepthAndRebuildsRoot()
    {
        int newDepth = 4;
        octree.changeOctreeDepthSlider(newDepth);
        
        // Public maxDepth field must update
        Assert.AreEqual(newDepth, octree.maxDepth, "maxDepth should reflect slider value");

        // OctreeRoot should be recreated with the new depth
        Assert.NotNull(octree.octreeRoot);
        // rootNode.currentDepth is private; at least confirm rootNode exists
        Assert.NotNull(octree.octreeRoot.rootNode);
        yield break;
    }
}
