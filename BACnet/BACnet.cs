//------------------------------------------------------------------------------------
// <copyright file="BACnet.cs" company="Regel Partners B.V.">
//     All rights are reserved. Reproduction or transmission in whole or in part, in
//     any form or by any means, electronic, mechanical or otherwise, is prohibited
//     without the prior written consent of the copyright owner.
// </copyright>
//------------------------------------------------------------------------------------
namespace RegelPartners.Protocols
{
    // directives
    using System;
    using System.Collections.Generic;
    using System.IO;
	using System.Linq;
	using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Initializes the BACnet class.
    /// </summary>
    public class BACnet
    {
        /// <summary>
        /// Specifies the protocol type.
        /// </summary>
        public enum ProtocolType
        {
            /// <summary>
            /// A BACnet Virtual Network or Ethernet protocol type.
            /// </summary>
            BACnetVirtualNetworkOrEthernet,

            /// <summary>
            /// A BACnet IP protocol type.
            /// </summary>
            BACnetIP,

            /// <summary>
            /// A BACnet MS/TP protocol type.
            /// </summary>
            BACnetMSTP,

            /// <summary>
            /// An unknown protocol type.
            /// </summary>
            Unknown
        };

        /// <summary>
        /// Specifies the object type.
        /// </summary>
        public enum ObjectType
        {
            /// <summary>
            /// An Analog Value object type.
            /// </summary>
            AnalogValue,

            /// <summary>
            /// An Analog Input object type.
            /// </summary>
            AnalogInput,

            /// <summary>
            /// An Binary Value object type.
            /// </summary>
            BinaryValue,

            /// <summary>
            /// An Binary Input object type.
            /// </summary>
            BinaryInput
        }

        /// <summary>
        /// Initializes the Device class.
        /// </summary>
        public class Device
        {
            /// <summary>
            /// Gets or sets the name of the device.
            /// </summary>
            public string Name { get; set; } = "Unknown";

            /// <summary>
            /// Gets or sets the vendor id of the device.
            /// </summary>
            public int VendorId { get; set; }

            /// <summary>
            /// Gets or sets the IP address of the device.
            /// </summary>
            public IPEndPoint IPAddress { get; set; }

            /// <summary>
            /// Gets or sets the IP port of the device.
            /// </summary>
            public int IPPort { get; set; }

            /// <summary>
            /// Gets or sets the network address of the device.
            /// </summary>
            public int NetworkAddress { get; set; }

            /// <summary>
            /// Gets the protocol type of the device.
            /// </summary>
            public ProtocolType ProtocolType { get; } = ProtocolType.Unknown;

            /// <summary>
            /// Gets or sets the instance of the device.
            /// </summary>
            public UInt32 Instance { get; set; }

            /// <summary>
            /// Gets or sets the MAC address of the device.
            /// </summary>
            public UInt64 MacAddress { get; set; }

