using System.Collections;
using System.IO;
using UnityEngine;

namespace WaterVersionTest {
    //[ExecuteInEditMode]
    public class OceanRealTime : MonoBehaviour {
        private Mesh _waterMesh;
        public int Size = 64;
        private int _sizePlus1;
        public float Length = 10;
        private Vector3[] _originalVertices;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        //private Vector2[] _uvs;
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

//            _width = Size * Size / LayerCount;
//            _height = SampleCount * LayerCount;
//            Texture2D verticesTex = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
//            verticesTex.LoadRawTextureData(Resources.Load<TextAsset>("vertices").bytes);
//            verticesTex.Apply();
//            GetComponent<Renderer>().sharedMaterial.SetTexture("_VerticesTex", verticesTex);
//            Texture2D normalsTex = new Texture2D(_width, _height, TextureFormat.RGBAHalf, false);
//            normalsTex.LoadRawTextureData(Resources.Load<TextAsset>("normals").bytes);
//            normalsTex.Apply();
//            GetComponent<Renderer>().sharedMaterial.SetTexture("_NormalsTex", normalsTex);
//            if (LayerCount > 1) {
//                verticesTex.filterMode = FilterMode.Point;
//                normalsTex.filterMode = FilterMode.Point;
//            }
//            Resources.UnloadUnusedAssets();

        private void UpdateMesh() {
            _sizePlus1 = Size + 1;
            _width = Size * Size / LayerCount;
            _height = SampleCount * LayerCount;
            _waterMesh = GetComponent<MeshFilter>().sharedMesh;
            if (_waterMesh == null) {
                _waterMesh = new Mesh();
                GetComponent<MeshFilter>().sharedMesh = _waterMesh;
            }
            _waterMesh.name = "Ocean Mesh";
            int triangleIndex = 0;
            _originalVertices = new Vector3[_sizePlus1 * _sizePlus1];
            int[] triangles = new int[Size * Size * 6];
            _vertices = new Vector3[_sizePlus1 * _sizePlus1];
            _normals = new Vector3[_sizePlus1 * _sizePlus1];
            //_uvs = new Vector2[Size * Size];

            _htilde0 = new Complex[_sizePlus1 * _sizePlus1];
            _htilde0MkConj = new Complex[_sizePlus1 * _sizePlus1];


            for (int j = 0; j < _sizePlus1; j++) {
                for (int i = 0; i < _sizePlus1; i++) {
                    // set vertices
                    int index = j * _sizePlus1 + i;

                    _htilde0[index] = hTilde_0(i, j);
                    _htilde0MkConj[index] = hTilde_0(-i, -j).Conjugate();

                    _originalVertices[index].x = _vertices[index].x = (i - Size / 2) * Length / Size;
                    _originalVertices[index].y = _vertices[index].y = 0;
                    _originalVertices[index].z = _vertices[index].z = (j - Size / 2) * Length / Size;

                    _normals[index] = Vector3.up;
                    // set triangles
                    if (j < Size && i < Size) {
                        triangles[triangleIndex + 0] = index;
                        triangles[triangleIndex + 1] = index + _sizePlus1;
                        triangles[triangleIndex + 2] = index + _sizePlus1+ 1;

                        triangles[triangleIndex + 3] = index;
                        triangles[triangleIndex + 4] = index + _sizePlus1+ 1;
                        triangles[triangleIndex + 5] = index + 1;

                        triangleIndex += 6;
                    }
                    // set uv
                    //_uvs[index].x = (index % _width * 1f + 0.5f) / _width;
                    //_uvs[index].y = (Mathf.Floor(index * 1f / _width) * SampleCount + 0.5f) / _height;
                }
            }
            _waterMesh.Clear();
            _waterMesh.vertices = _vertices;
            //_waterMesh.uv = _uvs;
            _waterMesh.normals = _normals;
            _waterMesh.triangles = triangles;
        }

        private void Start() {
            _fastFourierTransform = new FastFourierTransform(Size);
            _hTilde = new Complex[Size * Size];
            _hTildeSlopex = new Complex[Size * Size];
            _hTildeSlopez = new Complex[Size * Size];
            _hTildeDx = new Complex[Size * Size];
            _hTildeDz = new Complex[Size * Size];
            UpdateMesh();
        }

        private void Update() {
            EvaluateWavesFft(Time.time);
            _waterMesh.vertices = _vertices;
        }


