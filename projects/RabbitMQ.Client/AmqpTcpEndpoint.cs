// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 2.0.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (c) 2007-2025 Broadcom. All Rights Reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v2.0:
//
//---------------------------------------------------------------------------
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
//  Copyright (c) 2007-2025 Broadcom. All Rights Reserved.
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RabbitMQ.Client
{
    /// <summary>
    /// Represents a TCP-addressable AMQP peer: a host name and port number.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some of the constructors take, as a convenience, a <see cref="System.Uri"/>
    /// instance representing an AMQP server address. The use of Uri
    /// here is not standardised - Uri is simply a convenient
    /// container for internet-address-like components. In particular,
    /// the Uri "Scheme" property is ignored: only the "Host" and
    /// "Port" properties are extracted.
    /// </para>
    /// </remarks>
    public class AmqpTcpEndpoint
    {
        /// <summary>
        /// Default Amqp ssl port.
        /// </summary>
        public const int DefaultAmqpSslPort = 5671;

        /// <summary>
        /// Indicates that the default port for the protocol should be used.
        /// </summary>
        public const int UseDefaultPort = -1;

        private int _port;

        private readonly uint _maxInboundMessageBodySize;

        /// <summary>
        /// Creates a new instance of the <see cref="AmqpTcpEndpoint"/>.
        /// </summary>
        /// <param name="hostName">Hostname.</param>
        /// <param name="portOrMinusOne"> Port number. If the port number is -1, the default port number will be used.</param>
        /// <param name="ssl">Ssl option.</param>
        /// <param name="maxInboundMessageBodySize">Maximum message size from RabbitMQ.</param>
        public AmqpTcpEndpoint(string hostName, int portOrMinusOne, SslOption ssl,
            uint maxInboundMessageBodySize)
        {
            HostName = hostName;
            _port = portOrMinusOne;
            Ssl = ssl;
            _maxInboundMessageBodySize = Math.Min(maxInboundMessageBodySize,
                InternalConstants.DefaultRabbitMqMaxInboundMessageBodySize);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AmqpTcpEndpoint"/>.
        /// </summary>
        /// <param name="hostName">Hostname.</param>
        /// <param name="portOrMinusOne"> Port number. If the port number is -1, the default port number will be used.</param>
        /// <param name="ssl">Ssl option.</param>
        public AmqpTcpEndpoint(string hostName, int portOrMinusOne, SslOption ssl) :
            this(hostName, portOrMinusOne, ssl, ConnectionFactory.DefaultMaxInboundMessageBodySize)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AmqpTcpEndpoint"/>.
        /// </summary>
        /// <param name="hostName">Hostname.</param>
        /// <param name="portOrMinusOne"> Port number. If the port number is -1, the default port number will be used.</param>
        public AmqpTcpEndpoint(string hostName, int portOrMinusOne = -1) :
            this(hostName, portOrMinusOne, new SslOption())
        {
        }

        /// <summary>
        /// Construct an AmqpTcpEndpoint with "localhost" as the hostname, and using the default port.
        /// </summary>
        public AmqpTcpEndpoint() : this("localhost")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AmqpTcpEndpoint"/> with the given Uri and ssl options.
        /// </summary>
        /// <remarks>
        /// Please see the class overview documentation for information about the Uri format in use.
        /// </remarks>
        public AmqpTcpEndpoint(Uri uri, SslOption ssl) : this(uri.Host, uri.Port, ssl)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AmqpTcpEndpoint"/> with the given Uri.
        /// </summary>
        /// <remarks>
        /// Please see the class overview documentation for information about the Uri format in use.
        /// </remarks>
        public AmqpTcpEndpoint(Uri uri) : this(uri.Host, uri.Port)
        {
        }

        /// <summary>
        /// Clones the endpoint.
        /// </summary>
        /// <returns>A copy with the same hostname, port, and TLS settings</returns>
        public object Clone()
        {
            return new AmqpTcpEndpoint(HostName, _port, Ssl, _maxInboundMessageBodySize);
        }

        /// <summary>
        /// Clones the endpoint using the provided hostname.
        /// </summary>
        /// <param name="hostname">Hostname to use</param>
        /// <returns>A copy with the provided hostname and port/TLS settings of this endpoint</returns>
        public AmqpTcpEndpoint CloneWithHostname(string hostname)
        {
            return new AmqpTcpEndpoint(hostname, _port, Ssl, _maxInboundMessageBodySize);
        }

        /// <summary>
        /// Retrieve or set the hostname of this <see cref="AmqpTcpEndpoint"/>.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>Retrieve or set the port number of this
        ///AmqpTcpEndpoint. A port number of -1 causes the default
        ///port number.</summary>
        public int Port
        {
            get
            {
                if (_port != UseDefaultPort)
                {
                    return _port;
                }
                if (Ssl.Enabled)
                {
                    return DefaultAmqpSslPort;
                }
                return Protocol.DefaultPort;
            }
            set { _port = value; }
        }

        /// <summary>
        /// Retrieve IProtocol of this <see cref="AmqpTcpEndpoint"/>.
        /// </summary>
        public IProtocol Protocol
        {
            get { return Protocols.DefaultProtocol; }
        }

        /// <summary>
        /// Used to force the address family of the endpoint.
        /// Use <see cref="System.Net.Sockets.AddressFamily.InterNetwork" /> to force to IPv4.
        /// Use <see cref="System.Net.Sockets.AddressFamily.InterNetworkV6" /> to force to IPv6.
        /// Or use <see cref="System.Net.Sockets.AddressFamily.Unknown" /> to attempt both IPv6 and IPv4.
        /// </summary>
        public System.Net.Sockets.AddressFamily AddressFamily { get; set; } = ConnectionFactory.DefaultAddressFamily;

        /// <summary>
        /// Retrieve the TLS options for this AmqpTcpEndpoint. If not set, null is returned.
        /// </summary>
        public SslOption Ssl { get; set; }

        /// <summary>
        /// Get the maximum size for a message in bytes. 
        /// The default value is defined in <see cref="ConnectionFactory.DefaultMaxInboundMessageBodySize"/>. 
        /// </summary>
        public uint MaxInboundMessageBodySize
        {
            get { return _maxInboundMessageBodySize; }
        }

        /// <summary>
        /// Construct an instance from a protocol and an address in "hostname:port" format.
        /// </summary>
        /// <remarks>
        /// If the address string passed in contains ":", it is split
        /// into a hostname and a port-number part. Otherwise, the
        /// entire string is used as the hostname, and the port-number
        /// is set to -1 (meaning the default number for the protocol
        /// variant specified).
        /// Hostnames provided as IPv6 must appear in square brackets ([]).
        /// </remarks>
        public static AmqpTcpEndpoint Parse(string address)
        {
            Match match = Regex.Match(address, @"^\s*\[([%:0-9A-Fa-f]+)\](:(.*))?\s*$");
            string port;
            int portNumber;
            if (match.Success)
            {
                GroupCollection groups = match.Groups;
                portNumber = -1;
                if (groups[2].Success)
                {
                    port = groups[3].Value;
                    portNumber = (port.Length == 0) ? -1 : int.Parse(port);
                }
                return new AmqpTcpEndpoint(match.Groups[1].Value, portNumber);
            }
            int index = address.LastIndexOf(':');
            if (index == -1)
            {
                return new AmqpTcpEndpoint(address);
            }
            port = address.Substring(index + 1).Trim();
            portNumber = (port.Length == 0) ? -1 : int.Parse(port);
            return new AmqpTcpEndpoint(address.Substring(0, index), portNumber);
        }

        /// <summary>
        /// Splits the passed-in string on ",", and passes the substrings to <see cref="Parse"/>.
        /// </summary>
        /// <remarks>
        /// Accepts a string of the form "hostname:port,
        /// hostname:port, ...", where the ":port" pieces are
        /// optional, and returns a corresponding array of <see cref="AmqpTcpEndpoint"/>s.
        /// </remarks>
        public static AmqpTcpEndpoint[] ParseMultiple(string addresses)
        {
            string[] partsArr = addresses.Split(',');
            var results = new List<AmqpTcpEndpoint>();
            foreach (string partRaw in partsArr)
            {
                string part = partRaw.Trim();
                if (part.Length > 0)
                {
                    results.Add(Parse(part));
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Compares this instance by value (protocol, hostname, port) against another instance.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (!(obj is AmqpTcpEndpoint other))
            {
                return false;
            }
            if (other.HostName != HostName)
            {
                return false;
            }
            if (other.Port != Port)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Implementation of hash code depending on protocol, hostname and port,
        /// to line up with the implementation of <see cref="Equals"/>.
        /// </summary>
        public override int GetHashCode()
        {
            return HostName.GetHashCode() ^ Port;
        }

        /// <summary>
        /// Returns a URI-like string of the form amqp-PROTOCOL://HOSTNAME:PORTNUMBER.
        /// </summary>
        /// <remarks>
        /// This method is intended mainly for debugging and logging use.
        /// </remarks>
        public override string ToString()
        {
            return $"{(Ssl.Enabled ? "amqps" : "amqp")}://{HostName}:{Port}";
        }
    }
}
