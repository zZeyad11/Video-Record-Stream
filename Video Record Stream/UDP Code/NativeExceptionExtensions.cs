﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PclSocketException = Video_Record_Stream.SocketException;
using PlatformSocketException = System.Net.Sockets.SocketException;

namespace Video_Record_Stream
{
    public static class NativeExceptionExtensions
    {
        internal static readonly HashSet<Type> NativeSocketExceptions = new HashSet<Type> { typeof(PlatformSocketException) };

        public static Task WrapNativeSocketExceptions(this Task task)
        {
            return task.ContinueWith(
                t =>
                {

                    if (!t.IsFaulted)
                        return t;
                  
                    var ex = t.Exception?.InnerException ?? t.Exception;

                    throw (NativeExceptionExtensions.NativeSocketExceptions.Contains(ex.GetType())) 
                        ? new PclSocketException(ex)
                        : ex;
                });
        }

        public static Task<T> WrapNativeSocketExceptions<T>(this Task<T> task)
        {
            return task.ContinueWith<T>(
                t =>
                {
                    if (!t.IsFaulted)
                        return t.Result;

                    var ex = t.Exception?.InnerException ?? t.Exception;

                    throw (NativeExceptionExtensions.NativeSocketExceptions.Contains(ex.GetType()))
                        ? new PclSocketException(ex)
                        : ex;
                });
        }
    }
}
