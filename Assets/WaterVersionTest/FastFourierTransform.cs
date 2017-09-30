using System.ComponentModel;
using UnityEngine;

namespace WaterVersionTest {
    public class FastFourierTransform {
        private uint N, which;
        private uint log_2_N;
        float pi2;

        private uint[] reversed; // pointer

        public Complex[][] T;

        public Complex[][] c = new Complex[2][];
        /*
        public:
        complex** T;
        complex* c[2];
        cFFT(unsigned int N);
        ~cFFT();
        unsigned int reverse(unsigned int i);
        complex t(unsigned int x, unsigned int N);
        void fft(complex* input, complex* output, int stride, int offset);
        */

        public FastFourierTransform(uint N) {
            this.N = N;
            reversed = null;
            pi2 = Mathf.PI * 2;
            log_2_N = (uint) (Mathf.Log(N) / Mathf.Log(2));
            reversed = new uint[N];
            for (int i = 0; i < N; i++) {
                reversed[i] = reverse((uint) i);
            }

            int pow2 = 1;

            T = new Complex[log_2_N][];

            for (int i = 0; i < log_2_N; i++) {
                T[i] = new Complex[pow2];
                for (int j = 0; j < pow2; j++) {
                    T[i][j] = t(j, pow2 * 2);
                }
                pow2 *= 2;
            }

            c[0] = new Complex[N];
            c[1] = new Complex[N];
            which = 0;
        }

        uint reverse(uint i) {
            uint res = 0;
            for (int j = 0; j < log_2_N; j++) {
                res = (res << 1) + (i & 1);
                i >>= 1;
            }
            return res;
        }

// 
        Complex t(uint x, uint N) {
            return new Complex(Mathf.Cos(pi2 * x / N), Mathf.Sin(pi2 * x / N));
        }

        void fft(Complex[] input, Complex[] output, int stride, int offset) {
            for (int i = 0; i < N; i++) {
                c[which][i] = input[reversed[i] * stride + offset];
            }
            uint loops = N >> 1;
            int size = 1 << 1;
            int size_over_2 = 1;
            int w_ = 0;
            for (int i = 0; i < log_2_N; i++) {
                which ^= 1;
                for (int j = 0; j < loops; j++) {
                    for (int k = 0; k < size_over_2; k++) {
                        c[which][size * j + k] = c[which ^ 1][size * j + k] +
                                                 c[which ^ 1][size * j + size_over_2 + k] * T[w_][k];
                    }

                    for (int k = size_over_2; k < size; k++) {
                        c[which][size * j + k] = c[which ^ 1][size * j - size_over_2 + k] -
                                                 c[which ^ 1][size * j + k] * T[w_][k - size_over_2];
                    }
                }
                loops >>= 1;
                size <<= 1;
                size_over_2 <<= 1;
                w_++;
            }

            for (int i = 0; i < N; i++) {
                output[i * stride + offset] = c[which][i];
            }
        }


