using UnityEngine;

namespace WaterVersionTest {
    public class OceanGpuRealTime : MonoBehaviour {
        public int Size;
        public float Length;
        public float PhillipsSpectrum = 0.0005f;
        public ComputeShader H0;
        public ComputeShader Hkt;
        public Vector2 Wind;

        private void Start() {
            RenderTexture h0Tex =
                new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            h0Tex.Create();
            int kernelHandler = H0.FindKernel("CSMain");
            H0.SetTexture(kernelHandler, "Result", h0Tex);
            H0.SetInt("Size", Size);
            H0.SetFloat("Length", Length);
            H0.SetFloat("PhillipsSpectrum", PhillipsSpectrum);
            H0.SetFloats("Wind", Wind.x, Wind.y);
            H0.SetFloat("Gravity", Physics.gravity.magnitude);
            H0.Dispatch(kernelHandler, Size / 8, Size / 8, 1);

            kernelHandler = Hkt.FindKernel("CSMain");
            Hkt.SetTexture(kernelHandler, "h0Tex", h0Tex);
            Hkt.SetInt("Size", Size);
            Hkt.SetFloat("Length", Length);
            Hkt.SetFloat("Gravity", Physics.gravity.magnitude);
            // temp
            hkt = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGHalf) {enableRandomWrite = true};
            hkt.Create();
            dx = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGHalf) {enableRandomWrite = true};
            dx.Create();
            dz = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGHalf) {enableRandomWrite = true};
            dz.Create();
            Hkt.SetTexture(kernelHandler, "Hkt", hkt);
            Hkt.SetTexture(kernelHandler, "Dx", dx);
            Hkt.SetTexture(kernelHandler, "Dz", dz);
            Hkt.Dispatch(kernelHandler, Size / 8, Size / 8, 1);
        }

        public RenderTexture hkt, dx, dz;
    }
}