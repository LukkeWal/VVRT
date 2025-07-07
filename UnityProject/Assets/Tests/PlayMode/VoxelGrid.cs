// Play Mode tests for the ray casting level
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using _Project.Ray_Caster.Scripts.Voxel_Grid;

public class LevelRayCasting
{
    private GameObject go;
    VoxelGrid vg;
    // load the ray casting level
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return SceneManager.LoadSceneAsync("Ray Casting", LoadSceneMode.Single);
        yield return null;  // Wait one frame so all Awake/Start run
        go = new GameObject("TestVoxelGrid", typeof(VoxelGrid));
        vg = go.GetComponent<VoxelGrid>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Unload the scene (optional)
        yield return SceneManager.UnloadSceneAsync("Ray Casting");
    }

    [Test]
    public void DefaultSelectedVoxelGridDoneLoading_IsFalseForAllTypes()
    {
        // By default, no grid cache is set, so DoneLoading should be false
        foreach (VoxelGrid.VoxelGridType type in System.Enum.GetValues(typeof(VoxelGrid.VoxelGridType)))
        {
            vg.SelectedVoxelGrid = type;
            Assert.IsFalse(vg.SelectedVoxelGridDoneLoading(),
                $"Expected DoneLoading=false for {type}");
        }
    }

    [Test]
    public void SelectedVoxelGridDoneLoading_TrueAfterCacheInjection()
    {
        // Use reflection to set the private cache fields to a dummy non-null grid
        var dummyGrid = new float[1,1,1];
        foreach (VoxelGrid.VoxelGridType type in System.Enum.GetValues(typeof(VoxelGrid.VoxelGridType)))
        {
            // build field name e.g. "buckyGrid", "bunnyGrid"
            string typeName = type.ToString();
            string fieldName = char.ToLower(typeName[0]) + typeName.Substring(1) + "Grid";
            var field = typeof(VoxelGrid).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Field '{fieldName}' not found on VoxelGrid");

            // inject cache
            field.SetValue(vg, dummyGrid);
            vg.SelectedVoxelGrid = type;

            Assert.IsTrue(vg.SelectedVoxelGridDoneLoading(),
                $"Expected DoneLoading=true for {type} after cache set");

            // clear for next iteration
            field.SetValue(vg, null);
        }
    }

    [UnityTest]
    public IEnumerator TransformProperties_UpdateCorrectly()
    {
        var pos   = new Vector3(1, 2, 3);
        var rot   = new Vector3(10, 20, 30);
        var scale = new Vector3(2, 2, 2);

        vg.Position = pos;
        vg.Rotation = rot;
        vg.Scale    = scale;
        yield return null;

        // Assert position and scale exactly
        Assert.AreEqual(pos, vg.Position, "Position should match exactly");
        Assert.AreEqual(scale, vg.Scale, "Scale should match exactly");

        // Rotation round-trips through a Quaternion internally, so allow small epsilon
        const float eps = 0.01f;
        Assert.AreEqual(rot.x, vg.Rotation.x, eps, "Rotation.x within epsilon");
        Assert.AreEqual(rot.y, vg.Rotation.y, eps, "Rotation.y within epsilon");
        Assert.AreEqual(rot.z, vg.Rotation.z, eps, "Rotation.z within epsilon");
    }

    [Test]
    public void Get_ReturnsSingletonInstance()
    {
        Assert.AreEqual(vg, VoxelGrid.Get());
    }
}
