using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorrentHandler
{
    public partial class ChoiceForm : Form
    {
        private Button musicButton;
        private Button generalButton;
        private Button tvButton;
        private Button movieButton;

        public ChoiceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChoiceForm));
            this.movieButton = new System.Windows.Forms.Button();
            this.musicButton = new System.Windows.Forms.Button();
            this.generalButton = new System.Windows.Forms.Button();
            this.tvButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // movieButton
            // 
            this.movieButton.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.movieButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.movieButton.Location = new System.Drawing.Point(12, 12);
            this.movieButton.Name = "movieButton";
            this.movieButton.Size = new System.Drawing.Size(106, 33);
            this.movieButton.TabIndex = 0;
            this.movieButton.Text = "Movie";
            this.movieButton.UseVisualStyleBackColor = true;
            this.movieButton.Click += new System.EventHandler(this.movieButton_Click);
            // 
            // musicButton
            // 
            this.musicButton.BackColor = System.Drawing.SystemColors.Control;
            this.musicButton.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.musicButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.musicButton.Location = new System.Drawing.Point(134, 61);
            this.musicButton.Name = "musicButton";
            this.musicButton.Size = new System.Drawing.Size(106, 33);
            this.musicButton.TabIndex = 3;
            this.musicButton.Text = "Music";
            this.musicButton.UseVisualStyleBackColor = false;
            this.musicButton.Click += new System.EventHandler(this.musicButton_Click);
            // 
            // generalButton
            // 
            this.generalButton.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generalButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.generalButton.Location = new System.Drawing.Point(12, 61);
            this.generalButton.Name = "generalButton";
            this.generalButton.Size = new System.Drawing.Size(106, 33);
            this.generalButton.TabIndex = 2;
            this.generalButton.Text = "General";
            this.generalButton.UseVisualStyleBackColor = true;
            this.generalButton.Click += new System.EventHandler(this.generalButton_Click);
            // 
            // tvButton
            // 
            this.tvButton.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.tvButton.Location = new System.Drawing.Point(134, 12);
            this.tvButton.Name = "tvButton";
            this.tvButton.Size = new System.Drawing.Size(106, 33);
            this.tvButton.TabIndex = 1;
            this.tvButton.Text = "TV";
            this.tvButton.UseVisualStyleBackColor = true;
            this.tvButton.Click += new System.EventHandler(this.tvButton_Click);
            // 
            // ChoiceForm
            // 
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(255, 108);
            this.Controls.Add(this.tvButton);
            this.Controls.Add(this.generalButton);
            this.Controls.Add(this.musicButton);
            this.Controls.Add(this.movieButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(509, 91);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChoiceForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TorrentHandler";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        // Mouseclick Handlers
        private void movieButton_Click(object sender, EventArgs e)
        {
            sendTorrent(Globalvar.MoviesFocus, Globalvar.MoviesPath);
            Close();
        }
        private void tvButton_Click(object sender, EventArgs e)
        {
            sendTorrent(Globalvar.TVFocus, Globalvar.TVPath);
            Close();
        }
        private void generalButton_Click(object sender, EventArgs e)
        {
            sendTorrent(Globalvar.GeneralFocus, Globalvar.GeneralPath);
            Close();
        }
        private void musicButton_Click(object sender, EventArgs e)
        {
            sendTorrent(Globalvar.MusicFocus, Globalvar.MusicPath);
            Close();
        }

        public void sendTorrent(String focusWhich, String handlerPath)
        {
            startProgram(focusWhich, "");
            startProgram(handlerPath, '\"' + Globalvar.torrentFile + '\"');
        }
        public void startProgram(String fileName, String arguments)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = fileName;
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            
            if(arguments.Equals(""))
            {
                pProcess.WaitForExit();
            }
        }
        public static void createTooltip(String info)
        {
            String tooltipPath = Globalvar.getSetting("Tooltip");
            Globalvar.choiceForm.startProgram(tooltipPath, info);

            if (!Globalvar.isRelease)
                Console.WriteLine(info);
        }
    }
}
