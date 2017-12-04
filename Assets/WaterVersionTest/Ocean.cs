using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WaterVersionTest {
    //[ExecuteInEditMode]
    public class Ocean : MonoBehaviour {
        private Mesh _waterMesh;
        public int Size = 64;
        public float Length = 10;
        private Vector3[] _originalVertices;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _uvs;
        public Vector2 Wind = new Vector2(32, 32);
        public float PhillipsSpectrum = 0.0005f;

        private Complex[] _htilde0;
        private Complex[] _htilde0MkConj;

        // fft
        private FastFourierTransform _fastFourierTransform;

        private Complex[] _hTilde;

        private Complex[] _hTildeSlopex;
        private Complex[] _hTildeSlopez;
        private Complex[] _hTildeDx;

        private Complex[] _hTildeDz;

        public float Cycle = 1f;
        public int LayerCount = 8;
        private int _width, _height;

        public int SampleCount = 32;

        private void Start() {
            _width = Size * Size / LayerCount;
            _height = SampleCount * LayerCount;
            //_waterMesh = Resources.Load<Mesh>("mesh");
            //GetComponent<MeshFilter>().sharedMesh = _waterMesh;
            Texture2D verticesTex = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
            verticesTex.LoadRawTextureData(Resources.Load<TextAsset>("vertices").bytes);
            verticesTex.Apply();
            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", verticesTex);
            Texture2D normalsTex = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
            normalsTex.LoadRawTextureData(Resources.Load<TextAsset>("normals").bytes);
            normalsTex.Apply();
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", normalsTex);
            if (LayerCount > 1) {
                verticesTex.filterMode = FilterMode.Point;
                normalsTex.filterMode = FilterMode.Point;
            }
            Resources.UnloadUnusedAssets();
        }

        public Mesh UpdateMesh() {
            _width = Size * Size / LayerCount;
            _height = SampleCount * LayerCount;
            _waterMesh = GetComponent<MeshFilter>().sharedMesh;
            if (_waterMesh == null) {
                _waterMesh = new Mesh();
                GetComponent<MeshFilter>().sharedMesh = _waterMesh;
            }
            _waterMesh.name = "Ocean Mesh";
            int triangleIndex = 0;
            _originalVertices = new Vector3[Size * Size];
            int[] triangles = new int[(Size - 1) * (Size - 1) * 6];
            _vertices = new Vector3[Size * Size];
            _normals = new Vector3[Size * Size];
            _uvs = new Vector2[Size * Size];

            _htilde0 = new Complex[Size * Size];
            _htilde0MkConj = new Complex[Size * Size];


            for (int i = 0; i < Size; i++) {
                for (int j = 0; j < Size; j++) {
                    // set vertices
                    int index = i * Size + j;

                    _htilde0[index] = hTilde_0(j, i);
                    _htilde0MkConj[index] = hTilde_0(-j, -i).Conjugate();

                    _originalVertices[index].x = _vertices[index].x = (i - Size / 2) * Length;// / (Size - 1);
                    _originalVertices[index].y = _vertices[index].y = 0;
                    _originalVertices[index].z = _vertices[index].z = (j - Size / 2) * Length;// / (Size - 1);

                    _normals[index] = Vector3.up;
                    // set triangles
                    if (j < Size - 1 && i < Size - 1) {
                        triangles[triangleIndex + 0] = index;
                        triangles[triangleIndex + 1] = index + 1;
                        triangles[triangleIndex + 2] = index + Size + 1;

                        triangles[triangleIndex + 3] = index + Size + 1;
                        triangles[triangleIndex + 4] = index + Size;
                        triangles[triangleIndex + 5] = index;

                        triangleIndex += 6;
                    }
                    // set uv
                    //_uvs[index].x = (i + 0.5f) / Size;
                    _uvs[index].x = (i + 0.5f - Size/2f) * Length;// / Size;
                    _uvs[index].y = (j + 0.5f - Size/2f) * Length;// / Size;
                    //_uvs[index].y = (j + 0.5f) / Size;
                }
            }
            _waterMesh.Clear();
            _waterMesh.vertices = _vertices;
            _waterMesh.uv = _uvs;
            _waterMesh.normals = _normals;
            _waterMesh.triangles = triangles;
            _waterMesh.RecalculateTangents();
            //return Instantiate(_waterMesh);
            return _waterMesh;
        }

        public bool Baking;
        public int Progress;
        public ComputeShader ComputeShader;

        public IEnumerator BakeIntoTexture() {
            Baking = true;
            Progress = 0;
            print("Begin baking, Size: " + Size);
            yield return null;
            _fastFourierTransform = new FastFourierTransform(Size);
            _hTilde = new Complex[Size * Size];
            _hTildeSlopex = new Complex[Size * Size];
            _hTildeSlopez = new Complex[Size * Size];
            _hTildeDx = new Complex[Size * Size];
            _hTildeDz = new Complex[Size * Size];
//            Texture2D verticesTexture = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
//            Texture2D normalsTexture = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
//            for (int frame = 0; frame < SampleCount; frame++) {
//                EvaluateWavesFft(Cycle / SampleCount * frame);
//                for (int i = 0; i < Size * Size; i++) {
//                    verticesTexture.SetPixel(i % _width, i / _width * SampleCount + frame,
//                        new Color(_vertices[i].x, _vertices[i].y, _vertices[i].z));
//                    normalsTexture.SetPixel(i % _width, i / _width * SampleCount + frame,
//                        new Color(_normals[i].x, _normals[i].y, _normals[i].z));
//                }
//                Progress = frame;
//                yield return null;
//            }
//            verticesTexture.Apply();
//            normalsTexture.Apply();
//            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", verticesTexture);
//            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", normalsTexture);
//            byte[] verticesBytes = verticesTexture.GetRawTextureData();
//            FileStream fileStream = File.Open(Application.dataPath + "/Resources/vertices.bytes", FileMode.Create);
//            fileStream.Write(verticesBytes, 0, verticesBytes.Length);
//            fileStream.Flush();
//            fileStream.Close();
//            byte[] normalsBytes = normalsTexture.GetRawTextureData();
//            fileStream = File.Open(Application.dataPath + "/Resources/normals.bytes", FileMode.Create);
//            fileStream.Write(normalsBytes, 0, normalsBytes.Length);
//            fileStream.Flush();
//            fileStream.Close();
            //Texture2D htile0Tex = new Texture2D(Size, Size, TextureFormat.RGBAHalf, false);
//            for (int i = 0; i < Size; i++) {
//                for (int j = 0; j < Size; j++) {
////                    uint wang_hash(uint seed)
////                    {
//                    uint seed = (uint) (i * Size + j);
//                        seed = (seed ^ 61) ^ (seed >> 16);
//                        seed *= 9;
//                        seed = seed ^ (seed >> 4);
//                        seed *= 0x27d4eb2d;
//                        seed = seed ^ (seed >> 15);
//                        seed = 1664525 * seed + 1013904223;
//                    float temp = seed / (float) uint.MaxValue;
//                      //  print(seed/(float)uint.MaxValue);
////                    }
//                    //Complex temp = _htilde0[i * Size + j];
//                    //htile0Tex.SetPixel(i, j, new Color(temp.Real, temp.Imaginary, 0));
//                    Complex gaussian = new Complex(1, 1).Multiply(Phillips(j, i));
//                    htile0Tex.SetPixel(i, j, new Color(GaussianRandomVariable().Real, GaussianRandomVariable().Imaginary, 0));
//                }
//            }
//            htile0Tex.Apply();
            RenderTexture htile0Tex =
                new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGBHalf) {enableRandomWrite = true};
            htile0Tex.Create();
            int kernelHandler = ComputeShader.FindKernel("CSMain");
            ComputeShader.SetTexture(kernelHandler, "Result", htile0Tex);
            ComputeShader.SetInt("Size", Size);
            ComputeShader.SetFloat("Length", Length);
            ComputeShader.SetFloat("PhillipsSpectrum", PhillipsSpectrum);
            ComputeShader.SetFloats("Wind", Wind.x, Wind.y);
            ComputeShader.SetFloat("Gravity", Physics.gravity.magnitude);
            //ComputeBuffer buffer = new ComputeBuffer(Size * Size, 1, );

            ComputeShader.Dispatch(kernelHandler, Size / 8, Size / 8, 1);
            //htile0Tex.Apply();
            GetComponent<Renderer>().sharedMaterial.SetTexture("_htile0Tex", htile0Tex);
//            Texture2D temp = new Texture2D(Size, Size, TextureFormat.RGBAHalf, false);
//            RenderTexture.active = htile0Tex;
//            temp.ReadPixels(new Rect(0, 0, Size, Size),0 ,0 );
//            temp.Apply();
//            byte[] htile0Bytes = temp.EncodeToEXR();
//            FileStream fileStream = File.Open(Application.dataPath + "/Resources/htile0.exr", FileMode.Create);
//            fileStream.Write(htile0Bytes, 0, htile0Bytes.Length);
//            fileStream.Flush();
//            fileStream.Close();
            Progress = SampleCount;
            Baking = false;
            print("Baking done, sample count " + SampleCount);
//            if (LayerCount > 1) {
//                verticesTexture.filterMode = FilterMode.Point;
//                normalsTexture.filterMode = FilterMode.Point;
//            }
        }

        private void EvaluateWavesFft(float time) {
            float lambda = -1.0f;
            int index;

            for (int i = 0; i < Size; i++) {
                var kz = Mathf.PI * (2 * i - Size + 1) / Length;
                for (int j = 0; j < Size; j++) {
                    var kx = Mathf.PI * (2 * j - Size + 1) / Length;
                    var len = Mathf.Sqrt(kx * kx + kz * kz);
                    index = i * Size + j;

                    _hTilde[index] = HTilde(time, i, j);
                    _hTildeSlopex[index] = _hTilde[index] * new Complex(0, kx);
                    _hTildeSlopez[index] = _hTilde[index] * new Complex(0, kz);
                    if (len < 0.000001f) {
                        _hTildeDx[index] = new Complex();
                        _hTildeDz[index] = new Complex();
                    } else {
                        _hTildeDx[index] = _hTilde[index] * new Complex(0, -kx / len);
                        _hTildeDz[index] = _hTilde[index] * new Complex(0, -kz / len);
                    }
                }
            }

            for (int i = 0; i < Size; i++) {
                _fastFourierTransform.Fft(_hTilde, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeSlopex, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeSlopez, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeDx, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeDz, 1, i * Size);
            }
            for (int i = 0; i < Size; i++) {
                _fastFourierTransform.Fft(_hTilde, Size, i);
                _fastFourierTransform.Fft(_hTildeSlopex, Size, i);
                _fastFourierTransform.Fft(_hTildeSlopez, Size, i);
                _fastFourierTransform.Fft(_hTildeDx, Size, i);
                _fastFourierTransform.Fft(_hTildeDz, Size, i);
            }

            int[] signs = {1, -1};
            for (int i = 0; i < Size; i++) {
                for (int j = 0; j < Size; j++) {
                    index = i * Size + j; // index into h_tilde..

                    var sign = signs[(j + i) & 1];

                    _hTilde[index] = _hTilde[index].Multiply(sign);

                    // height
                    _vertices[index].y = _hTilde[index].Real;

                    // displacement
                    _hTildeDx[index] = _hTildeDx[index].Multiply(sign);
                    _hTildeDz[index] = _hTildeDz[index].Multiply(sign);
                    _vertices[index].x = _originalVertices[index].x + _hTildeDx[index].Real * lambda;
                    _vertices[index].z = _originalVertices[index].z + _hTildeDz[index].Real * lambda;

                    // normal
                    _hTildeSlopex[index] = _hTildeSlopex[index].Multiply(sign);
                    _hTildeSlopez[index] = _hTildeSlopez[index].Multiply(sign);
                    _normals[index] = new Vector3(-_hTildeSlopex[index].Real, 1f, -_hTildeSlopez[index].Real)
                        .normalized;
                }
            }
            for (int i = 0; i < Size; i++) {
                index = (Size - 1) * Size + i;
                _vertices[index].x = _originalVertices[index].x + _hTildeDx[i].Real * lambda;
                _vertices[index].y = _hTilde[i].Real;
                _vertices[index].z = _originalVertices[index].z + _hTildeDz[i].Real * lambda;
                _normals[index] = _normals[i];
                index = i * Size;
                _vertices[index + Size - 1].x = _originalVertices[index + Size - 1].x + _hTildeDx[index].Real * lambda;
                _vertices[index + Size - 1].y = _hTilde[index].Real;
                _vertices[index + Size - 1].z = _originalVertices[index + Size - 1].z + _hTildeDz[index].Real * lambda;
                _normals[index + Size - 1] = _normals[index];
            }
            index = Size * Size - 1;
            _vertices[index].x = _originalVertices[index].x + _hTildeDx[0].Real * lambda;
            _vertices[index].y = _hTilde[0].Real;
            _vertices[index].z = _originalVertices[index].z + _hTildeDz[0].Real * lambda;
            _normals[index] = _normals[0];
        }

        Complex GaussianRandomVariable() {
            float x1, x2, w;
            do {
                x1 = 2f * Random.value - 1f;
                x2 = 2f * Random.value - 1f;
                w = x1 * x1 + x2 * x2;
            } while (w >= 1f);
            w = Mathf.Sqrt((-2f * Mathf.Log(w)) / w);
            return new Complex(x1 * w, x2 * w);
        }

        float Phillips(int i, int j) {
            Vector2 k = new Vector2(Mathf.PI * (2 * i - Size) / Length, Mathf.PI * (2 * j - Size) / Length);
            float kLength = k.magnitude;
            if (kLength < 0.000001) return 0f;

            float kLength2 = kLength * kLength;
            float kLength4 = kLength2 * kLength2;

            float kDotW = Vector2.Dot(k.normalized, Wind.normalized);
            float kDotW2 = kDotW * kDotW; // * kDotW * kDotW * kDotW * kDotW;

            float wLength = Wind.magnitude;
            float l = wLength * wLength / Physics.gravity.magnitude;
            float l2 = l * l;

            float damping = 0.001f;
            float l2Damping2 = l2 * damping * damping;

            return PhillipsSpectrum * Mathf.Exp(-1.0f / (kLength2 * l2)) / kLength4 * kDotW2 *
                   Mathf.Exp(-kLength2 * l2Damping2);
        }

        float Dispersion(int i, int j) {
            float w0 = 2.0f * Mathf.PI / 200; //Cycle;
            float kx = Mathf.PI * (2 * i - Size) / Length;
            float kz = Mathf.PI * (2 * j - Size) / Length;
            //return Mathf.Floor(Mathf.Sqrt(Physics.gravity.magnitude * Mathf.Sqrt(kx * kx + kz * kz)) / w0) * w0;
            Vector2 k = new Vector2(Mathf.PI * (2 * i - Size) / Length, Mathf.PI * (2 * j - Size) / Length);
            return Mathf.Sqrt(Physics.gravity.magnitude * k.magnitude);
        }

        Complex hTilde_0(int i, int j) {
            Complex r = GaussianRandomVariable();
            return r.Multiply(Mathf.Sqrt(Phillips(i, j) / 2.0f));
        }

        Complex HTilde(float time, int i, int j) {
            int index = i * Size + j;

            Complex htilde0 = _htilde0[index]; //.Real, _htilde0[index].Imaginary);
            Complex htilde0Mkconj = _htilde0MkConj[index]; //.Real, _htilde0MkConj[index].Imaginary);

            float omegat = Dispersion(i, j) * time;

            float cos = Mathf.Cos(omegat);
            float sin = Mathf.Sin(omegat);

            Complex c0 = new Complex(cos, sin);
            Complex c1 = new Complex(cos, -sin);
            //return htilde0.Multiply(temp0)  + htilde0Mkconj.Multiply(temp1);

            return htilde0 * c0 + htilde0Mkconj * c1;
        }
    }
}