// Assets/Tests/EditMode/RTImageTests.cs
using NUnit.Framework;
using UnityEngine;
using _Project.Ray_Tracer.Scripts.Utility;

[TestFixture]
public class RTImageTests
{
    [Test]
    public void Constructor_SetsWidthHeightAndPixelsBlack()
    {
        var img = new RTImage(2, 3);
        Assert.AreEqual(2, img.Width);
        Assert.AreEqual(3, img.Height);
        Assert.AreEqual(2 * 3, img.Pixels.Length);
        foreach (var px in img.Pixels)
            Assert.AreEqual(Color.black, px);
    }

    [Test]
    public void Constructor_ClampsNegativeDimensionsToZero()
    {
        var img = new RTImage(-1, 5);
        Assert.AreEqual(0, img.Width);
        Assert.AreEqual(5, img.Height);
        Assert.AreEqual(0, img.Pixels.Length);
    }

    [Test]
    public void PixelsSetter_IgnoresTooSmallArrays()
    {
        var img = new RTImage(2, 2);
        var before = (Color[])img.Pixels.Clone();
        img.Pixels = new Color[3];  // smaller than 4 :contentReference[oaicite:0]{index=0}
        Assert.AreEqual(4, img.Pixels.Length);
        CollectionAssert.AreEqual(before, img.Pixels);
    }

    [Test]
    public void PixelsSetter_CopiesOnlyFirstWidthTimesHeight()
    {
        var img = new RTImage(2, 2);
        // Make an array of 10 distinct colors
        var source = new Color[10];
        for (int i = 0; i < source.Length; i++)
            source[i] = new Color(i/10f, i/10f, i/10f, i/10f);
        img.Pixels = source;       // should copy only first 4 entries :contentReference[oaicite:1]{index=1}
        for (int i = 0; i < 4; i++)
            Assert.AreEqual(source[i], img.Pixels[i]);
    }

    [Test]
    public void Reset_ChangesDimensionsAndFiresEvent()
    {
        var img = new RTImage(1, 1);
        bool invoked = false;
        img.OnImageChanged += () => invoked = true;

        img.Reset(3, 2);
        Assert.IsTrue(invoked);
        Assert.AreEqual(3, img.Width);
        Assert.AreEqual(2, img.Height);
        Assert.AreEqual(3 * 2, img.Pixels.Length);
        foreach (var px in img.Pixels)
            Assert.AreEqual(Color.black, px);
    }

    [Test]
    public void FromByteArray_ReturnsNullForInvalidLength()
    {
        // for width*height*3 = 12 required, give only 5
        var bad = RTImage.FromByteArray(new byte[5], 2, 2, alpha: false);
        Assert.IsNull(bad);
    }

    [Test]
    public void FromByteArray_CreatesCorrectImage_RGB()
    {
        // 2×1 image → 2 pixels, 3 channels each → 6 bytes
        byte[] data = {
            255,   0,   0,   // red
              0, 255,   0    // green
        };
        var img = RTImage.FromByteArray(data, 2, 1, alpha: false);
        Assert.NotNull(img);
        Assert.AreEqual(2, img.Width);
        Assert.AreEqual(1, img.Height);
        Assert.AreEqual(new Color(1f, 0f, 0f, 1f), img.Pixels[0]);
        Assert.AreEqual(new Color(0f, 1f, 0f, 1f), img.Pixels[1]);
    }

    [Test]
    public void FromByteArray_CreatesCorrectImage_RGBA()
    {
        // 1×1 image → 4 channels → 4 bytes
        byte[] data = { 128,  64,  32, 255 };
        var img = RTImage.FromByteArray(data, 1, 1, alpha: true);
        Assert.NotNull(img);
        Assert.AreEqual(1, img.Width);
        Assert.AreEqual(1, img.Height);
        var c = img.Pixels[0];
        Assert.AreEqual(128/255f, c.r, 1e-6f);
        Assert.AreEqual( 64/255f, c.g, 1e-6f);
        Assert.AreEqual( 32/255f, c.b, 1e-6f);
        Assert.AreEqual(255/255f, c.a, 1e-6f);
    }
}
