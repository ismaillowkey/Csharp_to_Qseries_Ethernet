using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.Profinet.Melsec;

namespace WindowsForms_to_Qseries
{
	public partial class Form1 : MetroFramework.Forms.MetroForm
	{
		private MelsecMcNet melsec_net = null;
		private Thread thread = null;
		private int timeSleep = 10;
		private bool isThreadRun = false;
		private List<PictureBox> LampXInput = new List<PictureBox>();
		private List<PictureBox> LampYOutput = new List<PictureBox>();

		public Form1()
		{
			InitializeComponent();
			melsec_net = new MelsecMcNet();

			LampXInput.Add(PBX0); LampYOutput.Add(PBY0);
			LampXInput.Add(PBX1); LampYOutput.Add(PBY1);
			LampXInput.Add(PBX2); LampYOutput.Add(PBY2);
			LampXInput.Add(PBX3); LampYOutput.Add(PBY3);
			LampXInput.Add(PBX4); LampYOutput.Add(PBY4);
			LampXInput.Add(PBX5); LampYOutput.Add(PBY5);
			LampXInput.Add(PBX6); LampYOutput.Add(PBY6);
			LampXInput.Add(PBX7); LampYOutput.Add(PBY7);
		}

		private void BtnConnect_Click(object sender, EventArgs e)
		{
			if (!System.Net.IPAddress.TryParse(TxtIPPLC.Text, out System.Net.IPAddress address))
			{
				MessageBox.Show("Wrong IP Address");
				return;
			}

			melsec_net.IpAddress = TxtIPPLC.Text;

			if (!int.TryParse(TxtPortPLC.Text, out int port))
			{
				MessageBox.Show("Port PLC Wrong");
				return;
			}

			melsec_net.Port = port;
			melsec_net.ReceiveTimeOut = 1000;
			melsec_net.ConnectTimeOut = 1000;
			melsec_net.ConnectClose();

			try
			{
				OperateResult connect = melsec_net.ConnectServer();
				if (connect.IsSuccess)
				{
					MessageBox.Show("ConnectedSuccess");
					TxtIPPLC.Enabled = false;
					TxtPortPLC.Enabled = false;
					BtnConnect.Enabled = false;
					BtnDisconnect.Enabled = true;

					StartReading();
				}
				else
				{
					MessageBox.Show("Connected Failed");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void StartReading()
		{
			// Start the background thread, periodically read the data in the plc, and then display in the curve control
			if (!isThreadRun)
			{
				//button27.Text = "Stop";
				isThreadRun = true;
				thread = new Thread(ThreadReadServer)
				{
					IsBackground = true
				};
				thread.Start();
			}
			else
			{
				//button27.Text = "Start";
				isThreadRun = false;
			}
		}

		private void ThreadReadServer()
		{

			if (melsec_net != null)
			{
				while (isThreadRun)
				{
					Thread.Sleep(timeSleep);
					try
					{
						//Task.Run(() =>
						//{
							// Read X0-X7
							var ReadXInput = melsec_net.ReadBool("X070", 8);
							
							Console.WriteLine(ReadXInput.Message);
								if (ReadXInput.IsSuccess)
								{
									// Tampilkan Data
									if (isThreadRun)
									{
										//Invoke(new Action<short>(AddDataCurve), read.Content);
										this.Invoke((MethodInvoker)delegate
										{
											TSStatusPLC.Text = "Connected to PLC";

											// runs on UI thread
											var value = ReadXInput.Content;
											for (ushort i = 0; i <= LampXInput.Count - 1; i++)
											{
												if (value[i])
												{
													LampXInput[i].Image = Properties.Resources.lamp_green_on;
												}
												else
												{
													LampXInput[i].Image = Properties.Resources.lamp_green_off;
												}

											}

											//metroLabel4.Text = Convert.ToString(value[0]);

										});
									}
								}
								else
								{
									this.Invoke((MethodInvoker)delegate
									{
										// runs on UI thread
										TSStatusPLC.Text = "reconnecting...";
									});
								}
					//});


					Task.Run(() => {
							// Read Y0-Y7
							var ReadYOutput = melsec_net.ReadBool("Y0A0", 8);
							if (ReadYOutput.IsSuccess)
							{
								// Tampilkan Data
								if (isThreadRun)
								{
									//Invoke(new Action<short>(AddDataCurve), read.Content);
									this.Invoke((MethodInvoker)delegate
									{
										// runs on UI thread
										var value = ReadYOutput.Content;
										for (ushort i = 0; i <= LampYOutput.Count - 1; i++)
										{
											if (value[i])
											{
												LampYOutput[i].Image = Properties.Resources.lamp_green_on;
											}
											else
											{
												LampYOutput[i].Image = Properties.Resources.lamp_green_off;
											}
										}
									});
								}
							}
						});
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						//MessageBox.Show("Read failed：" + ex.Message);
					}
				}
			}
		}

		private void BtnDisconnect_Click(object sender, EventArgs e)
		{
			melsec_net.ConnectClose();
			TxtIPPLC.Enabled = true;
			TxtPortPLC.Enabled = true;
			BtnConnect.Enabled = true;
			BtnDisconnect.Enabled = false;
		}
	}
}
