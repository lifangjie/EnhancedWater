using System;
using UnityEngine;

namespace WaterVersionTest {
    public class Complex {
        #region 字段

        //复数实部
        private float _real = 0.0f;

        //复数虚部
        private float _imaginary = 0.0f;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置复数的实部
        /// </summary>
        public float Real {
            get { return _real; }
            set { _real = value; }
        }

        /// <summary>
        /// 获取或设置复数的虚部
        /// </summary>
        public float Imaginary {
            get { return _imaginary; }
            set { _imaginary = value; }
        }

        #endregion


        #region 构造函数

        /// <summary>
        /// 默认构造函数，得到的复数为0
        /// </summary>
        public Complex()
            : this(0, 0) { }

        /// <summary>
        /// 只给实部赋值的构造函数，虚部将取0
        /// </summary>
        /// <param name="dbreal">实部</param>
        public Complex(float dbreal)
            : this(dbreal, 0) { }

        /// <summary>
        /// 一般形式的构造函数
        /// </summary>
        /// <param name="dbreal">实部</param>
        /// <param name="dbImage">虚部</param>
        public Complex(float dbreal, float dbImage) {
            _real = dbreal;
            _imaginary = dbImage;
        }

        /// <summary>
        /// 以拷贝另一个复数的形式赋值的构造函数
        /// </summary>
        /// <param name="other">复数</param>
        public Complex(Complex other) {
            _real = other._real;
            _imaginary = other._imaginary;
        }

        #endregion

        #region 重载

        //加法的重载
        public static Complex operator +(Complex comp1, Complex comp2) {
            return comp1.Add(comp2);
        }

        //减法的重载
        public static Complex operator -(Complex comp1, Complex comp2) {
            return comp1.Substract(comp2);
        }

        //乘法的重载
        public static Complex operator *(Complex comp1, Complex comp2) {
            return comp1.Multiply(comp2);
        }

        //==的重载
        public static bool operator ==(Complex z1, Complex z2) {
            return ((z1._real == z2._real) && (z1._imaginary == z2._imaginary));
        }

        //!=的重载
        public static bool operator !=(Complex z1, Complex z2) {
            if (z1._real == z2._real) {
                return (z1._imaginary != z2._imaginary);
            }
            return true;
        }

        /// <summary>
        /// 重载ToString方法,打印复数字符串
        /// </summary>
        /// <returns>打印字符串</returns>
        public override string ToString() {
            if (Real == 0 && _imaginary == 0) {
                return string.Format("{0}", 0);
            }
            if (Real == 0 && (_imaginary != 1 && _imaginary != -1)) {
                return string.Format("{0} i", _imaginary);
            }
            if (_imaginary == 0) {
                return string.Format("{0}", Real);
            }
            if (_imaginary == 1) {
                return string.Format("i");
            }
            if (_imaginary == -1) {
                return string.Format("- i");
            }
            if (_imaginary < 0) {
                return string.Format("{0} - {1} i", Real, -_imaginary);
            }
            return string.Format("{0} + {1} i", Real, _imaginary);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 复数加法
        /// </summary>
        /// <param name="comp">待加复数</param>
        /// <returns>返回相加后的复数</returns>
        public Complex Add(Complex comp) {
            float x = _real + comp._real;
            float y = _imaginary + comp._imaginary;

            return new Complex(x, y);
        }

        /// <summary>
        /// 复数减法
        /// </summary>
        /// <param name="comp">待减复数</param>
        /// <returns>返回相减后的复数</returns>
        public Complex Substract(Complex comp) {
            float x = _real - comp._real;
            float y = _imaginary - comp._imaginary;

            return new Complex(x, y);
        }

        /// <summary>
        /// 复数乘法
        /// </summary>
        /// <param name="comp">待乘复数</param>
        /// <returns>返回相乘后的复数</returns>
        public Complex Multiply(Complex comp) {
            float x = _real * comp._real - _imaginary * comp._imaginary;
            float y = _real * comp._imaginary + _imaginary * comp._real;

            return new Complex(x, y);
        }

        /// <summary>
        /// 复数乘法
        /// </summary>
        /// <param name="num">待乘实数</param>
        /// <returns>返回相乘后的复数</returns>
        public Complex Multiply(float num) {
            return new Complex(_real * num, _imaginary * num);
        }

        /// <summary>
        /// 获取复数的模/幅度
        /// </summary>
        /// <returns>返回复数的模</returns>
        public float GetModul() {
            return Mathf.Sqrt(_real * _real + _imaginary * _imaginary);
        }

        /// <summary>
        /// 获取复数的相位角，取值范围（-π，π]
        /// </summary>
        /// <returns>返回复数的相角</returns>
        public float GetAngle() {
            #region 原先求相角的实现，后发现Math.Atan2已经封装好后注释

            ////实部和虚部都为0
            //if (real == 0 && imaginary == 0)
            //{
            //    return 0;
            //}
            //if (real == 0)
            //{
            //    if (imaginary > 0)
            //        return Math.PI / 2;
            //    else
            //        return -Math.PI / 2;
            //}
            //else
            //{
            //    if (real > 0)
            //    {
            //        return Math.Atan2(imaginary, real);
            //    }
            //    else
            //    {
            //        if (imaginary >= 0)
            //            return Math.Atan2(imaginary, real) + Math.PI;
            //        else
            //            return Math.Atan2(imaginary, real) - Math.PI;
            //    }
            //}

            #endregion

            return Mathf.Atan2(_imaginary, _real);
        }

        /// <summary>
        /// 获取复数的共轭复数
        /// </summary>
        /// <returns>返回共轭复数</returns>
        public Complex Conjugate() {
            return new Complex(this._real, -this._imaginary);
        }

        #endregion
    }
}