using System.Collections.Generic;
using UnityEngine;

namespace WaterVersionTest {
    public class Ocean : MonoBehaviour {
        private Mesh _waterMesh;
        public int Size = 64;
        public float Length = 10;
        private Vector3[] _originalVertices;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        public Vector2 Wind = new Vector2(32, 32);
        public float A = 0.0005f;

        private Complex[] _htilde0;
        private Complex[] _htilde0MkConj;

        private void Awake() {
            _waterMesh = GetComponent<MeshFilter>().sharedMesh;
            int triangleIndex = 0;
            _originalVertices = new Vector3[Size * Size];
            _vertices = new Vector3[Size * Size];
            int[] triangles = new int[(Size - 1) * (Size - 1) * 6];
            _normals = new Vector3[Size * Size];

            _htilde0 = new Complex[Size * Size];
            _htilde0MkConj = new Complex[Size * Size];


            for (int i = 0; i < Size; i++) {
                for (int j = 0; j < Size; j++) {
                    // set vertices
                    int index = i * Size + j;

                    _htilde0[index] = hTilde_0(j, i);
                    _htilde0MkConj[index] = hTilde_0(-j, -i).Conjugate();

                    _originalVertices[index].x = _vertices[index].x = (i - Size / 2) * Length / Size;
                    _originalVertices[index].y = _vertices[index].y = 0;
                    _originalVertices[index].z = _vertices[index].z = (j - Size / 2) * Length / Size;
                    //normals[index] = Vector3.up;
                    // set triangles
                    if (j < Size - 1 && i < Size - 1) {
                        triangles[triangleIndex + 0] = (j * Size) + i;
                        triangles[triangleIndex + 1] = (j * Size) + i + 1;
                        triangles[triangleIndex + 2] = ((j + 1) * Size) + i + 1;

                        triangles[triangleIndex + 3] = (j * Size) + i;
                        triangles[triangleIndex + 4] = ((j + 1) * Size) + i + 1;
                        triangles[triangleIndex + 5] = ((j + 1) * Size) + i;

                        triangleIndex += 6;
                    }
                    // set uv
                    //float uvStepX = 1f / (Size - 1);
                    //float uvStepY = 1f / (Size - 1);
                    //_newUv[index].x = i*uvStepX;
                    //_newUv[index].y = j*uvStepY;
                }
            }
            _waterMesh.vertices = _vertices;
            _waterMesh.triangles = triangles;
            //mesh.uv = _newUv;
            //mesh.normals = _newNormals;
        }

        private void Update() {
            //_mesh = _waterMesh;
            float lambda = -1.0f;
            for (int m_prime = 0; m_prime < Size; m_prime++) {
                for (int n_prime = 0; n_prime < Size; n_prime++) {
                    var index = m_prime * Size + n_prime;

                    var x = new Vector2(_vertices[index].x, _vertices[index].z);

                    float height;
                    Vector2 displayment;
                    Vector3 normal;
                    h_D_and_n(x, Time.realtimeSinceStartup, out height, out displayment, out normal);

                    _vertices[index].y = height;

                    _vertices[index].x = _originalVertices[index].x + lambda * displayment.x;
                    _vertices[index].z = _originalVertices[index].z + lambda * displayment.y;

                    _normals[index] = normal;

//                    if (n_prime == 0 && m_prime == 0) {
//                        int tempIndex = index + Size - 1 + Size * (Size - 1);
//                        _vertices[tempIndex].y = height;
//
//                        _vertices[tempIndex].x = _originalVertices[tempIndex].x + lambda * displayment.x;
//                        _vertices[tempIndex].z = _originalVertices[tempIndex].z + lambda * displayment.y;
//
//                        _normals[tempIndex] = normal;
//                    }
//                    if (n_prime == 0) {
//                        vertices[index + N].y = h_d_and_n.h.a;
//
//                        vertices[index + N].x = vertices[index + N].ox + lambda * h_d_and_n.D.x;
//                        vertices[index + N].z = vertices[index + N].oz + lambda * h_d_and_n.D.y;
//
//                        _normals[index + Size] = normal;
//                        vertices[index + N].nx = h_d_and_n.n.x;
//                        vertices[index + N].ny = h_d_and_n.n.y;
//                        vertices[index + N].nz = h_d_and_n.n.z;
//                    }
//                    if (m_prime == 0) {
//                        vertices[index + Nplus1 * N].y = h_d_and_n.h.a;
//
//                        vertices[index + Nplus1 * N].x = vertices[index + Nplus1 * N].ox + lambda * h_d_and_n.D.x;
//                        vertices[index + Nplus1 * N].z = vertices[index + Nplus1 * N].oz + lambda * h_d_and_n.D.y;
//
//                        vertices[index + Nplus1 * N].nx = h_d_and_n.n.x;
//                        vertices[index + Nplus1 * N].ny = h_d_and_n.n.y;
//                        vertices[index + Nplus1 * N].nz = h_d_and_n.n.z;
//                    }
                }
            }
            
            //_waterMesh.SetVertices(new List<Vector3>(_vertices));
            _waterMesh.vertices = _vertices;
            _waterMesh.normals = _normals;
            //_waterMesh.SetNormals();
        }

