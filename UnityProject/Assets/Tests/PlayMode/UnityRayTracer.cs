// Assets/Tests/PlayMode/UnityRayTracerPlayModeTests.cs
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UnityRayTracerPlayModeTests
{
    private Type tracerType;
    private Component tracerComp;
    private MethodInfo getMI;
    private EventInfo onChangedEvent;
    private PropertyInfo epsilonProp;
    private PropertyInfo renderShadowsProp;
    private PropertyInfo maxDepthProp;
    private PropertyInfo superSamplingFactorProp;
    private PropertyInfo superSamplingVisualProp;
    private PropertyInfo backgroundColorProp;
    private Camera mainCam;
    private int eventCount;
    private MethodInfo onChangedCallbackMI;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // 1) Find the UnityRayTracer type
        tracerType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == "_Project.Ray_Tracer.Scripts.UnityRayTracer");
        Assert.NotNull(tracerType, "Could not find UnityRayTracer type");

        // 2) Create a MainCamera for BackgroundColor setter
        var camGO = new GameObject("MainCamera", typeof(Camera));
        camGO.tag = "MainCamera";
        mainCam = camGO.GetComponent<Camera>();

        // 3) Add the ray tracer component and let Awake/Start run
        var go = new GameObject("RayTracerGO");
        tracerComp = (Component)go.AddComponent(tracerType);
        yield return null;

        // 4) Cache reflection handles
        getMI                   = tracerType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static);
        onChangedEvent          = tracerType.GetEvent("OnRayTracerChanged", BindingFlags.Instance | BindingFlags.Public);
        epsilonProp             = tracerType.GetProperty("Epsilon",              BindingFlags.Instance | BindingFlags.Public);
        renderShadowsProp       = tracerType.GetProperty("RenderShadows",        BindingFlags.Instance | BindingFlags.Public);
        maxDepthProp            = tracerType.GetProperty("MaxDepth",             BindingFlags.Instance | BindingFlags.Public);
        superSamplingFactorProp = tracerType.GetProperty("SuperSamplingFactor",  BindingFlags.Instance | BindingFlags.Public);
        superSamplingVisualProp = tracerType.GetProperty("SuperSamplingVisual",  BindingFlags.Instance | BindingFlags.Public);
        backgroundColorProp     = tracerType.GetProperty("BackgroundColor",      BindingFlags.Instance | BindingFlags.Public);

        // 5) Locate our private callback stub
        onChangedCallbackMI = GetType().GetMethod(
            nameof(OnRayTracerChangedCallback),
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        // Final checks
        Assert.NotNull(getMI,               "Could not find Get()");
        Assert.NotNull(onChangedEvent,      "Could not find OnRayTracerChanged event");
        Assert.NotNull(epsilonProp,         "Could not find Epsilon property");
        Assert.NotNull(backgroundColorProp, "Could not find BackgroundColor property");
        Assert.NotNull(onChangedCallbackMI, "Could not find OnRayTracerChangedCallback method");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Clean up
        if (tracerComp != null)
            UnityEngine.Object.Destroy(tracerComp.gameObject);
        if (mainCam != null)
            UnityEngine.Object.Destroy(mainCam.gameObject);
        yield return null;
    }

    [Test]
    public void Get_ReturnsSingletonInstance()
    {
        var instance = getMI.Invoke(null, null);
        Assert.AreSame(tracerComp, instance,
            "UnityRayTracer.Get() should return the instance created in the scene");
    }

    /// <summary>
    /// Clears any existing subscribers, resets the counter, then adds exactly one handler.
    /// </summary>
    private void Subscribe()
    {
        // Clear out any previous handlers on this instance
        var backingField = tracerType.GetField(
            "OnRayTracerChanged",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (backingField != null)
        {
            backingField.SetValue(tracerComp, null);
        }

        // Reset our counter
        eventCount = 0;

        // Create a delegate of the correct event-handler type
        var handler = Delegate.CreateDelegate(
            onChangedEvent.EventHandlerType,
            this,
            onChangedCallbackMI
        );
        onChangedEvent.AddEventHandler(tracerComp, handler);
    }

    // Matches: public delegate void RayTracerChanged();
    private void OnRayTracerChangedCallback()
    {
        eventCount++;
    }

    [Test]
    public void SettingEpsilon_FiresOnRayTracerChanged()
    {
        float orig = (float)epsilonProp.GetValue(tracerComp);
        Subscribe();
        epsilonProp.SetValue(tracerComp, orig + 0.5f);
        Assert.AreEqual(1, eventCount, "Changing Epsilon should invoke OnRayTracerChanged exactly once");
    }

    [Test]
    public void SettingRenderShadows_FiresOnRayTracerChanged()
    {
        bool orig = (bool)renderShadowsProp.GetValue(tracerComp);
        Subscribe();
        renderShadowsProp.SetValue(tracerComp, !orig);
        Assert.AreEqual(1, eventCount, "Changing RenderShadows should invoke OnRayTracerChanged exactly once");
    }

    [Test]
    public void SettingMaxDepth_FiresOnRayTracerChanged()
    {
        int orig = (int)maxDepthProp.GetValue(tracerComp);
        Subscribe();
        maxDepthProp.SetValue(tracerComp, orig + 1);
        Assert.AreEqual(1, eventCount, "Changing MaxDepth should invoke OnRayTracerChanged exactly once");
    }

    [Test]
    public void SettingSuperSamplingFactor_FiresOnRayTracerChanged()
    {
        int orig = (int)superSamplingFactorProp.GetValue(tracerComp);
        Subscribe();
        superSamplingFactorProp.SetValue(tracerComp, orig + 1);
        Assert.AreEqual(1, eventCount, "Changing SuperSamplingFactor should invoke OnRayTracerChanged exactly once");
    }

    [Test]
    public void SettingSuperSamplingVisual_FiresOnRayTracerChanged()
    {
        bool orig = (bool)superSamplingVisualProp.GetValue(tracerComp);
        Subscribe();
        superSamplingVisualProp.SetValue(tracerComp, !orig);
        Assert.AreEqual(1, eventCount, "Changing SuperSamplingVisual should invoke OnRayTracerChanged exactly once");
    }

    [UnityTest]
    public IEnumerator SettingBackgroundColor_UpdatesCameraAndFiresEvent()
    {
        Subscribe();
        var newColor = Color.magenta;
        backgroundColorProp.SetValue(tracerComp, newColor);
        yield return null; // allow setter to update Camera.main.backgroundColor

        Assert.AreEqual(newColor, mainCam.backgroundColor,
            "BackgroundColor setter should update Camera.main.backgroundColor");
        Assert.AreEqual(1, eventCount,
            "Changing BackgroundColor should invoke OnRayTracerChanged exactly once");
    }
}
