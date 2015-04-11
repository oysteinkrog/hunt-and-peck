using System;
using JetBrains.Annotations;

namespace hap.NativeMethods
{
    public static class DisposeUtils
    {
        /// <summary>
        ///     Dispose and replace an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="newObject"></param>
        public static void DisposeReplace<T>(ref T obj, Func<T> newObject) where T : IDisposable
        {
            T d = obj;
            if (!Equals(d, default(T)))
            {
                d.Dispose();
            }

            obj = newObject();
        }

        /// <summary>
        ///     Dispose and null out an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposeObject"></param>
        public static void Dispose<T>(ref T disposeObject) where T : IDisposable
        {
            if (Equals(disposeObject, default(T)))
            {
                return;
            }

            T d = disposeObject;
            disposeObject = default(T);
            d.Dispose();
        }

        /// <summary>
        ///     Dispose an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposeObject"></param>
        public static void Dispose<T>(T disposeObject) where T : IDisposable
        {
            if (Equals(disposeObject, default(T)))
            {
                return;
            }
            disposeObject.Dispose();
        }

        /// <summary>
        ///     Dispose and null out all entries in an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposeArray"></param>
        [PublicAPI]
        public static void Dispose<T>(T[] disposeArray) where T : IDisposable
        {
            for (var i = 0; i < disposeArray.Length; i++)
            {
                Dispose(i, disposeArray);
            }
        }

        /// <summary>
        ///     Dispose and null out a specific entry in an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i"></param>
        /// <param name="disposeArray"></param>
        [PublicAPI]
        public static void Dispose<T>(int i, T[] disposeArray) where T : IDisposable
        {
            T d = disposeArray[i];
            if (!Equals(disposeArray[i], default(T)))
            {
                Dispose(d);
            }
            disposeArray[i] = default(T);
        }

        /// <summary>
        ///     Dispose and replace a specific entry in an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i"></param>
        /// <param name="disposeArray"></param>
        [PublicAPI]
        public static void DisposeReplace<T>(int i, T[] disposeArray, Func<T> newObject) where T : IDisposable
        {
            T d = disposeArray[i];
            if (!Equals(disposeArray[i], default(T)))
            {
                Dispose(d);
            }
            disposeArray[i] = newObject();
        }
    }
}