        void h_D_and_n(Vector2 x, float t, out float height, out Vector2 displayment, out Vector3 normal) {
            Complex h = new Complex(0.0f, 0.0f);
            Vector2 D = new Vector2(0.0f, 0.0f);
            Vector3 n = new Vector3(0.0f, 0.0f, 0.0f);

            Complex c, res, htilde_c;
            Vector2 k;
            float kx, kz, k_length, k_dot_x;

            for (int m_prime = 0; m_prime < Size; m_prime++) {
                kz = 2.0f * Mathf.PI * (m_prime - Size / 2.0f) / Length;
                for (int n_prime = 0; n_prime < Size; n_prime++) {
                    kx = 2.0f * Mathf.PI * (n_prime - Size / 2.0f) / Length;
                    k = new Vector2(kx, kz);

                    k_length = k.magnitude;
                    k_dot_x = Vector2.Dot(k, x);

                    c = new Complex(Mathf.Cos(k_dot_x), Mathf.Sin(k_dot_x));
                    htilde_c = hTilde(t, n_prime, m_prime) * c;

                    h = h + htilde_c;

                    n = n + new Vector3(-kx * (float) htilde_c.Imaginary, 0.0f, -kz * (float) htilde_c.Imaginary);

                    if (k_length < 0.000001) continue;
                    D = D + new Vector2(kx / k_length * (float) htilde_c.Imaginary,
                            kz / k_length * (float) htilde_c.Imaginary);
                }
            }

            n = (Vector3.up - n).normalized;

            height = (float) h.Real;
            displayment = D;
            normal = n;
        }

        Complex gaussianRandomVariable() {
            float x1, x2, w;
            do {
                x1 = 2f * Random.value - 1f;
                x2 = 2f * Random.value - 1f;
                w = x1 * x1 + x2 * x2;
            } while (w >= 1f);
            w = Mathf.Sqrt((-2f * Mathf.Log(w)) / w);
            return new Complex(x1 * w, x2 * w);
        }

        float phillips(int n_prime, int m_prime) {
            Vector2 k = new Vector2(Mathf.PI * (2 * n_prime - Size) / Length, Mathf.PI * (2 * m_prime - Size) / Length);
            float k_length = k.magnitude;
            if (k_length < 0.000001) return 0f;

            float k_length2 = k_length * k_length;
            float k_length4 = k_length2 * k_length2;

            float k_dot_w = Vector2.Dot(k.normalized, Wind.normalized);
            float k_dot_w2 = k_dot_w * k_dot_w;

            float w_length = Wind.magnitude;
            float L = w_length * w_length / Physics.gravity.magnitude;
            float L2 = L * L;

            float damping = 0.001f;
            float l2 = L2 * damping * damping;

            return A * Mathf.Exp(-1.0f / (k_length2 * L2)) / k_length4 * k_dot_w2 * Mathf.Exp(-k_length2 * l2);
        }

        float dispersion(int n_prime, int m_prime) {
            float w_0 = 2.0f * Mathf.PI / 200.0f;
            float kx = Mathf.PI * (2 * n_prime - Size) / Length;
            float kz = Mathf.PI * (2 * m_prime - Size) / Length;
            return Mathf.Floor(Mathf.Sqrt(Physics.gravity.magnitude * Mathf.Sqrt(kx * kx + kz * kz)) / w_0) * w_0;
        }

