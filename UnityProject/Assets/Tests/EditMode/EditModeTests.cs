using NUnit.Framework;
using UnityEngine;
using _Project.Ray_Caster.Scripts.RC_Ray;
using _Project.Ray_Caster.Scripts.Voxel_Grid;

public class LinearInterpolationTests
{
    private LinearInterpolation interp;

    [SetUp]
    public void SetUp()
    {
        interp = new LinearInterpolation();
    }

    [Test]
    public void MonoLinear_ReturnsExactLower_WhenPointAtLowerDensity()
    {
        // Arrange
        var table = new RCRay.ColorTableEntry[] {
            new RCRay.ColorTableEntry(Color.red,   0f),
            new RCRay.ColorTableEntry(Color.blue,  1f)
        };

        // Act
        var result = interp.MonoLinearInterpolation(0f, table);

        // Assert
        Assert.AreEqual(Color.red, result);
    }

    [Test]
    public void MonoLinear_ReturnsExactUpper_WhenPointAtUpperDensity()
    {
        var table = new RCRay.ColorTableEntry[] {
            new RCRay.ColorTableEntry(Color.red,   0f),
            new RCRay.ColorTableEntry(Color.blue,  1f)
        };

        var result = interp.MonoLinearInterpolation(1f, table);
        Assert.AreEqual(Color.blue, result);
    }

    [Test]
    public void MonoLinear_InterpolatesMidpoint_Correctly()
    {
        var table = new RCRay.ColorTableEntry[] {
            new RCRay.ColorTableEntry(Color.red,   0f),
            new RCRay.ColorTableEntry(Color.blue,  1f)
        };

        var result = interp.MonoLinearInterpolation(0.5f, table);
        // halfway between red (1,0,0,1) and blue (0,0,1,1) is (0.5,0,0.5,1)
        Assert.AreEqual(0.5f, result.r, 1e-6f);
        Assert.AreEqual(0f,   result.g, 1e-6f);
        Assert.AreEqual(0.5f, result.b, 1e-6f);
        Assert.AreEqual(1f,   result.a, 1e-6f);
    }

    [Test]
    public void TriLinear_CornerValues_Correct()
    {
        // Create a simple 2x2x2 voxel grid
        var go = new GameObject();
        var vg = go.AddComponent<VoxelGrid>();
        vg.SizeX = vg.SizeY = vg.SizeZ = 2;
        vg.Grid = new float[2,2,2];
        vg.Grid[0,0,0] = 0f;
        vg.Grid[1,0,0] = 1f;
        vg.Grid[0,1,0] = 0f;
        vg.Grid[1,1,0] = 0f;
        vg.Grid[0,0,1] = 0f;
        vg.Grid[1,0,1] = 0f;
        vg.Grid[0,1,1] = 0f;
        vg.Grid[1,1,1] = 0f;

        // At point (1,0,0) expect value 1
        float v = interp.TriLinearInterpolation(new Vector3(1f, 0f, 0f), vg);
        Assert.AreEqual(1f, v, 1e-6f);
    }

    [Test]
    public void TriLinear_InterpolateEdge_Correct()
    {
        var go = new GameObject();
        var vg = go.AddComponent<VoxelGrid>();
        vg.SizeX = vg.SizeY = vg.SizeZ = 2;
        vg.Grid = new float[2,2,2];
        vg.Grid[0,0,0] = 0f;
        vg.Grid[1,0,0] = 1f;
        vg.Grid[0,1,0] = 1f;
        vg.Grid[1,1,0] = 0f;
        vg.Grid[0,0,1] = 0f;
        vg.Grid[1,0,1] = 0f;
        vg.Grid[0,1,1] = 0f;
        vg.Grid[1,1,1] = 0f;

        // At point (0.5, 0.5, 0), interpolation on bottom face:
        // corners: (0,0)=0, (1,0)=1, (0,1)=1, (1,1)=0 → bilinear at (0.5,0.5) = 0.5
        float v = interp.TriLinearInterpolation(new Vector3(0.5f, 0.5f, 0f), vg);
        Assert.AreEqual(0.5f, v, 1e-6f);
    }
}