        void evaluateWavesFFT(float t) {
            float kx, kz, len, lambda = -1.0f;
            int index, index1;

            for (int m_prime = 0; m_prime < N; m_prime++) {
                kz = M_PI * (2.0f * m_prime - N) / length;
                for (int n_prime = 0; n_prime < N; n_prime++) {
                    kx = M_PI * (2 * n_prime - N) / length;
                    len = sqrt(kx * kx + kz * kz);
                    index = m_prime * N + n_prime;

                    h_tilde[index] = hTilde(t, n_prime, m_prime);
                    h_tilde_slopex[index] = h_tilde[index] * complex(0, kx);
                    h_tilde_slopez[index] = h_tilde[index] * complex(0, kz);
                    if (len < 0.000001f) {
                        h_tilde_dx[index] = complex(0.0f, 0.0f);
                        h_tilde_dz[index] = complex(0.0f, 0.0f);
                    } else {
                        h_tilde_dx[index] = h_tilde[index] * complex(0, -kx / len);
                        h_tilde_dz[index] = h_tilde[index] * complex(0, -kz / len);
                    }
                }
            }

            for (int m_prime = 0; m_prime < N; m_prime++) {
                fft->fft(h_tilde, h_tilde, 1, m_prime * N);
                fft->fft(h_tilde_slopex, h_tilde_slopex, 1, m_prime * N);
                fft->fft(h_tilde_slopez, h_tilde_slopez, 1, m_prime * N);
                fft->fft(h_tilde_dx, h_tilde_dx, 1, m_prime * N);
                fft->fft(h_tilde_dz, h_tilde_dz, 1, m_prime * N);
            }
            for (int n_prime = 0; n_prime < N; n_prime++) {
                fft->fft(h_tilde, h_tilde, N, n_prime);
                fft->fft(h_tilde_slopex, h_tilde_slopex, N, n_prime);
                fft->fft(h_tilde_slopez, h_tilde_slopez, N, n_prime);
                fft->fft(h_tilde_dx, h_tilde_dx, N, n_prime);
                fft->fft(h_tilde_dz, h_tilde_dz, N, n_prime);
            }

            int sign;
            float signs[] =  {
                1.0f, -1.0f
            }
            ;
            vector3 n;
            for (int m_prime = 0; m_prime < N; m_prime++) {
                for (int n_prime = 0; n_prime < N; n_prime++) {
                    index = m_prime * N + n_prime; // index into h_tilde..
                    index1 = m_prime * Nplus1 + n_prime; // index into vertices

                    sign = signs[(n_prime + m_prime) & 1];

                    h_tilde[index] = h_tilde[index] * sign;

                    // height
                    vertices[index1].y = h_tilde[index].a;

                    // displacement
                    h_tilde_dx[index] = h_tilde_dx[index] * sign;
                    h_tilde_dz[index] = h_tilde_dz[index] * sign;
                    vertices[index1].x = vertices[index1].ox + h_tilde_dx[index].a * lambda;
                    vertices[index1].z = vertices[index1].oz + h_tilde_dz[index].a * lambda;

                    // normal
                    h_tilde_slopex[index] = h_tilde_slopex[index] * sign;
                    h_tilde_slopez[index] = h_tilde_slopez[index] * sign;
                    n = vector3(0.0f - h_tilde_slopex[index].a, 1.0f, 0.0f - h_tilde_slopez[index].a).unit();
                    vertices[index1].nx = n.x;
                    vertices[index1].ny = n.y;
                    vertices[index1].nz = n.z;

                    // for tiling
                    if (n_prime == 0 && m_prime == 0) {
                        vertices[index1 + N + Nplus1 * N].y = h_tilde[index].a;

                        vertices[index1 + N + Nplus1 * N].x =
                            vertices[index1 + N + Nplus1 * N].ox + h_tilde_dx[index].a * lambda;
                        vertices[index1 + N + Nplus1 * N].z =
                            vertices[index1 + N + Nplus1 * N].oz + h_tilde_dz[index].a * lambda;

                        vertices[index1 + N + Nplus1 * N].nx = n.x;
                        vertices[index1 + N + Nplus1 * N].ny = n.y;
                        vertices[index1 + N + Nplus1 * N].nz = n.z;
                    }
                    if (n_prime == 0) {
                        vertices[index1 + N].y = h_tilde[index].a;

                        vertices[index1 + N].x = vertices[index1 + N].ox + h_tilde_dx[index].a * lambda;
                        vertices[index1 + N].z = vertices[index1 + N].oz + h_tilde_dz[index].a * lambda;

                        vertices[index1 + N].nx = n.x;
                        vertices[index1 + N].ny = n.y;
                        vertices[index1 + N].nz = n.z;
                    }
                    if (m_prime == 0) {
                        vertices[index1 + Nplus1 * N].y = h_tilde[index].a;

                        vertices[index1 + Nplus1 * N].x =
                            vertices[index1 + Nplus1 * N].ox + h_tilde_dx[index].a * lambda;
                        vertices[index1 + Nplus1 * N].z =
                            vertices[index1 + Nplus1 * N].oz + h_tilde_dz[index].a * lambda;

                        vertices[index1 + Nplus1 * N].nx = n.x;
                        vertices[index1 + Nplus1 * N].ny = n.y;
                        vertices[index1 + Nplus1 * N].nz = n.z;
                    }
                }
            }
        }
    }
}