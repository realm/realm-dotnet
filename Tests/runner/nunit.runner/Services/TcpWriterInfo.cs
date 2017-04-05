// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;

namespace NUnit.Runner.Services
{
    /// <summary>
    /// Represents the host and port to connect to
    /// </summary>
    public class TcpWriterInfo : IEquatable<TcpWriterInfo>
    {
        /// <summary>
        /// Constructs a <see cref="TcpWriterInfo"/>
        /// </summary>
        /// <param name="hostName">The host name or IP to connect to</param>
        /// <param name="port">The port to connect to</param>
        /// <param name="timeout">The timeout in seconds</param>
        public TcpWriterInfo(string hostName, int port, int timeout = 10)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if ((port <= 0) || (port > ushort.MaxValue))
            {
                throw new ArgumentException("Must be between 1 and ushort.MaxValue", nameof(port));
            }

            if (timeout <= 0)
            {
                throw new ArgumentException("Must be positive", nameof(timeout));
            }

            Hostname = hostName;
            Port = port;
            Timeout = timeout;
        }

        /// <summary>
        /// The host to connect to
        /// </summary>
        public string Hostname { get; set; }
        
        /// <summary>
        /// The port to connect to
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The connect timeout in seconds
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TcpWriterInfo other) => 
            Hostname.Equals(other.Hostname, StringComparison.OrdinalIgnoreCase) && Port == other.Port;

        /// <summary>
        /// Gets a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Hostname}:{Port}";
    }
}
