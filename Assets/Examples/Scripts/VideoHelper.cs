using UnityEngine;

namespace HappyFunTimesExample {

///<summary>
///Class to make it easy to manage a rectangle of Color32 pixels
///</summary>
class PixelRect {
    public PixelRect(int width, int height)
    {
        Resize(width, height);
    }
    public PixelRect() {
    }

    ///<summary>
    ///Resizes the rect.
    ///
    ///Calling Resize with the same width and height that
    ///it already is has no effect.
    ///</summary>
    ///<param name="newWidth">width to make rect</param>
    ///<param name="newHeight">height to make rect</param>
    public void Resize(int newWidth, int newHeight) {
        if (newWidth != width || newHeight != height) {
            width = newWidth;
            height = newHeight;
            pixels = new Color32[width * height];
        }
    }

    public int width = 0;
    public int height = 0;
    public Color32[] pixels;
}

///<summary>
///Class to make it easy to manage a rectangle of ints pixels
///</summary>
class IntRect {
    public IntRect(int width, int height) {
        Resize(width, height);
    }
    public IntRect() {
    }

    ///<summary>
    ///Resizes the rect.
    ///
    ///Calling Resize with the same width and height that
    ///it already is has no effect.
    ///</summary>
    ///<param name="newWidth">width to make rect</param>
    ///<param name="newHeight">height to make rect</param>
    public void Resize(int newWidth, int newHeight)
    {
        if (newWidth != width || newHeight != height)
        {
            width = newWidth;
            height = newHeight;
            pixels = new int[width * height];
        }
    }


    ///<summary>
    ///Sets the contents from a PixelRect
    ///</summary>
    ///<param name="pixelRect">PixelRect to copy pixels from</param>
    public void Set(PixelRect pixelRect)
    {
        Resize(pixelRect.width, pixelRect.height);
        int len = width * height;
        for (int ii = 0; ii < len; ++ii)
        {
            Color32 c = pixelRect.pixels[ii];
            pixels[ii] = c.r << 16 | c.g << 8 | c.b;
        }
    }

    public int width;
    public int height;
    public int[] pixels;
}

class VideoHelper {

    public VideoHelper(int width, int height)
    {
        SetSize(width, height);
    }

    public void SetSize(int width, int height)
    {
        m_scaledPixels.Resize(width, height);
    }

    public void Update(WebCamTexture camTex)
    {
        m_videoPixels.Resize(camTex.width, camTex.height);
        camTex.GetPixels32(m_videoPixels.pixels);
        TextureScale.ThreadedScale(m_videoPixels.pixels, m_videoPixels.width, m_videoPixels.height,
                                   m_scaledPixels.pixels, m_scaledPixels.width, m_scaledPixels.height,
                                   true);
        m_intPixels.Set(m_scaledPixels);
    }

    public IntRect Get()
    {
        return m_intPixels;
    }

    private PixelRect m_videoPixels = new PixelRect();
    private PixelRect m_scaledPixels = new PixelRect();
    private IntRect m_intPixels = new IntRect();
};


}  // namespace HappyFunTimesExample

