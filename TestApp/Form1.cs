namespace BACnetTest
{
    // directives   
    using System;
    using System.Windows.Forms;
    using RegelPartners.Protocols;
	using System.Net;
	using System.Linq;
	using System.Net.Sockets;

	public partial class MainForm : Form
    {

		private static IPAddress LocalIPAddress()
		{
			if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
			{
				return null;
			}

			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			return host
				.AddressList
				.LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
		}

		// fields
		// TODO: 
		//private BACnet bacnet = new BACnet();

		// Get the local IP Address
		private static IPAddress localip = LocalIPAddress();

		private BACnet bacnet = new BACnet((localip).ToString(), 47808);
		public BACnet.Device bacnetDevice = null;// bacnet.GetDevice(99999);

		public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ResultLabel.Text = null;
            VersionLabel.Text = "Assembly: " + About.ToString();
			if (bacnetDevice != null)
			{
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
        }

        private void WriteAV_Click(object sender, EventArgs e)
        {
            ResultLabel.Text = "Bezig met schrijven...";
            bool result = true;
            int writeCount = 20;
            BACnet.Device bacnetDevice = bacnet.GetDevice(Convert.ToInt32(nmGetDevice.Value));
			//BACnet.Device bacnetDevice = new BACnet.Device("192.168.14.221", 47808); // Priva, Virtual Network
			if (bacnetDevice != null)
			{
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
            BACnet.PropertyValue propertyValue = new BACnet.PropertyValue(BACnet.ObjectType.AnalogValue, 2, DateTime.Now.Second);
            for (int i = 0; i < writeCount; i++)
            {
                result &= bacnet.WritePropertyValue(bacnetDevice, propertyValue);
            }
            ResultLabel.Text = "Resultaat (" + writeCount.ToString() + "): " + (result ? "Success, " + propertyValue.ToString() : "Failure");
        }

        private void ReadAV_Click(object sender, EventArgs e)
        {
            ResultLabel.Text = "Bezig met lezen...";
            bool result = false;
			BACnet.Device bacnetDevice = bacnet.GetDevice(Convert.ToInt32(nmGetDevice.Value));
			//BACnet.Device bacnetDevice = new BACnet.Device("192.168.14.221", 47808); // Priva, Virtual Network
			if (bacnetDevice != null)
			{
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
            BACnet.PropertyValue propertyValue = new BACnet.PropertyValue(BACnet.ObjectType.AnalogValue, 2);
            result = bacnet.ReadPropertyValue(bacnetDevice, ref propertyValue);
            ResultLabel.Text = "Resultaat: " + (result ? "Success, " + propertyValue.ToString() : "Failure");
        }

        private void WriteBV_Click(object sender, EventArgs e)
        {
            ResultLabel.Text = "Bezig met schrijven...";
            bool result = false;
			BACnet.Device bacnetDevice = bacnet.GetDevice(Convert.ToInt32(nmGetDevice.Value));
			//BACnet.Device bacnetDevice = new BACnet.Device("192.168.14.221", 47808, 99999); // Priva, Virtual Network
			if (bacnetDevice != null)
			{
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
            BACnet.PropertyValue propertyValue = new BACnet.PropertyValue(BACnet.ObjectType.BinaryValue, 9, true);
            result = bacnet.WritePropertyValue(bacnetDevice, propertyValue);
            ResultLabel.Text = "Resultaat: " + (result ? "Success, " + propertyValue.ToString() : "Failure");
        }

        private void ReadBV_Click(object sender, EventArgs e)
        {
            ResultLabel.Text = "Bezig met lezen...";
            bool result = false;
			BACnet.Device bacnetDevice = bacnet.GetDevice(Convert.ToInt32(nmGetDevice.Value));
			//BACnet.Device bacnetDevice = new BACnet.Device("192.168.14.221", 47808, 99999); // Priva, Virtual Network
			if (bacnetDevice != null)
			{
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
            BACnet.PropertyValue propertyValue = new BACnet.PropertyValue(BACnet.ObjectType.BinaryValue, 9);
            result = bacnet.ReadPropertyValue(bacnetDevice, ref propertyValue);
            ResultLabel.Text = "Resultaat: " + (result ? "Success, " + propertyValue.ToString() : "Failure");
        }

		// Tests 'Get Device' functionality with the selected device #
		private void GetDevicebt_Click(object sender, EventArgs e)
		{
			bacnetDevice = bacnet.GetDevice(Convert.ToInt32(nmGetDevice.Value));
			if(bacnetDevice != null) {
				DeviceLabel.Text = "Device: " + bacnetDevice.ToString();
			}
		}
	}
}
