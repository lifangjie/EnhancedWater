using UnityEngine;

namespace WaterVersionTest {
    public class GpuFftTest : MonoBehaviour {
        public ComputeShader HktTest;
        private int _hktKernel;
        public Texture2D H0PlusOmega;
        public RenderTexture Hkt, Dx, Dy;

        private void Start() {
            H0PlusOmega = new Texture2D(Size, Size, TextureFormat.RGBAFloat, false);
            Color[] h0PlusOmega = new Color[Size * Size];
            InitHeightMap(ref h0PlusOmega);
            H0PlusOmega.SetPixels(h0PlusOmega);
            H0PlusOmega.Apply();

            Hkt = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {
                enableRandomWrite = true
            };
            Dx = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {
                enableRandomWrite = true
            };
            Dy = new RenderTexture(Size, Size, 0, RenderTextureFormat.RGFloat) {
                enableRandomWrite = true
            };
            _hktKernel = HktTest.FindKernel("UpdateSpectrumCS");
            HktTest.SetInt("Size", Size);
            HktTest.SetTexture(_hktKernel, "H0PlusOmega", H0PlusOmega);
            HktTest.SetTexture(_hktKernel, "Hkt", Hkt);
            HktTest.SetTexture(_hktKernel, "Dx", Dx);
            HktTest.SetTexture(_hktKernel, "Dy", Dy);
        }

        private void Update() {
            HktTest.SetFloat("Time", Time.time);
            HktTest.Dispatch(_hktKernel, Size / 16, Size / 16, 1);
        }


        // Must be power of 2.
        public int Size = 512;

        // Typical value is 1000 ~ 2000
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