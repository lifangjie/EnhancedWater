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
            H0.Dispatch(_h0Kernel, Size / 8, Size / 8, 1);
        }

        //public RenderTexture hkt, dx, dz;
        

        public Texture2D h0Test;
        private void Start() {
            Prepare();
            H0.Dispatch(_h0Kernel, Size / 8, Size / 8, 1);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", HktDtTargetR);
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", StTargetR);
            GetComponent<Renderer>().sharedMaterial.SetInt("_HeightMapSize", Size);
            h0Test = new Texture2D(516, 513, TextureFormat.RGFloat, false);
            Vector2[] out_h0 = new Vector2[516 * 513];
//            for (int i = 0; i < 516*513; i++) {
//                out_h0[i] = Vector2.zero;
//            }
            float[] omega = new float[516 * 513];
            InitHeightMap(new OceanParameter(), ref out_h0, ref omega);
            for (int i = 0; i < 516 * 513; i++) {
                h0Test.SetPixel(i / 516, i % 516, new Color(out_h0[i].x, out_h0[i].y, 0, 0));
            }
            h0Test.Apply();
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


        private float Gauss() {
            float u1 = Random.value;
            float u2 = Random.value;
            if (u1 < 1e-6f)
                u1 = 1e-6f;
            return Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
        }

        // Phillips Spectrum
        // K: normalized wave vector, W: wind direction, v: wind velocity, a: amplitude constant
        private float Phillips(Vector2 K, Vector2 W, float v, float a, float dir_depend) {
            // largest possible wave from constant wind of velocity v
            float l = v * v / (Physics.gravity.magnitude * 100);
            // damp out waves with very small length w << l
            float w = l / 1000;

            float Ksqr = K.x * K.x + K.y * K.y;
            float Kcos = K.x * W.x + K.y * W.y;
            float phillips = a * Mathf.Exp(-1 / (l * l * Ksqr)) / (Ksqr * Ksqr * Ksqr) * (Kcos * Kcos);

            // filter out waves moving opposite to wind
            if (Kcos < 0)
                phillips *= dir_depend;

            // damp out waves with very small length w << l
            return phillips * Mathf.Exp(-Ksqr * w * w);
        }

        public class OceanParameter {
            // Must be power of 2.
            public int dmap_dim = 512;

            // Typical value is 1000 ~ 2000
            public float patch_length = 2000;

            // Adjust the time interval for simulation.
            public float time_scale = 0.8f;

            // Amplitude for transverse wave. Around 1.0
            public float wave_amplitude = 0.35f;

            // Wind direction. Normalization not required.
            public Vector2 wind_dir = new Vector2(0.8f, 0.6f);

            // Around 100 ~ 1000
            public float wind_speed = 600f;

            // This value damps out the waves against the wind direction.
            // Smaller value means higher wind dependency.
            public float wind_dependency = 0.07f;

            // The amplitude for longitudinal wave. Must be positive.
            public float choppy_scale = 1.3f;
        };

        private const float HALF_SQRT_2 = 0.7071068f;

        private void InitHeightMap(OceanParameter oceanParameter, ref Vector2[] out_h0, ref float[] out_omega) {
            int i, j;
            Vector2 K, Kn;

            Vector2 wind_dir = oceanParameter.wind_dir.normalized;
            float a = oceanParameter.wave_amplitude * 1e-7f; // It is too small. We must scale it for editing.
            float v = oceanParameter.wind_speed;
            float dir_depend = oceanParameter.wind_dependency;

            int height_map_dim = oceanParameter.dmap_dim;
            float patch_length = oceanParameter.patch_length;

            // initialize random generator.

            for (i = 0; i <= height_map_dim; i++) {
                // K is wave-vector, range [-|DX/W, |DX/W], [-|DY/H, |DY/H]
                K.y = (-height_map_dim / 2.0f + i) * (2 * Mathf.PI / patch_length);

                for (j = 0; j <= height_map_dim; j++) {
                    K.x = (-height_map_dim / 2.0f + j) * (2 * Mathf.PI / patch_length);

                    float phil = (K.x == 0 && K.y == 0) ? 0 : Mathf.Sqrt(Phillips(K, wind_dir, v, a, dir_depend));

                    out_h0[i * (height_map_dim + 4) + j].x = phil * Gauss() * HALF_SQRT_2;
                    out_h0[i * (height_map_dim + 4) + j].y = phil * Gauss() * HALF_SQRT_2;

                    // The angular frequency is following the dispersion relation:
                    //            out_omega^2 = g*k
                    // The equation of Gerstner wave:
                    //            x = x0 - K/k * A * sin(dot(K, x0) - sqrt(g * k) * t), x is a 2D vector.
                    //            z = A * cos(dot(K, x0) - sqrt(g * k) * t)
                    // Gerstner wave shows that a point on a simple sinusoid wave is doing a uniform circular
                    // motion with the center (x0, y0, z0), radius A, and the circular plane is parallel to
                    // vector K.
                    out_omega[i * (height_map_dim + 4) + j] =
                        Mathf.Sqrt((Physics.gravity.magnitude * 100) * Mathf.Sqrt(K.x * K.x + K.y * K.y));
                }
            }
        }
    }
}