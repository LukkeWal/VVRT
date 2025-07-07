// Assets/Tests/PlayMode/AABBPlayModeTests.cs
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using _Project.Ray_Tracer.Scripts.Utility;

public class AABBPlayModeTests
{
    GameObject cube;
    AABB aabb;
    FieldInfo hitSphereField;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Create a simple cube (has MeshRenderer)
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        aabb = cube.AddComponent<AABB>();
        yield return null; // let Start() run and cache meshRenderer & bounds

        // Access private hitpointSphere field
        hitSphereField = typeof(AABB)
            .GetField("hitpointSphere", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(hitSphereField, "Could not reflect hitpointSphere field"); 
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (cube != null)
            GameObject.Destroy(cube);
        yield return null;
    }

    [UnityTest]
public IEnumerator showAABBToggle_CreatesAndDestroysHitpointSphere()
{
    // Arrange
    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    var aabb = cube.AddComponent<AABB>();
    yield return null; // let Start() cache bounds

    // reflect private hitpointSphere field
    var hitSphereField = typeof(AABB)
        .GetField("hitpointSphere", BindingFlags.Instance | BindingFlags.NonPublic);
    Assert.NotNull(hitSphereField);

    // Act: enable both AABB drawing and hit‐point drawing
    aabb.hitpoint     = new Vector3(1, 1, 1);
    aabb.drawHitpoint = true;
    aabb.showAABBToggle(); // drawAABB = true
    yield return null;     // Update() spawns the sphere

    // Assert it exists at the right spot
    var sphere = (GameObject)hitSphereField.GetValue(aabb);
    Assert.IsTrue(sphere != null && sphere.transform.position == aabb.hitpoint);

    // Act: toggle off and wait another frame for Destroy() to take effect
    aabb.showAABBToggle(); // drawAABB = false
    yield return null;     // Update() calls Destroy()
    yield return null;     // end-of-frame destruction

    // Assert: UnityEngine.Object == null when destroyed :contentReference[oaicite:0]{index=0}
    sphere = (GameObject)hitSphereField.GetValue(aabb);
    Assert.IsTrue(sphere == null, "hitpointSphere should be destroyed when drawAABB is false");

    Object.Destroy(cube);
}
}