            /// <summary>
            /// Gets or sets the MAC address length of the device.
            /// </summary>
            public byte MacAddressLength { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class.
            /// </summary>
            public Device()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class (BACnet Virtual Network or BACnet Ethernet).
            /// </summary>
            /// <param name="ipAddress">The IP address of the device.</param>
            /// <param name="ipPort">The IP port of the device.</param>
            /// <param name="networkAddress">The network address of the device.</param>
            /// <param name="instance">The instance of the device.</param>
            public Device(string ipAddress, int ipPort, int networkAddress, int instance) : this()
            {
                // get mac address bytes
                byte[] macAddressBytes = new byte[8];
                byte[] instanceBytes = BitConverter.GetBytes(instance);
                for (int i = 0; i < 4; i++) macAddressBytes[5 - i] = instanceBytes[i];

                // set properties
                this.IPAddress = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), ipPort);
                this.IPPort = ipPort;
                this.MacAddressLength = 6;
                this.ProtocolType = ProtocolType.BACnetVirtualNetworkOrEthernet;
                this.NetworkAddress = networkAddress;
                this.Instance = (uint)instance;
                this.MacAddress = BitConverter.ToUInt64(macAddressBytes, 0);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class (BACnet IP).
            /// </summary>
            /// <param name="ipAddress">The IP address of the device.</param>
            public Device(string ipAddress) : this(ipAddress, DefaultBACnetIPPort, 0)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class (BACnet IP).
            /// </summary>
            /// <param name="ipAddress">The IP address of the device.</param>
            /// <param name="ipPort">The IP port of the device.</param>
            public Device(string ipAddress, int ipPort) : this(ipAddress, ipPort, 0)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class (BACnet IP).
            /// </summary>
            /// <param name="ipAddress">The IP address of the device.</param>
            /// <param name="ipPort">The IP port of the device.</param>
            /// <param name="networkAddress">The network address of the device.</param>
            public Device(string ipAddress, int ipPort, int networkAddress) : this()
            {
                // set properties
                this.IPAddress = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), ipPort);
                this.IPPort = ipPort;
                this.ProtocolType = ProtocolType.BACnetIP;
                this.NetworkAddress = networkAddress;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Device" /> class (BACnet MS/TP).
            /// </summary>
            /// <param name="ipAddress">The IP address of the device.</param>
            /// <param name="ipPort">The IP port of the device.</param>
            /// <param name="networkAddress">The network address of the device.</param>
            /// <param name="macAddress">The MAC address of the device.</param>
            /// <param name="macAddressLength">The source length of the device.</param>
            public Device(string ipAddress, int ipPort, int networkAddress, byte macAddress, byte macAddressLength) : this()
            {
                // set properties
                this.IPAddress = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), ipPort);
                this.IPPort = ipPort;
                this.MacAddressLength = macAddressLength;
                this.ProtocolType = ProtocolType.BACnetMSTP;
                this.NetworkAddress = networkAddress;
                this.MacAddress = macAddress;
            }

            /// <summary>
            /// Converts the value of this instance to its equivalent string representation.
            /// </summary>
            /// <returns></returns>
            public new string ToString()
            {
                // return string
                string result = null;
                switch (this.ProtocolType)
                {
                    case ProtocolType.BACnetIP:
                        result = "Name: " + this.Name + ", Vendor Id: " + this.VendorId.ToString() + ", Protocol Type: " + this.ProtocolType.ToString() + ", IP Address: " + this.IPAddress.ToString() + ", Network Address: " + this.NetworkAddress.ToString();
                        break;
                    case ProtocolType.BACnetMSTP:
                        result = "Name: " + this.Name + ", Vendor Id: " + this.VendorId.ToString() + ", Protocol Type: " + this.ProtocolType.ToString() + ", IP Address: " + this.IPAddress.ToString() + ", Network Address: " + this.NetworkAddress.ToString() + ", MAC Address: " + this.MacAddress.ToString() + ", MAC Address Length: " + this.MacAddressLength.ToString();
                        break;
                    case ProtocolType.BACnetVirtualNetworkOrEthernet:
                        result = "Name: " + this.Name + ", Vendor Id: " + this.VendorId.ToString() + ", Protocol Type: " + this.ProtocolType.ToString() + ", IP Address: " + this.IPAddress.ToString() + ", Network Address: " + this.NetworkAddress.ToString() + ", MAC Address: " + this.MacAddress.ToString() + ", MAC Address Length: " + this.MacAddressLength.ToString() + ", Instance: " + this.Instance.ToString();
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// Initializes the PropertyValue class.
        /// </summary>
        public class PropertyValue
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyValue" /> class.
            /// </summary>
            public PropertyValue()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyValue" /> class.
            /// </summary>
            /// <param name="objectType">The object type of the property value.</param>
            /// <param name="instance">The instance of the property value.</param>
            public PropertyValue(ObjectType objectType, int instance) : this()
            {
                // set properties
                this.ObjectType = objectType;
                this.Instance = instance;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyValue" /> class.
            /// </summary>
            /// <param name="objectType">The object type of the property value.</param>
            /// <param name="instance">The instance of the property value.</param>
            /// <param name="value">The Boolean property value.</param>
            public PropertyValue(ObjectType objectType, int instance, bool value) : this()
            {
                // set properties
                this.ObjectType = objectType;
                this.Instance = instance;
                this.ValueBool = value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyValue" /> class.
            /// </summary>
            /// <param name="objectType">The object type of the property value.</param>
            /// <param name="instance">The instance of the property value.</param>
            /// <param name="value">The Single property value.</param>
            public PropertyValue(ObjectType objectType, int instance, float value) : this()
            {
                // set properties
                this.ObjectType = objectType;
                this.Instance = instance;
                this.ValueSingle = value;
            }

            /// <summary>
            /// Gets or sets the object type of the property value.
            /// </summary>
            public ObjectType ObjectType { get; set; }

            /// <summary>
            /// Gets or sets the instance of the property value.
            /// </summary>
            public int Instance { get; set; }

            /// <summary>
            /// Gets or sets the Boolean property value.
            /// </summary>
            public bool? ValueBool { get; set; }

            /// <summary>
            /// Gets or sets the Single property value.
            /// </summary>
            public float? ValueSingle { get; set; }

            /// <summary>
            /// Converts the value of this instance to its equivalent string representation.
            /// </summary>
            /// <returns></returns>
            public new string ToString()
            {
                // return string
                string result = "%1: %2";
                switch (this.ObjectType)
                {
                    case ObjectType.AnalogValue:
                        result = result.Replace("%1", "AV" + this.Instance.ToString()).Replace("%2", this.ValueSingle.HasValue ? this.ValueSingle.Value.ToString() : "null");
                        break;
                    case ObjectType.AnalogInput:
                        result = result.Replace("%1", "AI" + this.Instance.ToString()).Replace("%2", this.ValueSingle.HasValue ? this.ValueSingle.Value.ToString() : "null");
                        break;
                    case ObjectType.BinaryValue:
                        result = result.Replace("%1", "BV" + this.Instance.ToString()).Replace("%2", this.ValueBool.HasValue ? this.ValueBool.Value.ToString() : "null");
                        break;
                    case ObjectType.BinaryInput:
                        result = result.Replace("%1", "BI" + this.Instance.ToString()).Replace("%2", this.ValueBool.HasValue ? this.ValueBool.Value.ToString() : "null");
                        break;
                }
                return result;
            }
        }

        internal const int DefaultBACnetIPPort = 47808;

        /// <summary>
        /// Gets a device.
        /// </summary>
        /// <param name="objectId">The object id of the device.</param>
        /// <returns></returns>
        public Device GetDevice(int objectId)
        {
            // get device
            return GetDevice(objectId, 5000);
        }

        /// <summary>
        /// Reads a property value.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        public bool ReadPropertyValue(Device device, ref PropertyValue propertyValue)
        {
            // variables
            bool result = false;
            Property property = new Property();

            // validate device
            if (device == null)
            {
                throw new Exception("The BACnet device cannot be null.");
            }

            // read property value
            switch (propertyValue.ObjectType)
            {
                // analog value
                case ObjectType.AnalogValue:
                    result = SendReadProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property);
                    if (result)
                    {
                        propertyValue.ValueSingle = property.ValueSingle;
                    }
                    break;

                // analog input
                case ObjectType.AnalogInput:
                    result = SendReadProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property);
                    if (result)
                    {
                        propertyValue.ValueSingle = property.ValueSingle;
                    }
                    break;

                // binary value
                case ObjectType.BinaryValue:
                    result = SendReadProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property);
                    if (result)
                    {
                        propertyValue.ValueBool = (property.ValueEnum == 1);
                    }
                    break;

                // binary input
                case ObjectType.BinaryInput:
                    result = SendReadProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property);
                    if (result)
                    {
                        propertyValue.ValueBool = (property.ValueEnum == 1);
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Writes a property value (Priority=8).
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="objectType">The object type of the property value.</param>
        /// <param name="instance">The instance of the property value.</param>
        /// <param name="value">The Boolean property value.</param>
        /// <returns></returns>
        public bool WritePropertyValue(Device device, ObjectType objectType, int instance, bool value)
        {
            // write property value
            return WritePropertyValue(device, new PropertyValue(objectType, instance, value));
        }

        /// <summary>
        /// Writes a property value (Priority=8).
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="objectType">The object type of the property value.</param>
        /// <param name="instance">The instance of the property value.</param>
        /// <param name="value">The Single property value.</param>
        /// <returns></returns>
        public bool WritePropertyValue(Device device, ObjectType objectType, int instance, float value)
        {
            // write property value
            return WritePropertyValue(device, new PropertyValue(objectType, instance, value));
        }

        /// <summary>
        /// Writes a property value (Priority=8).
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        public bool WritePropertyValue(Device device, PropertyValue propertyValue)
        {
            // write property value
            return WritePropertyValue(device, propertyValue, 8);
        }

        /// <summary>
        /// Writes a property value.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="priority">The priority of the property.</param>
        /// <returns></returns>
        public bool WritePropertyValue(Device device, PropertyValue propertyValue, int priority)
        {
            // variables
            bool result = false;
            Property property = null;

            // validate device
            if (device == null)
            {
                throw new Exception("The BACnet device cannot be null.");
            }

            // write property value (if failed, retry with out-of-service)
            switch (propertyValue.ObjectType)
            {
                // analog value
                case ObjectType.AnalogValue:
                    property = new Property(propertyValue.ValueSingle.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    if (!result)
                    {
                        WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_VALUE, Enums.BACNET_PROPERTY_ID.PROP_OUT_OF_SERVICE, new Property(true), priority);
                        result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    }
                    break;

                // analog input
                case ObjectType.AnalogInput:
                    property = new Property(propertyValue.ValueSingle.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    if (!result)
                    {
                        WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_INPUT, Enums.BACNET_PROPERTY_ID.PROP_OUT_OF_SERVICE, new Property(true), priority);
                        result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    }
                    break;

                // binary value
                case ObjectType.BinaryValue:
                    property = new Property(propertyValue.ValueBool.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    if (!result)
                    {
                        WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_VALUE, Enums.BACNET_PROPERTY_ID.PROP_OUT_OF_SERVICE, new Property(true), priority);
                        result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    }
                    break;

                // binary input
                case ObjectType.BinaryInput:
                    property = new Property(propertyValue.ValueBool.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    if (!result)
                    {
                        WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_INPUT, Enums.BACNET_PROPERTY_ID.PROP_OUT_OF_SERVICE, new Property(true), priority);
                        result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, priority);
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Writes a Priva property value.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        /// <remarks>This is a wrong implementation of the BACnet protocol.</remarks>
        public bool WritePrivaPropertyValue(Device device, PropertyValue propertyValue)
        {
            // variables
            bool result = false;
            Property property = null;

            // validate device
            if (device == null)
            {
                throw new Exception("The BACnet device cannot be null.");
            }

            // write property value (if failed, retry with out-of-service)
            switch (propertyValue.ObjectType)
            {
                // analog value
                case ObjectType.AnalogValue:
                    property = new Property(propertyValue.ValueSingle.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, 8);
                    break;

                // analog input
                case ObjectType.AnalogInput:
                    property = new Property(propertyValue.ValueSingle.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_ANALOG_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, 8);
                    break;

                // binary value
                case ObjectType.BinaryValue:
                    property = new Property(propertyValue.ValueBool.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_VALUE, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, 8);
                    break;

                // binary input
                case ObjectType.BinaryInput:
                    property = new Property(propertyValue.ValueBool.Value);
                    result = WriteProperty(device, (uint)propertyValue.Instance, Enums.BACNET_OBJECT_TYPE.OBJECT_BINARY_INPUT, Enums.BACNET_PROPERTY_ID.PROP_PRESENT_VALUE, property, 8);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Writes tracing data to a file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="data">The data.</param>
        private void TraceToFile(string source, string data)
        {
            // write tracing data to file
            try
            {
                using (StreamWriter writer = new StreamWriter(About.Filename + ".log", true))
                {
                    writer.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss").PadRight(23) + (source).PadRight(59) + data);
                }
            }
            catch
            {
            }
        }

        #region Code written by Plus 1 Micro (does not comply with Regel Partners coding standards!)
        /**************************************************************************
        *
        * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
        * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
        * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
        * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
        * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
        * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
        * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        *
        *********************************************************************/

        /* COPYRIGHT
         -------------------------------------------
         Copyright (C) 2013-2015 Plus 1 Micro, Inc.

         This program is free software; you can redistribute it and/or
         modify it under the terms of the GNU General Public License
         as published by the Free Software Foundation; either version 2
         of the License, or (at your option) any later version.

         This program is distributed in the hope that it will be useful,
         but WITHOUT ANY WARRANTY; without even the implied warranty of
         MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
         GNU General Public License for more details.

         You should have received a copy of the GNU General Public License
         along with this program; if not, write to:
         The Free Software Foundation, Inc.
         59 Temple Place - Suite 330
         Boston, MA  02111-1307, USA.

         As a special exception, if other files instantiate templates or
         use macros or inline functions from this file, or you compile
         this file and link it with other works to produce a work based
         on this file, this file does not by itself cause the resulting
         work to be covered by the GNU General Public License. However
         the source code for this file must still be made available in
         accordance with section (3) of the GNU General Public License.

         This exception does not invalidate any other reasons why a work
         based on this file might be covered by the GNU General Public
         License.
         -------------------------------------------
        */

        private UdpClient SendUDP = null; // = new UdpClient(UDPPort);
        private UdpClient ReceiveUDP = null; // = new UdpClient(UDPPort, AddressFamily.InterNetwork);

        private IPEndPoint LocalEP = null;
        private IPEndPoint BroadcastEP = null;
        //IPEndPoint RemoteEP = null;

        //private const int UDPPort = 47808;
        private bool TimerDone = false;
        private int InvokeCounter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BACnet" /> class.
        /// </summary>
        public BACnet()
        {
            new BACnet(null, DefaultBACnetIPPort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BACnet" /> class.
        /// </summary>
        /// <param name="localIPAddress">The local IP address.</param>
        /// <param name="localIPPort">The local IP port.</param>
        public BACnet(string localIPAddress, int localIPPort)
        {
            // Machine dependent (little endian vs big endian)
            // In this case we have to reverse the bytes for the Server IP
            byte[] maskbytes = new byte[4];
            byte[] addrbytes = new byte[4];

            //byte[] addr = IPAddress.Parse(server).GetAddressBytes();
            //if (BitConverter.IsLittleEndian) 
            //  Array.Reverse(addr);
            //Server = BitConverter.ToUInt32(addr, 0);

            //if (WinSockStartUp() < 1)
            //  MessageBox.Show("Socket StartUp Error " + WinSockLastError().ToString());

            // Find the local IP address and Subnet Mask
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface Interface in Interfaces)
            {
                if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                //MessageBox.Show(Interface.Description);
                UnicastIPAddressInformationCollection UnicastIPInfoCol = Interface.GetIPProperties().UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    //MessageBox.Show("\tIP Address is {0}" + UnicatIPInfo.Address);
                    //MessageBox.Show("\tSubnet Mask is {0}" + UnicatIPInfo.IPv4Mask);
                    if (UnicatIPInfo.IPv4Mask != null)
                    {
                        byte[] tempbytes = UnicatIPInfo.IPv4Mask.GetAddressBytes();
                        if (tempbytes[0] == 255)
                        {
                            // We found the correct subnet mask, and probably the correct IP address
                            addrbytes = UnicatIPInfo.Address.GetAddressBytes();
                            maskbytes = UnicatIPInfo.IPv4Mask.GetAddressBytes();
                            break;
                        }
                    }
                }
            }
            // Set up broadcast address
            if (maskbytes[3] == 0) maskbytes[3] = 255; else maskbytes[3] = addrbytes[3];
            if (maskbytes[2] == 0) maskbytes[2] = 255; else maskbytes[2] = addrbytes[2];
            if (maskbytes[1] == 0) maskbytes[1] = 255; else maskbytes[1] = addrbytes[1];
            if (maskbytes[0] == 0) maskbytes[0] = 255; else maskbytes[0] = addrbytes[0];

            // 05/12/2016 (RP:JT): Use specified local IP Address (when provided)
            //IPAddress myip = new IPAddress(addrbytes);
            IPAddress myip = new IPAddress(addrbytes);
            if (!string.IsNullOrEmpty(localIPAddress))
            {
                myip = IPAddress.Parse(localIPAddress);
            }
            IPAddress broadcast = new IPAddress(maskbytes);

            LocalEP = new IPEndPoint(myip, localIPPort);
            BroadcastEP = new IPEndPoint(broadcast, localIPPort);

            SendUDP = new UdpClient();
            SendUDP.ExclusiveAddressUse = false;
            SendUDP.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            SendUDP.Client.Bind(LocalEP);

            ReceiveUDP = new UdpClient(localIPPort, AddressFamily.InterNetwork);

            // Init the Devices list
            Data.Devices = new List<Device>();
        }

		/// <summary>
		/// Gets a device.
		/// </summary>
		/// <param name="objectId">The object id of the device.</param>
		/// <param name="timeout">The time-out value in milliseconds.</param>
		/// <returns></returns>
		public Device GetDevice(int objectId, int timeout)
        {
            Device result = null;
			// Get the host data, send a Who-Is, accept responses and save in the DeviceList
			//ulong ipaddr = 0;
			//int count = 0;

			// Convert object ID to hexcode
			string hex = objectId.ToString("x6");
			Byte[] sendBytes = new Byte[20];
            Byte[] recvBytes = new Byte[512];

            // Dns stuff obsoleted ...
            //string hostname = Dns.GetHostName();
            //IPHostEntry host = Dns.GetHostByName(hostname);
            //IPHostEntry host = Dns.GetHostEntry(hostname);

            Data.Devices.Clear();

            // Send the request
            //MessageBox.Show("Send Who-Is (" + broadcast + ")");
            //MessageBox.Show("Send Who-Is");

            // Create the timer
            Timer IAmTimer = new Timer();
            using (IAmTimer)
            {
                IAmTimer.Tick += new EventHandler(Timer_Tick);

                try
                {
                    //PEP Use NPDU.Create and APDU.Create (when written)
                    sendBytes[0] = Enums.BACNET_BVLC_TYPE_BIP;
                    sendBytes[1] = Enums.BACNET_UNICAST_NPDU;
                    sendBytes[2] = 0;
                    sendBytes[3] = 20; // Checksum for number of bytes to send
                    sendBytes[4] = Enums.BACNET_PROTOCOL_VERSION;
                    sendBytes[5] = 0x20;  // Control flags
                    sendBytes[6] = 0xFF;  // Destination network address (65535)
                    sendBytes[7] = 0xFF;
                    sendBytes[8] = 0;     // Destination MAC layer address length, 0 = Broadcast
                    sendBytes[9] = 0xFF;  // Hop count = 255

                    sendBytes[10] = (Byte)Enums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST;
                    sendBytes[11] = (Byte)Enums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS;

					sendBytes[12] = (Byte)0x0b; // Low
					sendBytes[13] = (Byte)Convert.ToByte(hex.Substring(0, 2), 16);
					sendBytes[14] = (Byte)Convert.ToByte(hex.Substring(2, 2), 16);
					sendBytes[15] = (Byte)Convert.ToByte(hex.Substring(4, 2), 16);
					sendBytes[16] = (Byte)0x1b; // High
					sendBytes[17] = (Byte)Convert.ToByte(hex.Substring(0, 2), 16);
					sendBytes[18] = (Byte)Convert.ToByte(hex.Substring(2, 2), 16);
					sendBytes[19] = (Byte)Convert.ToByte(hex.Substring(4, 2), 16);
					sendBytes[3] = 20;




					//ipaddr = 0xC0A85CFF; // 192.168.92.FF
					//if (WinSockSendTo(sendBytes, 12, ipaddr) < 1)
					//{
					//  MessageBox.Show("Socket Send Error " + WinSockLastError().ToString());
					//  return;
					//}
					// Send the broadcast "who-is"
					//SendUDP.EnableBroadcast = true;
					//SendUDP.Connect(broadcast, UDPPort);
					SendUDP.EnableBroadcast = true;
                    SendUDP.Send(sendBytes, (20), BroadcastEP);

                    Socket sock = ReceiveUDP.Client;
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // Start the timer so we can receive multiple responses
                    TimerDone = false;
                    IAmTimer.Interval = timeout;
                    IAmTimer.Start();
                    while (!TimerDone)
                    {
                        Application.DoEvents();

                        // Process the response packets
                        //if (WinSockRecvReady() > 0)
                        //{
                        //  if (WinSockRecvFrom(recvBytes, ref count, ref ipaddr) > 0)
                        // Process the response packets
                        if (sock.Available > 0)
                        {
                            recvBytes = ReceiveUDP.Receive(ref RemoteIpEndPoint);
                            {
                                // Parse and save the BACnet data
                                int APDUOffset = NPDU.Parse(recvBytes, 4); // BVLL is always 4 bytes
                                if (APDU.ParseIAm(recvBytes, APDUOffset) > 0)
                                {
                                    if (APDU.ObjectID == objectId)
                                    {
                                        result = new Device();
                                        result.Name = "Device";
                                        result.MacAddressLength = NPDU.SLEN;
                                        result.IPAddress = RemoteIpEndPoint;
                                        result.NetworkAddress = NPDU.SNET;
                                        result.MacAddress = NPDU.SAddress;
                                        result.Instance = APDU.ObjectID;
                                        break;
                                    }
                                    // We should now have enough info to read/write properties for this device
                                }
                            }
                            // Restart the timer - as long as I-AM packets come, we'll wait
                            IAmTimer.Stop();
                            IAmTimer.Start();
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceToFile(null, e.ToString());
                }
                finally
                {
                    IAmTimer.Stop();
                }
                return result;
            }
        }

        // Bind Device Instance to the BACnet Address (we need SNET, SLEN, SADR, etc)
        private bool /*BACnetStack*/ BindBACnetDevice(UInt32 instance, ref int devidx)
        {
            // Linear (brute force) search for now
            for (int i = 0; i < Data.Devices.Count; i++)
            {
                Device dev = Data.Devices[i];
                if (instance == dev.Instance)
                {
                    devidx = i;
                    return true;
                }
            }
            return false;
        }

        // Timer Event for the Socket I/O
        private void /*BACnetStack*/ Timer_Tick(object sender, EventArgs e)
        {
            TimerDone = true;
        }

        private bool /*BACnetStack*/ GetIAm(int network, UInt32 objectid)
        {
            // Wait for I-Am packet
            Byte[] recvBytes = new Byte[512];
            bool found = false;

            // Create the timer
            Timer IAmTimer = new Timer();
            using (IAmTimer)
            {
                IAmTimer.Tick += new EventHandler(Timer_Tick);

                try
                {
                    Socket sock = ReceiveUDP.Client;
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // Start the timer
                    TimerDone = false;
                    IAmTimer.Interval = 1000;
                    IAmTimer.Start();
                    while (!TimerDone && !found)
                    {
                        Application.DoEvents();

                        // Process receive packets
                        if (sock.Available > 0)
                        {
                            recvBytes = ReceiveUDP.Receive(ref RemoteIpEndPoint);
                            {
                                // Parse the packet - is it IAm?
                                int APDUOffset = NPDU.Parse(recvBytes, 4); // BVLL is always 4 bytes
                                if (APDU.ParseIAm(recvBytes, APDUOffset) > 0)
                                {
                                    if ((network == NPDU.SNET) && (objectid == APDU.ObjectID))
                                    {
                                        // Found it!
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceToFile(null, e.ToString());
                }
                finally
                {
                    IAmTimer.Stop();
                }
            }
            return found;
        }

        // Do a Who-Is, and collect information about who answers -------------------------------------
        public void  /*BACnetStack*/ GetDevices(int milliseconds)
        {
            // Get the host data, send a Who-Is, accept responses and save in the DeviceList
            //ulong ipaddr = 0;
            //int count = 0;
            Byte[] sendBytes = new Byte[12];
            Byte[] recvBytes = new Byte[512];

            // Dns stuff obsoleted ...
            //string hostname = Dns.GetHostName();
            //IPHostEntry host = Dns.GetHostByName(hostname);
            //IPHostEntry host = Dns.GetHostEntry(hostname);

            Data.Devices.Clear();

            // Send the request
            //MessageBox.Show("Send Who-Is (" + broadcast + ")");
            //MessageBox.Show("Send Who-Is");

            // Create the timer
            Timer IAmTimer = new Timer();
            using (IAmTimer)
            {
                IAmTimer.Tick += new EventHandler(Timer_Tick);

                try
                {
                    //PEP Use NPDU.Create and APDU.Create (when written)
                    sendBytes[0] = Enums.BACNET_BVLC_TYPE_BIP;
                    sendBytes[1] = Enums.BACNET_UNICAST_NPDU;
                    sendBytes[2] = 0;
                    sendBytes[3] = 12;
                    sendBytes[4] = Enums.BACNET_PROTOCOL_VERSION;
                    sendBytes[5] = 0x20;  // Control flags
                    sendBytes[6] = 0xFF;  // Destination network address (65535)
                    sendBytes[7] = 0xFF;
                    sendBytes[8] = 0;     // Destination MAC layer address length, 0 = Broadcast
                    sendBytes[9] = 0xFF;  // Hop count = 255

                    sendBytes[10] = (Byte)Enums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST;
                    sendBytes[11] = (Byte)Enums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS;

                    //ipaddr = 0xC0A85CFF; // 192.168.92.FF
                    //if (WinSockSendTo(sendBytes, 12, ipaddr) < 1)
                    //{
                    //  MessageBox.Show("Socket Send Error " + WinSockLastError().ToString());
                    //  return;
                    //}
                    // Send the broadcast "who-is"
                    //SendUDP.EnableBroadcast = true;
                    //SendUDP.Connect(broadcast, UDPPort);
                    SendUDP.EnableBroadcast = true;
                    SendUDP.Send(sendBytes, 12, BroadcastEP);

                    Socket sock = ReceiveUDP.Client;
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // Start the timer so we can receive multiple responses
                    TimerDone = false;
                    IAmTimer.Interval = milliseconds;
                    IAmTimer.Start();
                    while (!TimerDone)
                    {
                        Application.DoEvents();

                        // Process the response packets
                        //if (WinSockRecvReady() > 0)
                        //{
                        //  if (WinSockRecvFrom(recvBytes, ref count, ref ipaddr) > 0)
                        // Process the response packets
                        if (sock.Available > 0)
                        {
                            recvBytes = ReceiveUDP.Receive(ref RemoteIpEndPoint);
                            {
                                // Parse and save the BACnet data
                                int APDUOffset = NPDU.Parse(recvBytes, 4); // BVLL is always 4 bytes
                                if (APDU.ParseIAm(recvBytes, APDUOffset) > 0)
                                {
                                    Device device = new Device();
                                    device.Name = "Device";
                                    device.MacAddressLength = NPDU.SLEN;
                                    device.IPAddress = RemoteIpEndPoint;
                                    device.NetworkAddress = NPDU.SNET;
                                    device.MacAddress = NPDU.SAddress;
                                    device.Instance = APDU.ObjectID;
                                    Data.Devices.Add(device);

                                    // We should now have enough info to read/write properties for this device
                                }
                            }
                            // Restart the timer - as long as I-AM packets come, we'll wait
                            IAmTimer.Stop();
                            IAmTimer.Start();
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceToFile(null, e.ToString());
                }
                finally
                {
                    IAmTimer.Stop();
                }
            }
        }
        private bool WriteProperty(Device device, uint instance, Enums.BACNET_OBJECT_TYPE objtype, Enums.BACNET_PROPERTY_ID objprop, Property property, int priority)
        {
            IPEndPoint remoteEP = device.IPAddress;
            if (remoteEP == null) return false;

            if (property == null) return false;

            Byte[] sendBytes = new Byte[50];
            Byte[] recvBytes = new Byte[512];
            uint len;
            //int count = 0;

            // BVLL
            sendBytes[0] = Enums.BACNET_BVLC_TYPE_BIP;
            sendBytes[1] = Enums.BACNET_UNICAST_NPDU;
            sendBytes[2] = 0x00;
            sendBytes[3] = 0x00;  // BVLL Length = 24?

            // NPDU
            sendBytes[4] = Enums.BACNET_PROTOCOL_VERSION;
            if (device.MacAddressLength == 0)
                sendBytes[5] = 0x04;  // Control flags, no destination address
            else
                sendBytes[5] = 0x24;  // Control flags, with broadcast or destination

            len = 6;
            if (device.MacAddressLength > 0)
            {
                // Get the (MSTP) Network number (2001)
                //sendBytes[6] = 0x07;  // Destination network address (2001)
                //sendBytes[7] = 0xD1;
                byte[] temp2 = new byte[2];
                temp2 = BitConverter.GetBytes(device.NetworkAddress);
                sendBytes[len++] = temp2[1];
                sendBytes[len++] = temp2[0];

                // Get the MAC address (0x0D)
                //sendBytes[8] = 0x01;  // MAC address length
                //sendBytes[9] = 0x0D;  // Destination MAC layer address
                byte[] temp4 = new byte[4];
                temp4 = BitConverter.GetBytes(device.MacAddress);

                // 15/01/2016 (RP:ArP/JT): Adjusted MAC address length

                //sendBytes[len++] = 0x01;  // MAC address length - adjust for other lengths ...
                //sendBytes[len++] = temp4[0];
                sendBytes[len++] = device.MacAddressLength;  // MAC address length - adjust for other lengths ...
                for (byte x = device.MacAddressLength; x > 0; x--)
                {
                    sendBytes[len++] = temp4[x - 1];
                }

                sendBytes[len++] = 0xFF;  // Hop count = 255
            }

            // APDU
            sendBytes[len++] = 0x00;  // Control flags
            sendBytes[len++] = 0x05;  // Max APDU length (1476)

            // Create invoke counter
            //sendBytes[len++] = InvokeCounter++;  // Invoke ID
            sendBytes[len++] = (byte)(InvokeCounter);
            InvokeCounter = ((InvokeCounter + 1) & 0xFF);

            sendBytes[len++] = 0x0F;  // Service Choice: Write Property request

            // Service Request (var part of APDU):
            // Set up Object ID (Context Tag)
            len = APDU.SetObjectID(ref sendBytes, len, objtype, instance);

            // Set up Property ID (Context Tag)
            len = APDU.SetPropertyID(ref sendBytes, len, objprop);

            // Set the value to send
            len = APDU.SetProperty(ref sendBytes, len, property);

            //PEP Optional array index goes here

            // Set priority
            if (priority > 0)
                len = APDU.SetPriority(ref sendBytes, len, priority);

            // Fix the BVLL length
            sendBytes[3] = (byte)len;

            // Create the timer (we could use a blocking recvFrom instead ...)
            Timer ReadPropTimer = new Timer();

            try
            {
                using (ReadPropTimer)
                {
                    int Count = 0;
                    ReadPropTimer.Tick += new EventHandler(Timer_Tick);

                    while (Count < 2)
                    {
                        SendUDP.EnableBroadcast = false;
                        SendUDP.Send(sendBytes, (int)len, remoteEP);

                        // Start the timer
                        TimerDone = false;
                        ReadPropTimer.Interval = 200; // 300;  // 100 ms
                        ReadPropTimer.Start();
                        while (!TimerDone)
                        {
                            // Wait for Confirmed Response
                            Application.DoEvents();

                            if (SendUDP.Client.Available > 0)
                            {
                                //recvBytes = SendUDP.Receive(ref RemoteEP);
                                recvBytes = SendUDP.Receive(ref remoteEP);

                                int APDUOffset = NPDU.Parse(recvBytes, 4); // BVLL is always 4 bytes
                                                                           // Check for APDU response, and decide what to do
                                                                           // 0x - Confirmed Request 
                                                                           // 1x - Un-Confirmed Request
                                                                           // 2x - Simple ACK
                                                                           // 3x - Complex ACK
                                                                           // 4x - Segment ACK
                                                                           // 5x - Error
                                                                           // 6x - Reject
                                                                           // 7x - Abort
                                if (recvBytes[APDUOffset] == 0x20)
                                {
                                    // Verify the Invoke ID is the same
                                    byte ic = (byte)(InvokeCounter == 0 ? 255 : InvokeCounter - 1);
                                    if (ic == recvBytes[APDUOffset + 1])
                                    {
                                        return true; // This will still execute the finally
                                    }
                                    //else
                                    //{
                                    //  MessageBox.Show("Invoke Counter Error");
                                    //  return false;
                                    //}
                                }
                            }
                        }
                        Count++;
                        Data.PacketRetryCount++;
                        ReadPropTimer.Stop(); // We'll start it over at the top of the loop
                    }
                    return false; // This will still execute the finally
                }
            }
            finally
            {
                ReadPropTimer.Stop();
            }
        }

        private bool /*BACnetStack*/ SendReadProperty(Device device, uint instance, Enums.BACNET_OBJECT_TYPE objtype, Enums.BACNET_PROPERTY_ID objprop, Property property)

        //out string value)
        // Parameters:
        //   Device index (for network and MAC address),
        //   Object Type, 
        //   Property ID,
        //   Value returned
        {
            // Create and send an Confirmed Request

            //value = "(none)";
            IPEndPoint remoteEP = device.IPAddress;
            if (remoteEP == null) return false;

            if (property == null) return false;

            //uint instance = BACnetData.Devices[deviceidx].Instance;

            Byte[] sendBytes = new Byte[50];
            Byte[] recvBytes = new Byte[512];
            uint len;

            // BVLL
            sendBytes[0] = Enums.BACNET_BVLC_TYPE_BIP;
            sendBytes[1] = Enums.BACNET_UNICAST_NPDU;
            sendBytes[2] = 0x00;
            sendBytes[3] = 0x00;  // BVLL Length, fix later (24?)

            // NPDU
            sendBytes[4] = Enums.BACNET_PROTOCOL_VERSION;
            if (device.MacAddressLength == 0)
                sendBytes[5] = 0x04;  // Control flags, no destination address
            else
                sendBytes[5] = 0x24;  // Control flags, with broadcast or destination address

            len = 6;
            if (device.MacAddressLength > 0)
            {
                // Get the (MSTP) Network number (2001)
                //sendBytes[6] = 0x07;  // Destination network address (2001)
                //sendBytes[7] = 0xD1;
                byte[] temp2 = new byte[2];
                temp2 = BitConverter.GetBytes(device.NetworkAddress);
                sendBytes[len++] = temp2[1];
                sendBytes[len++] = temp2[0];

                // Get the MAC address (0x0D)
                //sendBytes[8] = 0x01;  // MAC address length
                //sendBytes[9] = 0x0D;  // Destination MAC layer address
                byte[] temp4 = new byte[4];
                temp4 = BitConverter.GetBytes(device.MacAddress);

                // 15/01/2016 (RP:ArP/JT): Adjusted MAC address length

                //sendBytes[len++] = 0x01;  // MAC address length - adjust for other lengths ...
                sendBytes[len++] = device.MacAddressLength;  // MAC address length - adjust for other lengths ...
                for (byte x = device.MacAddressLength; x > 0; x--)
                {
                    sendBytes[len++] = temp4[x - 1];
                }

                // sendBytes[len++] = temp4[0];
                sendBytes[len++] = 0xFF;  // Hop count = 255
            }

            // APDU
            sendBytes[len++] = 0x00;  // Control flags
            sendBytes[len++] = 0x05;  // Max APDU length (1476)

            // Create invoke counter
            sendBytes[len++] = (byte)(InvokeCounter);
            InvokeCounter = ((InvokeCounter + 1) & 0xFF);

            sendBytes[len++] = 0x0C;  // Service Choice: Read Property request

            // Service Request (var part of APDU):
            // Set up Object ID (Context Tag)
            len = APDU.SetObjectID(ref sendBytes, len, objtype, instance);

            // Set up Property ID (Context Tag)
            len = APDU.SetPropertyID(ref sendBytes, len, objprop);

            // Fix the BVLL length
            sendBytes[3] = (byte)len;

            // Create the timer (we could use a blocking recvFrom instead ...)
            Timer ReadPropTimer = new Timer();
            try
            {
                int Count = 0;
                using (ReadPropTimer)
                {
                    ReadPropTimer.Tick += new EventHandler(Timer_Tick);

                    while (Count < 2)
                    {
                        SendUDP.EnableBroadcast = false;
                        SendUDP.Send(sendBytes, (int)len, remoteEP);

                        // Start the timer
                        TimerDone = false;
                        ReadPropTimer.Interval = 200;  // 100 ms
                        ReadPropTimer.Start();
                        while (!TimerDone)
                        {
                            // Wait for Confirmed Response
                            Application.DoEvents();

                            if (SendUDP.Client.Available > 0)
                            {
                                //recvBytes = SendUDP.Receive(ref RemoteEP);
                                recvBytes = SendUDP.Receive(ref remoteEP);

                                int APDUOffset = NPDU.Parse(recvBytes, 4); // BVLL is always 4 bytes

                                // Check for APDU response 
                                // 0x - Confirmed Request 
                                // 1x - Un-Confirmed Request
                                // 2x - Simple ACK
                                // 3x - Complex ACK
                                // 4x - Segment ACK
                                // 5x - Error
                                // 6x - Reject
                                // 7x - Abort
                                if (recvBytes[APDUOffset] == 0x30)
                                {
                                    // Verify the Invoke ID is the same
                                    byte ic = (byte)(InvokeCounter == 0 ? 255 : InvokeCounter - 1);
                                    if (ic == recvBytes[APDUOffset + 1])
                                    {
                                        APDU.ParseProperty(ref recvBytes, APDUOffset, property);
                                        return true;  // This will still execute the finally
                                    }
                                    //else
                                    //{
                                    //  MessageBox.Show("Invoke Counter Error");
                                    //  return false;
                                    //}
                                }
                            }
                        }
                        Count++;
                        Data.PacketRetryCount++;
                        ReadPropTimer.Stop(); // We'll start it over at the top of the loop
                    }
                    return false;  // This will still execute the finally
                }
            }
            finally
            {
                ReadPropTimer.Stop();
            }
        }

        //===============================================================================================
        // A .NET Implementaion of the BACnet Stack
        // Class Abstract:
        //    A BACnetStack is a Client-Server interface protocol implementation of the BACnet 
        //    Specification, allowing a connection between the Appication Layer and devices
        //    Application Entity => Application Layer <--> devices
        //    Specifically, it is a "BACnet User Element"
        //    Members include:
        //      Packet Creation and Processing
        //      Services (Who-Is, I-Am, ReadProperty, WriteProperty, Reject, etc)
        //      Objects (Device, etc)
        //      Network Layer Protocol
        //      Application Layer Protocol
        //      Transactions

        //-----------------------------------------------------------------------------------------------
        // BACnetTag Routines
        internal static class Tag
        {
            internal static byte TagNumber(byte tag)
            {
                int x = ((int)tag >> 4) & 0x0F;
                return (byte)x;
            }

            internal static byte Class(byte tag)
            {
                int x = ((int)tag >> 3) & 0x01;
                return (byte)x;
            }
            internal static byte LenValType(byte tag)
            {
                int x = (int)tag & 0x07;
                return (byte)x;
            }
        }

        //-----------------------------------------------------------------------------------------------
        // NPDU Routines
        internal static class NPDU
        {
            internal static byte PDUControl;
            internal static UInt16 DNET;
            internal static byte DLEN;
            internal static byte[] DADR;
            internal static UInt16 SNET;
            internal static byte SLEN;
            internal static byte[] SADR;
            internal static byte HopCount;
            internal static byte MessageType;
            internal static UInt16 VendorID;
            internal static UInt32 DAddress;
            internal static UInt32 SAddress;

            internal static void /*NPDU*/ Clear()
            {
                // Clear the packet members
                PDUControl = 0;
                DNET = 0;
                DLEN = 0;
                DADR = null;
                SNET = 0;
                SLEN = 0;
                SADR = null;
                HopCount = 0;
                MessageType = 0;
                VendorID = 0;
                DAddress = 0;
                SAddress = 0;
            }

            internal static int /*NPDU*/ Assemble(byte[] bytes, int offset)
            {
                // Create a NPUD packet in the bytes array, starting at offset given
                // Return the length
                int len = 0;
                return len;
            }

            internal static int /*NPDU*/ Parse(byte[] bytes, int offset)
            {
                // Returns the Length of the NPDU portion of the packet (offset of APDU)
                // We assume the BVLL is always present, so the NPDU always starts at offset 4
                int len = offset; // 4
                byte[] temp;
                Clear();
                if (bytes[len++] != 0x01) return 0;
                PDUControl = bytes[len++];  // 5
                if ((PDUControl & 0x20) > 0)
                {
                    // We have a Destination 
                    temp = new byte[2];
                    temp[1] = bytes[len++];
                    temp[0] = bytes[len++];
                    DNET = BitConverter.ToUInt16(temp, 0);
                    DLEN = bytes[len++];
                    if (DLEN == 1)
                    {
                        DADR = new byte[1];
                        DADR[0] = bytes[len++];
                        DAddress = (UInt32)DADR[0];
                    }
                    if (DLEN == 2)
                    {
                        temp = new byte[2];
                        DADR[1] = bytes[len++];
                        DADR[0] = bytes[len++];
                        DAddress = (UInt32)BitConverter.ToUInt16(DADR, 0);
                    }
                    if (DLEN == 4)
                    {
                        temp = new byte[4];
                        DADR[3] = bytes[len++];
                        DADR[2] = bytes[len++];
                        DADR[1] = bytes[len++];
                        DADR[0] = bytes[len++];
                        DAddress = BitConverter.ToUInt32(DADR, 0);
                    }
                    //PEP Other DLEN values ...

                }
                else
                    DLEN = 0;

                if ((PDUControl & 0x08) > 0)
                {
                    // We have a Source
                    temp = new byte[2];
                    temp[1] = bytes[len++];
                    temp[0] = bytes[len++];
                    SNET = BitConverter.ToUInt16(temp, 0);
                    SLEN = bytes[len++];
                    if (SLEN == 1)
                    {
                        SADR = new byte[1];
                        SADR[0] = bytes[len++];
                        SAddress = (UInt32)SADR[0];
                    }
                    if (SLEN == 2)
                    {
                        SADR = new byte[2];
                        SADR[1] = bytes[len++];
                        SADR[0] = bytes[len++];
                        SAddress = (UInt32)BitConverter.ToUInt16(SADR, 0);
                    }
                    if (SLEN == 4)
                    {
                        SADR = new byte[4];
                        SADR[3] = bytes[len++];
                        SADR[2] = bytes[len++];
                        SADR[1] = bytes[len++];
                        SADR[0] = bytes[len++];
                        SAddress = BitConverter.ToUInt32(SADR, 0);
                    }
                    if (SLEN == 6)
                    {
                        SADR = new byte[6];
                        SADR[5] = bytes[len++];
                        SADR[4] = bytes[len++];
                        SADR[3] = bytes[len++];
                        SADR[2] = bytes[len++];
                        SADR[1] = bytes[len++];
                        SADR[0] = bytes[len++];
                        SAddress = BitConverter.ToUInt32(SADR, 0);
                    }
                    if (SLEN == 8)
                    {
                        SADR = new byte[7];
                        SADR[7] = bytes[len++];
                        SADR[6] = bytes[len++];
                        SADR[5] = bytes[len++];
                        SADR[4] = bytes[len++];
                        SADR[3] = bytes[len++];
                        SADR[2] = bytes[len++];
                        SADR[1] = bytes[len++];
                        SADR[0] = bytes[len++];
                        SAddress = BitConverter.ToUInt32(SADR, 0);
                    }

                    //PEP Other SLEN values ...
                }
                else
                    SLEN = 0;

                if ((PDUControl & 0x20) > 0)
                {
                    HopCount = bytes[len++];  // Get the Hop Count 
                }

                /*
                      if ((PDUControl & 0x80) > 0)
                      {
                        MessageType = bytes[len + offset];
                        len++;                  // Message Type field
                          if (MessageType >= 0x80)
                          {
                            temp = new byte[2];
                            temp[0] = bytes[len + offset + 2];
                            temp[1] = bytes[len + offset + 1];
                            VendorID = BitConverter.ToUInt16(temp, 0);
                            len += 2;             // VendorID field
                          }
                        }
                        len += offset;
                      }
                */
                return len;
            }

        }

        //-----------------------------------------------------------------------------------------------
        // Property Class
        internal class Property
        {
            internal Enums.BACNET_APPLICATION_TAG Tag { get; set; }
            internal bool ValueBool { get; set; }
            internal uint ValueUInt { get; set; }
            internal int ValueInt { get; set; }
            internal float ValueSingle { get; set; }
            internal double ValueDouble { get; set; }
            internal byte[] ValueOctet { get; set; }
            internal string ValueString { get; set; }
            internal uint ValueEnum { get; set; }
            internal Enums.BACNET_OBJECT_TYPE ValueObjectType { get; set; }
            internal uint ValueObjectInstance { get; set; }
            internal string ToStringValue;
            //PEP Others ...

            // Constructors
            internal Property()
            {
                this.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_NULL;
                this.ValueBool = false;
                this.ValueUInt = 0;
                this.ValueInt = 0;
                this.ValueSingle = 0;
                this.ValueDouble = 0;
                this.ValueOctet = null;
                this.ValueString = "";
                this.ValueEnum = 0;
                this.ValueObjectType = Enums.BACNET_OBJECT_TYPE.OBJECT_DEVICE;
                this.ValueObjectInstance = 0;
                this.ToStringValue = "";
            }

            internal Property(float value) : this()
            {
                // set properties
                this.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_REAL;
                this.ValueSingle = value;
            }

            internal Property(bool value) : this()
            {
                // set properties
                this.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED;
                this.ValueEnum = (uint)(value ? 1 : 0);
            }
        }

        //-----------------------------------------------------------------------------------------------
        // APDU Routines
        internal static class APDU
        {
            internal static byte APDUType = 0;
            internal static UInt16 ObjectType = 0;
            internal static UInt32 ObjectID = 0;

            internal static int /*APDU*/ ParseIAm(byte[] bytes, int offset)
            {
                // Look for and parse I-Am Packet
                int len = 0;
                ObjectID = 0;
                APDUType = bytes[offset];
                if ((APDUType == 0x10) && (bytes[offset + 1] == 0x00))
                {
                    // Get the ObjectID
                    if (Tag.TagNumber(bytes[offset + 2]) != 12)
                        return 0;
                    byte[] temp = new byte[4];
                    temp[0] = bytes[offset + 6];
                    temp[1] = bytes[offset + 5];
                    temp[2] = (byte)((int)bytes[offset + 4] & 0x3F);
                    temp[3] = 0;
                    ObjectID = BitConverter.ToUInt32(temp, 0);
                    len = 5; //PEP Make the APDU length ...
                    return len;
                }
                else
                    return 0;
            }

            internal static int /*APDU*/ ParseRead(byte[] bytes, int offset, out int apptag)
            {
                // Look for and parse Read Property Complex ACK 
                apptag = 0xFF;
                int len = offset;
                if (bytes[len] != 0x30) return 0;   // APDU Complex ACK
                len += 2;
                if (bytes[len++] != 0x0C) return 0; // Read Property ACK

                //PEP Parse the Object ID
                //PEP 5 Bytes for Binary Object: 0x0C 0x00 0x0C 0x00 0x01
                //byte[] temp = new byte[4];
                //temp[0] = bytes[offset + 6];
                //temp[1] = bytes[offset + 5];
                //temp[2] = (byte)((int)bytes[offset + 4] & 0x3F);
                //temp[3] = 0;
                //ObjectID = BitConverter.ToUInt32(temp, 0);
                len += 5;

                // Parse the Property ID
                if (bytes[len] == 0x19)
                    len += 2; // 1 byte Property ID
                else if (bytes[len] == 0x1A)
                    len += 3; // 2 byte Property ID

                // Look for Array Index
                if (bytes[len] == 0x29)
                    len += 2; // 1 byte Array Index
                else if (bytes[len] == 0x2A)
                    len += 3; // 2 byte Array Index

                // Lok for Property Value
                len++; // 1 byte opening tag 0x3E
                apptag = bytes[len++]; // Look at Application Tag
                return len;
            }

            internal static uint /*APDU*/ AppUInt(byte[] bytes, int offset)
            {
                // AppTag = 0x21
                return bytes[offset];
            }

            internal static UInt16 /*APDU*/ AppUInt16(byte[] bytes, int offset)
            {
                // AppTag = 0x22
                byte[] temp = new byte[2];
                temp[1] = bytes[offset++];
                temp[0] = bytes[offset++];
                return BitConverter.ToUInt16(temp, 0);
            }

            internal static UInt32 /*APDU*/ AppUInt24(byte[] bytes, int offset)
            {
                // AppTag = 0x23
                byte[] temp = new byte[4];
                temp[3] = 0;
                temp[2] = bytes[offset++];
                temp[1] = bytes[offset++];
                temp[0] = bytes[offset++];
                return BitConverter.ToUInt32(temp, 0);
            }

            internal static UInt32 /*APDU*/ AppUInt32(byte[] bytes, int offset)
            {
                // AppTag = 0x24
                byte[] temp = new byte[4];
                temp[3] = bytes[offset++];
                temp[2] = bytes[offset++];
                temp[1] = bytes[offset++];
                temp[0] = bytes[offset++];
                return BitConverter.ToUInt32(temp, 0);
            }

            internal static float /*APDU*/ AppSingle(byte[] bytes, int offset)
            {
                // Apptag = 0x44
                byte[] temp = new byte[4];
                temp[3] = bytes[offset++];
                temp[2] = bytes[offset++];
                temp[1] = bytes[offset++];
                temp[0] = bytes[offset++];
                return BitConverter.ToSingle(temp, 0);
            }

            internal static byte[] /*APDU*/ AppOctet(byte[] bytes, int offset)
            {
                // AppTag = 0x65
                int length = bytes[offset++]; // length/value/type
                if ((offset > 0) && (length > 0))
                {
                    byte[] octet = new byte[length];
                    for (int i = 0; i < length; i++)
                        octet[i] = bytes[offset++];
                    return octet;
                }
                else
                    return null;
            }

            internal static string /*APDU*/ AppString(byte[] bytes, int offset)
            {
                // AppTag = 0x75
                int length = bytes[offset] - 1; // length/value/type
                if ((offset > 0) && (length > 0))
                    return Encoding.ASCII.GetString(bytes, offset + 2, length);
                else
                    return "???";
            }

            internal static uint /*APDU*/ SetObjectID(ref byte[] bytes, uint pos,
              Enums.BACNET_OBJECT_TYPE type, uint instance)
            {
                // Assemble Object ID portion of APDU
                UInt32 value = 0;

                //PEP Context Specific Tag number could differ
                bytes[pos++] = 0x0C;  // Tag number (BACnet Object ID)
                                      //bytes[pos++] = 0x01;
                                      //bytes[pos++] = 0x00;
                                      //bytes[pos++] = 0x00;
                                      //bytes[pos++] = 0x00;

                value = (UInt32)type;
                value = value & Enums.BACNET_MAX_OBJECT;
                value = value << Enums.BACNET_INSTANCE_BITS;
                value = value | (instance & Enums.BACNET_MAX_INSTANCE);
                //len = encode_unsigned32(apdu, value);
                byte[] temp4 = new byte[4];
                temp4 = BitConverter.GetBytes(value);
                bytes[pos++] = temp4[3];
                bytes[pos++] = temp4[2];
                bytes[pos++] = temp4[1];
                bytes[pos++] = temp4[0];

                return pos;
            }

            internal static uint /*APDU*/ SetPropertyID(ref byte[] bytes, uint pos,
              Enums.BACNET_PROPERTY_ID type)
            {
                // Assemble Property ID portion of APDU
                UInt32 value = (UInt32)type;
                if (value <= 255)
                {
                    bytes[pos++] = 0x19;  //PEP Context Specific Tag number, could differ
                    bytes[pos++] = (byte)type;
                }
                else if (value < 65535)
                {
                    bytes[pos++] = 0x1A;  //PEP Context Specific Tag number, could differ
                    byte[] temp2 = new byte[2];
                    temp2 = BitConverter.GetBytes(value);
                    bytes[pos++] = temp2[1];
                    bytes[pos++] = temp2[0];
                }
                return pos;
            }

            internal static uint /*APDU*/ SetArrayIdx(ref byte[] bytes, uint pos, int aidx)
            {
                // Assemble Property ID portion of APDU
                UInt32 value = (UInt32)aidx;
                if (value <= 255)
                {
                    bytes[pos++] = 0x29;  //PEP Context Specific Tag number, could differ
                    bytes[pos++] = (byte)aidx;
                }
                else if (value < 65535)
                {
                    bytes[pos++] = 0x2A;  //PEP Context Specific Tag number, could differ
                    byte[] temp2 = new byte[2];
                    temp2 = BitConverter.GetBytes(value);
                    bytes[pos++] = temp2[1];
                    bytes[pos++] = temp2[0];
                }
                return pos;
            }

            internal static uint /*APDU*/ SetProperty(ref byte[] bytes, uint pos, Property property)
            {
                // Convert property class into bytes
                int len;
                if (property != null)
                {
                    bytes[pos++] = 0x3E;  // Tag Open
                    switch (property.Tag)
                    {
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_NULL:
                            bytes[pos++] = 0x00;
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_BOOLEAN:
                            if (property.ValueBool)
                                bytes[pos++] = 0x11;
                            else
                                bytes[pos++] = 0x10;
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT:
                            // Tag could be 0x21, 0x22, 0x23, or 0x24
                            // We can't do Uint64?
                            UInt32 value = (UInt32)property.ValueUInt;
                            if (value <= 255) // 1 byte
                            {
                                bytes[pos++] = 0x21;
                                bytes[pos++] = (byte)value;
                            }
                            else if (value <= 65535)  // 2 bytes
                            {
                                bytes[pos++] = 0x22;
                                byte[] temp2 = new byte[2];
                                temp2 = BitConverter.GetBytes(value);
                                bytes[pos++] = temp2[1];
                                bytes[pos++] = temp2[0];
                            }
                            else if (value <= 16777215) // 3 bytes
                            {
                                bytes[pos++] = 0x23;
                                byte[] temp3 = new byte[3];
                                temp3 = BitConverter.GetBytes(value);
                                bytes[pos++] = temp3[2];
                                bytes[pos++] = temp3[1];
                                bytes[pos++] = temp3[0];
                            }
                            else // 4 bytes
                            {
                                bytes[pos++] = 0x24;
                                byte[] temp4 = new byte[4];
                                temp4 = BitConverter.GetBytes(value);
                                bytes[pos++] = temp4[3];
                                bytes[pos++] = temp4[2];
                                bytes[pos++] = temp4[1];
                                bytes[pos++] = temp4[0];
                            }
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_SIGNED_INT:
                            // Tag could be 0x31, 0x32, 0x33, 0x34
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_REAL:
                            // Tag is 0x44
                            bytes[pos++] = 0x44;
                            byte[] temp5 = new byte[4];
                            temp5 = BitConverter.GetBytes(property.ValueSingle);
                            bytes[pos++] = temp5[3];
                            bytes[pos++] = temp5[2];
                            bytes[pos++] = temp5[1];
                            bytes[pos++] = temp5[0];
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_DOUBLE:
                            // Tag is 0x55
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OCTET_STRING:
                            // Tag is 0x65, maximum 16 bytes!
                            bytes[pos++] = 0x65;
                            len = property.ValueOctet.Length;
                            bytes[pos++] = (byte)len;
                            for (int i = 0; i < len; i++)
                                bytes[pos++] = property.ValueOctet[i];
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_CHARACTER_STRING:
                            // Tag is 0x75, maximum 15 chars!
                            bytes[pos++] = 0x75;
                            len = property.ValueString.Length;
                            bytes[pos++] = (byte)(len + 1);  // Include character set byte
                            bytes[pos++] = 0; // ANSI
                            for (int i = 0; i < len; i++)
                                bytes[pos++] = (byte)property.ValueString[i];
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED:
                            // Tag could be 0x91, 0x92, 0x93, 0x94
                            bytes[pos++] = 0x91;
                            bytes[pos++] = (byte)property.ValueEnum;
                            break;
                        case Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                            // Tag is 0xC4
                            bytes[pos++] = 0xC4;
                            UInt32 id = ((UInt32)property.ValueObjectType) << 22;
                            id += (property.ValueObjectInstance & 0x3FFFFF);
                            byte[] temp6 = new byte[4];
                            temp6 = BitConverter.GetBytes(id);
                            bytes[pos++] = temp6[3];
                            bytes[pos++] = temp6[2];
                            bytes[pos++] = temp6[1];
                            bytes[pos++] = temp6[0];
                            break;
                    }
                    bytes[pos++] = 0x3F;  // Tag Close
                }
                return pos;
            }

            internal static bool /*APDU*/ ParseProperty(ref byte[] bytes, int pos, Property property)
            {
                // Convert bytes into Property
                if (property == null) return false;
                property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_NULL;
                int tag;
                int offset = APDU.ParseRead(bytes, pos, out tag);
                if (tag == 0x21)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                    property.ValueUInt = APDU.AppUInt(bytes, offset);
                    property.ToStringValue = property.ValueUInt.ToString();
                }
                if (tag == 0x22)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                    property.ValueUInt = APDU.AppUInt16(bytes, offset);
                    property.ToStringValue = property.ValueUInt.ToString();
                }
                if (tag == 0x23)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                    property.ValueUInt = APDU.AppUInt24(bytes, offset);
                    property.ToStringValue = property.ValueUInt.ToString();
                }
                if (tag == 0x24)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                    property.ValueUInt = APDU.AppUInt32(bytes, offset);
                    property.ToStringValue = property.ValueUInt.ToString();
                }
                if (tag == 0x44)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_REAL;
                    property.ValueSingle = APDU.AppSingle(bytes, offset);
                    property.ToStringValue = property.ValueSingle.ToString();
                }
                if (tag == 0x65)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OCTET_STRING;
                    property.ValueOctet = APDU.AppOctet(bytes, offset);
                    //PEP Do this in the yet-to-be-written Octet Class
                    string s = "";
                    for (int i = 0; i < property.ValueOctet.Length; i++)
                        s = s + property.ValueOctet[i].ToString("X2");
                    property.ToStringValue = s;
                }
                if (tag == 0x75)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_CHARACTER_STRING;
                    property.ValueString = APDU.AppString(bytes, offset);
                    property.ToStringValue = property.ValueString;
                }
                if (tag == 0x91)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED;
                    property.ValueEnum = bytes[offset];
                    property.ToStringValue = property.ValueEnum.ToString();

                }
                if (tag == 0xC4)
                {
                    property.Tag = Enums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID;
                    uint value = APDU.AppUInt32(bytes, offset);
                    property.ValueObjectType = (Enums.BACNET_OBJECT_TYPE)(value >> 22);
                    property.ValueObjectInstance = value & 0x3FFFFF;
                    property.ToStringValue = property.ValueObjectInstance.ToString();
                }
                return false;
            }

            internal static uint /*APDU*/ SetPriority(ref byte[] bytes, uint pos, int priority)
            {
                // Convert priority into bytes
                bytes[pos++] = 0x49;  //PEP Why x49???
                bytes[pos++] = (byte)priority;
                return pos;
            }

        }

        //-----------------------------------------------------------------------------------------------
        // Transaction State Machine
        internal class TransactionStateMachine
        {
            internal enum TSMState { IDLE, AWAIT_CONFIRMATION, AWAIT_RESPONSE };

            // Constructor
            internal TransactionStateMachine()
            {
                // Create the timer
                //Timer RequestTimer = new Timer();
                //RequestTimer.Tick += new EventHandler(RequestTimer_Tick);
            }

            // Welcome To The Machine - what to do here ?
        }

        internal static class Data
        {
            internal static List<Device> Devices;   // A list of devices after the WhoIs
            internal static int DeviceIndex = 0;        // The current device selected
            internal static UInt32 PacketRetryCount = 0;
        }
        internal class Enums
        {
            internal const int BACNET_MAX_OBJECT = 0x3FF;

            internal const int BACNET_MAX_INSTANCE = 0x3FFFFF;
            internal const int BACNET_INSTANCE_BITS = 22;

            internal const int BACNET_PROTOCOL_VERSION = 0x01;

            internal const int BACNET_UNICAST_NPDU = 0x0A;

            internal const byte BACNET_BVLC_TYPE_BIP = 0x81;

            internal enum BACNET_PROPERTY_ID
            {
                PROP_ACKED_TRANSITIONS = 0,
                PROP_ACK_REQUIRED = 1,
                PROP_ACTION = 2,
                PROP_ACTION_TEXT = 3,
                PROP_ACTIVE_TEXT = 4,
                PROP_ACTIVE_VT_SESSIONS = 5,
                PROP_ALARM_VALUE = 6,
                PROP_ALARM_VALUES = 7,
                PROP_ALL = 8,
                PROP_ALL_WRITES_SUCCESSFUL = 9,
                PROP_APDU_SEGMENT_TIMEOUT = 10,
                PROP_APDU_TIMEOUT = 11,
                PROP_APPLICATION_SOFTWARE_VERSION = 12,
                PROP_ARCHIVE = 13,
                PROP_BIAS = 14,
                PROP_CHANGE_OF_STATE_COUNT = 15,
                PROP_CHANGE_OF_STATE_TIME = 16,
                PROP_NOTIFICATION_CLASS = 17,
                PROP_BLANK_1 = 18,
                PROP_CONTROLLED_VARIABLE_REFERENCE = 19,
                PROP_CONTROLLED_VARIABLE_UNITS = 20,
                PROP_CONTROLLED_VARIABLE_VALUE = 21,
                PROP_COV_INCREMENT = 22,
                PROP_DATE_LIST = 23,
                PROP_DAYLIGHT_SAVINGS_STATUS = 24,
                PROP_DEADBAND = 25,
                PROP_DERIVATIVE_CONSTANT = 26,
                PROP_DERIVATIVE_CONSTANT_UNITS = 27,
                PROP_DESCRIPTION = 28,
                PROP_DESCRIPTION_OF_HALT = 29,
                PROP_DEVICE_ADDRESS_BINDING = 30,
                PROP_DEVICE_TYPE = 31,
                PROP_EFFECTIVE_PERIOD = 32,
                PROP_ELAPSED_ACTIVE_TIME = 33,
                PROP_ERROR_LIMIT = 34,
                PROP_EVENT_ENABLE = 35,
                PROP_EVENT_STATE = 36,
                PROP_EVENT_TYPE = 37,
                PROP_EXCEPTION_SCHEDULE = 38,
                PROP_FAULT_VALUES = 39,
                PROP_FEEDBACK_VALUE = 40,
                PROP_FILE_ACCESS_METHOD = 41,
                PROP_FILE_SIZE = 42,
                PROP_FILE_TYPE = 43,
                PROP_FIRMWARE_REVISION = 44,
                PROP_HIGH_LIMIT = 45,
                PROP_INACTIVE_TEXT = 46,
                PROP_IN_PROCESS = 47,
                PROP_INSTANCE_OF = 48,
                PROP_INTEGRAL_CONSTANT = 49,
                PROP_INTEGRAL_CONSTANT_UNITS = 50,
                PROP_ISSUE_CONFIRMED_NOTIFICATIONS = 51,
                PROP_LIMIT_ENABLE = 52,
                PROP_LIST_OF_GROUP_MEMBERS = 53,
                PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES = 54,
                PROP_LIST_OF_SESSION_KEYS = 55,
                PROP_LOCAL_DATE = 56,
                PROP_LOCAL_TIME = 57,
                PROP_LOCATION = 58,
                PROP_LOW_LIMIT = 59,
                PROP_MANIPULATED_VARIABLE_REFERENCE = 60,
                PROP_MAXIMUM_OUTPUT = 61,
                PROP_MAX_APDU_LENGTH_ACCEPTED = 62,
                PROP_MAX_INFO_FRAMES = 63,
                PROP_MAX_MASTER = 64,
                PROP_MAX_PRES_VALUE = 65,
                PROP_MINIMUM_OFF_TIME = 66,
                PROP_MINIMUM_ON_TIME = 67,
                PROP_MINIMUM_OUTPUT = 68,
                PROP_MIN_PRES_VALUE = 69,
                PROP_MODEL_NAME = 70,
                PROP_MODIFICATION_DATE = 71,
                PROP_NOTIFY_TYPE = 72,
                PROP_NUMBER_OF_APDU_RETRIES = 73,
                PROP_NUMBER_OF_STATES = 74,
                PROP_OBJECT_IDENTIFIER = 75,
                PROP_OBJECT_LIST = 76,
                PROP_OBJECT_NAME = 77,
                PROP_OBJECT_PROPERTY_REFERENCE = 78,
                PROP_OBJECT_TYPE = 79,
                PROP_OPTIONAL = 80,
                PROP_OUT_OF_SERVICE = 81,
                PROP_OUTPUT_UNITS = 82,
                PROP_EVENT_PARAMETERS = 83,
                PROP_POLARITY = 84,
                PROP_PRESENT_VALUE = 85,
                PROP_PRIORITY = 86,
                PROP_PRIORITY_ARRAY = 87,
                PROP_PRIORITY_FOR_WRITING = 88,
                PROP_PROCESS_IDENTIFIER = 89,
                PROP_PROGRAM_CHANGE = 90,
                PROP_PROGRAM_LOCATION = 91,
                PROP_PROGRAM_STATE = 92,
                PROP_PROPORTIONAL_CONSTANT = 93,
                PROP_PROPORTIONAL_CONSTANT_UNITS = 94,
                PROP_PROTOCOL_CONFORMANCE_CLASS = 95,       /* deleted in version 1 revision 2 */
                PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED = 96,
                PROP_PROTOCOL_SERVICES_SUPPORTED = 97,
                PROP_PROTOCOL_VERSION = 98,
                PROP_READ_ONLY = 99,
                PROP_REASON_FOR_HALT = 100,
                PROP_RECIPIENT = 101,
                PROP_RECIPIENT_LIST = 102,
                PROP_RELIABILITY = 103,
                PROP_RELINQUISH_DEFAULT = 104,
                PROP_REQUIRED = 105,
                PROP_RESOLUTION = 106,
                PROP_SEGMENTATION_SUPPORTED = 107,
                PROP_SETPOINT = 108,
                PROP_SETPOINT_REFERENCE = 109,
                PROP_STATE_TEXT = 110,
                PROP_STATUS_FLAGS = 111,
                PROP_SYSTEM_STATUS = 112,
                PROP_TIME_DELAY = 113,
                PROP_TIME_OF_ACTIVE_TIME_RESET = 114,
                PROP_TIME_OF_STATE_COUNT_RESET = 115,
                PROP_TIME_SYNCHRONIZATION_RECIPIENTS = 116,
                PROP_UNITS = 117,
                PROP_UPDATE_INTERVAL = 118,
                PROP_UTC_OFFSET = 119,
                PROP_VENDOR_IDENTIFIER = 120,
                PROP_VENDOR_NAME = 121,
                PROP_VT_CLASSES_SUPPORTED = 122,
                PROP_WEEKLY_SCHEDULE = 123,
                PROP_ATTEMPTED_SAMPLES = 124,
                PROP_AVERAGE_VALUE = 125,
                PROP_BUFFER_SIZE = 126,
                PROP_CLIENT_COV_INCREMENT = 127,
                PROP_COV_RESUBSCRIPTION_INTERVAL = 128,
                PROP_CURRENT_NOTIFY_TIME = 129,
                PROP_EVENT_TIME_STAMPS = 130,
                PROP_LOG_BUFFER = 131,
                PROP_LOG_DEVICE_OBJECT = 132,
                /* The enable property is renamed from log-enable in
                   Addendum b to ANSI/ASHRAE 135-2004(135b-2) */
                PROP_ENABLE = 133,
                PROP_LOG_INTERVAL = 134,
                PROP_MAXIMUM_VALUE = 135,
                PROP_MINIMUM_VALUE = 136,
                PROP_NOTIFICATION_THRESHOLD = 137,
                PROP_PREVIOUS_NOTIFY_TIME = 138,
                PROP_PROTOCOL_REVISION = 139,
                PROP_RECORDS_SINCE_NOTIFICATION = 140,
                PROP_RECORD_COUNT = 141,
                PROP_START_TIME = 142,
                PROP_STOP_TIME = 143,
                PROP_STOP_WHEN_FULL = 144,
                PROP_TOTAL_RECORD_COUNT = 145,
                PROP_VALID_SAMPLES = 146,
                PROP_WINDOW_INTERVAL = 147,
                PROP_WINDOW_SAMPLES = 148,
                PROP_MAXIMUM_VALUE_TIMESTAMP = 149,
                PROP_MINIMUM_VALUE_TIMESTAMP = 150,
                PROP_VARIANCE_VALUE = 151,
                PROP_ACTIVE_COV_SUBSCRIPTIONS = 152,
                PROP_BACKUP_FAILURE_TIMEOUT = 153,
                PROP_CONFIGURATION_FILES = 154,
                PROP_DATABASE_REVISION = 155,
                PROP_DIRECT_READING = 156,
                PROP_LAST_RESTORE_TIME = 157,
                PROP_MAINTENANCE_REQUIRED = 158,
                PROP_MEMBER_OF = 159,
                PROP_MODE = 160,
                PROP_OPERATION_EXPECTED = 161,
                PROP_SETTING = 162,
                PROP_SILENCED = 163,
                PROP_TRACKING_VALUE = 164,
                PROP_ZONE_MEMBERS = 165,
                PROP_LIFE_SAFETY_ALARM_VALUES = 166,
                PROP_MAX_SEGMENTS_ACCEPTED = 167,
                PROP_PROFILE_NAME = 168,
                PROP_AUTO_SLAVE_DISCOVERY = 169,
                PROP_MANUAL_SLAVE_ADDRESS_BINDING = 170,
                PROP_SLAVE_ADDRESS_BINDING = 171,
                PROP_SLAVE_PROXY_ENABLE = 172,
                PROP_LAST_NOTIFY_TIME = 173,
                PROP_SCHEDULE_DEFAULT = 174,
                PROP_ACCEPTED_MODES = 175,
                PROP_ADJUST_VALUE = 176,
                PROP_COUNT = 177,
                PROP_COUNT_BEFORE_CHANGE = 178,
                PROP_COUNT_CHANGE_TIME = 179,
                PROP_COV_PERIOD = 180,
                PROP_INPUT_REFERENCE = 181,
                PROP_LIMIT_MONITORING_INTERVAL = 182,
                PROP_LOGGING_DEVICE = 183,
                PROP_LOGGING_RECORD = 184,
                PROP_PRESCALE = 185,
                PROP_PULSE_RATE = 186,
                PROP_SCALE = 187,
                PROP_SCALE_FACTOR = 188,
                PROP_UPDATE_TIME = 189,
                PROP_VALUE_BEFORE_CHANGE = 190,
                PROP_VALUE_SET = 191,
                PROP_VALUE_CHANGE_TIME = 192,
                /* enumerations 193-206 are new */
                PROP_ALIGN_INTERVALS = 193,
                PROP_GROUP_MEMBER_NAMES = 194,
                PROP_INTERVAL_OFFSET = 195,
                PROP_LAST_RESTART_REASON = 196,
                PROP_LOGGING_TYPE = 197,
                PROP_MEMBER_STATUS_FLAGS = 198,
                PROP_NOTIFICATION_PERIOD = 199,
                PROP_PREVIOUS_NOTIFY_RECORD = 200,
                PROP_REQUESTED_UPDATE_INTERVAL = 201,
                PROP_RESTART_NOTIFICATION_RECIPIENTS = 202,
                PROP_TIME_OF_DEVICE_RESTART = 203,
                PROP_TIME_SYNCHRONIZATION_INTERVAL = 204,
                PROP_TRIGGER = 205,
                PROP_UTC_TIME_SYNCHRONIZATION_RECIPIENTS = 206,
                /* enumerations 207-211 are used in Addendum d to ANSI/ASHRAE 135-2004 */
                PROP_NODE_SUBTYPE = 207,
                PROP_NODE_TYPE = 208,
                PROP_STRUCTURED_OBJECT_LIST = 209,
                PROP_SUBORDINATE_ANNOTATIONS = 210,
                PROP_SUBORDINATE_LIST = 211,
                /* enumerations 212-225 are used in Addendum e to ANSI/ASHRAE 135-2004 */
                PROP_ACTUAL_SHED_LEVEL = 212,
                PROP_DUTY_WINDOW = 213,
                PROP_EXPECTED_SHED_LEVEL = 214,
                PROP_FULL_DUTY_BASELINE = 215,
                /* enumerations 216-217 are used in Addendum i to ANSI/ASHRAE 135-2004 */
                PROP_BLINK_PRIORITY_THRESHOLD = 216,
                PROP_BLINK_TIME = 217,
                /* enumerations 212-225 are used in Addendum e to ANSI/ASHRAE 135-2004 */
                PROP_REQUESTED_SHED_LEVEL = 218,
                PROP_SHED_DURATION = 219,
                PROP_SHED_LEVEL_DESCRIPTIONS = 220,
                PROP_SHED_LEVELS = 221,
                PROP_STATE_DESCRIPTION = 222,
                /* enumerations 223-225 are used in Addendum i to ANSI/ASHRAE 135-2004 */
                PROP_FADE_TIME = 223,
                PROP_LIGHTING_COMMAND = 224,
                PROP_LIGHTING_COMMAND_PRIORITY = 225,
                /* enumerations 226-235 are used in Addendum f to ANSI/ASHRAE 135-2004 */
                /* enumerations 236-243 are used in Addendum i to ANSI/ASHRAE 135-2004 */
                PROP_OFF_DELAY = 236,
                PROP_ON_DELAY = 237,
                PROP_POWER = 238,
                PROP_POWER_ON_VALUE = 239,
                PROP_PROGRESS_VALUE = 240,
                PROP_RAMP_RATE = 241,
                PROP_STEP_INCREMENT = 242,
                PROP_SYSTEM_FAILURE_VALUE = 243,
                /* The special property identifiers all, optional, and required  */
                /* are reserved for use in the ReadPropertyConditional and */
                /* ReadPropertyMultiple services or services not defined in this standard. */
                /* Enumerated values 0-511 are reserved for definition by ASHRAE.  */
                /* Enumerated values 512-4194303 may be used by others subject to the  */
                /* procedures and constraints described in Clause 23.  */
                PROP_BAUD_RATE = 9600,
                PROP_SERIAL_NUMBER = 9701
            }
            internal const int MAX_BACNET_PROPERTY_ID = 4194303;

            internal enum BACNET_ACTION
            {
                ACTION_DIRECT = 0,
                ACTION_REVERSE = 1
            }

            internal enum BACNET_BINARY_PV
            {
                MIN_BINARY_PV = 0,  /* for validating incoming values */
                BINARY_INACTIVE = 0,
                BINARY_ACTIVE = 1,
                MAX_BINARY_PV = 1,  /* for validating incoming values */
                BINARY_NULL = 2     /* our homemade way of storing this info */
            }

            internal enum BACNET_ACTION_VALUE_TYPE
            {
                ACTION_BINARY_PV,
                ACTION_UNSIGNED,
                ACTION_FLOAT
            }

            internal enum BACNET_EVENT_STATE
            {
                EVENT_STATE_NORMAL = 0,
                EVENT_STATE_FAULT = 1,
                EVENT_STATE_OFFNORMAL = 2,
                EVENT_STATE_HIGH_LIMIT = 3,
                EVENT_STATE_LOW_LIMIT = 4
            }

            internal enum BACNET_DEVICE_STATUS
            {
                STATUS_OPERATIONAL = 0,
                STATUS_OPERATIONAL_READ_ONLY = 1,
                STATUS_DOWNLOAD_REQUIRED = 2,
                STATUS_DOWNLOAD_IN_PROGRESS = 3,
                STATUS_NON_OPERATIONAL = 4,
                MAX_DEVICE_STATUS = 5
            }

            internal enum BACNET_ENGINEERING_UNITS
            {
                /* Acceleration */
                UNITS_METERS_PER_SECOND_PER_SECOND = 166,
                /* Area */
                UNITS_SQUARE_METERS = 0,
                UNITS_SQUARE_CENTIMETERS = 116,
                UNITS_SQUARE_FEET = 1,
                UNITS_SQUARE_INCHES = 115,
                /* Currency */
                UNITS_CURRENCY1 = 105,
                UNITS_CURRENCY2 = 106,
                UNITS_CURRENCY3 = 107,
                UNITS_CURRENCY4 = 108,
                UNITS_CURRENCY5 = 109,
                UNITS_CURRENCY6 = 110,
                UNITS_CURRENCY7 = 111,
                UNITS_CURRENCY8 = 112,
                UNITS_CURRENCY9 = 113,
                UNITS_CURRENCY10 = 114,
                /* Electrical */
                UNITS_MILLIAMPERES = 2,
                UNITS_AMPERES = 3,
                UNITS_AMPERES_PER_METER = 167,
                UNITS_AMPERES_PER_SQUARE_METER = 168,
                UNITS_AMPERE_SQUARE_METERS = 169,
                UNITS_FARADS = 170,
                UNITS_HENRYS = 171,
                UNITS_OHMS = 4,
                UNITS_OHM_METERS = 172,
                UNITS_MILLIOHMS = 145,
                UNITS_KILOHMS = 122,
                UNITS_MEGOHMS = 123,
                UNITS_SIEMENS = 173,        /* 1 mho equals 1 siemens */
                UNITS_SIEMENS_PER_METER = 174,
                UNITS_TESLAS = 175,
                UNITS_VOLTS = 5,
                UNITS_MILLIVOLTS = 124,
                UNITS_KILOVOLTS = 6,
                UNITS_MEGAVOLTS = 7,
                UNITS_VOLT_AMPERES = 8,
                UNITS_KILOVOLT_AMPERES = 9,
                UNITS_MEGAVOLT_AMPERES = 10,
                UNITS_VOLT_AMPERES_REACTIVE = 11,
                UNITS_KILOVOLT_AMPERES_REACTIVE = 12,
                UNITS_MEGAVOLT_AMPERES_REACTIVE = 13,
                UNITS_VOLTS_PER_DEGREE_KELVIN = 176,
                UNITS_VOLTS_PER_METER = 177,
                UNITS_DEGREES_PHASE = 14,
                UNITS_POWER_FACTOR = 15,
                UNITS_WEBERS = 178,
                /* Energy */
                UNITS_JOULES = 16,
                UNITS_KILOJOULES = 17,
                UNITS_KILOJOULES_PER_KILOGRAM = 125,
                UNITS_MEGAJOULES = 126,
                UNITS_WATT_HOURS = 18,
                UNITS_KILOWATT_HOURS = 19,
                UNITS_MEGAWATT_HOURS = 146,
                UNITS_BTUS = 20,
                UNITS_KILO_BTUS = 147,
                UNITS_MEGA_BTUS = 148,
                UNITS_THERMS = 21,
                UNITS_TON_HOURS = 22,
                /* Enthalpy */
                UNITS_JOULES_PER_KILOGRAM_DRY_AIR = 23,
                UNITS_KILOJOULES_PER_KILOGRAM_DRY_AIR = 149,
                UNITS_MEGAJOULES_PER_KILOGRAM_DRY_AIR = 150,
                UNITS_BTUS_PER_POUND_DRY_AIR = 24,
                UNITS_BTUS_PER_POUND = 117,
                /* Entropy */
                UNITS_JOULES_PER_DEGREE_KELVIN = 127,
                UNITS_KILOJOULES_PER_DEGREE_KELVIN = 151,
                UNITS_MEGAJOULES_PER_DEGREE_KELVIN = 152,
                UNITS_JOULES_PER_KILOGRAM_DEGREE_KELVIN = 128,
                /* Force */
                UNITS_NEWTON = 153,
                /* Frequency */
                UNITS_CYCLES_PER_HOUR = 25,
                UNITS_CYCLES_PER_MINUTE = 26,
                UNITS_HERTZ = 27,
                UNITS_KILOHERTZ = 129,
                UNITS_MEGAHERTZ = 130,
                UNITS_PER_HOUR = 131,
                /* Humidity */
                UNITS_GRAMS_OF_WATER_PER_KILOGRAM_DRY_AIR = 28,
                UNITS_PERCENT_RELATIVE_HUMIDITY = 29,
                /* Length */
                UNITS_MILLIMETERS = 30,
                UNITS_CENTIMETERS = 118,
                UNITS_METERS = 31,
                UNITS_INCHES = 32,
                UNITS_FEET = 33,
                /* Light */
                UNITS_CANDELAS = 179,
                UNITS_CANDELAS_PER_SQUARE_METER = 180,
                UNITS_WATTS_PER_SQUARE_FOOT = 34,
                UNITS_WATTS_PER_SQUARE_METER = 35,
                UNITS_LUMENS = 36,
                UNITS_LUXES = 37,
                UNITS_FOOT_CANDLES = 38,
                /* Mass */
                UNITS_KILOGRAMS = 39,
                UNITS_POUNDS_MASS = 40,
                UNITS_TONS = 41,
                /* Mass Flow */
                UNITS_GRAMS_PER_SECOND = 154,
                UNITS_GRAMS_PER_MINUTE = 155,
                UNITS_KILOGRAMS_PER_SECOND = 42,
                UNITS_KILOGRAMS_PER_MINUTE = 43,
                UNITS_KILOGRAMS_PER_HOUR = 44,
                UNITS_POUNDS_MASS_PER_SECOND = 119,
                UNITS_POUNDS_MASS_PER_MINUTE = 45,
                UNITS_POUNDS_MASS_PER_HOUR = 46,
                UNITS_TONS_PER_HOUR = 156,
                /* Power */
                UNITS_MILLIWATTS = 132,
                UNITS_WATTS = 47,
                UNITS_KILOWATTS = 48,
                UNITS_MEGAWATTS = 49,
                UNITS_BTUS_PER_HOUR = 50,
                UNITS_KILO_BTUS_PER_HOUR = 157,
                UNITS_HORSEPOWER = 51,
                UNITS_TONS_REFRIGERATION = 52,
                /* Pressure */
                UNITS_PASCALS = 53,
                UNITS_HECTOPASCALS = 133,
                UNITS_KILOPASCALS = 54,
                UNITS_MILLIBARS = 134,
                UNITS_BARS = 55,
                UNITS_POUNDS_FORCE_PER_SQUARE_INCH = 56,
                UNITS_CENTIMETERS_OF_WATER = 57,
                UNITS_INCHES_OF_WATER = 58,
                UNITS_MILLIMETERS_OF_MERCURY = 59,
                UNITS_CENTIMETERS_OF_MERCURY = 60,
                UNITS_INCHES_OF_MERCURY = 61,
                /* Temperature */
                UNITS_DEGREES_CELSIUS = 62,
                UNITS_DEGREES_KELVIN = 63,
                UNITS_DEGREES_KELVIN_PER_HOUR = 181,
                UNITS_DEGREES_KELVIN_PER_MINUTE = 182,
                UNITS_DEGREES_FAHRENHEIT = 64,
                UNITS_DEGREE_DAYS_CELSIUS = 65,
                UNITS_DEGREE_DAYS_FAHRENHEIT = 66,
                UNITS_DELTA_DEGREES_FAHRENHEIT = 120,
                UNITS_DELTA_DEGREES_KELVIN = 121,
                /* Time */
                UNITS_YEARS = 67,
                UNITS_MONTHS = 68,
                UNITS_WEEKS = 69,
                UNITS_DAYS = 70,
                UNITS_HOURS = 71,
                UNITS_MINUTES = 72,
                UNITS_SECONDS = 73,
                UNITS_HUNDREDTHS_SECONDS = 158,
                UNITS_MILLISECONDS = 159,
                /* Torque */
                UNITS_NEWTON_METERS = 160,
                /* Velocity */
                UNITS_MILLIMETERS_PER_SECOND = 161,
                UNITS_MILLIMETERS_PER_MINUTE = 162,
                UNITS_METERS_PER_SECOND = 74,
                UNITS_METERS_PER_MINUTE = 163,
                UNITS_METERS_PER_HOUR = 164,
                UNITS_KILOMETERS_PER_HOUR = 75,
                UNITS_FEET_PER_SECOND = 76,
                UNITS_FEET_PER_MINUTE = 77,
                UNITS_MILES_PER_HOUR = 78,
                /* Volume */
                UNITS_CUBIC_FEET = 79,
                UNITS_CUBIC_METERS = 80,
                UNITS_IMPERIAL_GALLONS = 81,
                UNITS_LITERS = 82,
                UNITS_US_GALLONS = 83,
                /* Volumetric Flow */
                UNITS_CUBIC_FEET_PER_SECOND = 142,
                UNITS_CUBIC_FEET_PER_MINUTE = 84,
                UNITS_CUBIC_METERS_PER_SECOND = 85,
                UNITS_CUBIC_METERS_PER_MINUTE = 165,
                UNITS_CUBIC_METERS_PER_HOUR = 135,
                UNITS_IMPERIAL_GALLONS_PER_MINUTE = 86,
                UNITS_LITERS_PER_SECOND = 87,
                UNITS_LITERS_PER_MINUTE = 88,
                UNITS_LITERS_PER_HOUR = 136,
                UNITS_US_GALLONS_PER_MINUTE = 89,
                /* Other */
                UNITS_DEGREES_ANGULAR = 90,
                UNITS_DEGREES_CELSIUS_PER_HOUR = 91,
                UNITS_DEGREES_CELSIUS_PER_MINUTE = 92,
                UNITS_DEGREES_FAHRENHEIT_PER_HOUR = 93,
                UNITS_DEGREES_FAHRENHEIT_PER_MINUTE = 94,
                UNITS_JOULE_SECONDS = 183,
                UNITS_KILOGRAMS_PER_CUBIC_METER = 186,
                UNITS_KW_HOURS_PER_SQUARE_METER = 137,
                UNITS_KW_HOURS_PER_SQUARE_FOOT = 138,
                UNITS_MEGAJOULES_PER_SQUARE_METER = 139,
                UNITS_MEGAJOULES_PER_SQUARE_FOOT = 140,
                UNITS_NO_UNITS = 95,
                UNITS_NEWTON_SECONDS = 187,
                UNITS_NEWTONS_PER_METER = 188,
                UNITS_PARTS_PER_MILLION = 96,
                UNITS_PARTS_PER_BILLION = 97,
                UNITS_PERCENT = 98,
                UNITS_PERCENT_OBSCURATION_PER_FOOT = 143,
                UNITS_PERCENT_OBSCURATION_PER_METER = 144,
                UNITS_PERCENT_PER_SECOND = 99,
                UNITS_PER_MINUTE = 100,
                UNITS_PER_SECOND = 101,
                UNITS_PSI_PER_DEGREE_FAHRENHEIT = 102,
                UNITS_RADIANS = 103,
                UNITS_RADIANS_PER_SECOND = 184,
                UNITS_REVOLUTIONS_PER_MINUTE = 104,
                UNITS_SQUARE_METERS_PER_NEWTON = 185,
                UNITS_WATTS_PER_METER_PER_DEGREE_KELVIN = 189,
                UNITS_WATTS_PER_SQUARE_METER_DEGREE_KELVIN = 141
                /* Enumerated values 0-255 are reserved for definition by ASHRAE. */
                /* Enumerated values 256-65535 may be used by others subject to */
                /* the procedures and constraints described in Clause 23. */
                /* The last enumeration used in this version is 189. */
            }
            internal enum BACNET_POLARITY
            {
                POLARITY_NORMAL = 0,
                POLARITY_REVERSE = 1
            }

            internal enum BACNET_PROGRAM_REQUEST
            {
                PROGRAM_REQUEST_READY = 0,
                PROGRAM_REQUEST_LOAD = 1,
                PROGRAM_REQUEST_RUN = 2,
                PROGRAM_REQUEST_HALT = 3,
                PROGRAM_REQUEST_RESTART = 4,
                PROGRAM_REQUEST_UNLOAD = 5
            }

            internal enum BACNET_PROGRAM_STATE
            {
                PROGRAM_STATE_IDLE = 0,
                PROGRAM_STATE_LOADING = 1,
                PROGRAM_STATE_RUNNING = 2,
                PROGRAM_STATE_WAITING = 3,
                PROGRAM_STATE_HALTED = 4,
                PROGRAM_STATE_UNLOADING = 5
            }

            internal enum BACNET_PROGRAM_ERROR
            {
                PROGRAM_ERROR_NORMAL = 0,
                PROGRAM_ERROR_LOAD_FAILED = 1,
                PROGRAM_ERROR_INTERNAL = 2,
                PROGRAM_ERROR_PROGRAM = 3,
                PROGRAM_ERROR_OTHER = 4
                /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
                /* Enumerated values 64-65535 may be used by others subject to  */
                /* the procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_RELIABILITY
            {
                RELIABILITY_NO_FAULT_DETECTED = 0,
                RELIABILITY_NO_SENSOR = 1,
                RELIABILITY_OVER_RANGE = 2,
                RELIABILITY_UNDER_RANGE = 3,
                RELIABILITY_OPEN_LOOP = 4,
                RELIABILITY_SHORTED_LOOP = 5,
                RELIABILITY_NO_OUTPUT = 6,
                RELIABILITY_UNRELIABLE_OTHER = 7,
                RELIABILITY_PROCESS_ERROR = 8,
                RELIABILITY_MULTI_STATE_FAULT = 9,
                RELIABILITY_CONFIGURATION_ERROR = 10,
                RELIABILITY_COMMUNICATION_FAILURE = 12,
                RELIABILITY_TRIPPED = 13
                /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
                /* Enumerated values 64-65535 may be used by others subject to  */
                /* the procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_EVENT_TYPE
            {
                EVENT_CHANGE_OF_BITSTRING = 0,
                EVENT_CHANGE_OF_STATE = 1,
                EVENT_CHANGE_OF_VALUE = 2,
                EVENT_COMMAND_FAILURE = 3,
                EVENT_FLOATING_LIMIT = 4,
                EVENT_OUT_OF_RANGE = 5,
                /*  complex-event-type        (6), -- see comment below */
                /*  event-buffer-ready   (7), -- context tag 7 is deprecated */
                EVENT_CHANGE_OF_LIFE_SAFETY = 8,
                EVENT_EXTENDED = 9,
                EVENT_BUFFER_READY = 10,
                EVENT_UNSIGNED_RANGE = 11
                /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
                /* Enumerated values 64-65535 may be used by others subject to  */
                /* the procedures and constraints described in Clause 23.  */
                /* It is expected that these enumerated values will correspond to  */
                /* the use of the complex-event-type CHOICE [6] of the  */
                /* BACnetNotificationParameters production. */
                /* The last enumeration used in this version is 11. */
            }

            internal enum BACNET_FILE_ACCESS_METHOD
            {
                FILE_RECORD_ACCESS = 0,
                FILE_STREAM_ACCESS = 1,
                FILE_RECORD_AND_STREAM_ACCESS = 2
            }

            internal enum BACNET_LIFE_SAFETY_MODE
            {
                MIN_LIFE_SAFETY_MODE = 0,
                LIFE_SAFETY_MODE_OFF = 0,
                LIFE_SAFETY_MODE_ON = 1,
                LIFE_SAFETY_MODE_TEST = 2,
                LIFE_SAFETY_MODE_MANNED = 3,
                LIFE_SAFETY_MODE_UNMANNED = 4,
                LIFE_SAFETY_MODE_ARMED = 5,
                LIFE_SAFETY_MODE_DISARMED = 6,
                LIFE_SAFETY_MODE_PREARMED = 7,
                LIFE_SAFETY_MODE_SLOW = 8,
                LIFE_SAFETY_MODE_FAST = 9,
                LIFE_SAFETY_MODE_DISCONNECTED = 10,
                LIFE_SAFETY_MODE_ENABLED = 11,
                LIFE_SAFETY_MODE_DISABLED = 12,
                LIFE_SAFETY_MODE_AUTOMATIC_RELEASE_DISABLED = 13,
                LIFE_SAFETY_MODE_DEFAULT = 14,
                MAX_LIFE_SAFETY_MODE = 14
                /* Enumerated values 0-255 are reserved for definition by ASHRAE.  */
                /* Enumerated values 256-65535 may be used by others subject to  */
                /* procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_LIFE_SAFETY_OPERATION
            {
                LIFE_SAFETY_OP_NONE = 0,
                LIFE_SAFETY_OP_SILENCE = 1,
                LIFE_SAFETY_OP_SILENCE_AUDIBLE = 2,
                LIFE_SAFETY_OP_SILENCE_VISUAL = 3,
                LIFE_SAFETY_OP_RESET = 4,
                LIFE_SAFETY_OP_RESET_ALARM = 5,
                LIFE_SAFETY_OP_RESET_FAULT = 6,
                LIFE_SAFETY_OP_UNSILENCE = 7,
                LIFE_SAFETY_OP_UNSILENCE_AUDIBLE = 8,
                LIFE_SAFETY_OP_UNSILENCE_VISUAL = 9
                /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
                /* Enumerated values 64-65535 may be used by others subject to  */
                /* procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_LIFE_SAFETY_STATE
            {
                MIN_LIFE_SAFETY_STATE = 0,
                LIFE_SAFETY_STATE_QUIET = 0,
                LIFE_SAFETY_STATE_PRE_ALARM = 1,
                LIFE_SAFETY_STATE_ALARM = 2,
                LIFE_SAFETY_STATE_FAULT = 3,
                LIFE_SAFETY_STATE_FAULT_PRE_ALARM = 4,
                LIFE_SAFETY_STATE_FAULT_ALARM = 5,
                LIFE_SAFETY_STATE_NOT_READY = 6,
                LIFE_SAFETY_STATE_ACTIVE = 7,
                LIFE_SAFETY_STATE_TAMPER = 8,
                LIFE_SAFETY_STATE_TEST_ALARM = 9,
                LIFE_SAFETY_STATE_TEST_ACTIVE = 10,
                LIFE_SAFETY_STATE_TEST_FAULT = 11,
                LIFE_SAFETY_STATE_TEST_FAULT_ALARM = 12,
                LIFE_SAFETY_STATE_HOLDUP = 13,
                LIFE_SAFETY_STATE_DURESS = 14,
                LIFE_SAFETY_STATE_TAMPER_ALARM = 15,
                LIFE_SAFETY_STATE_ABNORMAL = 16,
                LIFE_SAFETY_STATE_EMERGENCY_POWER = 17,
                LIFE_SAFETY_STATE_DELAYED = 18,
                LIFE_SAFETY_STATE_BLOCKED = 19,
                LIFE_SAFETY_STATE_LOCAL_ALARM = 20,
                LIFE_SAFETY_STATE_GENERAL_ALARM = 21,
                LIFE_SAFETY_STATE_SUPERVISORY = 22,
                LIFE_SAFETY_STATE_TEST_SUPERVISORY = 23,
                MAX_LIFE_SAFETY_STATE = 0
                /* Enumerated values 0-255 are reserved for definition by ASHRAE.  */
                /* Enumerated values 256-65535 may be used by others subject to  */
                /* procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_SILENCED_STATE
            {
                SILENCED_STATE_UNSILENCED = 0,
                SILENCED_STATE_AUDIBLE_SILENCED = 1,
                SILENCED_STATE_VISIBLE_SILENCED = 2,
                SILENCED_STATE_ALL_SILENCED = 3
                /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
                /* Enumerated values 64-65535 may be used by others subject to */
                /* procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_MAINTENANCE
            {
                MAINTENANCE_NONE = 0,
                MAINTENANCE_PERIODIC_TEST = 1,
                AINTENANCE_NEED_SERVICE_OPERATIONAL = 2,
                MAINTENANCE_NEED_SERVICE_INOPERATIVE = 3
                /* Enumerated values 0-255 are reserved for definition by ASHRAE.  */
                /* Enumerated values 256-65535 may be used by others subject to  */
                /* procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_NOTIFY_TYPE
            {
                NOTIFY_ALARM = 0,
                NOTIFY_EVENT = 1,
                NOTIFY_ACK_NOTIFICATION = 2
            }

            internal enum BACNET_OBJECT_TYPE
            {
                //OBJECT_NULL = -1,
                OBJECT_ANALOG_INPUT = 0,
                OBJECT_ANALOG_OUTPUT = 1,
                OBJECT_ANALOG_VALUE = 2,
                OBJECT_BINARY_INPUT = 3,
                OBJECT_BINARY_OUTPUT = 4,
                OBJECT_BINARY_VALUE = 5,
                OBJECT_CALENDAR = 6,
                OBJECT_COMMAND = 7,
                OBJECT_DEVICE = 8,
                OBJECT_EVENT_ENROLLMENT = 9,
                OBJECT_FILE = 10,
                OBJECT_GROUP = 11,
                OBJECT_LOOP = 12,
                OBJECT_MULTI_STATE_INPUT = 13,
                OBJECT_MULTI_STATE_OUTPUT = 14,
                OBJECT_NOTIFICATION_CLASS = 15,
                OBJECT_PROGRAM = 16,
                OBJECT_SCHEDULE = 17,
                OBJECT_AVERAGING = 18,
                OBJECT_MULTI_STATE_VALUE = 19,
                OBJECT_TRENDLOG = 20,
                OBJECT_LIFE_SAFETY_POINT = 21,
                OBJECT_LIFE_SAFETY_ZONE = 22,
                OBJECT_ACCUMULATOR = 23,
                OBJECT_PULSE_CONVERTER = 24,
                OBJECT_EVENT_LOG = 25,
                OBJECT_GLOBAL_GROUP = 26,
                OBJECT_TREND_LOG_MULTIPLE = 27,
                OBJECT_LOAD_CONTROL = 28,
                OBJECT_STRUCTURED_VIEW = 29,
                /* what is object type 30? */
                OBJECT_LIGHTING_OUTPUT = 31,
                /* Enumerated values 0-127 are reserved for definition by ASHRAE. */
                /* Enumerated values 128-1023 may be used by others subject to  */
                /* the procedures and constraints described in Clause 23. */
                MAX_ASHRAE_OBJECT_TYPE = 32,        /* used for bit string loop */
                MAX_BACNET_OBJECT_TYPE = 1023
            }

            internal enum BACNET_SEGMENTATION
            {
                SEGMENTATION_BOTH = 0,
                SEGMENTATION_TRANSMIT = 1,
                SEGMENTATION_RECEIVE = 2,
                SEGMENTATION_NONE = 3,
                MAX_BACNET_SEGMENTATION = 4
            }

            internal enum BACNET_VT_CLASS
            {
                VT_CLASS_DEFAULT = 0,
                VT_CLASS_ANSI_X34 = 1,      /* real name is ANSI X3.64 */
                VT_CLASS_DEC_VT52 = 2,
                VT_CLASS_DEC_VT100 = 3,
                VT_CLASS_DEC_VT220 = 4,
                VT_CLASS_HP_700_94 = 5,     /* real name is HP 700/94 */
                VT_CLASS_IBM_3130 = 6
                /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
                /* Enumerated values 64-65535 may be used by others subject to  */
                /* the procedures and constraints described in Clause 23. */
            }

            internal enum BACNET_CHARACTER_STRING_ENCODING
            {
                CHARACTER_ANSI_X34 = 0,
                CHARACTER_MS_DBCS = 1,
                CHARACTER_JISC_6226 = 2,
                CHARACTER_UCS4 = 3,
                CHARACTER_UCS2 = 4,
                CHARACTER_ISO8859 = 5
            }

            internal enum BACNET_APPLICATION_TAG
            {
                BACNET_APPLICATION_TAG_NULL = 0,
                BACNET_APPLICATION_TAG_BOOLEAN = 1,
                BACNET_APPLICATION_TAG_UNSIGNED_INT = 2,
                BACNET_APPLICATION_TAG_SIGNED_INT = 3,
                BACNET_APPLICATION_TAG_REAL = 4,
                BACNET_APPLICATION_TAG_DOUBLE = 5,
                BACNET_APPLICATION_TAG_OCTET_STRING = 6,
                BACNET_APPLICATION_TAG_CHARACTER_STRING = 7,
                BACNET_APPLICATION_TAG_BIT_STRING = 8,
                BACNET_APPLICATION_TAG_ENUMERATED = 9,
                BACNET_APPLICATION_TAG_DATE = 10,
                BACNET_APPLICATION_TAG_TIME = 11,
                BACNET_APPLICATION_TAG_OBJECT_ID = 12,
                BACNET_APPLICATION_TAG_RESERVE1 = 13,
                BACNET_APPLICATION_TAG_RESERVE2 = 14,
                BACNET_APPLICATION_TAG_RESERVE3 = 15,
                MAX_BACNET_APPLICATION_TAG = 16
            }

            /* note: these are not the real values, */
            /* but are shifted left for easy encoding */
            internal enum BACNET_PDU_TYPE
            {
                PDU_TYPE_CONFIRMED_SERVICE_REQUEST = 0,
                PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST = 0x10,
                PDU_TYPE_SIMPLE_ACK = 0x20,
                PDU_TYPE_COMPLEX_ACK = 0x30,
                PDU_TYPE_SEGMENT_ACK = 0x40,
                PDU_TYPE_ERROR = 0x50,
                PDU_TYPE_REJECT = 0x60,
                PDU_TYPE_ABORT = 0x70
            }

            internal enum BACNET_CONFIRMED_SERVICE
            {
                /* Alarm and Event Services */
                SERVICE_CONFIRMED_ACKNOWLEDGE_ALARM = 0,
                SERVICE_CONFIRMED_COV_NOTIFICATION = 1,
                SERVICE_CONFIRMED_EVENT_NOTIFICATION = 2,
                SERVICE_CONFIRMED_GET_ALARM_SUMMARY = 3,
                SERVICE_CONFIRMED_GET_ENROLLMENT_SUMMARY = 4,
                SERVICE_CONFIRMED_GET_EVENT_INFORMATION = 29,
                SERVICE_CONFIRMED_SUBSCRIBE_COV = 5,
                SERVICE_CONFIRMED_SUBSCRIBE_COV_PROPERTY = 28,
                SERVICE_CONFIRMED_LIFE_SAFETY_OPERATION = 27,
                /* File Access Services */
                SERVICE_CONFIRMED_ATOMIC_READ_FILE = 6,
                SERVICE_CONFIRMED_ATOMIC_WRITE_FILE = 7,
                /* Object Access Services */
                SERVICE_CONFIRMED_ADD_LIST_ELEMENT = 8,
                SERVICE_CONFIRMED_REMOVE_LIST_ELEMENT = 9,
                SERVICE_CONFIRMED_CREATE_OBJECT = 10,
                SERVICE_CONFIRMED_DELETE_OBJECT = 11,
                SERVICE_CONFIRMED_READ_PROPERTY = 12,
                SERVICE_CONFIRMED_READ_PROP_CONDITIONAL = 13,
                SERVICE_CONFIRMED_READ_PROP_MULTIPLE = 14,
                SERVICE_CONFIRMED_READ_RANGE = 26,
                SERVICE_CONFIRMED_WRITE_PROPERTY = 15,
                SERVICE_CONFIRMED_WRITE_PROP_MULTIPLE = 16,
                /* Remote Device Management Services */
                SERVICE_CONFIRMED_DEVICE_COMMUNICATION_CONTROL = 17,
                SERVICE_CONFIRMED_PRIVATE_TRANSFER = 18,
                SERVICE_CONFIRMED_TEXT_MESSAGE = 19,
                SERVICE_CONFIRMED_REINITIALIZE_DEVICE = 20,
                /* Virtual Terminal Services */
                SERVICE_CONFIRMED_VT_OPEN = 21,
                SERVICE_CONFIRMED_VT_CLOSE = 22,
                SERVICE_CONFIRMED_VT_DATA = 23,
                /* Security Services */
                SERVICE_CONFIRMED_AUTHENTICATE = 24,
                SERVICE_CONFIRMED_REQUEST_KEY = 25,
                /* Services added after 1995 */
                /* readRange (26) see Object Access Services */
                /* lifeSafetyOperation (27) see Alarm and Event Services */
                /* subscribeCOVProperty (28) see Alarm and Event Services */
                /* getEventInformation (29) see Alarm and Event Services */
                MAX_BACNET_CONFIRMED_SERVICE = 30
            }

            internal enum BACNET_UNCONFIRMED_SERVICE
            {
                SERVICE_UNCONFIRMED_I_AM = 0,
                SERVICE_UNCONFIRMED_I_HAVE = 1,
                SERVICE_UNCONFIRMED_COV_NOTIFICATION = 2,
                SERVICE_UNCONFIRMED_EVENT_NOTIFICATION = 3,
                SERVICE_UNCONFIRMED_PRIVATE_TRANSFER = 4,
                SERVICE_UNCONFIRMED_TEXT_MESSAGE = 5,
                SERVICE_UNCONFIRMED_TIME_SYNCHRONIZATION = 6,
                SERVICE_UNCONFIRMED_WHO_HAS = 7,
                SERVICE_UNCONFIRMED_WHO_IS = 8,
                SERVICE_UNCONFIRMED_UTC_TIME_SYNCHRONIZATION = 9,
                /* Other services to be added as they are defined. */
                /* All choice values in this production are reserved */
                /* for definition by ASHRAE. */
                /* Proprietary extensions are made by using the */
                /* UnconfirmedPrivateTransfer service. See Clause 23. */
                MAX_BACNET_UNCONFIRMED_SERVICE = 10
            }

            /* Bit String Enumerations */
            internal enum BACNET_SERVICES_SUPPORTED
            {
                /* Alarm and Event Services */
                SERVICE_SUPPORTED_ACKNOWLEDGE_ALARM = 0,
                SERVICE_SUPPORTED_CONFIRMED_COV_NOTIFICATION = 1,
                SERVICE_SUPPORTED_CONFIRMED_EVENT_NOTIFICATION = 2,
                SERVICE_SUPPORTED_GET_ALARM_SUMMARY = 3,
                SERVICE_SUPPORTED_GET_ENROLLMENT_SUMMARY = 4,
                SERVICE_SUPPORTED_GET_EVENT_INFORMATION = 39,
                SERVICE_SUPPORTED_SUBSCRIBE_COV = 5,
                SERVICE_SUPPORTED_SUBSCRIBE_COV_PROPERTY = 38,
                SERVICE_SUPPORTED_LIFE_SAFETY_OPERATION = 37,
                /* File Access Services */
                SERVICE_SUPPORTED_ATOMIC_READ_FILE = 6,
                SERVICE_SUPPORTED_ATOMIC_WRITE_FILE = 7,
                /* Object Access Services */
                SERVICE_SUPPORTED_ADD_LIST_ELEMENT = 8,
                SERVICE_SUPPORTED_REMOVE_LIST_ELEMENT = 9,
                SERVICE_SUPPORTED_CREATE_OBJECT = 10,
                SERVICE_SUPPORTED_DELETE_OBJECT = 11,
                SERVICE_SUPPORTED_READ_PROPERTY = 12,
                SERVICE_SUPPORTED_READ_PROP_CONDITIONAL = 13,
                SERVICE_SUPPORTED_READ_PROP_MULTIPLE = 14,
                SERVICE_SUPPORTED_READ_RANGE = 35,
                SERVICE_SUPPORTED_WRITE_PROPERTY = 15,
                SERVICE_SUPPORTED_WRITE_PROP_MULTIPLE = 16,
                /* Remote Device Management Services */
                SERVICE_SUPPORTED_DEVICE_COMMUNICATION_CONTROL = 17,
                SERVICE_SUPPORTED_PRIVATE_TRANSFER = 18,
                SERVICE_SUPPORTED_TEXT_MESSAGE = 19,
                SERVICE_SUPPORTED_REINITIALIZE_DEVICE = 20,
                /* Virtual Terminal Services */
                SERVICE_SUPPORTED_VT_OPEN = 21,
                SERVICE_SUPPORTED_VT_CLOSE = 22,
                SERVICE_SUPPORTED_VT_DATA = 23,
                /* Security Services */
                SERVICE_SUPPORTED_AUTHENTICATE = 24,
                SERVICE_SUPPORTED_REQUEST_KEY = 25,
                SERVICE_SUPPORTED_I_AM = 26,
                SERVICE_SUPPORTED_I_HAVE = 27,
                SERVICE_SUPPORTED_UNCONFIRMED_COV_NOTIFICATION = 28,
                SERVICE_SUPPORTED_UNCONFIRMED_EVENT_NOTIFICATION = 29,
                SERVICE_SUPPORTED_UNCONFIRMED_PRIVATE_TRANSFER = 30,
                SERVICE_SUPPORTED_UNCONFIRMED_TEXT_MESSAGE = 31,
                SERVICE_SUPPORTED_TIME_SYNCHRONIZATION = 32,
                SERVICE_SUPPORTED_UTC_TIME_SYNCHRONIZATION = 36,
                SERVICE_SUPPORTED_WHO_HAS = 33,
                SERVICE_SUPPORTED_WHO_IS = 34,
                /* Other services to be added as they are defined. */
                /* All values in this production are reserved */
                /* for definition by ASHRAE. */
                MAX_BACNET_SERVICES_SUPPORTED = 40
            }

            internal enum BACNET_BVLC_FUNCTION
            {
                BVLC_RESULT = 0,
                BVLC_WRITE_BROADCAST_DISTRIBUTION_TABLE = 1,
                BVLC_READ_BROADCAST_DIST_TABLE = 2,
                BVLC_READ_BROADCAST_DIST_TABLE_ACK = 3,
                BVLC_FORWARDED_NPDU = 4,
                BVLC_REGISTER_FOREIGN_DEVICE = 5,
                BVLC_READ_FOREIGN_DEVICE_TABLE = 6,
                BVLC_READ_FOREIGN_DEVICE_TABLE_ACK = 7,
                BVLC_DELETE_FOREIGN_DEVICE_TABLE_ENTRY = 8,
                BVLC_DISTRIBUTE_BROADCAST_TO_NETWORK = 9,
                BVLC_ORIGINAL_UNICAST_NPDU = 10,
                BVLC_ORIGINAL_BROADCAST_NPDU = 11,
                MAX_BVLC_FUNCTION = 12
            }

            internal enum BACNET_BVLC_RESULT
            {
                BVLC_RESULT_SUCCESSFUL_COMPLETION = 0x0000,
                BVLC_RESULT_WRITE_BROADCAST_DISTRIBUTION_TABLE_NAK = 0x0010,
                BVLC_RESULT_READ_BROADCAST_DISTRIBUTION_TABLE_NAK = 0x0020,
                BVLC_RESULT_REGISTER_FOREIGN_DEVICE_NAK = 0X0030,
                BVLC_RESULT_READ_FOREIGN_DEVICE_TABLE_NAK = 0x0040,
                BVLC_RESULT_DELETE_FOREIGN_DEVICE_TABLE_ENTRY_NAK = 0x0050,
                BVLC_RESULT_DISTRIBUTE_BROADCAST_TO_NETWORK_NAK = 0x0060
            }

            /* Bit String Enumerations */
            internal enum BACNET_STATUS_FLAGS
            {
                STATUS_FLAG_IN_ALARM = 0,
                STATUS_FLAG_FAULT = 1,
                STATUS_FLAG_OVERRIDDEN = 2,
                STATUS_FLAG_OUT_OF_SERVICE = 3
            }

            internal enum BACNET_ACKNOWLEDGMENT_FILTER
            {
                ACKNOWLEDGMENT_FILTER_ALL = 0,
                ACKNOWLEDGMENT_FILTER_ACKED = 1,
                ACKNOWLEDGMENT_FILTER_NOT_ACKED = 2
            }

            internal enum BACNET_EVENT_STATE_FILTER
            {
                EVENT_STATE_FILTER_OFFNORMAL = 0,
                EVENT_STATE_FILTER_FAULT = 1,
                EVENT_STATE_FILTER_NORMAL = 2,
                EVENT_STATE_FILTER_ALL = 3,
                EVENT_STATE_FILTER_ACTIVE = 4
            }

            internal enum BACNET_SELECTION_LOGIC
            {
                SELECTION_LOGIC_AND = 0,
                SELECTION_LOGIC_OR = 1,
                SELECTION_LOGIC_ALL = 2
            }

            internal enum BACNET_RELATION_SPECIFIER
            {
                RELATION_SPECIFIER_EQUAL = 0,
                RELATION_SPECIFIER_NOT_EQUAL = 1,
                RELATION_SPECIFIER_LESS_THAN = 2,
                RELATION_SPECIFIER_GREATER_THAN = 3,
                RELATION_SPECIFIER_LESS_THAN_OR_EQUAL = 4,
                RELATION_SPECIFIER_GREATER_THAN_OR_EQUAL = 5
            }

            internal enum BACNET_COMMUNICATION_ENABLE_DISABLE
            {
                COMMUNICATION_ENABLE = 0,
                COMMUNICATION_DISABLE = 1,
                COMMUNICATION_DISABLE_INITIATION = 2,
                MAX_BACNET_COMMUNICATION_ENABLE_DISABLE = 3
            }

            internal enum BACNET_MESSAGE_PRIORITY
            {
                MESSAGE_PRIORITY_NORMAL = 0,
                MESSAGE_PRIORITY_URGENT = 1,
                MESSAGE_PRIORITY_CRITICAL_EQUIPMENT = 2,
                MESSAGE_PRIORITY_LIFE_SAFETY = 3
            }

            /*Network Layer Message Type */
            /*If Bit 7 of the control octet described in 6.2.2 is 1, */
            /* a message type octet shall be present as shown in Figure 6-1. */
            /* The following message types are indicated: */
            internal enum BACNET_NETWORK_MESSAGE_TYPE
            {
                NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK = 0,
                NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK = 1,
                NETWORK_MESSAGE_I_COULD_BE_ROUTER_TO_NETWORK = 2,
                NETWORK_MESSAGE_REJECT_MESSAGE_TO_NETWORK = 3,
                NETWORK_MESSAGE_ROUTER_BUSY_TO_NETWORK = 4,
                NETWORK_MESSAGE_ROUTER_AVAILABLE_TO_NETWORK = 5,
                NETWORK_MESSAGE_INIT_RT_TABLE = 6,
                NETWORK_MESSAGE_INIT_RT_TABLE_ACK = 7,
                NETWORK_MESSAGE_ESTABLISH_CONNECTION_TO_NETWORK = 8,
                NETWORK_MESSAGE_DISCONNECT_CONNECTION_TO_NETWORK = 9,
                /* X'0A' to X'7F': Reserved for use by ASHRAE, */
                /* X'80' to X'FF': Available for vendor proprietary messages */
                NETWORK_MESSAGE_INVALID = 0x100
            }


            internal enum BACNET_REINITIALIZED_STATE_OF_DEVICE
            {
                REINITIALIZED_STATE_COLD_START = 0,
                REINITIALIZED_STATE_WARM_START = 1,
                REINITIALIZED_STATE_START_BACKUP = 2,
                REINITIALIZED_STATE_END_BACKUP = 3,
                REINITIALIZED_STATE_START_RESTORE = 4,
                REINITIALIZED_STATE_END_RESTORE = 5,
                REINITIALIZED_STATE_ABORT_RESTORE = 6,
                REINITIALIZED_STATE_IDLE = 255
            }

            internal enum BACNET_ABORT_REASON
            {
                ABORT_REASON_OTHER = 0,
                ABORT_REASON_BUFFER_OVERFLOW = 1,
                ABORT_REASON_INVALID_APDU_IN_THIS_STATE = 2,
                ABORT_REASON_PREEMPTED_BY_HIGHER_PRIORITY_TASK = 3,
                ABORT_REASON_SEGMENTATION_NOT_SUPPORTED = 4,
                /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
                /* Enumerated values 64-65535 may be used by others subject to */
                /* the procedures and constraints described in Clause 23. */
                MAX_BACNET_ABORT_REASON = 5,
                FIRST_PROPRIETARY_ABORT_REASON = 64,
                LAST_PROPRIETARY_ABORT_REASON = 65535
            }

            internal enum BACNET_BACNET_REJECT_REASON
            {
                REJECT_REASON_OTHER = 0,
                REJECT_REASON_BUFFER_OVERFLOW = 1,
                REJECT_REASON_INCONSISTENT_PARAMETERS = 2,
                REJECT_REASON_INVALID_PARAMETER_DATA_TYPE = 3,
                REJECT_REASON_INVALID_TAG = 4,
                REJECT_REASON_MISSING_REQUIRED_PARAMETER = 5,
                REJECT_REASON_PARAMETER_OUT_OF_RANGE = 6,
                REJECT_REASON_TOO_MANY_ARGUMENTS = 7,
                REJECT_REASON_UNDEFINED_ENUMERATION = 8,
                REJECT_REASON_UNRECOGNIZED_SERVICE = 9,
                /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
                /* Enumerated values 64-65535 may be used by others subject to */
                /* the procedures and constraints described in Clause 23. */
                MAX_BACNET_REJECT_REASON = 10,
                FIRST_PROPRIETARY_REJECT_REASON = 64,
                LAST_PROPRIETARY_REJECT_REASON = 65535
            }

            internal enum BACNET_ERROR_CLASS
            {
                ERROR_CLASS_DEVICE = 0,
                ERROR_CLASS_OBJECT = 1,
                ERROR_CLASS_PROPERTY = 2,
                ERROR_CLASS_RESOURCES = 3,
                ERROR_CLASS_SECURITY = 4,
                ERROR_CLASS_SERVICES = 5,
                ERROR_CLASS_VT = 6,
                /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
                /* Enumerated values 64-65535 may be used by others subject to */
                /* the procedures and constraints described in Clause 23. */
                MAX_BACNET_ERROR_CLASS = 7,
                FIRST_PROPRIETARY_ERROR_CLASS = 64,
                LAST_PROPRIETARY_ERROR_CLASS = 65535
            }

            /* These are sorted in the order given in
               Clause 18. ERROR, REJECT AND ABORT CODES
               The Class and Code pairings are required
               to be used in accordance with Clause 18. */
            internal enum BACNET_ERROR_CODE
            {
                /* valid for all classes */
                ERROR_CODE_OTHER = 0,

                /* Error Class - Device */
                ERROR_CODE_DEVICE_BUSY = 3,
                ERROR_CODE_CONFIGURATION_IN_PROGRESS = 2,
                ERROR_CODE_OPERATIONAL_PROBLEM = 25,

                /* Error Class - Object */
                ERROR_CODE_DYNAMIC_CREATION_NOT_SUPPORTED = 4,
                ERROR_CODE_NO_OBJECTS_OF_SPECIFIED_TYPE = 17,
                ERROR_CODE_OBJECT_DELETION_NOT_PERMITTED = 23,
                ERROR_CODE_OBJECT_IDENTIFIER_ALREADY_EXISTS = 24,
                ERROR_CODE_READ_ACCESS_DENIED = 27,
                ERROR_CODE_UNKNOWN_OBJECT = 31,
                ERROR_CODE_UNSUPPORTED_OBJECT_TYPE = 36,

                /* Error Class - Property */
                ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41,
                ERROR_CODE_DATATYPE_NOT_SUPPORTED = 47,
                ERROR_CODE_INCONSISTENT_SELECTION_CRITERION = 8,
                ERROR_CODE_INVALID_ARRAY_INDEX = 42,
                ERROR_CODE_INVALID_DATA_TYPE = 9,
                ERROR_CODE_NOT_COV_PROPERTY = 44,
                ERROR_CODE_OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED = 45,
                ERROR_CODE_PROPERTY_IS_NOT_AN_ARRAY = 50,
                /* ERROR_CODE_READ_ACCESS_DENIED = 27, */
                ERROR_CODE_UNKNOWN_PROPERTY = 32,
                ERROR_CODE_VALUE_OUT_OF_RANGE = 37,
                ERROR_CODE_WRITE_ACCESS_DENIED = 40,

                /* Error Class - Resources */
                ERROR_CODE_NO_SPACE_FOR_OBJECT = 18,
                ERROR_CODE_NO_SPACE_TO_ADD_LIST_ELEMENT = 19,
                ERROR_CODE_NO_SPACE_TO_WRITE_PROPERTY = 20,

                /* Error Class - Security */
                ERROR_CODE_AUTHENTICATION_FAILED = 1,
                /* ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41, */
                ERROR_CODE_INCOMPATIBLE_SECURITY_LEVELS = 6,
                ERROR_CODE_INVALID_OPERATOR_NAME = 12,
                ERROR_CODE_KEY_GENERATION_ERROR = 15,
                ERROR_CODE_PASSWORD_FAILURE = 26,
                ERROR_CODE_SECURITY_NOT_SUPPORTED = 28,
                ERROR_CODE_TIMEOUT = 30,

                /* Error Class - Services */
                /* ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41, */
                ERROR_CODE_COV_SUBSCRIPTION_FAILED = 43,
                ERROR_CODE_DUPLICATE_NAME = 48,
                ERROR_CODE_DUPLICATE_OBJECT_ID = 49,
                ERROR_CODE_FILE_ACCESS_DENIED = 5,
                ERROR_CODE_INCONSISTENT_PARAMETERS = 7,
                ERROR_CODE_INVALID_CONFIGURATION_DATA = 46,
                ERROR_CODE_INVALID_FILE_ACCESS_METHOD = 10,
                ERROR_CODE_ERROR_CODE_INVALID_FILE_START_POSITION = 11,
                ERROR_CODE_INVALID_PARAMETER_DATA_TYPE = 13,
                ERROR_CODE_INVALID_TIME_STAMP = 14,
                ERROR_CODE_MISSING_REQUIRED_PARAMETER = 16,
                /* ERROR_CODE_OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED = 45, */
                ERROR_CODE_PROPERTY_IS_NOT_A_LIST = 22,
                ERROR_CODE_SERVICE_REQUEST_DENIED = 29,

                /* Error Class - VT */
                ERROR_CODE_UNKNOWN_VT_CLASS = 34,
                ERROR_CODE_UNKNOWN_VT_SESSION = 35,
                ERROR_CODE_NO_VT_SESSIONS_AVAILABLE = 21,
                ERROR_CODE_VT_SESSION_ALREADY_CLOSED = 38,
                ERROR_CODE_VT_SESSION_TERMINATION_FAILURE = 39,

                /* unused */
                ERROR_CODE_RESERVED1 = 33,
                /* Enumerated values 0-255 are reserved for definition by ASHRAE. */
                /* Enumerated values 256-65535 may be used by others subject to */
                /* the procedures and constraints described in Clause 23. */
                /* The last enumeration used in this version is 50. */
                MAX_BACNET_ERROR_CODE = 51,
                FIRST_PROPRIETARY_ERROR_CODE = 256,
                LAST_PROPRIETARY_ERROR_CODE = 65535
            }

            internal enum BACNET_REINITIALIZED_STATE
            {
                BACNET_REINIT_COLDSTART = 0,
                BACNET_REINIT_WARMSTART = 1,
                BACNET_REINIT_STARTBACKUP = 2,
                BACNET_REINIT_ENDBACKUP = 3,
                BACNET_REINIT_STARTRESTORE = 4,
                BACNET_REINIT_ENDRESTORE = 5,
                BACNET_REINIT_ABORTRESTORE = 6,
                MAX_BACNET_REINITIALIZED_STATE = 7
            }

            internal enum BACNET_NODE_TYPE
            {
                BACNET_NODE_UNKNOWN = 0,
                BACNET_NODE_SYSTEM = 1,
                BACNET_NODE_NETWORK = 2,
                BACNET_NODE_DEVICE = 3,
                BACNET_NODE_ORGANIZATIONAL = 4,
                BACNET_NODE_AREA = 5,
                BACNET_NODE_EQUIPMENT = 6,
                BACNET_NODE_POINT = 7,
                BACNET_NODE_COLLECTION = 8,
                BACNET_NODE_PROPERTY = 9,
                BACNET_NODE_FUNCTIONAL = 10,
                BACNET_NODE_OTHER = 11
            }

            internal enum BACNET_SHED_STATE
            {
                BACNET_SHED_INACTIVE = 0,
                BACNET_SHED_REQUEST_PENDING = 1,
                BACNET_SHED_COMPLIANT = 2,
                BACNET_SHED_NON_COMPLIANT = 3
            }

            internal enum BACNET_LIGHTING_OPERATION
            {
                BACNET_LIGHTS_STOP = 0,
                BACNET_LIGHTS_FADE_TO = 1,
                BACNET_LIGHTS_FADE_TO_OVER = 2,
                BACNET_LIGHTS_RAMP_TO = 3,
                BACNET_LIGHTS_RAMP_TO_AT_RATE = 4,
                BACNET_LIGHTS_RAMP_UP = 5,
                BACNET_LIGHTS_RAMP_UP_AT_RATE = 6,
                BACNET_LIGHTS_RAMP_DOWN = 7,
                BACNET_LIGHTS_RAMP_DOWN_AT_RATE = 8,
                BACNET_LIGHTS_STEP_UP = 9,
                BACNET_LIGHTS_STEP_DOWN = 10,
                BACNET_LIGHTS_STEP_UP_BY = 11,
                BACNET_LIGHTS_STEP_DOWN_BY = 12,
                BACNET_LIGHTS_GOTO_LEVEL = 13,
                BACNET_LIGHTS_RELINQUISH = 14
            }

            /* NOTE: BACNET_DAYS_OF_WEEK is different than BACNET_WEEKDAY */
            /* 0=Monday-6=Sunday */
            internal enum BACNET_DAYS_OF_WEEK
            {
                BACNET_DAYS_OF_WEEK_MONDAY = 0,
                BACNET_DAYS_OF_WEEK_TUESDAY = 1,
                BACNET_DAYS_OF_WEEK_WEDNESDAY = 2,
                BACNET_DAYS_OF_WEEK_THURSDAY = 3,
                BACNET_DAYS_OF_WEEK_FRIDAY = 4,
                BACNET_DAYS_OF_WEEK_SATURDAY = 5,
                BACNET_DAYS_OF_WEEK_SUNDAY = 6
            }
        }
        #endregion
    }
}