using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Text;


namespace ChatClient
{
	public class Form1 : Form
	{
		string userName = "Guest";
		//string passWord = "password";
		string SentText;

		Thread connectThread;
		Thread breakConnectionThread;

		Byte[] SendBuffer;
		Byte[] Buffer;

		TcpClient Client;

		public Form1()
		{
			InitializeComponent();

			Form.CheckForIllegalCrossThreadCalls = false;
		}

	    protected override void OnClosed(EventArgs e)
		{
			Logout ();
			Thread.Sleep(20);
		    base.OnClosed(e);
		}

		public void Logout ()
		{
			if (_connected) {
				SendBuffer = System.Text.Encoding.UTF8.GetBytes(userName + " has logged off.");
				Client.GetStream().Write(SendBuffer, 0, SendBuffer.Length);
				Thread.Sleep(20);
				Buffer = new Byte[Client.Available];
				Client.GetStream().Read(Buffer, 0, Buffer.Length);
				outputBox.Text = outputBox.Text + Environment.NewLine + System.Text.Encoding.UTF8.GetString(Buffer);
				_connected = false;
				connectButton.Text = "Connect";
			}
		}

		public void connectButton_Click (object sender, EventArgs e)
		{
			if (_connected)
			{
				_connected = false;
			}
			else
			{
				if (!_connected)
				{
					createConnection ();
				}
			}
		}

		public void createConnection ()
		{
			if(_connecting == true)
			{
				msg ("SERVER", "Please wait. The client is trying to process your previous request.");
				return;
			}
			connectThread = new Thread(Connect);
			connectThread.IsBackground = true;
			connectThread.Start();
		}

		public Boolean IPv4Check(string ip)
		{
			string[] parts = ip.Split('.');
			if (parts.Length < 4)
			{
			    // not a IPv4 string in X.X.X.X format
				return false;
			}
			else
			{
			    foreach(string part in parts)
			    {
			        byte checkPart = 0;
			        if(!byte.TryParse(part, out checkPart))
			        {
			            // not a valid IPv4 string in X.X.X.X format
						return false;
			        }
			    }
			    // it is a valid IPv4 string in X.X.X.X format
				return true;
			}
		}

		public void breakConnection ()
		{
			int timeOut = 0;
			while (_connecting) {
				if(timeOut >= 2)
				{
					_connecting = false;
					msg("SERVER","Connection timeout. Perhaps you entered an invalid IP Address?");
				}
				timeOut++;
				Thread.Sleep(1000);

			}
		}

		public void Connect ()
		{
			// Check if they are currently connecting
			_connecting = true;

			// Makes sure they aren't trying to connect too long.
			// If 15 seconds pass, cancel.
			breakConnectionThread = new Thread(breakConnection);
			breakConnectionThread.IsBackground = true;
			breakConnectionThread.Start();

			string IP = textIP.Text;

			/*Console.WriteLine("IP is: " + IP);
			IPAddress DNSIP = Dns.GetHostAddresses(IP)[0];
			Console.WriteLine("DNS IP is: " + DNSIP);
			IP = Convert.ToString(DNSIP);
			Console.WriteLine("DNS IP coverted is: " + IP);

			if(!IPv4Check (IP))
			{
				msg ("SERVER", "Please enter a valid IP number.");
				_connecting = false;
				return;
			}*/


			int portNumber;

			if (!(int.TryParse (textPort.Text, out portNumber))) {
				msg ("SERVER", "Please enter a valid port number.");
				_connecting = false;
				return;
			}

			if (0 >= portNumber | portNumber >= 65536) {
				msg ("SERVER", "Your port number is out of range.");
				_connecting = false;
				return;
			}

			try
			{
				Client = new TcpClient (IP, portNumber);
				statusLabel.Text = "Server Connected";
				connectButton.Text = "Disconnect";
				SendBuffer = System.Text.Encoding.UTF8.GetBytes(userName + " has logged in.");
				Client.GetStream().Write(SendBuffer, 0, SendBuffer.Length);
				_connected = true;
				_connecting = false;

				breakConnectionThread.Abort();

				while(_connected)
				{
					if(SentText != null)
					{
						SendBuffer = System.Text.Encoding.UTF8.GetBytes(SentText);
						Client.GetStream().Write(SendBuffer, 0, SendBuffer.Length);
						SentText = null;
					}

					if (Client.Available > 0)
					{
						Buffer = new Byte[Client.Available];
						Client.GetStream().Read(Buffer, 0, Buffer.Length);
						string chatText = System.Text.Encoding.UTF8.GetString(Buffer);
						if(chatText == "START OF WHO LIST 66671208")
						{
							whoList.Text = "";
							chatText = "";
						}
						else
						{
							if(chatText.Contains("USERS >>"))
							{
								whoList.Text = whoList.Text + Environment.NewLine + chatText.Substring(0,chatText.Length-1);
							}
							else
							{
								outputBox.Text = outputBox.Text + Environment.NewLine + System.Text.Encoding.UTF8.GetString(Buffer);
								//outputBox.AppendText(Environment.NewLine + System.Text.Encoding.UTF8.GetString(Buffer));
								outputBox.Refresh();
							}
						}
					}

					Thread.Sleep(5);
				}
				SendBuffer = System.Text.Encoding.UTF8.GetBytes(userName + " has logged off.");
				Client.GetStream().Write(SendBuffer, 0, SendBuffer.Length);
				Thread.Sleep(20);
				Buffer = new Byte[Client.Available];
				Client.GetStream().Read(Buffer, 0, Buffer.Length);
				outputBox.Text = outputBox.Text + Environment.NewLine + System.Text.Encoding.UTF8.GetString(Buffer);
				_connected = false;
				connectButton.Text = "Connect";
			}
			catch (SocketException SE)
			{
				msg("SERVER","An error occure while connecting: " + SE.Message + ".");
				_connecting = false;
				return;
			}
			catch (Exception exception)
			{
				string error = "An error occured while connecting [" + exception.Message + "]\n";
				throw new Exception(error);
			}
		}

