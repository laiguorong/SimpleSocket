using System;
using System.Collections.Generic;
using System.Net;
using SimpleSocket.Config;

namespace SimpleSocket.Common
{
    public static class Extensions
    {
        public static T[] CloneRange<T>(this IList<T> source, int offset, int length)
        {
            T[] target;

            var array = source as T[];

            if (array != null)
            {
                target = new T[length];
                Array.Copy(array, offset, target, 0, length);
                return target;
            }

            target = new T[length];

            for (int i = 0; i < length; i++)
            {
                target[i] = source[offset + i];
            }

            return target;
        }
        public static IPEndPoint GetListenEndPoint(this ListenOptions listenOptions)
        {
            var ip = listenOptions.Ip;
            var port = listenOptions.Port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Any;
            }
            else if ("IpV6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.IPv6Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            return new IPEndPoint(ipAddress, port);
        }
    }
}