        private void EvaluateWavesFft(float time) {
            float lambda = -1.0f;
            int index;

            for (int j = 0; j < Size; j++) {
                var kz = Mathf.PI * (2 * j - Size) / Length;
                for (int i = 0; i < Size; i++) {
                    var kx = Mathf.PI * (2 * i - Size) / Length;
                    var len = Mathf.Sqrt(kx * kx + kz * kz);
                    index = j * Size + i;

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
                _fastFourierTransform.Fft(_hTilde, _hTilde, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeSlopex, _hTildeSlopex, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeSlopez, _hTildeSlopez, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeDx, _hTildeDx, 1, i * Size);
                _fastFourierTransform.Fft(_hTildeDz, _hTildeDz, 1, i * Size);
            }
            for (int i = 0; i < Size; i++) {
                _fastFourierTransform.Fft(_hTilde, _hTilde, Size, i);
                _fastFourierTransform.Fft(_hTildeSlopex, _hTildeSlopex, Size, i);
                _fastFourierTransform.Fft(_hTildeSlopez, _hTildeSlopez, Size, i);
                _fastFourierTransform.Fft(_hTildeDx, _hTildeDx, Size, i);
                _fastFourierTransform.Fft(_hTildeDz, _hTildeDz, Size, i);
            }

            int[] signs = {1, -1};
            for (int j = 0; j < Size; j++) {
                for (int i = 0; i < Size; i++) {
                    index = j * Size + i; // index into h_tilde..
                    var index1 = j * _sizePlus1 + i;

                    var sign = signs[(j + i) & 1];

                    _hTilde[index] = _hTilde[index].Multiply(sign);

                    // height
                    _vertices[index1].y = _hTilde[index].Real;

                    // displacement
                    _hTildeDx[index] = _hTildeDx[index].Multiply(sign);
                    _hTildeDz[index] = _hTildeDz[index].Multiply(sign);
                    _vertices[index1].x = _originalVertices[index1].x + _hTildeDx[index].Real * lambda;
                    _vertices[index1].z = _originalVertices[index1].z + _hTildeDz[index].Real * lambda;

                    // normal
                    _hTildeSlopex[index] = _hTildeSlopex[index].Multiply(sign);
                    _hTildeSlopez[index] = _hTildeSlopez[index].Multiply(sign);
                    _normals[index1] = new Vector3(-_hTildeSlopex[index].Real, 1f, -_hTildeSlopez[index].Real)
                        .normalized;
                    
                    // for tiling
                    if (i == 0 && j == 0) {
                        _vertices[index1 + Size + _sizePlus1 * Size].y = _hTilde[index].Real;

                        _vertices[index1 + Size + _sizePlus1 * Size].x = _originalVertices[index1 + Size + _sizePlus1 * Size].x + _hTildeDx[index].Real * lambda;
                        _vertices[index1 + Size + _sizePlus1 * Size].z = _originalVertices[index1 + Size + _sizePlus1 * Size].z + _hTildeDz[index].Real * lambda;
			
                        //_vertices[index1 + Size + _sizePlus1 * Size] =  _normals[index];
                    }
                    if (i == 0) {
                        _vertices[index1 + Size].y = _hTilde[index].Real;

                        _vertices[index1 + Size].x = _originalVertices[index1 + Size].x + _hTildeDx[index].Real * lambda;
                        _vertices[index1 + Size].z = _originalVertices[index1 + Size].z + _hTildeDz[index].Real * lambda;
			
                        //_vertices[index1 + Size] =  _normals[index];
                    }
                    if (j == 0) {
                        _vertices[index1 + _sizePlus1 * Size].y = _hTilde[index].Real;

                        _vertices[index1 + _sizePlus1 * Size].x = _originalVertices[index1 + _sizePlus1 * Size].x + _hTildeDx[index].Real * lambda;
                        _vertices[index1 + _sizePlus1 * Size].z = _originalVertices[index1 + _sizePlus1 * Size].z + _hTildeDz[index].Real * lambda;
			
                        //_vertices[index1 + _sizePlus1 * Size] = _normals[index];
                    }
                }
            }
//            for (int i = 0; i < Size; i++) {
//                index = (Size - 1) * Size + i;
//                _vertices[index].x = _originalVertices[index].x + _hTildeDx[i].Real * lambda;
//                _vertices[index].y = _hTilde[i].Real;
//                _vertices[index].z = _originalVertices[index].z + _hTildeDz[i].Real * lambda;
//                _normals[index] = _normals[i];
//                index = i * Size;
//                _vertices[index + Size - 1].x = _originalVertices[index + Size - 1].x + _hTildeDx[index].Real * lambda;
//                _vertices[index + Size - 1].y = _hTilde[index].Real;
//                _vertices[index + Size - 1].z = _originalVertices[index + Size - 1].z + _hTildeDz[index].Real * lambda;
//                _normals[index + Size - 1] = _normals[index];
//            }
//            index = Size * Size - 1;
//            _vertices[index].x = _originalVertices[index].x + _hTildeDx[0].Real * lambda;
//            _vertices[index].y = _hTilde[0].Real;
//            _vertices[index].z = _originalVertices[index].z + _hTildeDz[0].Real * lambda;
//            _normals[index] = _normals[0];
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
            float kDotW2 = kDotW * kDotW;// * kDotW * kDotW * kDotW * kDotW;

            float wLength = Wind.magnitude;
            float l = wLength * wLength / Physics.gravity.magnitude;
            float l2 = l * l;

            float damping = 0.001f;
            float l2Damping2 = l2 * damping * damping;

            return PhillipsSpectrum * Mathf.Exp(-1.0f / (kLength2 * l2)) / kLength4 * kDotW2 *
                   Mathf.Exp(-kLength2 * l2Damping2);
        }

        float Dispersion(int i, int j) {
            float w0 = 2.0f * Mathf.PI / 200;//Cycle;
            float kx = Mathf.PI * (2 * i - Size) / Length;
            float kz = Mathf.PI * (2 * j - Size) / Length;
            return Mathf.Floor(Mathf.Sqrt(Physics.gravity.magnitude * Mathf.Sqrt(kx * kx + kz * kz)) / w0) * w0;
        }

        Complex hTilde_0(int i, int j) {
            Complex r = GaussianRandomVariable();
            return r.Multiply(Mathf.Sqrt(Phillips(i, j) / 2.0f));
        }

        Complex HTilde(float time, int i, int j) {
            int index = j * _sizePlus1 + i;

            Complex htilde0 = _htilde0[index]; //.Real, _htilde0[index].Imaginary);
            Complex htilde0Mkconj = _htilde0MkConj[index]; //.Real, _htilde0MkConj[index].Imaginary);

            float omegat = Dispersion(i, j) * time;

            float cos = Mathf.Cos(omegat);
            float sin = Mathf.Sin(omegat);

            Complex c0 = new Complex(cos, sin);
            Complex c1 = new Complex(cos, -sin);

            return htilde0 * c0 + htilde0Mkconj * c1;
        }
    }
}