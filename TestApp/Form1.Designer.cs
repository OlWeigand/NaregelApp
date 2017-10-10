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

namespace BACnetTest
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.WriteAV = new System.Windows.Forms.Button();
			this.WriteBV = new System.Windows.Forms.Button();
			this.ResultLabel = new System.Windows.Forms.Label();
			this.ReadAV = new System.Windows.Forms.Button();
			this.ReadBV = new System.Windows.Forms.Button();
			this.VersionLabel = new System.Windows.Forms.Label();
			this.DeviceLabel = new System.Windows.Forms.Label();
			this.GetDevicebt = new System.Windows.Forms.Button();
			this.nmGetDevice = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.nmGetDevice)).BeginInit();
			this.SuspendLayout();
			// 
			// WriteAV
			// 
			this.WriteAV.Location = new System.Drawing.Point(14, 23);
			this.WriteAV.Name = "WriteAV";
			this.WriteAV.Size = new System.Drawing.Size(102, 23);
			this.WriteAV.TabIndex = 61;
			this.WriteAV.Text = "Write AV";
			this.WriteAV.UseVisualStyleBackColor = true;
			this.WriteAV.Click += new System.EventHandler(this.WriteAV_Click);
			// 
			// WriteBV
			// 
			this.WriteBV.Location = new System.Drawing.Point(14, 53);
			this.WriteBV.Name = "WriteBV";
			this.WriteBV.Size = new System.Drawing.Size(102, 23);
			this.WriteBV.TabIndex = 61;
			this.WriteBV.Text = "Write BV";
			this.WriteBV.UseVisualStyleBackColor = true;
			this.WriteBV.Click += new System.EventHandler(this.WriteBV_Click);
			// 
			// ResultLabel
			// 
			this.ResultLabel.Location = new System.Drawing.Point(12, 91);
			this.ResultLabel.Name = "ResultLabel";
			this.ResultLabel.Size = new System.Drawing.Size(474, 13);
			this.ResultLabel.TabIndex = 63;
			this.ResultLabel.Text = "ResultLabel";
			// 
			// ReadAV
			// 
			this.ReadAV.Location = new System.Drawing.Point(155, 23);
			this.ReadAV.Name = "ReadAV";
			this.ReadAV.Size = new System.Drawing.Size(102, 23);
			this.ReadAV.TabIndex = 61;
			this.ReadAV.Text = "Read AV";
			this.ReadAV.UseVisualStyleBackColor = true;
			this.ReadAV.Click += new System.EventHandler(this.ReadAV_Click);
			// 
			// ReadBV
			// 
			this.ReadBV.Location = new System.Drawing.Point(155, 53);
			this.ReadBV.Name = "ReadBV";
			this.ReadBV.Size = new System.Drawing.Size(102, 23);
			this.ReadBV.TabIndex = 61;
			this.ReadBV.Text = "Read BV";
			this.ReadBV.UseVisualStyleBackColor = true;
			this.ReadBV.Click += new System.EventHandler(this.ReadBV_Click);
			// 
			// VersionLabel
			// 
			this.VersionLabel.Location = new System.Drawing.Point(12, 113);
			this.VersionLabel.Name = "VersionLabel";
			this.VersionLabel.Size = new System.Drawing.Size(474, 13);
			this.VersionLabel.TabIndex = 63;
			this.VersionLabel.Text = "VersionLabel";
			// 
			// DeviceLabel
			// 
			this.DeviceLabel.Location = new System.Drawing.Point(11, 133);
			this.DeviceLabel.Name = "DeviceLabel";
			this.DeviceLabel.Size = new System.Drawing.Size(474, 74);
			this.DeviceLabel.TabIndex = 64;
			this.DeviceLabel.Text = "DeviceLabel";
			// 
			// GetDevicebt
			// 
			this.GetDevicebt.Location = new System.Drawing.Point(292, 22);
			this.GetDevicebt.Name = "GetDevicebt";
			this.GetDevicebt.Size = new System.Drawing.Size(75, 23);
			this.GetDevicebt.TabIndex = 65;
			this.GetDevicebt.Text = "Get Device";
			this.GetDevicebt.UseVisualStyleBackColor = true;
			this.GetDevicebt.Click += new System.EventHandler(this.GetDevicebt_Click);
			// 
			// nmGetDevice
			// 
			this.nmGetDevice.Location = new System.Drawing.Point(374, 25);
			this.nmGetDevice.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.nmGetDevice.Name = "nmGetDevice";
			this.nmGetDevice.Size = new System.Drawing.Size(120, 20);
			this.nmGetDevice.TabIndex = 66;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(498, 218);
			this.Controls.Add(this.nmGetDevice);
			this.Controls.Add(this.GetDevicebt);
			this.Controls.Add(this.DeviceLabel);
			this.Controls.Add(this.VersionLabel);
			this.Controls.Add(this.ResultLabel);
			this.Controls.Add(this.WriteBV);
			this.Controls.Add(this.ReadBV);
			this.Controls.Add(this.ReadAV);
			this.Controls.Add(this.WriteAV);
			this.Name = "MainForm";
			this.Text = "BACnet Test";
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.nmGetDevice)).EndInit();
			this.ResumeLayout(false);

    }

    #endregion
        private System.Windows.Forms.Button WriteAV;
        private System.Windows.Forms.Button WriteBV;
        private System.Windows.Forms.Label ResultLabel;
        private System.Windows.Forms.Button ReadAV;
        private System.Windows.Forms.Button ReadBV;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label DeviceLabel;
		private System.Windows.Forms.Button GetDevicebt;
		private System.Windows.Forms.NumericUpDown nmGetDevice;
	}
}