		private volatile bool _connected = false;
		private volatile bool _connecting = false;

		//
		// Button functions: Connect, Login, Register
		//
		/*
		private void connectButton_Click(object sender, EventArgs e)
		{
			String Address = null;
			String Name = null;
			String TypeBuffer = "";

			Console.Write("Server IP Address or Domain Name: ");
			while (Address == null)
				Address = Console.ReadLine();

			while (Name == null)
				Name = Console.ReadLine();

			TcpClient Client = new TcpClient(Address, 6667);
			while (true)
			{
				while (Console.KeyAvailable)
				{
					ConsoleKeyInfo CI = Console.ReadKey();
					TypeBuffer += CI.KeyChar;

					if (CI.Key == ConsoleKey.Enter)
					{
						Byte[] SendBuffer = System.Text.Encoding.UTF8.GetBytes("SAY " + Name + " " + TypeBuffer);
						Client.GetStream().Write(SendBuffer, 0, SendBuffer.Length);
						TypeBuffer = "";
					}
				}

				if (Client.Available > 0)
				{
					Byte[] Buffer = new Byte[Client.Available];
					Client.GetStream().Read(Buffer, 0, Buffer.Length);
					Console.WriteLine("");
					Console.WriteLine(System.Text.Encoding.UTF8.GetString(Buffer));
				}

				Thread.Sleep(5);
			}
		}
        */

		private void loginButton_Click(object sender, EventArgs e)
		{
			msg("SERVER","This feature is not finished. It is still under construction.");
		}

		private void registerButton_Click(object sender, EventArgs e)
		{
			msg("SERVER","This feature is not finished. It is still under construction.");
		}


		//
		// KeyHandlers: For OnPress buttons.
		//
		private void inputBox_KeyHandler(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if(textUsername.TextLength<4)
				{
					msg("SERVER","Your username must be more than 3 characters.");
					return;
				}

				if(textUsername.TextLength>18)
				{
					msg("SERVER","Your username is greater than 18 characters. Please shorten it.");
					return;
				}

				if(inputBox.TextLength<1)
				{
					return;
				}
				chat(inputBox.Text);
				inputBox.Text = "";
			}
		}

