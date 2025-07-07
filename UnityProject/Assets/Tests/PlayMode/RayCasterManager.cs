using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RayCasterManagerAndUnityRayCasterReflectionTests
{
    private Type managerType;
    private Component managerInstance;

    private Type casterType;
    private Component casterInstance;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Ignore any internal exceptions thrown during Awake/Start
        LogAssert.ignoreFailingMessages = true;

        // 1) Locate types in loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        managerType = assemblies
            .Select(a => a.GetType("_Project.Ray_Caster.Scripts.RayCasterManager", false))
            .FirstOrDefault(t => t != null);
        Assert.NotNull(managerType, "Could not find RayCasterManager type");

        casterType = assemblies
            .Select(a => a.GetType("_Project.Ray_Caster.Scripts.UnityRayCaster", false))
            .FirstOrDefault(t => t != null);
        Assert.NotNull(casterType, "Could not find UnityRayCaster type");

        // 2) Create and add VoxelGrid to satisfy dependencies
        var vgGO = new GameObject("VG", typeof(_Project.Ray_Caster.Scripts.Voxel_Grid.VoxelGrid));
        yield return null;

        // 3) Create and add UnityRayCaster
        var casterGO = new GameObject("URC");
        casterInstance = (Component)casterGO.AddComponent(casterType);
        yield return null;
        // Manually assign static instance field
        var casterField = casterType.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        casterField.SetValue(null, casterInstance);

        // 4) Create and add RayCasterManager
        var managerGO = new GameObject("RCM");
        managerInstance = (Component)managerGO.AddComponent(managerType);
        yield return null;
        // Manually assign static instance field
        var managerField = managerType.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        managerField.SetValue(null, managerInstance);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        LogAssert.ignoreFailingMessages = false;
        if (managerInstance != null) UnityEngine.Object.Destroy(managerInstance.gameObject);
        if (casterInstance != null) UnityEngine.Object.Destroy(casterInstance.gameObject);
        var vg = GameObject.Find("VG");
        if (vg != null) UnityEngine.Object.Destroy(vg);
        yield return null;
    }

    [Test]
    public void Manager_Get_ReturnsInstance()
    {
        var getMI = managerType.GetMethod("Get", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(getMI, "Could not find Get() on RayCasterManager");
        var instance = getMI.Invoke(null, null);
        Assert.AreSame(managerInstance, instance, "RayCasterManager.Get() should return the added instance");
    }

    [Test]
    public void Manager_ShowPreview_Property_Toggles()
    {
        var prop = managerType.GetProperty("ShowPreview", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(prop, "Could not find ShowPreview property");
        bool original = (bool)prop.GetValue(managerInstance);
        prop.SetValue(managerInstance, !original);
        bool now = (bool)prop.GetValue(managerInstance);
        Assert.AreEqual(!original, now, "ShowPreview property should toggle");
    }

    [Test]
    public void Caster_Get_ReturnsInstance()
    {
        var getMI = casterType.GetMethod("Get", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(getMI, "Could not find Get() on UnityRayCaster");
        var instance = getMI.Invoke(null, null);
        Assert.AreSame(casterInstance, instance, "UnityRayCaster.Get() should return the added instance");
    }

    [Test]
    public void Caster_DoRayTermination_Property_Toggles()
    {
        var prop = casterType.GetProperty("DoRayTermination", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(prop, "Could not find DoRayTermination property");
        bool original = (bool)prop.GetValue(casterInstance);
        prop.SetValue(casterInstance, !original);
        bool now = (bool)prop.GetValue(casterInstance);
        Assert.AreEqual(!original, now, "DoRayTermination property should toggle");
    }

    [Test]
    public void Caster_DistanceBetweenSamples_Property_Updates()
    {
        var prop = casterType.GetProperty("DistanceBetweenSamples", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(prop, "Could not find DistanceBetweenSamples property");
        float original = (float)prop.GetValue(casterInstance);
        float testVal = original + 0.123f;
        prop.SetValue(casterInstance, testVal);
        float now = (float)prop.GetValue(casterInstance);
        Assert.AreEqual(testVal, now, 1e-6f, "DistanceBetweenSamples property should update");
    }
}
