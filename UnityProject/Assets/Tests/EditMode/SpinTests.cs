// Assets/Tests/EditMode/SpinTests.cs
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpinTests
{
    private Spin spinComp;
    private FieldInfo spinField;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("SpinTester");
        spinComp = go.AddComponent<Spin>();
        // reflect private 'spin' flag :contentReference[oaicite:4]{index=4}
        spinField = typeof(Spin).GetField("spin", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(spinField);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(spinComp.gameObject);
    }

    [Test]
    public void SpinToggle_FlipsInternalFlag()
    {
        // default false
        Assert.IsFalse((bool)spinField.GetValue(spinComp));

        // toggle on
        spinComp.spinToggle();
        Assert.IsTrue((bool)spinField.GetValue(spinComp));

        // toggle off
        spinComp.spinToggle();
        Assert.IsFalse((bool)spinField.GetValue(spinComp));
    }
}
