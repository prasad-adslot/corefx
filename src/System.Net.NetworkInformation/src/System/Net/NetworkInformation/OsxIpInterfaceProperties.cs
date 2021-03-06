﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Net.NetworkInformation
{
    internal class OsxIpInterfaceProperties : UnixIPInterfaceProperties
    {
        private readonly OsxIPv4InterfaceProperties _ipv4Properties;
        private readonly OsxIPv6InterfaceProperties _ipv6Properties;
        private readonly GatewayIPAddressInformationCollection _gatewayAddresses;

        public OsxIpInterfaceProperties(OsxNetworkInterface oni, int mtu) : base(oni)
        {
            _ipv4Properties = new OsxIPv4InterfaceProperties(oni, mtu);
            _ipv6Properties = new OsxIPv6InterfaceProperties(oni, mtu);
            _gatewayAddresses = GetGatewayAddresses(oni.Index);
        }

        public override IPAddressInformationCollection AnycastAddresses
        {
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        public override IPAddressCollection DhcpServerAddresses
        {
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        public override GatewayIPAddressInformationCollection GatewayAddresses
        {
            get
            {
                return _gatewayAddresses;
            }
        }

        public override bool IsDnsEnabled
        {
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        public override bool IsDynamicDnsEnabled
        {
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        public override IPAddressCollection WinsServersAddresses
        {
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        public override IPv4InterfaceProperties GetIPv4Properties()
        {
            return _ipv4Properties;
        }

        public override IPv6InterfaceProperties GetIPv6Properties()
        {
            return _ipv6Properties;
        }

        private static unsafe GatewayIPAddressInformationCollection GetGatewayAddresses(int interfaceIndex)
        {
            HashSet<IPAddress> addressSet = new HashSet<IPAddress>();
            if (Interop.Sys.EnumerateGatewayAddressesForInterface((uint)interfaceIndex,
                (gatewayAddressInfo) =>
                {
                    byte[] ipBytes = new byte[gatewayAddressInfo->NumAddressBytes];
                    fixed (byte* ipArrayPtr = ipBytes)
                    {
                        Buffer.MemoryCopy(gatewayAddressInfo->AddressBytes, ipArrayPtr, ipBytes.Length, ipBytes.Length);
                    }
                    IPAddress ipAddress = new IPAddress(ipBytes);
                    addressSet.Add(ipAddress);
                }) == -1)
            {
                throw new NetworkInformationException();
            }

            GatewayIPAddressInformationCollection collection = new GatewayIPAddressInformationCollection();
            foreach (IPAddress address in addressSet)
            {
                collection.InternalAdd(new SimpleGatewayIPAddressInformation(address));
            }

            return collection;
        }
    }
}
