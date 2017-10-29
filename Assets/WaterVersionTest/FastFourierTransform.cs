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

        public FastFourierTransform(int N) {
            this.N = (uint) N;
            reversed = null;
            pi2 = Mathf.PI * 2;
            log_2_N = (uint) (Mathf.Log(N) / Mathf.Log(2));
            reversed = new uint[N];
            for (int i = 0; i < N; i++) {
                reversed[i] = Reverse((uint) i);
            }

            uint pow2 = 1;

            T = new Complex[log_2_N][];

            for (int i = 0; i < log_2_N; i++) {
                T[i] = new Complex[pow2];
                for (uint j = 0; j < pow2; j++) {
                    T[i][j] = t(j, pow2 * 2);
                }
                pow2 *= 2;
            }

            c[0] = new Complex[N];
            c[1] = new Complex[N];
            for (int i = 0; i < c[0].Length; i++) {
                c[0][i] = new Complex();
                c[1][i] = new Complex();
            }
            which = 0;
        }

        private uint Reverse(uint i) {
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

        public void Fft(Complex[] input, int stride, int offset) {
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
                input[i * stride + offset] = c[which][i];
            }
        }
    }
}