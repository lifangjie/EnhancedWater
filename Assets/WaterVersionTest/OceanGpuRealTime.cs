using UnityEngine;

namespace WaterVersionTest {
    public class OceanGpuRealTime : MonoBehaviour {
        public int Size;
        public float Length;
        public float PhillipsSpectrum = 0.0005f;
        public ComputeShader H0;
        public ComputeShader Hkt;
        public ComputeShader Fft;
        public Vector2 Wind;

        public RenderTexture _h0Tex;
        private int _h0Kernal; // = H0.FindKernel("CSMain");
        private int _hktKernal; // = H0.FindKernel("CSMain");
        private int _fftKernal;

        public void ChangeWind(Vector2 wind) {
            H0.SetFloats("Wind", wind.x, wind.y);
            H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1);
        }

        //public RenderTexture hkt, dx, dz;
        private void Start() {
            Prepare();
            H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1);

        }

        public RenderTexture hkt, dx, dz, ButterflyTex;
        public RenderTexture TextureTargetR, TextureTargetI;

        private void Update() {
            Hkt.Dispatch(_hktKernal, Size / 8, Size / 8, 1);
            Fft.Dispatch(_fftKernal, Size/8, Size/8, 1);
        }


        private void Prepare() {
            _h0Tex = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            _h0Tex.Create();
            _h0Kernal = H0.FindKernel("CSMain");
            H0.SetTexture(_h0Kernal, "Result", _h0Tex);
            H0.SetInt("Size", Size);
            H0.SetFloat("Length", Length);
            H0.SetFloat("PhillipsSpectrum", PhillipsSpectrum);
            H0.SetFloats("Wind", Wind.x, Wind.y);
            H0.SetFloat("Gravity", Physics.gravity.magnitude);
            H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1);

            _hktKernal = Hkt.FindKernel("CSMain");
            Hkt.SetTexture(_hktKernal, "h0Tex", _h0Tex);
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
            Hkt.SetTexture(_hktKernal, "Hkt", hkt);
            Hkt.SetTexture(_hktKernal, "Dx", dx);
            Hkt.SetTexture(_hktKernal, "Dz", dz);
            TextureTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            TextureTargetR.Create();
            TextureTargetI = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            TextureTargetI.Create();
            ButterflyTex = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            ButterflyTex.Create();
            
            
            _fftKernal = Fft.FindKernel("Butterfly");
            Fft.SetFloat("Length", 16);
            Fft.SetTexture(_fftKernal, "TextureSourceR", hkt);
            Fft.SetTexture(_fftKernal, "TextureSourceI", dx);
            Fft.SetTexture(_fftKernal, "TextureTargetR", TextureTargetR);
            Fft.SetTexture(_fftKernal, "TextureTargetI", TextureTargetI);
            Fft.SetTexture(_fftKernal, "ButterflyTex", ButterflyTex);
        }
    }
}