		private void loginBox_KeyHandler(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if(textUsername.TextLength<4)
				{
					msg("SERVER","Your username must be more than 3 characters.");
					return;
				}

				if(textUsername.TextLength>18)
				{
					msg("SERVER","Your username is greater than 18 characters. Please shorten it.");
					return;
				}
				msg("SERVER","Your username is now " + textUsername.Text + ".");
				string rememberUN = this.userName;
				this.userName = textUsername.Text;
				chatLabel.Text = this.userName + ":";

				int rememberSX = inputBox.Size.Width;
				int rememberSY = inputBox.Size.Height;
				int rememberLX = inputBox.Location.X;
				int rememberLY = inputBox.Location.Y;


				inputBox.Location = new System.Drawing.Point(Convert.ToInt32((rememberLX/rememberUN.Length)*userName.Length), rememberLY);
	            inputBox.Size = new System.Drawing.Size(Convert.ToInt32((rememberSX/rememberUN.Length)*userName.Length), rememberSY);
			}
		}


		//
		// System Message and User Chat System
		//
		public void msg(string user, string mesg)
        {
            outputBox.Text = outputBox.Text + Environment.NewLine + user + " >> " + mesg;
        }

		public void chat (string mesg)
		{
			if(!_connected)
			{
				msg ("SERVER", "Please connect to a server first before writing.");
				return;
			}

			if(mesg.Length > 500)
			{
				msg ("SERVER", "Your message is longer than 500 words. Please shorten it.");
				msg ("SAVED MESSAGE", mesg);
				return;
			}

			if(SentText != null)
				SentText = SentText + Environment.NewLine + userName + ": " + mesg;
			else
			{
				SentText = userName + ": " + mesg;
			}
		}

		//
		// User Interface
		//
		public void InitializeComponent ()
		{
			this.connectButton = new System.Windows.Forms.Button();
			this.ipLabel = new System.Windows.Forms.Label();
			this.portLabel = new System.Windows.Forms.Label();
			this.textIP = new System.Windows.Forms.TextBox();
			this.textPort = new System.Windows.Forms.TextBox();
			this.statusLabel = new System.Windows.Forms.Label();
			this.chatLabel = new System.Windows.Forms.Label();
			this.outputBox = new System.Windows.Forms.TextBox();
			this.whoList = new System.Windows.Forms.TextBox();
			this.inputBox = new System.Windows.Forms.TextBox();
			this.textUsername = new System.Windows.Forms.TextBox();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.usernameLabel = new System.Windows.Forms.Label();
			this.passwordLabel = new System.Windows.Forms.Label();
			this.loginButton = new System.Windows.Forms.Button();
			this.registerButton = new System.Windows.Forms.Button();
    		this.SuspendLayout();


			//
			// Connect Button
			//
			this.connectButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.connectButton.Location = new System.Drawing.Point(445, 67);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new System.Drawing.Size(63, 20);
			this.connectButton.TabIndex = 1;
			this.connectButton.Text = "Connect";
			this.connectButton.UseVisualStyleBackColor = true;
			this.connectButton.Click += new System.EventHandler(this.connectButton_Click);


			//
			// IP Label
			//
			this.ipLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.ipLabel.AutoSize = true;
			this.ipLabel.Location = new System.Drawing.Point(280, 14);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(94, 13);
            this.ipLabel.TabIndex = 0;
            this.ipLabel.Text = "IP Address:";


			//
			// Port Label
			//
			this.portLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.portLabel.AutoSize = true;
			this.portLabel.Location = new System.Drawing.Point(302, 42);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(94, 13);
            this.portLabel.TabIndex = 0;
            this.portLabel.Text = "Port:";

			//
			// IP Text Box
			//
			this.textIP.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.textIP.Location = new System.Drawing.Point(340, 12);
            this.textIP.Name = "textIP";
            this.textIP.Size = new System.Drawing.Size(169, 20);
            this.textIP.TabIndex = 2;
			this.textIP.Text = "xirre.servegame.org";

			//
			// Port Text Box
			//
			this.textPort.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.textPort.Location = new System.Drawing.Point(340, 40);
            this.textPort.Name = "textPort";
            this.textPort.Size = new System.Drawing.Size(65, 20);
            this.textPort.TabIndex = 2;
			this.textPort.Text = "6667";


			//
			// Status Label
			//
			this.statusLabel.AutoSize = true;
			this.statusLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.statusLabel.Location = new System.Drawing.Point(340, 69);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(94, 13);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Not Connected";


			//
			// Chat Label
			//
			this.chatLabel.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.chatLabel.AutoSize = true;
			this.chatLabel.Location = new System.Drawing.Point(5, 570);
            this.chatLabel.Name = "chatLabel";
            this.chatLabel.Size = new System.Drawing.Size(94, 13);
            this.chatLabel.TabIndex = 0;
			this.chatLabel.Text = "Guest:";


			//
			// Chat Output Box
			//
			this.outputBox.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.outputBox.Multiline = true;
			this.outputBox.BackColor = System.Drawing.ColorTranslator.FromHtml("#00adee");
			this.outputBox.ReadOnly = true;
			this.outputBox.Location = new System.Drawing.Point(5, 95);
            this.outputBox.Name = "outputBox";
            this.outputBox.Size = new System.Drawing.Size(390, 450);
            this.outputBox.TabIndex = 2;
			this.outputBox.ScrollBars = ScrollBars.Vertical;
			this.outputBox.Text = "Welcome to Xirre Chat!" + Environment.NewLine + "Login does not work as of yet. However, you can type a username and use that to talk with. Don't change the server settings. They're not fully bugproof." + Environment.NewLine + "Coming soon: Register/Login, Who List, Host your own chat room, bugproof server settings.";


			//
			// Logged in People
			//
			this.whoList.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right);
			this.whoList.Multiline = true;
			this.whoList.BackColor = System.Drawing.ColorTranslator.FromHtml("#00adee");
			this.whoList.ReadOnly = true;
			this.whoList.Location = new System.Drawing.Point(395, 95);
			this.whoList.Name = "whoList";
			this.whoList.Size = new System.Drawing.Size(115, 450);
			this.whoList.TabIndex = 2;
			this.whoList.ScrollBars = ScrollBars.Vertical;
			this.whoList.Text = "Who List:" + Environment.NewLine + "In Progress..";


			/// <summary>
			/// Chat Input Box
			/// </summary>
			this.inputBox.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.inputBox.Location = new System.Drawing.Point(45, 570);
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(450, 20);
            this.inputBox.TabIndex = 2;
			this.inputBox.KeyDown += new KeyEventHandler(inputBox_KeyHandler);


			//
			// Username
			//
			this.textUsername.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.textUsername.Location = new System.Drawing.Point(60, 12);
            this.textUsername.Name = "textUsername";
            this.textUsername.Size = new System.Drawing.Size(130, 20);
            this.textUsername.TabIndex = 2;
			this.textUsername.Text = "Guest";
			this.textUsername.KeyDown += new KeyEventHandler(loginBox_KeyHandler);


			//
			// Password
			//
			this.textPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.textPassword.Location = new System.Drawing.Point(60, 40);
            this.textPassword.Name = "textPassword";
            this.textPassword.Size = new System.Drawing.Size(130, 20);
            this.textPassword.TabIndex = 2;
			this.textPassword.KeyDown += new KeyEventHandler(loginBox_KeyHandler);


			//
			// Username Label
			//
			this.usernameLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.usernameLabel.AutoSize = true;
			this.usernameLabel.Location = new System.Drawing.Point(5, 14);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(94, 13);
            this.usernameLabel.TabIndex = 0;
            this.usernameLabel.Text = "Username:";


			//
			// Password Label
			//
			this.passwordLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.passwordLabel.AutoSize = true;
			this.passwordLabel.Location = new System.Drawing.Point(5, 42);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(94, 13);
            this.passwordLabel.TabIndex = 0;
            this.passwordLabel.Text = " Password:";


			//
			// Login
			//
			this.loginButton.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.loginButton.Location = new System.Drawing.Point(60, 67);
			this.loginButton.Name = "loginButton";
			this.loginButton.Size = new System.Drawing.Size(57, 20);
			this.loginButton.TabIndex = 1;
			this.loginButton.Text = "Login";
			this.loginButton.UseVisualStyleBackColor = true;
			this.loginButton.Click += new System.EventHandler(this.loginButton_Click);


			//
			// Register
			//
			this.registerButton.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.registerButton.Location = new System.Drawing.Point(133, 67);
			this.registerButton.Name = "loginButton";
			this.registerButton.Size = new System.Drawing.Size(57, 20);
			this.registerButton.TabIndex = 1;
			this.registerButton.Text = "Register";
			this.registerButton.UseVisualStyleBackColor = true;
			this.registerButton.Click += new System.EventHandler(this.registerButton_Click);


			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.ColorTranslator.FromHtml("#008abe");
			this.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            this.ClientSize = new System.Drawing.Size(515, 600);
			this.Controls.Add(this.connectButton);
			this.Controls.Add(this.ipLabel);
			this.Controls.Add(this.portLabel);
			this.Controls.Add(this.textIP);
			this.Controls.Add(this.textPort);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.chatLabel);
			this.Controls.Add(this.outputBox);
			this.Controls.Add (this.whoList);
			this.Controls.Add(this.inputBox);
			this.Controls.Add(this.textUsername);
			this.Controls.Add(this.textPassword);
			this.Controls.Add(this.usernameLabel);
			this.Controls.Add(this.passwordLabel);
			this.Controls.Add(this.loginButton);
			this.Controls.Add(this.registerButton);
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle; // No Re-size
            this.MaximizeBox = false;
			this.MinimumSize =  new System.Drawing.Size(520, 300);
            this.Name = "Form1";
            this.Text = "Xirre Chat";
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		private System.Windows.Forms.Button connectButton;
		private System.Windows.Forms.Label ipLabel;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label portLabel;
		private System.Windows.Forms.TextBox textIP;
		private System.Windows.Forms.TextBox textPort;
		private System.Windows.Forms.Label chatLabel;
		private System.Windows.Forms.TextBox outputBox;
		private System.Windows.Forms.TextBox whoList;
		private System.Windows.Forms.TextBox inputBox;
		private System.Windows.Forms.TextBox textUsername;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label usernameLabel;
		private System.Windows.Forms.Label passwordLabel;
		private System.Windows.Forms.Button loginButton;
		private System.Windows.Forms.Button registerButton;
    }
}