        Complex hTilde_0(int n_prime, int m_prime) {
            Complex r = gaussianRandomVariable();
            return r.Multiply(Mathf.Sqrt(phillips(n_prime, m_prime) / 2.0f));
        }

        Complex hTilde(float t, int n_prime, int m_prime) {
            int index = m_prime * Size + n_prime;

            Complex htilde0 = new Complex(_htilde0[index].Real, _htilde0[index].Imaginary);
            Complex htilde0mkconj = new Complex(_htilde0MkConj[index].Real, _htilde0MkConj[index].Imaginary);

            float omegat = dispersion(n_prime, m_prime) * t;

            float cos_ = Mathf.Cos(omegat);
            float sin_ = Mathf.Sin(omegat);

            Complex c0 = new Complex(cos_, sin_);
            Complex c1 = new Complex(cos_, -sin_);

            Complex res = htilde0 * c0 + htilde0mkconj * c1;

            return htilde0 * c0 + htilde0mkconj * c1;
        }


//class cOcean {
//  private:
//	bool geometry;				// flag to render geometry or surface
//
//	float g;				// gravity constant
//	int N, Nplus1;				// dimension -- N should be a power of 2
//	float A;				// phillips spectrum parameter -- affects heights of waves
//	vector2 w;				// wind parameter
//	float length;				// length parameter
//	complex *h_tilde,			// for fast fourier transform
//		*h_tilde_slopex, *h_tilde_slopez,
//		*h_tilde_dx, *h_tilde_dz;
//	cFFT *fft;				// fast fourier transform
//
//	vertex_ocean *vertices;			// vertices for vertex buffer object
//	unsigned int *indices;			// indicies for vertex buffer object
//	unsigned int indices_count;		// number of indices to render
//	GLuint vbo_vertices, vbo_indices;	// vertex buffer objects
//
//	GLuint glProgram, glShaderV, glShaderF;	// shaders
//	GLint vertex, normal, texture, light_position, projection, view, model;	// attributes and uniforms
//
//  protected:
//  public:
//	cOcean(const int N, const float A, const vector2 w, const float length, bool geometry);
//	~cOcean();
//	void release();
//
//	float dispersion(int n_prime, int m_prime);		// deep water
//	float phillips(int n_prime, int m_prime);		// phillips spectrum
//	complex hTilde_0(int n_prime, int m_prime);
//	complex hTilde(float t, int n_prime, int m_prime);
//	complex_vector_normal h_D_and_n(vector2 x, float t);
//	void evaluateWaves(float t);
//	void evaluateWavesFFT(float t);
//	void render(float t, glm::vec3 light_pos, glm::mat4 Projection, glm::mat4 View, glm::mat4 Model, bool use_fft);
//};
//
//cOcean::cOcean(const int N, const float A, const vector2 w, const float length, const bool geometry) :
//	g(9.81), geometry(geometry), N(N), Nplus1(N+1), A(A), w(w), length(length),
//	vertices(0), indices(0), h_tilde(0), h_tilde_slopex(0), h_tilde_slopez(0), h_tilde_dx(0), h_tilde_dz(0), fft(0)
//{
//	h_tilde        = new complex[N*N];
//	h_tilde_slopex = new complex[N*N];
//	h_tilde_slopez = new complex[N*N];
//	h_tilde_dx     = new complex[N*N];
//	h_tilde_dz     = new complex[N*N];
//	fft            = new cFFT(N);
//	vertices       = new vertex_ocean[Nplus1*Nplus1];
//	indices        = new unsigned int[Nplus1*Nplus1*10];
//
//	int index;
//
//	complex htilde0, htilde0mk_conj;
//	for (int m_prime = 0; m_prime < Nplus1; m_prime++) {
//		for (int n_prime = 0; n_prime < Nplus1; n_prime++) {
//			index = m_prime * Nplus1 + n_prime;
//
//			htilde0        = hTilde_0( n_prime,  m_prime);
//			htilde0mk_conj = hTilde_0(-n_prime, -m_prime).conj();
//
//			vertices[index].a  = htilde0.a;
//			vertices[index].b  = htilde0.b;
//			vertices[index]._a = htilde0mk_conj.a;
//			vertices[index]._b = htilde0mk_conj.b;
//
//			vertices[index].ox = vertices[index].x =  (n_prime - N / 2.0f) * length / N;
//			vertices[index].oy = vertices[index].y =  0.0f;
//			vertices[index].oz = vertices[index].z =  (m_prime - N / 2.0f) * length / N;
//
//			vertices[index].nx = 0.0f;
//			vertices[index].ny = 1.0f;
//			vertices[index].nz = 0.0f;
//		}
//	}
//
//	indices_count = 0;
//	for (int m_prime = 0; m_prime < N; m_prime++) {
//		for (int n_prime = 0; n_prime < N; n_prime++) {
//			index = m_prime * Nplus1 + n_prime;
//
//			if (geometry) {
//				indices[indices_count++] = index;				// lines
//				indices[indices_count++] = index + 1;
//				indices[indices_count++] = index;
//				indices[indices_count++] = index + Nplus1;
//				indices[indices_count++] = index;
//				indices[indices_count++] = index + Nplus1 + 1;
//				if (n_prime == N - 1) {
//					indices[indices_count++] = index + 1;
//					indices[indices_count++] = index + Nplus1 + 1;
//				}
//				if (m_prime == N - 1) {
//					indices[indices_count++] = index + Nplus1;
//					indices[indices_count++] = index + Nplus1 + 1;
//				}
//			} else {
//				indices[indices_count++] = index;				// two triangles
//				indices[indices_count++] = index + Nplus1;
//				indices[indices_count++] = index + Nplus1 + 1;
//				indices[indices_count++] = index;
//				indices[indices_count++] = index + Nplus1 + 1;
//				indices[indices_count++] = index + 1;
//			}
//		}
//	}
//
//	createProgram(glProgram, glShaderV, glShaderF, "src/oceanv.sh", "src/oceanf.sh");
//	vertex         = glGetAttribLocation(glProgram, "vertex");
//	normal         = glGetAttribLocation(glProgram, "normal");
//	texture        = glGetAttribLocation(glProgram, "texture");
//	light_position = glGetUniformLocation(glProgram, "light_position");
//	projection     = glGetUniformLocation(glProgram, "Projection");
//	view           = glGetUniformLocation(glProgram, "View");
//	model          = glGetUniformLocation(glProgram, "Model");
//
//	glGenBuffers(1, &vbo_vertices);
//	glBindBuffer(GL_ARRAY_BUFFER, vbo_vertices);
//	glBufferData(GL_ARRAY_BUFFER, sizeof(vertex_ocean)*(Nplus1)*(Nplus1), vertices, GL_DYNAMIC_DRAW);
//
//	glGenBuffers(1, &vbo_indices);
//	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, vbo_indices);
//	glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices_count*sizeof(unsigned int), indices, GL_STATIC_DRAW);
//}
//
//cOcean::~cOcean() {
//	if (h_tilde)		delete [] h_tilde;
//	if (h_tilde_slopex)	delete [] h_tilde_slopex;
//	if (h_tilde_slopez)	delete [] h_tilde_slopez;
//	if (h_tilde_dx)		delete [] h_tilde_dx;
//	if (h_tilde_dz)		delete [] h_tilde_dz;
//	if (fft)		delete fft;
//	if (vertices)		delete [] vertices;
//	if (indices)		delete [] indices;
//}
//
//void cOcean::release() {
//	glDeleteBuffers(1, &vbo_indices);
//	glDeleteBuffers(1, &vbo_vertices);
//	releaseProgram(glProgram, glShaderV, glShaderF);
//}
//
//float cOcean::dispersion(int n_prime, int m_prime) {
//	float w_0 = 2.0f * Mathf.PI / 200.0f;
//	float kx = Mathf.PI * (2 * n_prime - N) / length;
//	float kz = Mathf.PI * (2 * m_prime - N) / length;
//	return floor(sqrt(g * sqrt(kx * kx + kz * kz)) / w_0) * w_0;
//}
//
//float cOcean::phillips(int n_prime, int m_prime) {
//	vector2 k(Mathf.PI * (2 * n_prime - N) / length,
//		  Mathf.PI * (2 * m_prime - N) / length);
//	float k_length  = k.length();
//	if (k_length < 0.000001) return 0.0;
//
//	float k_length2 = k_length  * k_length;
//	float k_length4 = k_length2 * k_length2;
//
//	float k_dot_w   = k.unit() * w.unit();
//	float k_dot_w2  = k_dot_w * k_dot_w * k_dot_w * k_dot_w * k_dot_w * k_dot_w;
//
//	float w_length  = w.length();
//	float L         = w_length * w_length / g;
//	float L2        = L * L;
//	
//	float damping   = 0.001;
//	float l2        = L2 * damping * damping;
//
//	return A * exp(-1.0f / (k_length2 * L2)) / k_length4 * k_dot_w2 * exp(-k_length2 * l2);
//}
//
//complex cOcean::hTilde_0(int n_prime, int m_prime) {
//	complex r = gaussianRandomVariable();
//	return r * sqrt(phillips(n_prime, m_prime) / 2.0f);
//}
//
//complex cOcean::hTilde(float t, int n_prime, int m_prime) {
//	int index = m_prime * Nplus1 + n_prime;
//
//	complex htilde0(vertices[index].a,  vertices[index].b);
//	complex htilde0mkconj(vertices[index]._a, vertices[index]._b);
//
//	float omegat = dispersion(n_prime, m_prime) * t;
//
//	float cos_ = cos(omegat);
//	float sin_ = sin(omegat);
//
//	complex c0(cos_,  sin_);
//	complex c1(cos_, -sin_);
//
//	complex res = htilde0 * c0 + htilde0mkconj * c1;
//
//	return htilde0 * c0 + htilde0mkconj*c1;
//}
//

//
//void cOcean::evaluateWaves(float t) {
//	float lambda = -1.0;
//	int index;
//	vector2 x;
//	vector2 d;
//	complex_vector_normal h_d_and_n;
//	for (int m_prime = 0; m_prime < N; m_prime++) {
//		for (int n_prime = 0; n_prime < N; n_prime++) {
//			index = m_prime * Nplus1 + n_prime;
//
//			x = vector2(vertices[index].x, vertices[index].z);
//
//			h_d_and_n = h_D_and_n(x, t);
//
//			vertices[index].y = h_d_and_n.h.a;
//
//			vertices[index].x = vertices[index].ox + lambda*h_d_and_n.D.x;
//			vertices[index].z = vertices[index].oz + lambda*h_d_and_n.D.y;
//
//			vertices[index].nx = h_d_and_n.n.x;
//			vertices[index].ny = h_d_and_n.n.y;
//			vertices[index].nz = h_d_and_n.n.z;
//
//			if (n_prime == 0 && m_prime == 0) {
//				vertices[index + N + Nplus1 * N].y = h_d_and_n.h.a;
//			
//				vertices[index + N + Nplus1 * N].x = vertices[index + N + Nplus1 * N].ox + lambda*h_d_and_n.D.x;
//				vertices[index + N + Nplus1 * N].z = vertices[index + N + Nplus1 * N].oz + lambda*h_d_and_n.D.y;
//
//				vertices[index + N + Nplus1 * N].nx = h_d_and_n.n.x;
//				vertices[index + N + Nplus1 * N].ny = h_d_and_n.n.y;
//				vertices[index + N + Nplus1 * N].nz = h_d_and_n.n.z;
//			}
//			if (n_prime == 0) {
//				vertices[index + N].y = h_d_and_n.h.a;
//			
//				vertices[index + N].x = vertices[index + N].ox + lambda*h_d_and_n.D.x;
//				vertices[index + N].z = vertices[index + N].oz + lambda*h_d_and_n.D.y;
//
//				vertices[index + N].nx = h_d_and_n.n.x;
//				vertices[index + N].ny = h_d_and_n.n.y;
//				vertices[index + N].nz = h_d_and_n.n.z;
//			}
//			if (m_prime == 0) {
//				vertices[index + Nplus1 * N].y = h_d_and_n.h.a;
//			
//				vertices[index + Nplus1 * N].x = vertices[index + Nplus1 * N].ox + lambda*h_d_and_n.D.x;
//				vertices[index + Nplus1 * N].z = vertices[index + Nplus1 * N].oz + lambda*h_d_and_n.D.y;
//				
//				vertices[index + Nplus1 * N].nx = h_d_and_n.n.x;
//				vertices[index + Nplus1 * N].ny = h_d_and_n.n.y;
//				vertices[index + Nplus1 * N].nz = h_d_and_n.n.z;
//			}
//		}
//	}
//}
//
//void cOcean::evaluateWavesFFT(float t) {
//	float kx, kz, len, lambda = -1.0f;
//	int index, index1;
//
//	for (int m_prime = 0; m_prime < N; m_prime++) {
//		kz = Mathf.PI * (2.0f * m_prime - N) / length;
//		for (int n_prime = 0; n_prime < N; n_prime++) {
//			kx = Mathf.PI*(2 * n_prime - N) / length;
//			len = sqrt(kx * kx + kz * kz);
//			index = m_prime * N + n_prime;
//
//			h_tilde[index] = hTilde(t, n_prime, m_prime);
//			h_tilde_slopex[index] = h_tilde[index] * complex(0, kx);
//			h_tilde_slopez[index] = h_tilde[index] * complex(0, kz);
//			if (len < 0.000001f) {
//				h_tilde_dx[index]     = complex(0.0f, 0.0f);
//				h_tilde_dz[index]     = complex(0.0f, 0.0f);
//			} else {
//				h_tilde_dx[index]     = h_tilde[index] * complex(0, -kx/len);
//				h_tilde_dz[index]     = h_tilde[index] * complex(0, -kz/len);
//			}
//		}
//	}
//
//	for (int m_prime = 0; m_prime < N; m_prime++) {
//		fft->fft(h_tilde, h_tilde, 1, m_prime * N);
//		fft->fft(h_tilde_slopex, h_tilde_slopex, 1, m_prime * N);
//		fft->fft(h_tilde_slopez, h_tilde_slopez, 1, m_prime * N);
//		fft->fft(h_tilde_dx, h_tilde_dx, 1, m_prime * N);
//		fft->fft(h_tilde_dz, h_tilde_dz, 1, m_prime * N);
//	}
//	for (int n_prime = 0; n_prime < N; n_prime++) {
//		fft->fft(h_tilde, h_tilde, N, n_prime);
//		fft->fft(h_tilde_slopex, h_tilde_slopex, N, n_prime);
//		fft->fft(h_tilde_slopez, h_tilde_slopez, N, n_prime);
//		fft->fft(h_tilde_dx, h_tilde_dx, N, n_prime);
//		fft->fft(h_tilde_dz, h_tilde_dz, N, n_prime);
//	}
//
//	int sign;
//	float signs[] = { 1.0f, -1.0f };
//	vector3 n;
//	for (int m_prime = 0; m_prime < N; m_prime++) {
//		for (int n_prime = 0; n_prime < N; n_prime++) {
//			index  = m_prime * N + n_prime;		// index into h_tilde..
//			index1 = m_prime * Nplus1 + n_prime;	// index into vertices
//
//			sign = signs[(n_prime + m_prime) & 1];
//
//			h_tilde[index]     = h_tilde[index] * sign;
//
//			// height
//			vertices[index1].y = h_tilde[index].a;
//
//			// displacement
//			h_tilde_dx[index] = h_tilde_dx[index] * sign;
//			h_tilde_dz[index] = h_tilde_dz[index] * sign;
//			vertices[index1].x = vertices[index1].ox + h_tilde_dx[index].a * lambda;
//			vertices[index1].z = vertices[index1].oz + h_tilde_dz[index].a * lambda;
//			
//			// normal
//			h_tilde_slopex[index] = h_tilde_slopex[index] * sign;
//			h_tilde_slopez[index] = h_tilde_slopez[index] * sign;
//			n = vector3(0.0f - h_tilde_slopex[index].a, 1.0f, 0.0f - h_tilde_slopez[index].a).unit();
//			vertices[index1].nx =  n.x;
//			vertices[index1].ny =  n.y;
//			vertices[index1].nz =  n.z;
//
//			// for tiling
//			if (n_prime == 0 && m_prime == 0) {
//				vertices[index1 + N + Nplus1 * N].y = h_tilde[index].a;
//
//				vertices[index1 + N + Nplus1 * N].x = vertices[index1 + N + Nplus1 * N].ox + h_tilde_dx[index].a * lambda;
//				vertices[index1 + N + Nplus1 * N].z = vertices[index1 + N + Nplus1 * N].oz + h_tilde_dz[index].a * lambda;
//			
//				vertices[index1 + N + Nplus1 * N].nx =  n.x;
//				vertices[index1 + N + Nplus1 * N].ny =  n.y;
//				vertices[index1 + N + Nplus1 * N].nz =  n.z;
//			}
//			if (n_prime == 0) {
//				vertices[index1 + N].y = h_tilde[index].a;
//
//				vertices[index1 + N].x = vertices[index1 + N].ox + h_tilde_dx[index].a * lambda;
//				vertices[index1 + N].z = vertices[index1 + N].oz + h_tilde_dz[index].a * lambda;
//			
//				vertices[index1 + N].nx =  n.x;
//				vertices[index1 + N].ny =  n.y;
//				vertices[index1 + N].nz =  n.z;
//			}
//			if (m_prime == 0) {
//				vertices[index1 + Nplus1 * N].y = h_tilde[index].a;
//
//				vertices[index1 + Nplus1 * N].x = vertices[index1 + Nplus1 * N].ox + h_tilde_dx[index].a * lambda;
//				vertices[index1 + Nplus1 * N].z = vertices[index1 + Nplus1 * N].oz + h_tilde_dz[index].a * lambda;
//			
//				vertices[index1 + Nplus1 * N].nx =  n.x;
//				vertices[index1 + Nplus1 * N].ny =  n.y;
//				vertices[index1 + Nplus1 * N].nz =  n.z;
//			}
//		}
//	}
//}
//
//void cOcean::render(float t, glm::vec3 light_pos, glm::mat4 Projection, glm::mat4 View, glm::mat4 Model, bool use_fft) {
//	static bool eval = false;
//	if (!use_fft && !eval) {
//		eval = true;
//		evaluateWaves(t);
//	} else if (use_fft) {
//		evaluateWavesFFT(t);
//	}
//
//	glUseProgram(glProgram);
//	glUniform3f(light_position, light_pos.x, light_pos.y, light_pos.z);
//	glUniformMatrix4fv(projection, 1, GL_FALSE, glm::value_ptr(Projection));
//	glUniformMatrix4fv(view,       1, GL_FALSE, glm::value_ptr(View));
//	glUniformMatrix4fv(model,      1, GL_FALSE, glm::value_ptr(Model));
//
//	glBindBuffer(GL_ARRAY_BUFFER, vbo_vertices);
//	glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(vertex_ocean) * Nplus1 * Nplus1, vertices);
//	glEnableVertexAttribArray(vertex);
//	glVertexAttribPointer(vertex, 3, GL_FLOAT, GL_FALSE, sizeof(vertex_ocean), 0);
//	glEnableVertexAttribArray(normal);
//	glVertexAttribPointer(normal, 3, GL_FLOAT, GL_FALSE, sizeof(vertex_ocean), (char *)NULL + 12);
//	glEnableVertexAttribArray(texture);
//	glVertexAttribPointer(texture, 3, GL_FLOAT, GL_FALSE, sizeof(vertex_ocean), (char *)NULL + 24);
//
//	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, vbo_indices);
//	for (int j = 0; j < 10; j++) {
//		for (int i = 0; i < 10; i++) {
//			Model = glm::scale(glm::mat4(1.0f), glm::vec3(5.f,5.f,5.f));
//			Model = glm::translate(Model, glm::vec3(length * i, 0, length * -j));
//			glUniformMatrix4fv(model, 1, GL_FALSE, glm::value_ptr(Model));
//			glDrawElements(geometry ? GL_LINES : GL_TRIANGLES, indices_count, GL_UNSIGNED_INT, 0);
//		}
//	}
//}
    }
}