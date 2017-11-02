using UnityEngine;

namespace WaterVersionTest {
    public class GpuFft : MonoBehaviour {
        public int Size;
        public float Length;
        public float PhillipsSpectrum = 0.0005f;
        public ComputeShader H0;
        public ComputeShader Hkt;
        public ComputeShader FftRow;
        public ComputeShader FftCol;
        public Vector2 Wind;

        public RenderTexture _h0Tex;
        private int _h0Kernal; // = H0.FindKernel("CSMain");
        private int _hktKernal; // = H0.FindKernel("CSMain");
        private int _fftRowKernal;
        private int _fftColKernal;

        public void ChangeWind(Vector2 wind) {
            H0.SetFloats("Wind", wind.x, wind.y);
            H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1);
        }

        //public RenderTexture hkt, dx, dz;
        private void Start() {
            Prepare();
            H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1);
        }

        public RenderTexture hkt, dx, dz;
        public RenderTexture TextureSourceR, TextureSourceI;
        public RenderTexture TextureTargetR, TextureTargetI;

        private void Update() {
            //H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1); 
            Hkt.SetFloat("Time", Time.time);
            Hkt.Dispatch(_hktKernal, Size / 8, Size / 8, 1);
            FftRow.Dispatch(_fftRowKernal, 1, Size, 1);
            FftCol.Dispatch(_fftColKernal, 1, Size, 1);
//            RenderTexture.active = _h0Tex;
//            Texture2D temp = new Texture2D(Size, Size, TextureFormat.RGBAFloat, false);
//            temp.ReadPixels(new Rect(0, 0, Size, Size),0 ,0 );
//            temp.Apply();
//            byte[] htile0Bytes = temp.EncodeToEXR();
//            FileStream fileStream = File.Open(Application.dataPath + "/Resources/displacements.exr", FileMode.Create);
//            fileStream.Write(htile0Bytes, 0, htile0Bytes.Length);
//            fileStream.Flush();
//            fileStream.Close();
//            Debug.Break();
            //float rand_1_05(in float2 uv)
            //{
//                float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
//                return abs(noise.x + noise.y) * 0.5;
            //}
        }


        private void Prepare() {
            _h0Tex = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            _h0Tex.Create();
            _h0Kernal = H0.FindKernel("CSMain");
            //H0.SetFloat("Time", Time.realtimeSinceStartup);
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
            hkt = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            hkt.Create();
            dx = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            dx.Create();
            dz = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            dz.Create();
            Hkt.SetTexture(_hktKernal, "Hkt", hkt);
            Hkt.SetTexture(_hktKernal, "Dx", dx);
            Hkt.SetTexture(_hktKernal, "Dz", dz);
            
            TextureSourceR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            TextureSourceR.Create();
            TextureSourceI = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            TextureSourceI.Create();
            TextureTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            TextureTargetR.Create();
            TextureTargetI = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            TextureTargetI.Create();
            //ButterflyTex = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            //ButterflyTex.Create();


            _fftRowKernal= FftRow.FindKernel("Butterfly");
            FftRow.SetTexture(_fftRowKernal, "hkt", hkt);
            FftRow.SetTexture(_fftRowKernal, "dx", dx);
            FftRow.SetTexture(_fftRowKernal, "dz", dz);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetR", TextureSourceR);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetI", TextureSourceI);

            _fftColKernal = FftCol.FindKernel("Butterfly");
            FftCol.SetTexture(_fftColKernal, "TextureSourceR", TextureSourceR);
            FftCol.SetTexture(_fftColKernal, "TextureSourceI", TextureSourceI);
            FftCol.SetTexture(_fftColKernal, "TextureTargetR", TextureTargetR);
            FftCol.SetTexture(_fftColKernal, "TextureTargetI", TextureTargetI);
            //Fft.SetTexture(_fftKernal, "ButterflyTex", ButterflyTex);
        }
    }
}