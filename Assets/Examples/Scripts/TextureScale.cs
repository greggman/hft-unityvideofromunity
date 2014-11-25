// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable

using System.Threading;
using UnityEngine;

public class TextureScale
{
    public class ThreadData
    {
        public int start;
        public int end;
        public ThreadData (int s, int e) {
            start = s;
            end = e;
        }
    }

    private static Color32[] srcColors;
    private static Color32[] dstColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    private static Mutex mutex;

    public static void Point (WebCamTexture src, Texture2D dst, int newWidth, int dstHeight)
    {
        ThreadedScale (src, dst, newWidth, dstHeight, false);
    }

    public static void Bilinear (WebCamTexture src, Texture2D dst, int newWidth, int dstHeight)
    {
        ThreadedScale (src, dst, newWidth, dstHeight, true);
    }

    public static void ThreadedScale (WebCamTexture src, Texture2D dst, int dstWidth, int dstHeight, bool useBilinear)
    {
        Color32[] srcColors = src.GetPixels32();
        Color32[] dstColors = new Color32[dstWidth * dstHeight];

        ThreadedScale(srcColors, src.width, src.height, dstColors, dstWidth, dstHeight, useBilinear);

        if (dst.width != dstWidth || dst.height != dstHeight) {
            dst.Resize(dstWidth, dstHeight);
        }
        dst.SetPixels32(dstColors);
        dst.Apply();
    }

    public static void ThreadedScale (Color32[] src, int srcWidth, int srcHeight, Color32[] dst, int dstWidth, int dstHeight, bool useBilinear)
    {
        srcColors = src;
        dstColors = dst;

        if (useBilinear)
        {
            ratioX = 1.0f / ((float)dstWidth / (srcWidth-1));
            ratioY = 1.0f / ((float)dstHeight / (srcHeight-1));
        }
        else {
            ratioX = ((float)srcWidth) / dstWidth;
            ratioY = ((float)srcHeight) / dstHeight;
        }
        w = srcWidth;
        w2 = dstWidth;
        var cores = Mathf.Min(SystemInfo.processorCount, dstHeight);
        var slice = dstHeight/cores;

        finishCount = 0;
        if (mutex == null) {
            mutex = new Mutex(false);
        }
        if (cores > 1)
        {
            int i = 0;
            ThreadData threadData;
            for (i = 0; i < cores-1; i++) {
                threadData = new ThreadData(slice * i, slice * (i + 1));
                ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                Thread thread = new Thread(ts);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice*i, dstHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
            while (finishCount < cores)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            ThreadData threadData = new ThreadData(0, dstHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
        }
    }

    public static void BilinearScale (System.Object obj)
    {
        ThreadData threadData = (ThreadData) obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            int yFloor = (int)Mathf.Floor(y * ratioY);
            var y1 = yFloor * w;
            var y2 = (yFloor+1) * w;
            var yw = y * w2;

            for (var x = 0; x < w2; x++) {
                int xFloor = (int)Mathf.Floor(x * ratioX);
                var xLerp = x * ratioX-xFloor;
                dstColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(srcColors[y1 + xFloor], srcColors[y1 + xFloor+1], xLerp),
                                                       ColorLerpUnclamped(srcColors[y2 + xFloor], srcColors[y2 + xFloor+1], xLerp),
                                                       y*ratioY-yFloor);
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    public static void PointScale (System.Object obj)
    {
        ThreadData threadData = (ThreadData) obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            var thisY = (int)(ratioY * y) * w;
            var yw = y * w2;
            for (var x = 0; x < w2; x++) {
                dstColors[yw + x] = srcColors[(int)(thisY + ratioX*x)];
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    private static Color32 ColorLerpUnclamped (Color32 c1, Color32 c2, float value)
    {
        return new Color32(System.Convert.ToByte(c1.r + (c2.r - c1.r)*value),
                           System.Convert.ToByte(c1.g + (c2.g - c1.g)*value),
                           System.Convert.ToByte(c1.b + (c2.b - c1.b)*value),
                           System.Convert.ToByte(c1.a + (c2.a - c1.a)*value));
    }
}
