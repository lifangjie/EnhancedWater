using UnityEngine;

namespace WaterVersionTest {
    public class GpuFftTest : MonoBehaviour {
        public ComputeShader HktTest;
        private int _hktKernel;
        public ComputeShader FftRow, FftCol;
        private int _fftRowKernel, _fftColKernel;
        public ComputeShader GenerateDisplacement;
        private int _generateDisplacementKernelStep1;
        private int _generateDisplacementKernelStep2;

        private Texture2D _h0PlusOmega;

        //public RenderTexture Hkt, Dx, Dy;
        private RenderTexture _hktReal, _hktImagination;

        private RenderTexture _tempReal, _tempImagination;

        public RenderTexture OutputReal, OutputImagination;

        private Renderer _renderer;
        private int _perlinMovementId;
        private void Start() {
            CreateSurfaceMesh ctx = new CreateSurfaceMesh();
            ctx.Create();
            _h0PlusOmega = new Texture2D(Size, Size, TextureFormat.RGBAFloat, false);
            Color[] h0PlusOmega = new Color[Size * Size];
            InitHeightMap(ref h0PlusOmega);
            _h0PlusOmega.SetPixels(h0PlusOmega);
            _h0PlusOmega.Apply();
            
            _hktReal = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            _hktReal.Create();
            _hktImagination = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            _hktImagination.Create();
            _tempReal = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            _tempReal.Create();
            _tempImagination = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            _tempImagination.Create();
            OutputReal = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            OutputReal.Create();
            OutputImagination = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBFloat) {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat
            };
            OutputImagination.Create();
            _hktKernel = HktTest.FindKernel("UpdateSpectrumCS");
            HktTest.SetInt("Size", Size);
            HktTest.SetTexture(_hktKernel, "H0PlusOmega", _h0PlusOmega);
//            HktTest.SetTexture(_hktKernel, "Hkt", Hkt);
//            HktTest.SetTexture(_hktKernel, "Dx", Dx);
//            HktTest.SetTexture(_hktKernel, "Dy", Dy);
            HktTest.SetTexture(_hktKernel, "HktReal", _hktReal);
            HktTest.SetTexture(_hktKernel, "HktImagination", _hktImagination);

            _fftRowKernel = FftRow.FindKernel("Butterfly");
            _fftColKernel = FftCol.FindKernel("Butterfly");

            FftRow.SetTexture(_fftRowKernel, "TextureSourceR", _hktReal);
            FftRow.SetTexture(_fftRowKernel, "TextureSourceI", _hktImagination);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetR", _tempReal);
            FftRow.SetTexture(_fftRowKernel, "TextureTargetI", _tempImagination);

            FftCol.SetTexture(_fftColKernel, "TextureSourceR", _tempReal);
            FftCol.SetTexture(_fftColKernel, "TextureSourceI", _tempImagination);
            FftCol.SetTexture(_fftColKernel, "TextureTargetR", OutputReal);
            FftCol.SetTexture(_fftColKernel, "TextureTargetI", OutputImagination);

            _generateDisplacementKernelStep1 = GenerateDisplacement.FindKernel("CSMain");
            _generateDisplacementKernelStep2 = GenerateDisplacement.FindKernel("CSMain2");
            
            _renderer = GetComponent<Renderer>();
            //_perlinMovementId = _renderer.sharedMaterial.GetInt("PerlinMovement");
            _renderer.sharedMaterial.SetTexture("DisplacementTexture", OutputImagination);
        }

        private void Update() {
            HktTest.SetFloat("Time", Time.time);
            HktTest.Dispatch(_hktKernel, Size / 16, Size / 16, 1);
            FftRow.Dispatch(_fftRowKernel, 1, Size, 1);
            FftCol.Dispatch(_fftColKernel, 1, Size, 1);
            GenerateDisplacement.SetFloat("ChoppyScale", ChoppyScale);
            GenerateDisplacement.SetFloat("GridLen", 0.256f);
            GenerateDisplacement.SetTexture(_generateDisplacementKernelStep1, "Input", OutputReal);
            GenerateDisplacement.SetTexture(_generateDisplacementKernelStep1, "Result", OutputImagination);
            GenerateDisplacement.Dispatch(_generateDisplacementKernelStep1, Size/16, Size/16, 1);
            GenerateDisplacement.SetTexture(_generateDisplacementKernelStep2, "Input", OutputImagination);
            GenerateDisplacement.SetTexture(_generateDisplacementKernelStep2, "Result", OutputReal);
            GenerateDisplacement.Dispatch(_generateDisplacementKernelStep2, Size/16, Size/16, 1);
            
            Vector3 perlinMovement = -WindDir * Time.time * PerlinSpeed;
            _renderer.sharedMaterial.SetVector("PerlinMovement", perlinMovement);
        }

