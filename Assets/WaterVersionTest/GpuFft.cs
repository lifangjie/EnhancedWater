using System.IO;
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
            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", HktDtTargetR);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", StTargetR);
            GetComponent<Renderer>().sharedMaterial.SetInt("_HeightMapSize", Size);
        }

        public RenderTexture HktDtTargetR, HktDtTargetI, StTargetR, StTargetI;
        public RenderTexture HktDtTempR, HktDtTempI, StTempR, StTempI;

        private void Update() {
            
            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", HktDtTargetR);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", StTargetR);
            GetComponent<Renderer>().sharedMaterial.SetInt("_HeightMapSize", Size);
            //H0.Dispatch(_h0Kernal, Size / 8, Size / 8, 1); 
            Hkt.SetFloat("Time", Time.time);
            Hkt.Dispatch(_hktKernal, Size / 8, Size / 8, 1);
            
            FftRow.SetTexture(_fftRowKernal, "TextureSourceR", HktDtTargetR);
            FftRow.SetTexture(_fftRowKernal, "TextureSourceI", HktDtTargetI);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetR", HktDtTempR);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetI", HktDtTempI);
            FftRow.Dispatch(_fftRowKernal, 1, Size, 1);
            
            FftCol.SetTexture(_fftColKernal, "TextureSourceR", HktDtTempR);
            FftCol.SetTexture(_fftColKernal, "TextureSourceI", HktDtTempI);
            FftCol.SetTexture(_fftColKernal, "TextureTargetR", HktDtTargetR);
            FftCol.SetTexture(_fftColKernal, "TextureTargetI", HktDtTargetI);
            FftCol.Dispatch(_fftColKernal, 1, Size, 1);
            
            FftRow.SetTexture(_fftRowKernal, "TextureSourceR", StTargetR);
            FftRow.SetTexture(_fftRowKernal, "TextureSourceI", StTargetI);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetR", StTempR);
            FftRow.SetTexture(_fftRowKernal, "TextureTargetI", StTempI);
            FftRow.Dispatch(_fftRowKernal, 1, Size, 1);
            
            FftCol.SetTexture(_fftColKernal, "TextureSourceR", StTempR);
            FftCol.SetTexture(_fftColKernal, "TextureSourceI", StTempI);
            FftCol.SetTexture(_fftColKernal, "TextureTargetR", StTargetR);
            FftCol.SetTexture(_fftColKernal, "TextureTargetI", StTargetI);
            FftCol.Dispatch(_fftColKernal, 1, Size, 1);
//            RenderTexture.active = TextureTargetR;
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

            HktDtTempR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTempR.Create();
            HktDtTempI= new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTempI.Create();
            StTempR = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTempR.Create();
            StTempI= new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTempI.Create();
            
            HktDtTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTargetR.Create();
            HktDtTargetI= new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTargetI.Create();
            StTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTargetR.Create();
            StTargetI= new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTargetI.Create();
            
            Hkt.SetTexture(_hktKernal, "HktDtR", HktDtTargetR);
            Hkt.SetTexture(_hktKernal, "HktDtI", HktDtTargetI);
            Hkt.SetTexture(_hktKernal, "StR", StTargetR);
            Hkt.SetTexture(_hktKernal, "StI", StTargetI);

            _fftRowKernal= FftRow.FindKernel("Butterfly");

            _fftColKernal = FftCol.FindKernel("Butterfly");
        }
    }
}