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
        private int _h0Kernel;
        private int _hktKernel;
        private int _fftRowKernel;
        private int _fftColKernel;

        public void ChangeWind(Vector2 wind) {
            H0.SetFloats("Wind", wind.x, wind.y);
            H0.Dispatch(_h0Kernel, Size / 16, Size / 16, 1);
        }

        //public RenderTexture hkt, dx, dz;
        private void Start() {
            Prepare();
            H0.Dispatch(_h0Kernel, Size / 16, Size / 16, 1);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", HktDtTargetR);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", StTargetR);
            GetComponent<Renderer>().sharedMaterial.SetInt("_HeightMapSize", Size);
        }

        public RenderTexture HktDtTargetR, HktDtTargetI, StTargetR, StTargetI;
        [HideInInspector] public RenderTexture HktDtTempR, HktDtTempI, StTempR, StTempI;

        private void Update() {
            Hkt.SetFloat("Time", Time.time);
            Hkt.Dispatch(_hktKernel, Size / 8, Size / 8, 1);

            FftRow.SetTexture(_fftRowKernel, "TextureSourceR", HktDtTargetR);
            FftRow.SetTexture(_fftRowKernel, "TextureSourceI", HktDtTargetI);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetR", HktDtTempR);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetI", HktDtTempI);
            FftRow.Dispatch(_fftRowKernel, 1, Size, 1);

            FftCol.SetTexture(_fftColKernel, "TextureSourceR", HktDtTempR);
            FftCol.SetTexture(_fftColKernel, "TextureSourceI", HktDtTempI);
            FftCol.SetTexture(_fftColKernel, "TextureTargetR", HktDtTargetR);
            FftCol.SetTexture(_fftColKernel, "TextureTargetI", HktDtTargetI);
            FftCol.Dispatch(_fftColKernel, 1, Size, 1);

            FftRow.SetTexture(_fftRowKernel, "TextureSourceR", StTargetR);
            FftRow.SetTexture(_fftRowKernel, "TextureSourceI", StTargetI);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetR", StTempR);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetI", StTempI);
            FftRow.Dispatch(_fftRowKernel, 1, Size, 1);

            FftCol.SetTexture(_fftColKernel, "TextureSourceR", StTempR);
            FftCol.SetTexture(_fftColKernel, "TextureSourceI", StTempI);
            FftCol.SetTexture(_fftColKernel, "TextureTargetR", StTargetR);
            FftCol.SetTexture(_fftColKernel, "TextureTargetI", StTargetI);
            FftCol.Dispatch(_fftColKernel, 1, Size, 1);

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
            _h0Kernel = H0.FindKernel("CSMain");
            //H0.SetFloat("Time", Time.realtimeSinceStartup);
            H0.SetTexture(_h0Kernel, "Result", _h0Tex);
            H0.SetInt("Size", Size);
            H0.SetFloat("Length", Length);
            H0.SetFloat("PhillipsSpectrum", PhillipsSpectrum);
            H0.SetFloats("Wind", Wind.x, Wind.y);
            H0.SetFloat("Gravity", Physics.gravity.magnitude);
            H0.Dispatch(_h0Kernel, Size / 8, Size / 8, 1);

            _hktKernel = Hkt.FindKernel("CSMain");
            Hkt.SetTexture(_hktKernel, "h0Tex", _h0Tex);
            Hkt.SetInt("Size", Size);
            Hkt.SetFloat("Length", Length);
            Hkt.SetFloat("Gravity", Physics.gravity.magnitude);

            HktDtTempR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTempR.Create();
            HktDtTempI = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {enableRandomWrite = true};
            HktDtTempI.Create();
            StTempR = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTempR.Create();
            StTempI = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {enableRandomWrite = true};
            StTempI.Create();

            HktDtTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Repeat
            };
            HktDtTargetR.Create();
            HktDtTargetI = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Repeat
            };
            HktDtTargetI.Create();
            StTargetR = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Repeat
            };
            StTargetR.Create();
            StTargetI = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Repeat
            };
            StTargetI.Create();

            Hkt.SetTexture(_hktKernel, "HktDtR", HktDtTargetR);
            Hkt.SetTexture(_hktKernel, "HktDtI", HktDtTargetI);
            Hkt.SetTexture(_hktKernel, "StR", StTargetR);
            Hkt.SetTexture(_hktKernel, "StI", StTargetI);

            _fftRowKernel = FftRow.FindKernel("Butterfly");

            _fftColKernel = FftCol.FindKernel("Butterfly");
        }
    }
}