        public float PerlinSize = 1.0f;
        public float PerlinSpeed = 0.06f;
//        public Vector3 PerlinAmplitude = new Vector3(35, 42, 57);
//        public Vector3 PerlinGradient = new Vector3(1.4f, 1.6f, 2.2f);
//        public Vector3 PerlinOctave = new Vector3(1.12f, 0.59f, 0.23f);

        // Must be power of 2.
        public int Size = 512;

        // yTpical value  1is000 ~ 2000
        public float Length = 2000;

        // Adjust the time interval for simulation.
        public float TimeScale = 0.8f;

        // Amplitude for transverse wave. Around 1.0
        public float WaveAmplitude = 0.35f;

        // Wind direction. Normalization not required.
        public Vector2 WindDir = new Vector2(0.8f, 0.6f);

        // Around 100 ~ 1000
        public float WindSpeed = 600f;

        // This value damps out the waves against the wind direction.
        // Smaller value means higher wind dependency.
        public float WindDependency = 0.07f;

        // The amplitude for longitudinal wave. Must be positive.
        public float ChoppyScale = 1.3f;

        private const float HalfSqrt2 = 0.7071068f;

        private void InitHeightMap(ref Color[] h0PlusOmega) {
            Vector2 windDir = WindDir.normalized;
            for (int i = 0; i < Size; i++) {
                // K is wave-vector, range [-|DX/W, |DX/W], [-|DY/H, |DY/H]
                Vector2 k;
                k.y = (-Size / 2.0f + i) * (2 * Mathf.PI / Length);

                for (int j = 0; j < Size; j++) {
                    k.x = (-Size / 2.0f + j) * (2 * Mathf.PI / Length);

                    float phil = 0;
                    if (Mathf.Abs(k.x) > 1e-6 || Mathf.Abs(k.y) > 1e-6) {
                        phil = Mathf.Sqrt(Phillips(k, windDir, WindSpeed, WaveAmplitude * 1e-7f, WindDependency));
                    }

                    //out_h0[i * (Size + 4) + j].x = phil * Gauss() * HALF_SQRT_2;
                    //out_h0[i * (Size + 4) + j].y = phil * Gauss() * HALF_SQRT_2;
                    h0PlusOmega[i * (Size + 0) + j].r = phil * Gauss() * HalfSqrt2;
                    h0PlusOmega[i * (Size + 0) + j].g = phil * Gauss() * HalfSqrt2;

                    // The angular frequency is following the dispersion relation:
                    //            out_omega^2 = g*k
                    // The equation of Gerstner wave:
                    //            x = x0 - K/k * A * sin(dot(K, x0) - sqrt(g * k) * t), x is a 2D vector.
                    //            z = A * cos(dot(K, x0) - sqrt(g * k) * t)
                    // Gerstner wave shows that a point on a simple sinusoid wave is doing a uniform circular
                    // motion with the center (x0, y0, z0), radius A, and the circular plane is parallel to
                    // vector K.
                    h0PlusOmega[i * (Size + 0) + j].b =
                        Mathf.Sqrt((Physics.gravity.magnitude * 100) * Mathf.Sqrt(k.x * k.x + k.y * k.y));
                }
            }
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
        private float Phillips(Vector2 k, Vector2 windDir, float v, float a, float dirDepend) {
            // largest possible wave from constant wind of velocity v
            float l = v * v / (Physics.gravity.magnitude * 100);
            // damp out waves with very small length w << l
            float w = l / 1000;

            float ksqr = k.x * k.x + k.y * k.y;
            float kcos = k.x * windDir.x + k.y * windDir.y;
            float phillips = a * Mathf.Exp(-1 / (l * l * ksqr)) / (ksqr * ksqr * ksqr) * (kcos * kcos);

            // filter out waves moving opposite to wind
            if (kcos < 0)
                phillips *= dirDepend;

            // damp out waves with very small length w << l
            return phillips * Mathf.Exp(-ksqr * w * w);
        }
    }
}