using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using _Project.Ray_Caster.Scripts.RC_Ray;

public class SampleObjectPoolAndRendererPlayModeTests
{
    [UnityTest]
    public IEnumerator SampleRenderer_UpdatesTransformAndMaterial()
    {
        // Arrange: Create GameObject with required components
        var go = new GameObject("TestSampleRenderer", typeof(MeshFilter), typeof(MeshRenderer));
        var renderer = go.AddComponent<SampleRenderer>();
        yield return null; // wait for Awake()

        // Act & Assert: Origin sets position
        var testOrigin = new Vector3(1f, 2f, 3f);
        renderer.Origin = testOrigin;
        Assert.AreEqual(testOrigin, go.transform.position);

        // Radius sets localScale
        const float testRadius = 0.5f;
        renderer.Radius = testRadius;
        Assert.AreEqual(new Vector3(testRadius, testRadius, testRadius), go.transform.localScale);

        // MyColor sets material color
        var testColor = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        renderer.MyColor = testColor;
        var meshRenderer = go.GetComponent<MeshRenderer>();
        Assert.AreEqual(testColor, meshRenderer.material.color);

        // Cleanup
        Object.Destroy(go);
    }
}
